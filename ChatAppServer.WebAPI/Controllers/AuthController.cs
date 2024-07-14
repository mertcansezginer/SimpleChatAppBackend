using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ChatAppServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class AuthController(ApplicationDbContext context) : ControllerBase
    {
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
        {
            bool isNameExists = await context.Users.AnyAsync(p => p.Name == request.Name, cancellationToken);

            if (isNameExists)
            {
                return BadRequest(new { Message = "Bu kullanıcı adı daha önce kullanılmış." });
            }

            User user = new()
            {
                Name = request.Name,
                PasswordHash = HashPassword(request.Password)
            };

            await context.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string name, string password, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

            if (user == null || user.PasswordHash != HashPassword(password))
            {
                return BadRequest(new { Message = "Kullanıcı adı veya şifre hatalı." });
            }

            user.Status = "online";

            await context.SaveChangesAsync(cancellationToken);

            return Ok(user);
        }
    }
}