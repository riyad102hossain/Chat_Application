using ChatServer.Context;
using ChatServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Hubs
{
    public sealed class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public static Dictionary<string, Guid> Users = new();
        public async Task Connect(Guid userId)
        {
            Users.Add(Context.ConnectionId, userId);
            User? user = await _context.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = "online";
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid userId;
            Users.TryGetValue(Context.ConnectionId, out userId);
            Users.Remove(Context.ConnectionId);
            User? user = await _context.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = "offline";
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
            }
        }
    }
}
