using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PhoenixApi.Configuration
{
    /// <summary>
    /// To add Swagger desc for required roles!
    /// </summary>
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
