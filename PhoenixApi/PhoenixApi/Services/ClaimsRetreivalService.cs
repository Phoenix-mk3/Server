using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PhoenixApi.Services
{
    public interface ICliamsRetrievalService
    {
        Guid GetSubjectFromClaims(ClaimsPrincipal claimsPrincipal);
    }
    public class ClaimsRetrievalService(ILogger<ClaimsRetrievalService> logger) : ICliamsRetrievalService
    {
        public Guid GetSubjectFromClaims(ClaimsPrincipal claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var subject = claims.FindFirst(ClaimTypes.NameIdentifier) ?? claims.FindFirst(JwtRegisteredClaimNames.Sub);

            if (subject != null && Guid.TryParse(subject.Value, out Guid subjectId))
            {
                return subjectId;
            }
            throw new ArgumentNullException(nameof(subjectId));

        }
    }
}
