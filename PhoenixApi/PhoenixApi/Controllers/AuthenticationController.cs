using PhoenixApi.Services;
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
        private readonly IAuthenticationService _authService;
        private readonly IConfiguration _configuration;
        private readonly ApiDbContext _context;

        public AuthenticationController(IAuthenticationService authService, IConfiguration configuration, ApiDbContext context)
        {
            _authService = authService;
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
            try
            {
                var token = _authService.GenerateJwtToken(hub.HubId);
                return Ok(new { AccessToken = token });
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
