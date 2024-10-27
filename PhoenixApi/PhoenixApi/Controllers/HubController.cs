﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoenixApi.Services;
using System.Security.Claims;

namespace PhoenixApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HubController(IHubService hubService) : ControllerBase
    {
        [HttpGet("all")]
        [Authorize(Policy = "HubOnly")]
        public async Task<IActionResult> GetAll()
        {
            var hubs = await hubService.GetAllHubsAsync();
            return Ok(hubs);
        }

        [HttpPost("create")]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            await hubService.CreateHubAsync(name);
            return Ok();
        }

        [HttpPut("update-name")]
        [Authorize(Roles = "Hub, User")]
        public async Task<IActionResult> UpdateName([FromBody] string name)
        {
            await hubService.UpdateHubName(name, User);
            return Ok();
        }
    }
}