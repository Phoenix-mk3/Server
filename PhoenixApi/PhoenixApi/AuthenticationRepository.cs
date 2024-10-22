using Microsoft.EntityFrameworkCore;
using PhoenixApi.Controllers;
using PhoenixApi.Data;
using PhoenixApi.Models;

namespace PhoenixApi
{
    public interface IAuthenticationRepository
    {
        Hub GetHub(HubLoginDto loginDto);
    }
    public class AuthenticationRepository(ApiDbContext context, ILogger<AuthenticationRepository> logger): IAuthenticationRepository
    {
        public Hub GetHub(HubLoginDto loginDto)
        {
            try
            {
                Hub hub = context.Hubs.SingleOrDefault(h => h.ClientId == loginDto.ClientId && h.IsActive);
                return hub;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Unable to find hub with client id {clientId}, error: {error}", loginDto.ClientId, ex);
                throw ex;
            }
        }
    }
}
