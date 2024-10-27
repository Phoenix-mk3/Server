using PhoenixApi.Models;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Services
{
    public interface IHubService
    {
        Task CreateHubAsync(string name = null);
        Task<IEnumerable<Hub>> GetAllHubsAsync();
        Task UpdateHubName(string name, ClaimsPrincipal claims);
    }
    public class HubService(IHubRepository hubRepository, IUnitOfWork unitOfWork) : IHubService
    {
        public async Task<IEnumerable<Hub>> GetAllHubsAsync()
        {

            IEnumerable<Hub> hubs = await hubRepository.GetAllAsync();
            return hubs;
        }

        public async Task CreateHubAsync(string name = null)
        {
            Hub hub = new()
            {
                HubId = Guid.NewGuid(),
                Name = name
            };

            await hubRepository.AddAsync(hub);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateHub(Guid hubId, Hub hub)
        {
            await hubRepository.UpdateAsync(hubId, hub);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateHubName(string name, ClaimsPrincipal claims)
        {
            Guid hubId = GetHubIdFromClaims(claims);

            await hubRepository.UpdateNameAsync(hubId, name);
            await unitOfWork.SaveChangesAsync();
        }

        private Guid GetHubIdFromClaims(ClaimsPrincipal claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var hubIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier) ?? claims.FindFirst(JwtRegisteredClaimNames.Sub);

            if (hubIdClaim != null && Guid.TryParse(hubIdClaim.Value, out Guid hubId))
            {
                return hubId;
            }
            throw new ArgumentNullException(nameof(hubId));

        }
    }
}
