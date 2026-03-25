using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PulseDesk.Data;
using PulseDesk.DTOs.Auth;
using PulseDesk.Models;
using PulseDesk.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PulseDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        private readonly IConfiguration _config = config;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTOs.Auth.RegisterRequest req)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (existingUser != null) { return BadRequest(new { message = "Email already in use." }); }

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = UserRole.Customer
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTOs.Auth.LoginRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid Credentials" });

            }

            bool validPassword = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);

            if (!validPassword)
            {
                return Unauthorized(new { message = "Invalid Credentials" });
            }
            var token = GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        private string GenerateToken(User user)
        {
            var secret = _config["JwtSettings:Secret"]!;
            var secureKeyBytes = Encoding.UTF8.GetBytes(secret);
            var signingKey = new SymmetricSecurityKey(secureKeyBytes);

            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(
                    int.Parse(_config["JwtSettings:ExpiryDays"]!)),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
