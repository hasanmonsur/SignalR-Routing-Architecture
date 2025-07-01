

using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5050/chathub?userId=5050")
    .WithAutomaticReconnect()
    .Build();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"💬 Received message from {user}: {message}");
});


connection.Reconnected += (connectionId) =>
{
    Console.WriteLine($"🔁 Reconnected: {connectionId}");
    return Task.CompletedTask;
};

connection.Closed += (error) =>
{
    Console.WriteLine("❌ Connection closed.");
    return Task.CompletedTask;
};

await connection.StartAsync();
Console.WriteLine("✅ Connected to SignalR hub.");

Console.Write("Your name: ");
var userName = Console.ReadLine();

while (true)
{
    var message = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine($"📤 Sending: {message}");
        await connection.SendAsync("SendMessage", userName, message);
    }
}