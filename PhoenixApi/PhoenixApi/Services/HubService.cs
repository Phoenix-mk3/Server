using PhoenixApi.Models;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Services
{
    public interface IHubService
    {
        Task CreateHubAsync(string name = null);
        Task<IEnumerable<Hub>> GetAllHubsAsync();
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
    }
}
