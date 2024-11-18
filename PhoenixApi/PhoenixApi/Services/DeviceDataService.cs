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
        Task AddNewData(IEnumerable<DeviceDataDto> deviceDataDto, string categoryIn, ClaimsPrincipal claims);
        Task<IEnumerable<DeviceDataResponse>> GetAllDeviceDataFromDevice(Guid deviceId);
        Task<DeviceDataResponse> GetDeviceDataFromId(int deviceDataId);
    }
    public class DeviceDataService(IDeviceDataRepository deviceDataRepository, IUnitOfWork unitOfWork, IDeviceService deviceService, ILookupService<UnitLookup> unitLookupService, ILookupService<DataTypeLookup> typeLookupService, ILookupService<DataCategoryLookup> categoryLookupService) : IDeviceDataService
    {
        public async Task AddNewData(IEnumerable<DeviceDataDto> deviceDataDto, string categoryIn, ClaimsPrincipal claims)
        {
            foreach (var devideDDto in deviceDataDto)
            {
                var devices = await deviceService.GetHubDevices(claims);
                var device = devices.FirstOrDefault(d => d.DeviceId == devideDDto.DeviceId);

                var unit = await unitLookupService.FindAsync(devideDDto.Unit);
                var type = await typeLookupService.FindAsync(devideDDto.DataType);
                var category = await categoryLookupService.FindAsync(categoryIn);


                DeviceData data = new()
                {
                    Device = device,
                    Value = devideDDto.Value,
                    Unit = unit,
                    CreatedAt = devideDDto.CreatedAt,
                    Type = type,
                    Category = category
                };

                await deviceDataRepository.AddAsync(data);
            }
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<DeviceDataResponse>> GetAllDeviceDataFromDevice(Guid deviceId)
        {
            var device = await deviceService.GetDeviceById(deviceId);

            var deviceData = await deviceDataRepository.GetAllDataByDeviceAsync(device.DeviceId);

            List<DeviceDataResponse> dataResponse = new();


            foreach(DeviceData data in deviceData)
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
