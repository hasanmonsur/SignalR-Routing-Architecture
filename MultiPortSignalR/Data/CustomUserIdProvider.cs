using Microsoft.AspNetCore.SignalR;

namespace MultiPortSignalR.Data
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Extract from query string
            var httpContext = connection.GetHttpContext();
            var userId = httpContext?.Request.Query["userId"].ToString();
            return userId; // This becomes Context.UserIdentifier
        }
    }
}
