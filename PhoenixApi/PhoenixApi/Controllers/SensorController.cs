using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Models.DtoIn;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Hub")]
        public async Task<IActionResult> CreateNewSensorData([FromBody]DeviceDataDto deviceDataDto)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpPut]
        [Authorize(Roles = "Hub")]
        public async Task<IActionResult> UpdateSensorData([FromBody]DeviceDataDto newDeviceDataDto, int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpDelete]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteSensorData(int deviceDataId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetSensorData()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

    }
}
