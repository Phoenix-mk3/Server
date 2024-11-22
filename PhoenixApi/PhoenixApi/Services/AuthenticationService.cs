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
        Task<AuthResponse> AuthorizeSubject(Guid clientId, string clientSecret, bool isHub);
        Task<Hub> GetHubByClientId(Guid clientId);
        LoginDto GenerateLoginCredentials();
        Task UpdateHubWithCredentials(Guid hubId, Guid clientId, string clientString);
        Task<UserAuthResponse> BuildUserAuthResponse(ClaimsPrincipal claims);
        Task UpdateUserWithCredentials(ClaimsPrincipal claims, Guid clientId, string clientSecret);
        Task CheckUserInHub(ClaimsPrincipal claims, Guid hubId);
    }
    public class AuthenticationService(IHubRepository hubRepository, IConfiguration config, ILogger<AuthenticationService> logger, IUnitOfWork unitOfWork, IClaimsRetrievalService claimsRetrievalService, IUserRepository userRepository): IAuthenticationService
    {
        public async Task<UserAuthResponse> BuildUserAuthResponse(ClaimsPrincipal claims)
        {
            logger.LogInformation("Starting BuildUserAuthResponse for user authentication.");

            try
            {
                //Retrieve Hub ID from claims
                Guid hubId;
                try
                {
                    hubId = claimsRetrievalService.GetSubjectIdFromClaims(claims);
                    logger.LogInformation("Successfully retrieved from auth token Hub ID: {HubId}", hubId);
                }
                catch (ArgumentNullException ex)
                {
                    logger.LogError(ex, "Claims retrieval failed. Claims were null or missing required fields.");
                    throw new InvalidOperationException("Invalid claims data provided.", ex);
                }

                //Retrieve Hub by ID
                Hub hub;
                try
                {
                    hub = await GetHubByHubId(hubId);
                    logger.LogInformation("Successfully retrieved Hub with ID: {HubId}", hubId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to retrieve Hub for Hub ID: {HubId}", hubId);
                    throw new InvalidOperationException($"Failed to retrieve hub with ID: {hubId}", ex);
                }

                //Create new User
                var user = new User
                {
                    Id = Guid.NewGuid()
                };
                user.Hubs.Add(hub);
                logger.LogInformation("Created new User with ID: {UserId}", user.Id);

                //Add User to repository and save changes
                try
                {
                    await userRepository.AddAsync(user);
                    await unitOfWork.SaveChangesAsync();
                    logger.LogInformation("Successfully added User with ID: {UserId} to the database.", user.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to save User to the database. User ID: {UserId}", user.Id);
                    throw new InvalidOperationException("Failed to save user data to the database.", ex);
                }

                //Generate temporary JWT token
                string token;
                try
                {
                    int timeInMinutes = 10;
                    token = await GenerateJwtToken(user.Id.ToString(), AuthRole.TempUser, timeInMinutes);
                    logger.LogInformation("Generated temporary JWT token for User ID: {UserId}", user.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to generate JWT token for User ID: {UserId}", user.Id);
                    throw new InvalidOperationException("Failed to generate authentication token.", ex);
                }

                //Build response
                var userAuthResponse = new UserAuthResponse
                {
                    UserId = user.Id,
                    TemporaryAuthToken = token
                };
                logger.LogInformation("Successfully built UserAuthResponse for User ID: {UserId}", user.Id);

                return userAuthResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in BuildUserAuthResponse.");
                throw;
            }
            finally
            {
                logger.LogInformation("Completed BuildUserAuthResponse.");
            }
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
        private async Task<string> GenerateJwtToken(string subject, AuthRole authRole, int expirationTime = 12 * 60)
        {
            logger.LogInformation("Starting JWT token generation for subject: {Subject}, Role: {AuthRole}, Expiration: {ExpirationTime} minutes.", subject, authRole, expirationTime);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var signingKey = GetSigningKeyFromConfig();

                if (string.IsNullOrEmpty(signingKey))
                {
                    logger.LogError("Signing key configuration is missing or invalid.");
                    throw new InvalidOperationException("JWT signing key is not configured.");
                }

                var issuer = config["Jwt:Issuer"];
                var audience = config["Jwt:Audience"];

                if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                {
                    logger.LogError("JWT issuer or audience configuration is missing.");
                    throw new InvalidOperationException("JWT issuer or audience configuration is invalid.");
                }

                var tokenIdentifier = Guid.NewGuid();
                var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, subject),
                    new(JwtRegisteredClaimNames.Jti, tokenIdentifier.ToString()),
                    new(JwtRegisteredClaimNames.Iat, issuedAt, ClaimValueTypes.Integer64),
                    new(ClaimTypes.Role, authRole.ToString())
                };

                //This is so we can test using admin keys (static set of hubId in db context, then assign that id the admin role here)
                if (subject == "cbb69446-b121-4549-a4eb-b8d7384072c2")
                {
                    logger.LogInformation("Applying admin role for special subject ID: {Subject}", subject);
                    claims.RemoveAt(claims.Count - 1); // Remove the original role
                    claims.Add(new Claim(ClaimTypes.Role, nameof(AuthRole.Admin)));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiration = DateTime.UtcNow.AddMinutes(expirationTime);

                //Generate the token
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: expiration,
                    signingCredentials: creds
                );

                logger.LogInformation("Generated JWT token. Identifier: {Jti}, Subject: {Subject}, Role: {AuthRole}, Expires: {Expiration}",
                    tokenIdentifier, subject, authRole, expiration);

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate JWT token for subject: {Subject} with role: {AuthRole}.", subject, authRole);
                throw new InvalidOperationException("Error occurred during JWT token generation.", ex);
            }
        }

        private bool ClientSecretIsValid(string clientSecretIn, string storedClientSecret)
        {

            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(clientSecretIn));
            var hashedSecret = Convert.ToBase64String(hashedBytes);
            return hashedSecret == storedClientSecret;
        }

        public async Task<Hub> GetHubByClientId(Guid clientId)
        {
            logger.LogInformation("Attempting to retrieve Hub with Client ID: {ClientId}", clientId);

            Hub hub = await hubRepository.GetHubByClientIdAsync(clientId);

            if (hub == null)
            {
                logger.LogWarning("Hub with Client ID '{ClientId}' not found in the database.", clientId);
                throw new KeyNotFoundException($"Hub with Client ID {clientId} not found.");
            }

            logger.LogInformation("Successfully retrieved Hub with Client ID: {ClientId}", clientId);
            return hub;
        }


        private async Task<Hub> GetHubByHubId(Guid hubId)
        {
            Hub hub = await hubRepository.GetByIdAsync(hubId) ?? throw new KeyNotFoundException($"Could not find Hub with id {hubId}");
            return hub;
        }

        public LoginDto GenerateLoginCredentials()
        {
            try
            {
                logger.LogInformation("Starting generation of new login credentials.");

                var newClientId = Guid.NewGuid();
                var newClientSecret = Guid.NewGuid().ToString();

                var loginDto = new LoginDto
                {
                    ClientId = newClientId,
                    ClientSecret = newClientSecret
                };

                logger.LogInformation("Successfully generated new login credentials. ClientId: {ClientId}", newClientId);

                return loginDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while generating login credentials.");
                throw new InvalidOperationException("Failed to generate login credentials.", ex);
            }
        }

        public async Task UpdateHubWithCredentials(Guid hubId, Guid clientId, string clientSecret)
        {
            logger.LogInformation("Starting UpdateHubWithCredentials for Hub ID: {HubId}", hubId);

            //Validate inputs
            if (clientId == Guid.Empty || string.IsNullOrEmpty(clientSecret))
            {
                logger.LogWarning("Invalid ClientId or ClientSecret provided for Hub ID: {HubId}", hubId);
                throw new ArgumentException("ClientId or ClientSecret cannot be empty or null.");
            }

            //Retrieve the hub
            Hub hub = await GetHubByHubId(hubId);
            logger.LogInformation("Retrieved Hub with ID: {HubId}", hubId);

            //Check for existing credentials
            if (hub.ClientId != null || hub.ClientSecret != null)
            {
                logger.LogWarning("Hub {HubId} already has ClientId or ClientSecret assigned.", hubId);
                throw new DuplicateClientInfoException($"ClientId or Secret already exists for Hub {hubId}");
            }

            //Update the hub with new credentials
            hub.ClientId = clientId;
            hub.ClientSecret = HashSecret(clientSecret);

            logger.LogInformation("Updating Hub with new credentials. Hub ID: {HubId}, ClientId: {ClientId}", hubId, clientId);

            try
            {
                await hubRepository.UpdateAsync(hubId, hub);
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Successfully updated Hub {HubId} with new credentials.", hubId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update Hub {HubId} with new credentials.", hubId);
                throw new InvalidOperationException($"An error occurred while updating Hub {hubId}.", ex);
            }
        }


        public async Task UpdateUserWithCredentials(ClaimsPrincipal claims, Guid clientId, string clientSecret)
        {
            logger.LogInformation("Starting UpdateUserWithCredentials for ClaimsPrincipal: {Claims}, ClientId: {ClientId}", claims.Identity.Name, clientId);

            //Validate inputs
            if (clientId == Guid.Empty || string.IsNullOrEmpty(clientSecret))
            {
                logger.LogWarning("Invalid ClientId or ClientSecret provided.");
                throw new ArgumentException("ClientId or ClientSecret cannot be empty or null.");
            }

            //Retrieve User ID from claims
            var userId = claimsRetrievalService.GetSubjectIdFromClaims(claims);
            logger.LogInformation("Retrieved User ID: {UserId} from ClaimsPrincipal.", userId);

            //Retrieve the user
            User user;
            try
            {
                user = await userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    logger.LogWarning("User with ID: {UserId} not found.", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving user with ID: {UserId}.", userId);
                throw;
            }

            //Check for existing credentials
            if (user.ClientId != null || user.ClientSecret != null)
            {
                logger.LogWarning("User {UserId} already has ClientId or ClientSecret assigned.", userId);
                throw new DuplicateClientInfoException($"ClientId or Secret already exists for user {userId}.");
            }

            //Update the user with new credentials
            user.ClientId = clientId;
            user.ClientSecret = HashSecret(clientSecret);
            logger.LogInformation("Updating User {UserId} with new credentials. ClientId: {ClientId}.", userId, clientId);

            //Save changes
            try
            {
                await userRepository.UpdateAsync(userId, user);
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Successfully updated User {UserId} with new credentials.", userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update User {UserId} with new credentials.", userId);
                throw new InvalidOperationException($"An error occurred while updating User {userId}.", ex);
            }
        }


        public async Task CheckUserInHub(ClaimsPrincipal claims, Guid hubId)
        {
            logger.LogInformation("Starting CheckUserInHub. HubId: {HubId}", hubId);

            //Retrieve User ID from claims
            var userId = claimsRetrievalService.GetSubjectIdFromClaims(claims);
            logger.LogInformation("Retrieved User ID: {UserId} from ClaimsPrincipal.", userId);

            //Retrieve Hub and User
            var hub = await hubRepository.GetByIdAsync(hubId);
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                logger.LogWarning("User with ID: {UserId} not found.", userId);
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            if (hub == null)
            {
                logger.LogWarning("Hub with ID: {HubId} not found.", hubId);
                throw new InvalidOperationException($"Hub with ID {hubId} not found.");
            }

            //Check if User is associated with the Hub
            if (!user.Hubs.Any(h => h.HubId == hub.HubId))
            {
                logger.LogWarning("User {UserId} is not associated with Hub {HubId}.", userId, hubId);
                throw new UnauthorizedAccessException($"User {userId} is not associated with the specified Hub {hubId}.");
            }

            logger.LogInformation("User {UserId} is associated with Hub {HubId}.", userId, hubId);
        }


        private async Task<SubjectDto> CreateSubject(Guid clientId, bool isHub)
        {
            logger.LogInformation("Starting CreateSubject for ClientId: {ClientId}, IsHub: {IsHub}", clientId, isHub);

            SubjectDto subject = new();

            if (isHub)
            {
                //Fetch Hub by ClientId
                var hub = await hubRepository.GetHubByClientIdAsync(clientId);
                if (hub == null)
                {
                    logger.LogWarning("Hub with ClientId: {ClientId} not found in the database.", clientId);
                    throw new KeyNotFoundException($"Hub with ClientId {clientId} not found in the database.");
                }

                logger.LogInformation("Hub found for ClientId: {ClientId}. HubId: {HubId}", clientId, hub.HubId);

                subject.Id = hub.HubId;
                subject.ClientSecret = hub.ClientSecret!;
                subject.ClientId = (Guid)hub.ClientId!;
                subject.Role = AuthRole.Hub;
            }
            else
            {
                //Fetch User by ClientId
                var user = await userRepository.GetUserByClientIdAsync(clientId);
                if (user == null)
                {
                    logger.LogWarning("User with ClientId: {ClientId} not found in the database.", clientId);
                    throw new KeyNotFoundException($"User with ClientId {clientId} not found in the database.");
                }

                logger.LogInformation("User found for ClientId: {ClientId}. UserId: {UserId}", clientId, user.Id);

                subject.Id = user.Id;
                subject.ClientSecret = user.ClientSecret!;
                subject.ClientId = (Guid)user.ClientId!;
                subject.Role = AuthRole.User;
            }

            logger.LogInformation("Successfully created SubjectDto for ClientId: {ClientId}, Role: {Role}", clientId, subject.Role);
            return subject;
        }



        public async Task<AuthResponse> AuthorizeSubject(Guid clientId, string clientSecret, bool isHub)
        {
            logger.LogInformation("Starting authorization for ClientId: {ClientId}, IsHub: {IsHub}", clientId, isHub);

            //Create the subject (user or hub)
            SubjectDto subject;
            try
            {
                subject = await CreateSubject(clientId, isHub);
                logger.LogInformation("Successfully created subject for ClientId: {ClientId}, IsHub: {IsHub}", clientId, isHub);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create subject for ClientId: {ClientId}, IsHub: {IsHub}", clientId, isHub);
                throw new InvalidOperationException($"An error occurred while creating the subject for ClientId: {clientId}", ex);
            }

            //Validate client secret
            if (!ClientSecretIsValid(clientSecret, subject.ClientSecret))
            {
                logger.LogWarning("Invalid client secret for ClientId: {ClientId}", clientId);
                throw new UnauthorizedAccessException("Client secret is invalid.");
            }
            logger.LogInformation("Client secret validated successfully for ClientId: {ClientId}", clientId);

            //Generate access token
            var expirationTime = 12 * 60;
            var accessToken = await GenerateJwtToken(subject.Id.ToString(), subject.Role, expirationTime);
            var expiration = DateTime.UtcNow.AddMinutes(expirationTime);

            logger.LogInformation("Authorization successful for ClientId: {ClientId}. Token expires at: {Expiration}", clientId, expiration);

            return new AuthResponse
            {
                AccessToken = accessToken,
                Expiration = expiration
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
