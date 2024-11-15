using PhoenixApi.Services;
using PhoenixApi.Data;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using PhoenixApi.Extensions;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using PhoenixApi.Configuration;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;
using Microsoft.EntityFrameworkCore;

Thread.Sleep(5000);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationHandler, AdminBypassHandler>();


builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHubRepository, HubRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceDataRepository, DeviceDataRepository>();

builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IHubService, HubService>();
builder.Services.AddTransient<IClaimsRetrievalService, ClaimsRetrievalService>();
builder.Services.AddTransient<IDeviceService, DeviceService>(); 
builder.Services.AddTransient<IDeviceDataService, DeviceDataService>();
builder.Services.AddScoped(typeof(ILookupService<>), typeof(LookupService<>));
//builder.Services.AddTransient<IEnumLookupService<DeviceTypeEnum>, EnumLookupService<DeviceTypeEnum>>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "jwtToken_Auth_API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token with bearer fromat like bearer[space] token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { 
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
        new string[]{ } 
        }
    });
    c.OperationFilter<RolesOperationFilter>();
});

builder.AddNpgsqlDbContext<ApiDbContext>("ApiDb");



var app = builder.Build();


app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.CreateDbIfNotExists();

app.Run();
