using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Models.DtoIn;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Hub, User")]
        public async Task<IActionResult> CreateNewSettings([FromBody] DeviceDataDto deviceDataDto)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpPut]
        [Authorize(Roles = "Hub, User")]
        public async Task<IActionResult> UpdateSettings([FromBody] DeviceDataDto newDeviceDataDto, int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSettings(int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpGet]
        [Authorize(Roles = "Hub, User")]
        public async Task<IActionResult> GetSettings()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}
