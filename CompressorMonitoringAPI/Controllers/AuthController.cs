using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CompressorMonitoringAPI.Data;
using Microsoft.AspNetCore.Authorization;
using CompressorMonitoringAPI.Services;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly AppDbContext _context;

        public AuthController(JwtService jwtService, AppDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Ищем пользователя в базе данных
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
            }

            // Обновляем время последнего входа
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                FullName = user.FullName,
                Expires = DateTime.Now.AddHours(8)
            };

            return Ok(response);
        }

        [HttpPost("verify")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            // Токен уже валидирован middleware, просто возвращаем информацию о пользователе
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            return Ok(new 
            { 
                Username = username, 
                Role = role, 
                FullName = fullName,
                IsValid = true 
            });
        }

        // Модели для запросов и ответов
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public DateTime Expires { get; set; }
        }
    }
}