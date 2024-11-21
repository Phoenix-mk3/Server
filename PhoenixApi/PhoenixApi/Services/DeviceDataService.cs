using Microsoft.Extensions.Logging;
using PhoenixApi.Models;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.DtoIn;
using PhoenixApi.Models.Lookups;
using PhoenixApi.Models.Responses;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using System.Globalization;
using System.Security.Claims;

namespace PhoenixApi.Services
{
    public interface IDeviceDataService
    {
        Task AddNewData(IEnumerable<DeviceDataDto> deviceDataDto, string categoryIn);
        Task<IEnumerable<DeviceDataResponse>> GetAllDeviceDataFromDevice(Guid deviceId);
        Task<DeviceDataResponse> GetDeviceDataFromId(int deviceDataId);
    }
    public class DeviceDataService(ILogger<DeviceDataService> logger, IDeviceDataRepository deviceDataRepository, IUnitOfWork unitOfWork, IDeviceService deviceService, ILookupService<UnitLookup> unitLookupService, ILookupService<DataTypeLookup> typeLookupService, ILookupService<DataCategoryLookup> categoryLookupService) : IDeviceDataService
    {
        public async Task AddNewData(IEnumerable<DeviceDataDto> deviceDataDto, string categoryIn)
        {
            if (deviceDataDto == null || !deviceDataDto.Any())
            {
                throw new ArgumentNullException(nameof(deviceDataDto), "Device data DTO collection is null or empty.");
            }

            foreach (var deviceDDto in deviceDataDto)
            {
                try
                {
                    var devices = await deviceService.GetHubDevices(deviceDDto.HubId);
                    var device = devices.FirstOrDefault(d => d.DeviceId == deviceDDto.DeviceId) ?? throw new ArgumentException($"Couldn't find device {deviceDDto.DeviceId} associated with hub {deviceDDto.HubId}");

                    var unit = await unitLookupService.FindAsync(deviceDDto.Unit) ?? throw new ArgumentException($"Couldn't find Unit {deviceDDto.Unit}", nameof(deviceDDto.Unit));
                    var type = await typeLookupService.FindAsync(deviceDDto.DataType) ?? throw new ArgumentException($"Couldn't find Data Type {deviceDDto.DataType}", nameof(deviceDDto.DataType));
                    var category = await categoryLookupService.FindAsync(categoryIn) ?? throw new ArgumentException($"Couldn't find category {categoryIn}", nameof(categoryIn));

                    DeviceData data = new()
                    {
                        Device = device,
                        Value = deviceDDto.Value,
                        Unit = unit,
                        CreatedAt = deviceDDto.CreatedAt,
                        Type = type,
                        Category = category
                    };

                    await deviceDataRepository.AddAsync(data);
                }
                catch (ArgumentException argEx)
                {
                    // Log argument-related exceptions for debugging
                    logger.LogWarning(argEx, "Validation error while processing device data: {DeviceDataDto}", deviceDDto);
                    throw;
                }
                catch (Exception ex)
                {
                    // Log unexpected errors
                    logger.LogError(ex, "An error occurred while processing device data: {DeviceDataDto}", deviceDDto);
                    throw new InvalidOperationException("An error occurred while adding new data to the database.", ex);
                }
            }
            try
            {
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while saving changes to the database.");
                throw new InvalidOperationException("Failed to save changes to the database.", ex);
            }
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
