using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MultiPortSignalR.Data;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Connect to PostgreSQL to get ports
var portDbConn = "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=postgres";
builder.Services.AddDbContext<PortContext>(options =>
    options.UseNpgsql(portDbConn));

//-----------
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// Build temporary service provider to query DB
using var tempProvider = builder.Services.BuildServiceProvider();

using var portDb = tempProvider.GetRequiredService<PortContext>();

var portList = await portDb.AppPorts.Select(p => p.Port).ToListAsync();

if (!portList.Any())
    portList.Add(5000); // fallback

// Configure Kestrel to listen on all retrieved ports
builder.WebHost.ConfigureKestrel(options =>
{
    foreach (var port in portList.Distinct())
    {
        options.ListenAnyIP(port);
    }
});

// Add SignalR and app services
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapHub<ChatHub>("/chathub");
Console.WriteLine("? SignalR Server is running at http://localhost:5050/chathub");

//app.MapGet("/", () => $"SignalR running on ports: {string.Join(", ", portList)}");

app.Run();


public class ChatHub : Hub
{
    private static ConcurrentDictionary<string, string> userConnections = new();
    public override async Task OnConnectedAsync()
    {

        var userId = Context.UserIdentifier; // now comes from query string

        userConnections[userId] = Context.ConnectionId;

        Console.WriteLine($"{userId} Client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier; // or from query
                                             // Find the user by their connection ID
        var connectionId = Context.ConnectionId;

        // Remove the connectionId from the dictionary
        var userToRemove = userConnections.FirstOrDefault(kvp => kvp.Value == connectionId).Key;

        if (!string.IsNullOrEmpty(userToRemove))
        {
            userConnections.TryRemove(userToRemove, out _);
        }

        Console.WriteLine($"{userId} Client disconnected: {Context.ConnectionId}");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine($"?? Message received from {user}: {message}");
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendMessageToUser(string user, string message)
    {
        var userId = Context.UserIdentifier; // or from query

        if (userConnections.TryGetValue(user, out var connId))
        {
            Console.WriteLine($"{user} Message received from {userId}: {message}");
            await Clients.Client(connId).SendAsync("ReceiveMessage", userId, message);
        }
    }
}
