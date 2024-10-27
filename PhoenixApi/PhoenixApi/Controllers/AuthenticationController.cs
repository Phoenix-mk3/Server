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
using Microsoft.AspNetCore.Authentication.OAuth;

namespace PhoenixApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthenticationService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateHub([FromBody] HubLoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            var hub = await _authService.GetHubByClientId(loginDto.ClientId);

            if (hub == null || !await _authService.ClientSecretIsValid(loginDto))
            {
                return Unauthorized("Invalid login");
            }

            try
            {
                var token = await _authService.GenerateJwtTokenWithHubId(hub.HubId);
                var expiration = DateTime.UtcNow.AddHours(12);

                var response = new AuthResponse
                {
                    AccessToken = token,
                    Expiration = expiration,
                };

                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("hub-credentials")]
        public async Task<IActionResult> GetHubCredentials([FromQuery] Guid hubId)
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
            catch (Exception ex)
            {
                _logger.LogWarning("Internal error during 'Get Hub Credentials'. {ex} ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(login);
        }
    }
    public class HubLoginDto
    {
        public required Guid ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }


}
