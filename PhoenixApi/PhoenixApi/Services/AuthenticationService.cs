using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Services
{
    public interface IAuthenticationService
    {
        string GenerateJwtToken(Guid hubId);
    }
    public class AuthenticationService(IConfiguration config, ILogger<AuthenticationService> logger) : IAuthenticationService
    {
        public string GenerateJwtToken(Guid hubId)
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
                Expires = DateTime.UtcNow.AddMinutes(30), // Token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
