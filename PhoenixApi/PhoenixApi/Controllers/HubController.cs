using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Services;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HubController(IHubService hubService) : ControllerBase
    {
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var hubs = await hubService.GetAllHubsAsync();
            return Ok(hubs);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            await hubService.CreateHubAsync(name);
            return Ok();
        }
    }
}
