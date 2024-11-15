using PhoenixApi.Models;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.DtoIn;
using PhoenixApi.Models.Lookups;
using PhoenixApi.Models.Responses;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.Security.Claims;

namespace PhoenixApi.Services
{
    public interface IDeviceDataService
    {
        Task AddNewData(DeviceDataDto deviceDataDto, string categoryIn, ClaimsPrincipal claims);
        Task<IEnumerable<DeviceDataResponse>> GetAllDeviceDataFromDevice(Guid deviceId);
        Task<DeviceDataResponse> GetDeviceDataFromId(int deviceDataId);
    }
    public class DeviceDataService(IDeviceDataRepository deviceDataRepository, IUnitOfWork unitOfWork, IDeviceService deviceService, ILookupService<UnitLookup> unitLookupService, ILookupService<DataTypeLookup> typeLookupService, ILookupService<DataCategoryLookup> categoryLookupService) : IDeviceDataService
    {
        public async Task AddNewData(DeviceDataDto deviceDataDto, string categoryIn, ClaimsPrincipal claims)
        {
            var devices = await deviceService.GetHubDevices(claims);
            var device = devices.FirstOrDefault(d => d.DeviceId == deviceDataDto.DeviceId);

            var unit = await unitLookupService.FindAsync(deviceDataDto.Unit);
            var type = await typeLookupService.FindAsync(deviceDataDto.DataType);
            var category = await categoryLookupService.FindAsync(categoryIn);


            DeviceData data = new()
            {
                Device = device,
                Value = deviceDataDto.Value,
                Unit = unit,
                CreatedAt = deviceDataDto.CreatedAt,
                Type = type,
                Category = category
            };

            await deviceDataRepository.AddAsync(data);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<DeviceDataResponse>> GetAllDeviceDataFromDevice(Guid deviceId)
        {
            var device = await deviceService.GetDeviceById(deviceId);
            List<DeviceDataResponse> dataResponse = new();


            foreach(DeviceData data in device.Data)
            {
                dataResponse.Add(new DeviceDataResponse
                {
                    Id = data.Id,
                    DeviceId = data.DeviceId,
                    Category = data.Category,
                    CreatedAt = data.CreatedAt,
                    Type = data.Type,
                    Unit = data.Unit,
                    Value = data.Value
                });
            }

            return dataResponse;
        }
        public async Task<DeviceDataResponse> GetDeviceDataFromId(int id)
        {
            DeviceData data = await deviceDataRepository.GetByIdAsync(id);

            DeviceDataResponse dataResponse = new()
            {
                Id = data.Id,
                DeviceId = data.DeviceId,
                Category = data.Category,
                CreatedAt = data.CreatedAt,
                Type = data.Type,
                Unit = data.Unit,
                Value = data.Value

            };

            return dataResponse;
        }
    }
}
