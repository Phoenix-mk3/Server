using Microsoft.EntityFrameworkCore;
using PhoenixApi.Controllers;
using PhoenixApi.Data;
using PhoenixApi.Models;

namespace PhoenixApi.Repositories
{
    public interface IHubRepository
    {
        Task<Hub> GetHubWithClientIdAsync(HubLoginDto loginDto);
        Task AddHubAsync(Hub hub);
        void RemoveHub(Hub hub);
    }
    public class HubRepository(ApiDbContext context, ILogger<HubRepository> logger) : IHubRepository
    {
        public async Task<Hub> GetHubWithClientIdAsync(HubLoginDto loginDto)
        {
            try
            {
                Hub hub = await context.Hubs.SingleOrDefaultAsync(h => h.ClientId == loginDto.ClientId && h.IsActive)!;
                return hub!;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Unable to find hub with client id {clientId}, error: {error}", loginDto.ClientId, ex);
                return null;
            }
        }
        public async Task AddHubAsync(Hub hub) 
        {
            try
            {
                await context.Hubs.AddAsync(hub);
            }
            catch (Exception ex) 
            {
                logger.LogError("Failed to add hub to database. Exception: {ex}", ex);
            }
        }
        public void RemoveHub(Hub hub)
        {
            try
            {
                context.Hubs.Remove(hub);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to remove hub from database. Exception: {ex}", ex);
            }
        }

    }
}
