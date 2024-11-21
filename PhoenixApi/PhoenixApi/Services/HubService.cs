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
        Task FactoryResetAsync(ClaimsPrincipal claims);
        Task<IEnumerable<Hub>> GetAllHubsAsync();
        Task<Hub> GetSingleHubAsync(ClaimsPrincipal claims);
        Task<Hub> GetSingleHubAsync(Guid hubId);
        Task UpdateHubName(string name, ClaimsPrincipal claims);
    }
    public class HubService(IHubRepository hubRepository, IUnitOfWork unitOfWork, IClaimsRetrievalService claimsRetrievalService) : IHubService
    {
        public async Task<IEnumerable<Hub>> GetAllHubsAsync()
        {
            IEnumerable<Hub> hubs = await hubRepository.GetAllAsync();
            return hubs;
        }

        public async Task<Hub> GetSingleHubAsync(ClaimsPrincipal claims)
        {
            var hubId = claimsRetrievalService.GetSubjectIdFromClaims(claims);
            Hub hub = await hubRepository.GetByIdAsync(hubId) ?? throw new ArgumentNullException(nameof(hubId), "Hub not found!");
            return hub;
        }
        public async Task<Hub> GetSingleHubAsync(Guid hubId)
        {
            Hub hub = await hubRepository.GetByIdAsync(hubId) ?? throw new ArgumentNullException(nameof(hubId), "Hub not found!");
            return hub;
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
            Guid hubId = claimsRetrievalService.GetSubjectIdFromClaims(claims);

            await hubRepository.UpdateNameAsync(hubId, name);
            await unitOfWork.SaveChangesAsync();
        }
        public async Task FactoryResetAsync(ClaimsPrincipal claims)
        {
            Guid hubId = claimsRetrievalService.GetSubjectIdFromClaims(claims);
            var hub = await hubRepository.GetByIdAsync(hubId) ?? throw new ArgumentNullException("hub", "Hub not found");

            await hubRepository.DeleteAsync(hubId);

            Hub newHub = new()
            {
                HubId = hubId,
                CreatedAt = hub.CreatedAt
            };

            await hubRepository.AddAsync(newHub);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
