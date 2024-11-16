using Microsoft.EntityFrameworkCore;
using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;

namespace PhoenixApi.Repositories
{
    public interface IDeviceDataRepository : IRepository<DeviceData, int>
    {
    }
    public class DeviceDataRepository(ApiDbContext context) : RepositoryBase<DeviceData, int>(context), IDeviceDataRepository
    {

    }
}
