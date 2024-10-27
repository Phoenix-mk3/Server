using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhoenixApi.Controllers;
using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;
using PhoenixApi.UnitofWork;

namespace PhoenixApi.Repositories
{
    public interface IHubRepository: IRepository<Hub, Guid>
    {
        Task<Hub> GetHubByClientIdAsync(Guid clientId);
        Task UpdateNameAsync(Guid hubId, string name);
    }
    public class HubRepository : RepositoryBase<Hub, Guid>, IHubRepository
    {
        private readonly ILogger _logger;
        public HubRepository(ApiDbContext context, ILogger<HubRepository> logger) : base(context) 
        {
            _logger = logger;
        }

        public async Task<Hub> GetHubByClientIdAsync(Guid clientId)
        {
            Hub? hub = await _dbSet.FirstOrDefaultAsync(h => h.ClientId == clientId && h.IsActive);
            return hub;
        }

        public async Task UpdateNameAsync(Guid hubId, string name)
        {
            Hub? hub = await _dbSet.FirstOrDefaultAsync(h => h.HubId == hubId && h.IsActive);
            hub.Name = name;
            _dbSet.Entry(hub).CurrentValues.SetValues(name);
        }
    }
}
