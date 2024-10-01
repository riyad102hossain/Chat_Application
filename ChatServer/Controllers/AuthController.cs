using ChatServer.Context;
using ChatServer.Dtos;
using ChatServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Controllers
{
    [Route("api/[controller]/[action]")]
    
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileService _fileService; // Inject the FileService
        public AuthController(AppDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }


        [HttpPost]
        public async Task<IActionResult>Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
        {
            bool isNameExists = await _context.Users.AnyAsync(p => p.Name == request.Name,cancellationToken );

            if (isNameExists)
            {
                return BadRequest(new { message = "Invalid name " });
            }

            //string avatar = FileService.FileSaveToServer(request.File, "/wwwroot/avatar");
        
            string avatar = await _fileService.FileSaveToServer(request.File, Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatar"));

            User user = new()
            {
                Name = request.Name,
                Avatar = avatar,
            };

            await _context.Users.AddAsync(user,cancellationToken);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string name, CancellationToken cancellationToken)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

            if (user is null)
            {
                return BadRequest(new { Message = "Kullanıcı bulunamadı" });
            }

            user.Status = "online";

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(user);
        }

    }
}
