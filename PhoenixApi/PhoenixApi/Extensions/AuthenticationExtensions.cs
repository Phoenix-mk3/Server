using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PhoenixApi.Models.Security;
using System.Text;

namespace PhoenixApi.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]))

            };
            
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        error = "Unauthorized",
                        message = "Authentication is required to access this resource"
                    };
                    return context.Response.WriteAsJsonAsync(result);
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        error = "Authentication Failed",
                        message = context.Exception?.Message ?? "An error occured during authentication."
                    };
                    return context.Response.WriteAsJsonAsync(result);
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        error = "Forbidden",
                        message = "You do not have permission to access this resource"
                    };
                    return context.Response.WriteAsJsonAsync(result);
                }
            };
        });
            services.AddAuthorizationBuilder()
                .AddPolicy(nameof(AuthPerms.IsAdmin), policy => policy.RequireClaim("Permission", nameof(AuthPerms.IsAdmin)));

            return services;
        }
    }
}
