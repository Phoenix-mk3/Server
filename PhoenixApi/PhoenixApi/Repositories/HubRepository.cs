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
    }
}
