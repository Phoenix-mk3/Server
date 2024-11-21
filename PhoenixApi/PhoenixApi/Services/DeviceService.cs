using PhoenixApi.Models;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.Lookups;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.Security.Claims;

namespace PhoenixApi.Services
{
    public interface IDeviceService
    {
        Task<Guid> CreateNewDeviceAsync(ClaimsPrincipal claims, string deviceType);
        Task<IEnumerable<Device>> GetAllDevices();
        Task<Device> GetDeviceById(Guid deviceId);
        Task<IEnumerable<Device>> GetHubDevices(ClaimsPrincipal claims);
        Task<IEnumerable<Device>> GetHubDevices(Guid hubId);
        Task RemoveDeviceAsync(Guid deviceId);
        Task UpdateName(string name, Guid deviceId);
    }
    public class DeviceService(IDeviceRepository deviceRepository, ILookupService<DeviceTypeLookup> deviceLookupService, IHubService hubService, IUnitOfWork unitOfWork) : IDeviceService
    {
        public async Task<Guid> CreateNewDeviceAsync(ClaimsPrincipal claims, string deviceTypeInput)
        {
            var deviceType = await deviceLookupService.FindAsync(deviceTypeInput);
            Hub hub = await hubService.GetSingleHubAsync(claims);


            Device device = new()
            {
                DeviceId = Guid.NewGuid(),
                Hub = hub,
                Type = deviceType
            };

            await deviceRepository.AddAsync(device);
            await unitOfWork.SaveChangesAsync();

            return device.DeviceId;
        }

        public async Task RemoveDeviceAsync(Guid deviceId)
        {
            await deviceRepository.DeleteAsync(deviceId);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateName(string name, Guid deviceId)
        {
            await deviceRepository.UpdateNameAsync(deviceId, name);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Device>> GetAllDevices()
        {
            var devices = await deviceRepository.GetAllAsync();
            return devices;
        }

        public async Task<IEnumerable<Device>> GetHubDevices(ClaimsPrincipal claims)
        {
            var hub = await hubService.GetSingleHubAsync(claims);
            var devices = hub.Devices;
            return devices;
        }
        public async Task<IEnumerable<Device>> GetHubDevices(Guid hubId)
        {
            var hub = await hubService.GetSingleHubAsync(hubId);
            var devices = hub.Devices;
            return devices;
        }

        public async Task<Device> GetDeviceById(Guid deviceId)
        {
            var device = await deviceRepository.GetByIdAsync(deviceId);
            return device;
        }
    }
}
