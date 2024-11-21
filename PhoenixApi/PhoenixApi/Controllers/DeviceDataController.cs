using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.DtoIn;
using PhoenixApi.Models.Security;
using PhoenixApi.Services;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceDataController(IDeviceDataService deviceDataService, ILogger<DeviceDataController> logger) : ControllerBase
    {
        [HttpPost("sensor")]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> CreateNewSensorData([FromBody] IEnumerable<DeviceDataDto> deviceDataDto)
        {
            if (deviceDataDto == null || !deviceDataDto.Any())
            {
                logger.LogWarning("Device data is null or empty: {DeviceDataDto}", deviceDataDto);
                return BadRequest("Device data cannot be null or empty.");
            }

            try
            {
                await deviceDataService.AddNewData(deviceDataDto, "sensor");
                return Created(string.Empty, null);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Operational error occurred: {Message}", ex.Message);
                return StatusCode(500, "A server error occurred while processing the request.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding new sensor data: {DeviceDataDto}", deviceDataDto);
                return StatusCode(500, "An internal server error occurred.");
            }
        }


        //Need identical because of roles
        [HttpPost("settings")]
        [Authorize(Roles = $"{nameof(AuthRole.Hub)}, {nameof(AuthRole.User)}")]
        public async Task<IActionResult> CreateNewSettings([FromBody] IEnumerable<DeviceDataDto> deviceDataDto)
        {
            if (deviceDataDto == null || !deviceDataDto.Any())
            {
                logger.LogWarning("Device data is null or empty: {DeviceDataDto}", deviceDataDto);
                return BadRequest("Device data cannot be null or empty.");
            }

            try
            {
                await deviceDataService.AddNewData(deviceDataDto, "sensor");
                return Created(string.Empty, null);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Operational error occurred: {Message}", ex.Message);
                return StatusCode(500, "A server error occurred while processing the request.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding new sensor data: {DeviceDataDto}", deviceDataDto);
                return StatusCode(500, "An internal server error occurred.");
            }
        }


        [HttpPut("sensor")]
        [Authorize(Roles = "Hub")]
        public async Task<IActionResult> UpdateSensorData([FromBody] DeviceDataDto newDeviceDataDto, int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpDelete("sensor")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteSensorData(int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }


        [HttpGet("device")]
        [Authorize(Roles = $"{nameof(AuthRole.Hub)}, {nameof(AuthRole.User)}")]
        public async Task<IActionResult> GetSensorData([FromQuery]DeviceDto device)
        {
            var deviceData = await deviceDataService.GetAllDeviceDataFromDevice(device.DeviceId);

            return Ok(deviceData);
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(AuthRole.Hub)}, {nameof(AuthRole.User)}")]
        public async Task<IActionResult> GetSensorData([FromQuery] DataIdDto dataId)
        {
            var deviceData = await deviceDataService.GetDeviceDataFromId(dataId.Id);

            return Ok(deviceData);
        }



        //-------Settings-----------

        
        [HttpPut("settings")]
        [Authorize(Roles = "Hub, User")]
        public async Task<IActionResult> UpdateSettings([FromBody] DeviceDataDto newDeviceDataDto, int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpDelete("settings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSettings(int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}
