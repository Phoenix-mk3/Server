using Microsoft.IdentityModel.Tokens;
using PhoenixApi.Controllers;
using PhoenixApi.Models;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Services
{
    public interface IAuthenticationService
    {
        Task<string> GenerateJwtToken(Guid hubId);
        Task<bool> ClientSecretIsValid(HubLoginDto hubLoginDto);
        Task<Hub> GetHubByClientId(Guid clientId);
        HubLoginDto GenerateHubCredentials();
        Task UpdateHubWithCredentials(Guid hubId, HubLoginDto loginDto);
    }
    public class AuthenticationService(IHubRepository hubRepository, IConfiguration config, ILogger<AuthenticationService> logger, IUnitOfWork unitOfWork): IAuthenticationService
    {
        public async Task<string> GenerateJwtToken(Guid hubId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var signingKey = config["JwtSecretKey"] ?? null;
            if (signingKey == null)
            {
                logger.LogError("Signing not found. {signKeyStatus}", signingKey);
                throw new ArgumentNullException(nameof(signingKey));
            }
            logger.LogInformation("Retrieved Signing key");
            logger.LogDebug("{signingKey}", signingKey);
            var key = Encoding.ASCII.GetBytes(signingKey); // Secret key for signing JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([new Claim("hubId", hubId.ToString())]),
                Expires = DateTime.UtcNow.AddHours(12), // Token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<bool> ClientSecretIsValid(HubLoginDto loginDto)
        {
            Hub hub = await hubRepository.GetHubByClientIdAsync(loginDto.ClientId);

            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(loginDto.ClientSecret));
            var hashedSecret = Convert.ToBase64String(hashedBytes);
            return hashedSecret == hub.ClientSecret;
        }

        public async Task<Hub> GetHubByClientId(Guid clientId)
        {
            Hub hub = await hubRepository.GetHubByClientIdAsync(clientId);
            return hub;
        }

        private async Task<Hub> GetHubByHubId(Guid hubId)
        {
            Hub hub = await hubRepository.GetByIdAsync(hubId) ?? throw new KeyNotFoundException($"Could not find Hub with id {hubId}");
            return hub;
        }

        public HubLoginDto GenerateHubCredentials()
        {
            var newClientId = Guid.NewGuid();
            var newClientSecret = Guid.NewGuid().ToString();

            HubLoginDto loginDto = new()
            {
                ClientId = newClientId,
                ClientSecret = newClientSecret
            };

            return loginDto;
        }
        public async Task UpdateHubWithCredentials(Guid hubId, HubLoginDto loginDto)
        {
            Hub hub = await GetHubByHubId(hubId);
            if (hub.ClientId != null || hub.ClientSecret != null)
            {
                throw new DuplicateClientInfoException($"ClientId or Secret already exists for hub {hubId}");
            }

            hub.ClientId = loginDto.ClientId;
            hub.ClientSecret = HashSecret(loginDto.ClientSecret!);

            await hubRepository.UpdateAsync(hubId, hub);
            await unitOfWork.SaveChangesAsync();

        }


        private string HashSecret(string secret)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            var hashedSecret = Convert.ToBase64String(hashedBytes);
            return hashedSecret;
        }
    }
    
    public class DuplicateClientInfoException(string message) : Exception(message)
    {
    }
}
