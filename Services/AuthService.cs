using Todo.Api.Data;
using Todo.Api.DTOs.Auth;
using Todo.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Todo.Api.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context; //link for db
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // keeps data from appsettings.json
        }

        //Registration
        public async Task<AuthUserResponse?> Register(RegisterRequest request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null) return null;

            //password becomes hash
            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hash,
                DisplayName = request.DisplayName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName
            };
        }
        //Login
        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid) return null;

            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresInSeconds = 3600,
                User = new AuthUserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                }
            };
        }
        //token generation
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}