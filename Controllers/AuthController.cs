using Microsoft.AspNetCore.Authorization;
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
    /// <summary>
    /// Handles any user authentication or authorization (Login, Registration and Role Management)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AppDbContext db, IConfiguration config) : BaseController
    {
        private readonly AppDbContext _db = db;

        private readonly IConfiguration _config = config;

        /// <summary>
        /// Registers a user to the system and returns a success message. Users always are registered as a Customer by default (Admins can change the role of a user later)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/Auth/register
        ///     
        /// </remarks>
        /// <param name="req">The request object coming from the client</param>
        /// <returns>A success response</returns>
        /// <response code="200">Allows the user to register</response>
        /// <response code="401">If the user is not found/Authorized</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
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

        /// <summary>
        /// Logs the user into the system and returns a JWT token for authentication
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/Auth/login
        ///     
        /// </remarks>
        /// <param name="req">The request object coming from the client</param>
        /// <returns>A AuthResponse object</returns>
        /// <response code="200">Allows the user to login</response>
        /// <response code="401">If the user is not found/Authorized</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
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


        /// <summary>
        /// Updates the role of a user (Only an Admin can change a user's role)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT api/Auth/123/role
        ///     
        /// </remarks>
        /// <param name="id">The unique identifier for the user that will be changed</param>
        /// <param name="req">The request body coming from the client</param>
        /// <returns>Success Response</returns>
        /// <response code="200">Returns a success response for user's role change</response>
        /// <response code="401">If the user is not found/Authorized</response>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody]UpdateRoleRequest req)
        {
            var user = await _db.Users.FindAsync(id);

            if(user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if(user.Id == CurrentUserId)
            {
                return BadRequest(new { message = "Cannot change your own role" });
            }

            var oldRole = user.Id;

            user.Role = req.Role;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Updated role from {oldRole} to {req.Role}" });
        }

        // Helper method to generate JWT token (authenticated users)
        private string GenerateToken(User user)
        {
            var secret = _config["JwtSettings:Secret"]!;
            var secureKeyBytes = Encoding.UTF8.GetBytes(secret);
            var signingKey = new SymmetricSecurityKey(secureKeyBytes);

            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
            new Claim("nameid", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("role", user.Role.ToString())
            };

            var issuer = _config["JwtSettings:Issuer"]!;
            var audience = _config["JwtSettings:Audience"]!;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(
                    int.Parse(_config["JwtSettings:ExpiryDays"]!)),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
