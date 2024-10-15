using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhoenixApi.Data;
using PhoenixApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApiDbContext _context;

        public AuthenticationController(IConfiguration configuration, ApiDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Endpoint to authenticate a hub and generate a JWT token
        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateHub([FromBody] HubLoginDto loginDto)
        {
            
            // Find the hub by clientId
            var hub = await _context.Hubs.SingleOrDefaultAsync(h => h.ClientId == loginDto.ClientId && h.IsActive);

            if (hub == null)
            {
                return Unauthorized(); // Return 401 Unauthorized if client ID or secret is invalid
            }

            // Generate a JWT token for the hub
            var token = GenerateJwtToken(hub.HubId);

            return Ok(new { AccessToken = token });
        }

        private string GenerateJwtToken(Guid hubId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSecretKey"]); // Secret key for signing JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("hubId", hubId.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(30), // Token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Return the JWT as a string
        }

        private bool VerifyClientSecret(string providedSecret, string storedHashedSecret)
        {
            // Compare provided client secret with the hashed version in the database
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(providedSecret));
                var hashedSecret = Convert.ToBase64String(hashedBytes);
                return hashedSecret == storedHashedSecret;
            }
        }
    }

    public class HubLoginDto
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
