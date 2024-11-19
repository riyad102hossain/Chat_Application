using ChatServer.Context;
using ChatServer.Dtos;
using ChatServer.Hubs;
using ChatServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public ChatController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;

        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<User> users = await _context.Users.OrderBy(p => p.Name).ToListAsync();
            return Ok(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
        {
            List<Chat> chats =
                await _context
                .Chats
                .Where(p =>
                p.UserId == userId && p.ToUserId == toUserId ||
                p.ToUserId == userId && p.UserId == toUserId)
                .OrderBy(p => p.Date)
                .ToListAsync(cancellationToken);

            return Ok(chats);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] IFormFile? file, [FromForm] SendMessageDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors)
                                       .Select(x => x.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            var chat = new Chat
            {
                UserId = request.UserId,
                ToUserId = request.ToUserId,
                Message = request.Message,
                Date = DateTime.Now
            };

            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot", "uploads", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                chat.FilePath = $"/uploads/{file.FileName}";
                chat.Message = $"Sent a file: {file.FileName}";
            }

            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            // Notify the receiver via SignalR
            string connectionId = ChatHub.Users.FirstOrDefault(p => p.Value == chat.ToUserId).Key;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("Messages", chat);
            }

            return Ok(chat);
        }




    }
}
