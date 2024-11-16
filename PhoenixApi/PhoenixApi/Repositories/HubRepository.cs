using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhoenixApi.Controllers;
using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Models.Lookups;
using PhoenixApi.Repositories.Base;
using PhoenixApi.UnitofWork;

namespace PhoenixApi.Repositories
{
    public interface IHubRepository: IRepository<Hub, Guid>
    {
        Task<Hub> GetHubByClientIdAsync(Guid clientId);
        Task UpdateNameAsync(Guid hubId, string name);
    }
    public class HubRepository(ApiDbContext context) : RepositoryBase<Hub, Guid>(context), IHubRepository
    {
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
        public override async Task<Hub?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(h => h.Devices)
                .ThenInclude(d => d.Type)
                .Include(h => h.Devices)
                .ThenInclude(d => d.Data)
                .Include(h => h.Users)
                .FirstOrDefaultAsync(h => h.HubId == id);

        }
    }
}
