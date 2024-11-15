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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PhoenixApi.Models.Security;

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

        [HttpGet("hub-user-auth")]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> UserAuthThroughHub()
        {
            try
            {
                var response = await _authService.BuildUserAuthResponse(User);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong during hub-user-auth");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateSubject([FromBody] LoginDto loginDto, [FromQuery]bool isHub)
        {
            if (loginDto == null || loginDto.ClientId == Guid.Empty || string.IsNullOrEmpty(loginDto.ClientSecret))
            {
                return BadRequest("Request body cannot be null");
            }

            try
            {
                AuthResponse response = await _authService.AuthorizeSubject(loginDto.ClientId, loginDto.ClientSecret, isHub);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Client not found in database for ClientId {clientId}", loginDto.ClientId);
                return NotFound(new { ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for ClientId {clientId}", loginDto.ClientId);
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Invalid client secret." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error occurred while authorizing ClientId {clientId}", loginDto.ClientId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("user-credentials")]
        [Authorize(Roles = nameof(AuthRole.TempUser))]
        public async Task<IActionResult> GetUserCredentials([FromQuery] Guid hubId)
        {
            try
            {
                await _authService.CheckUserInHub(User, hubId);
                LoginDto login = _authService.GenerateLoginCredentials();
                await _authService.UpdateUserWithCredentials(User, login.ClientId, login.ClientSecret);
                login.ClientSecret.Append(char.Parse(AuthRole.User.ToString()));
                return Ok(login);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Could not find user {user} in hub {hubId}", User, hubId);
                return NotFound(new { ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Could not find user or hub association in database");
                return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Could not find user in hub with hubId {hubId}", hubId);
                return NotFound("Hub not found or inactive.");
            }
            catch (DuplicateClientInfoException ex)
            {
                _logger.LogWarning(ex, "Attempted to set ClientId or ClientSecret more than once for user {user}", User);
                return Conflict("ClientId or Secret already exists for this user.");
            }
            catch (Exception ex)
            {
                _logger.LogError("An internal error occurred during 'Get User Credentials'. {ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("hub-credentials")]
        public async Task<IActionResult> GetHubCredentials([FromQuery] Guid hubId)
        {
            LoginDto login;
            try
            {
                login = _authService.GenerateLoginCredentials();
                await _authService.UpdateHubWithCredentials(hubId, login.ClientId, login.ClientSecret);

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
    public class LoginDto
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
