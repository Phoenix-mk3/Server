using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.Security;
using PhoenixApi.Services;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController(IDeviceService deviceService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> CreateDevice([FromBody]DeviceCreationDto deviceDto)
        {
            Guid deviceId = await deviceService.CreateNewDeviceAsync(User, deviceDto.DeviceType);
            return Ok(deviceId);
        }

        [HttpDelete]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> DeleteDevice([FromBody]DeviceDto deviceDto)
        {
            await deviceService.RemoveDeviceAsync(deviceDto.DeviceId);
            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = $"{nameof(AuthRole.Hub)}, {nameof(AuthRole.User)}")]
        public async Task<IActionResult> UpdateName([FromBody]UpdateNameDto nameDto)
        {
            await deviceService.UpdateName(nameDto.Name, nameDto.DeviceId);
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> GetHubDevices()
        {
            var devices = await deviceService.GetHubDevices(User);
            return Ok(devices);
        }

        [HttpGet("all")]
        [Authorize(Roles = nameof(AuthRole.Admin))]
        public async Task<IActionResult> GetAllDevices()
        {
            var devices = await deviceService.GetAllDevices();
            return Ok(devices);
        }

    }
}
