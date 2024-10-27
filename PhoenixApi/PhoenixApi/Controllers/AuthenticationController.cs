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
        private readonly IHubService _hubService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthenticationService authService, IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
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
            HubLoginDto login;
            try
            {
                login = _authService.GenerateHubCredentials();
                await _authService.UpdateHubWithCredentials(hubId, login);

            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Could not find hub with id {hubId}", hubId);
                return NotFound("Hub not found or inactive.");
            }
            catch (DuplicateClientInfoException ex) 
            {
                _logger.LogWarning(ex, "Attempted to set ClientId or ClientSecret more than once for hub {HubId}", hubId);
                return Conflict("ClientId or Secret already exists for this hub.");
            }

            return Ok(new
            {
                login.ClientId,
                login.ClientSecret
            });
        }
    }
    public class HubLoginDto
    {
        public required Guid ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }

    
}
