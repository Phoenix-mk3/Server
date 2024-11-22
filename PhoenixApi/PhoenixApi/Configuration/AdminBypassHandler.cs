using Microsoft.AspNetCore.Authorization;
using PhoenixApi.Models.Security;

namespace PhoenixApi.Configuration
{
    /// <summary>
    /// Authorization handler to bypass role-based or claim-based requirements for admin users.
    /// </summary>
    /// <remarks>
    /// This bypass ensures that users with the "Admin" role or an "IsAdmin" claim set to "true" 
    /// are granted access to all controller endpoints. This is because an admin has universal 
    /// access rights across the application. Even if a token lacks specific hub or user roles, 
    /// the presence of admin privileges is sufficient to authorize access to any resource.
    /// </remarks>
    public class AdminBypassHandler : AuthorizationHandler<IAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IAuthorizationRequirement requirement)
        {
            if (context.User.IsInRole(nameof(AuthRole.Admin)) ||
                context.User.HasClaim(c => c.Type == nameof(AuthPerms.IsAdmin) && c.Value == "true"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
