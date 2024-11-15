using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PhoenixApi.Controllers;
using PhoenixApi.Models;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.Responses;
using PhoenixApi.Models.Security;
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
        Task<Hub> GetHubByClientId(Guid clientId);
        LoginDto GenerateLoginCredentials();
        Task UpdateHubWithCredentials(Guid hubId, Guid clientId, string clientString);
        Task<UserAuthResponse> BuildUserAuthResponse(ClaimsPrincipal claims);
        Task UpdateUserWithCredentials(ClaimsPrincipal claims, Guid clientId, string clientSecret);
        Task CheckUserInHub(ClaimsPrincipal claims, Guid hubId);
        Task<AuthResponse> AuthorizeSubject(Guid clientId, string clientSecret, bool isHub);
    }
    public class AuthenticationService(IHubRepository hubRepository, IConfiguration config, ILogger<AuthenticationService> logger, IUnitOfWork unitOfWork, IClaimsRetrievalService claimsRetrievalService, IUserRepository userRepository): IAuthenticationService
    {
        public async Task<UserAuthResponse> BuildUserAuthResponse(ClaimsPrincipal claims)
        {
            Guid hubId = claimsRetrievalService.GetSubjectIdFromClaims(claims);
            Hub hub = await GetHubByHubId(hubId);

            User user = new User();
            user.Id = Guid.NewGuid();
            user.Hubs.Add(hub);

            var clientId = Guid.NewGuid();
            user.ClientId = clientId;

            var clientSecret = Guid.NewGuid();
            user.ClientSecret = HashSecret(clientSecret.ToString());

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            var token = await GenerateJwtToken(user.Id.ToString(), AuthRole.TempUser, 10);

            UserAuthResponse userAuthResponse = new UserAuthResponse()
            {
                UserId = user.Id,
                TemporaryAuthToken = token
            };

            return userAuthResponse;
        }
        private string GetSigningKeyFromConfig()
        {
            var signingKey = config["Jwt:SecretKey"] ?? null;
            if (signingKey == null)
            {
                logger.LogError("Signing not found. {signKeyStatus}", signingKey);
                throw new ArgumentNullException(nameof(signingKey));
            }
            logger.LogInformation("Retrieved Signing key");
            return signingKey;
        }
        private async Task<string> GenerateJwtToken(string subject, AuthRole authRole, int expirationTime = 12*60)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var signingKey = GetSigningKeyFromConfig();

            var tokenIdentifier = Guid.NewGuid();
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, subject),
                new(JwtRegisteredClaimNames.Jti, tokenIdentifier.ToString()),
                new(JwtRegisteredClaimNames.Iat, issuedAt, ClaimValueTypes.Integer64),
                new(ClaimTypes.Role, authRole.ToString())
            };


            //TESTING PURPOSES ONLY, REMOVE LATER)
            if (subject == "cbb69446-b121-4549-a4eb-b8d7384072c2")
            {
                claims.RemoveAt(claims.Count-1);
                claims.Add(new Claim(ClaimTypes.Role, nameof(AuthRole.Admin)));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(expirationTime);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: creds
                );

            logger.LogInformation("New key generated with id '{jti}', for {authRole} '{hubId}'. Expires at: {expr}", tokenIdentifier, authRole, subject, expiration);


            return tokenHandler.WriteToken(token);
        }
        private bool ClientSecretIsValid(string clientSecretIn, string storedClientSecret)
        {

            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(clientSecretIn));
            var hashedSecret = Convert.ToBase64String(hashedBytes);
            return hashedSecret == storedClientSecret;
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

        public LoginDto GenerateLoginCredentials()
        {
            var newClientId = Guid.NewGuid();
            var newClientSecret = Guid.NewGuid().ToString();

            LoginDto loginDto = new()
            {
                ClientId = newClientId,
                ClientSecret = newClientSecret
            };

            return loginDto;
        }
        public async Task UpdateHubWithCredentials(Guid hubId, Guid clientId, string clientSecret)
        {
            if(clientId == Guid.Empty || clientSecret == null)
            {
                throw new ArgumentNullException("ClientId or ClientSecret cannot be empty (null)!");
            }

            Hub hub = await GetHubByHubId(hubId);
            if (hub.ClientId != null || hub.ClientSecret != null)
            {
                throw new DuplicateClientInfoException($"ClientId or Secret already exists for hub {hubId}");
            }

            hub.ClientId = clientId;
            hub.ClientSecret = HashSecret(clientSecret);

            await hubRepository.UpdateAsync(hubId, hub);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserWithCredentials(ClaimsPrincipal claims, Guid clientId, string clientSecret)
        {
            if (clientId == Guid.Empty || clientSecret == null)
            {
                throw new ArgumentNullException("ClientId or ClientSecret cannot be empty (null)!");
            }

            var userId = claimsRetrievalService.GetSubjectIdFromClaims(claims);

            User user = await userRepository.GetByIdAsync(userId);
            if (user.ClientId != null || user.ClientSecret != null)
            {
                throw new DuplicateClientInfoException($"ClientId or Secret already exists for user {userId}");
            }

            user.ClientId = clientId;
            user.ClientSecret = HashSecret(clientSecret);

            await userRepository.UpdateAsync(userId, user);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task CheckUserInHub(ClaimsPrincipal claims,  Guid hubId)
        {
            var userId = claimsRetrievalService.GetSubjectIdFromClaims(claims);

            var hub = await hubRepository.GetByIdAsync(hubId);
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null) throw new InvalidOperationException("User not found.");
            if (hub == null) throw new InvalidOperationException("Hub not found.");

            if (!user.Hubs.Any(h => h.HubId == hub.HubId))
            {
                throw new UnauthorizedAccessException("User is not associated with the specified hub.");
            }
        }

        private async Task<SubjectDto> CreateSubject(Guid clientId, bool isHub)
        {
            SubjectDto subject = new();
            if (isHub)
            {
                var hub = await hubRepository.GetHubByClientIdAsync(clientId) ?? throw new ArgumentNullException("Hub not found in database");

                subject.Id = hub.HubId;
                subject.ClientSecret = hub.ClientSecret!;
                subject.ClientId = (Guid)hub.ClientId!;
                subject.Role = AuthRole.Hub;
            }
            else
            {
                var user = await userRepository.GetUserByClientIdAsync(clientId) ?? throw new ArgumentNullException("User not found in database");

                subject.Id = user.Id;
                subject.ClientSecret = user.ClientSecret!;
                subject.ClientId = (Guid)user.ClientId!;
                subject.Role = AuthRole.User;
            }
            return subject;
        }


        public async Task<AuthResponse> AuthorizeSubject(Guid clientId, string clientSecret, bool isHub)
        {
            SubjectDto subject = await CreateSubject(clientId, isHub);

            if(!ClientSecretIsValid(clientSecret, subject.ClientSecret))
            {
                throw new UnauthorizedAccessException("Client secret is invalid");
            }

            var expirationTime = 12 * 60;

            return new AuthResponse
            {
                AccessToken = await GenerateJwtToken(subject.Id.ToString(), subject.Role, expirationTime),
                Expiration = DateTime.UtcNow.AddMinutes(expirationTime)
            };
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
