using Microsoft.EntityFrameworkCore;
using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;

namespace PhoenixApi.Repositories
{
    public interface IDeviceRepository : IRepository<Device, Guid>
    {
        Task UpdateNameAsync(Guid deviceId, string name);
    }
    public class DeviceRepository(ApiDbContext context) : RepositoryBase<Device, Guid>(context), IDeviceRepository
    {
        public async Task UpdateNameAsync(Guid deviceId, string name)
        {
            Device? device = await _dbSet.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            device.Name = name;
            _dbSet.Entry(device).CurrentValues.SetValues(name);
        }
    }
}
