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
    public class HubService(IHubRepository hubRepo, IUnitOfWork unitOfWork) : IHubService
    {
        public async Task<IEnumerable<Hub>> GetAllHubsAsync()
        {

            IEnumerable<Hub> hubs = await hubRepo.GetAllAsync();
            return hubs;
        }

        public async Task CreateHubAsync(string name = null)
        {
            Hub hub = new()
            {
                HubId = Guid.NewGuid(),
                Name = name,
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = HashSecret(Guid.NewGuid().ToString())
            };

            await hubRepo.AddAsync(hub);
            await unitOfWork.SaveChangesAsync();
        }
        private static string HashSecret(string secret)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            var hashedSecret = Convert.ToBase64String(hashedBytes);
            return hashedSecret;
        }
    }
}
