using Microsoft.EntityFrameworkCore;
using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;

namespace PhoenixApi.Repositories
{
    public interface IDeviceDataRepository : IRepository<DeviceData, int>
    {
        Task<IEnumerable<DeviceData>> GetAllDataByDeviceAsync(Guid deviceId);
    }
    public class DeviceDataRepository(ApiDbContext context) : RepositoryBase<DeviceData, int>(context), IDeviceDataRepository
    {
        public async Task<IEnumerable<DeviceData>> GetAllDataByDeviceAsync(Guid deviceId)
        {
            var allData = await GetAllAsync();
            var deviceData = new List<DeviceData>();

            foreach (var data in allData)
            {
                if(data.DeviceId == deviceId)
                {
                    deviceData.Add(data);
                }
            }
            return deviceData;

        }
    }
}
