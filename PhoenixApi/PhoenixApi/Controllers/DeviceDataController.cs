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
    public class DeviceDataController(IDeviceDataService deviceDataService) : ControllerBase
    {
        [HttpPost("sensor")]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> CreateNewSensorData([FromBody]DeviceDataDto deviceDataDto)
        {
            await deviceDataService.AddNewData(deviceDataDto, "sensor", User);
            return Ok();
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

        [HttpPost("settings")]
        [Authorize(Roles = "Hub, User")]
        public async Task<IActionResult> CreateNewSettings([FromBody] DeviceDataDto deviceDataDto)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
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
