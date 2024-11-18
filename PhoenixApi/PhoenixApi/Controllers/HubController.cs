using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Models.Security;
using PhoenixApi.Services;
using System.Security.Claims;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HubController(IHubService hubService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = nameof(AuthRole.Hub))]
        public async Task<IActionResult> GetSelf()
        {
            var hub = await hubService.GetSingleHubAsync(User);
            return Ok(hub);
        }

        [HttpGet("all")]
        [Authorize(Roles = nameof(AuthRole.Admin))]
        public async Task<IActionResult> GetAll()
        {
            var hubs = await hubService.GetAllHubsAsync();
            return Ok(hubs);
        }

        [HttpPost("create")]
        [Authorize(Roles = nameof(AuthRole.Admin))]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            await hubService.CreateHubAsync(name);
            return Ok();
        }

        [HttpPut("update-name")]
        [Authorize(Roles = $"{nameof(AuthRole.Hub)}, {nameof(AuthRole.User)}")]
        public async Task<IActionResult> UpdateName([FromBody] string name)
        {
            await hubService.UpdateHubName(name, User);
            return Ok();
        }

        [HttpDelete("factory-reset")]
        [Authorize(Roles = $"{nameof(AuthRole.Hub)}")]
        public async Task<IActionResult> FactoryReset()
        {
            await hubService.FactoryResetAsync(User);
            return Ok();
        }

    }
}
