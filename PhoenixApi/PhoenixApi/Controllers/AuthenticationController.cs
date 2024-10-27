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
using PhoenixApi.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

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

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateHub([FromBody] HubLoginDto loginDto)
        {
            var hub = await _authService.GetHubByClientId(loginDto.ClientId);

            if (hub == null || !await _authService.ClientSecretIsValid(loginDto))
            {
                return Unauthorized();
            }

            try
            {
                var token = await _authService.GenerateJwtToken(hub.HubId);
                return Ok(new { AccessToken = token });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpGet("get-hub-credentials/{hubId}")]
        public async Task<IActionResult> GetHubCredentials(Guid hubId)
        {
            var hub = await _context.Hubs.SingleOrDefaultAsync(h => h.HubId == hubId && h.IsActive);

            if (hub == null)
            {
                return NotFound("Hub not found or inactive.");
            }

            return Ok(new
            {
                ClientId = hub.ClientId,
                ClientSecret = hub.ClientSecret
            });
        }
    }
    public class HubLoginDto
    {
        public required Guid ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }

    
}
