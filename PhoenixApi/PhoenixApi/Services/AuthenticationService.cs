using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PhoenixApi.Controllers;
using PhoenixApi.Models;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
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
            var signingKey = config["Jwt:SecretKey"] ?? null;
            if (signingKey == null)
            {
                logger.LogError("Signing not found. {signKeyStatus}", signingKey);
                throw new ArgumentNullException(nameof(signingKey));
            }
            logger.LogInformation("Retrieved Signing key");

            var jti = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, hubId.ToString()),
                new(JwtRegisteredClaimNames.Jti, jti.ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.Role, "Hub")
            };


            //TESTING PURPOSES ONLY, REMOVE LATER)
            if (hubId.ToString() == "cbb69446-b121-4549-a4eb-b8d7384072c2")
            {
                claims.Add(new Claim("Permission", "IsAdmin"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(12);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: creds
                );

            logger.LogInformation("New key generated with id '{jti}', for hub '{hubId}'. Expires at: {expr}", jti, hubId, expiration);


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
            if (hub == null)
            {
                logger.LogWarning("Hub with client id '{clientId}' not found in Database", clientId);
                throw new KeyNotFoundException($"Hub with client id {clientId} not found");
            }
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
    
    public class DuplicateClientInfoException : Exception
    {
        public DuplicateClientInfoException() : base("ClientId or Secret already exists") { }
        public DuplicateClientInfoException(string message) : base(message) { }

    }
}
