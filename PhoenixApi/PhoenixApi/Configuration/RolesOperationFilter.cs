using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PhoenixApi.Configuration
{
    /// <summary>
    /// Operation filter for Swagger to append required roles to the endpoint description.
    /// </summary>
    /// <remarks>
    /// This filter checks for any roles specified in the [Authorize] attribute on API endpoints
    /// and appends a formatted list of these roles to the endpoint's description in the Swagger UI.
    /// 
    /// This ensures that developers and consumers of the API can easily see what roles are required
    /// to access each endpoint directly from the Swagger documentation.
    /// </remarks>
    public class RolesOperationFilter : IOperationFilter
    {
        
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var requiredRoles = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Roles)
                .Where(roles => !string.IsNullOrWhiteSpace(roles))
                .Distinct();

            if (requiredRoles.Any())
            {
                operation.Description += $"<br><b>Required Roles:</b> {string.Join(", ", requiredRoles)}";
            }
        }
    }
}
