using Microsoft.AspNetCore.Authorization;
using PhoenixApi.Models.Security;

namespace PhoenixApi.Configuration
{
    public class AdminBypassHandler : AuthorizationHandler<IAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IAuthorizationRequirement requirement)
        {
            // Check if the user has the Admin role or an IsAdmin claim
            if (context.User.IsInRole(nameof(AuthRole.Admin)) ||
                context.User.HasClaim(c => c.Type == nameof(AuthPerms.IsAdmin) && c.Value == "true"))
            {
                // Grant access by succeeding the requirement
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
