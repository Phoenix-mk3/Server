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
        public async Task<IActionResult> AuthenticateClient([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            return StatusCode(StatusCodes.Status501NotImplemented)  ;
        }


        [HttpPost("login-hub")]
        public async Task<IActionResult> AuthenticateHub([FromBody] LoginDto loginDto)
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
                var token = await _authService.GenerateHubToken(hub.HubId);
                var expiration = DateTime.UtcNow.AddHours(12);

                var response = new AuthResponse
                {
                    AccessToken = token,
                    Expiration = expiration,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred {ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("user-credentials")]
        [Authorize(Roles = nameof(AuthRole.TempUser))]
        public async Task<IActionResult> GetUserCredentials([FromQuery] Guid hubId)
        {
            try
            {
                await _authService.CheckUserInHub(User, hubId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Could not find user {user} in hub {hubId}", User, hubId);
                return NotFound(new { ex.Message });
            }
            catch(UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Could not find user or hub association in databse");
                return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
            }
            catch (Exception ex) // Handle unexpected exceptions
            {
                _logger.LogError("An internal error occurred during 'Get User Credentials'. {ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }

            LoginDto login;
            try
            {
                login = _authService.GenerateLoginCredentials();
                await _authService.UpdateUserWithCredentials(User, login);
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
                _logger.LogWarning("An internal error during 'Get User Credentials'. {ex} ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
            login.ClientSecret.Append(char.Parse(AuthRole.User.ToString()));
            return Ok(login);
        }

        [HttpGet("hub-credentials")]
        public async Task<IActionResult> GetHubCredentials([FromQuery] Guid hubId)
        {
            LoginDto login;
            try
            {
                login = _authService.GenerateLoginCredentials();
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
    public class LoginDto
    {
        public required Guid ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
    //public class UserLoginDto
    //{
    //    public required string TempA
    //}
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }
    


}
