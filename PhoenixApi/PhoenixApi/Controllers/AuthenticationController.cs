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
        private readonly IAuthenticationRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ApiDbContext _context;

        public AuthenticationController(IAuthenticationService authService, IAuthenticationRepository authRepo, IConfiguration configuration)
        {
            _authRepository = authRepo;
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateHub([FromBody] HubLoginDto loginDto)
        {
            var hub = _authRepository.GetHub(loginDto);

            if (hub == null || !_authService.ClientSecretIsValid(loginDto))
            {
                return Unauthorized();
            }

            try
            {
                var token = _authService.GenerateJwtToken(hub.HubId);
                return Ok(new { AccessToken = token });
            }
            catch 
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    public class HubLoginDto
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
