using PhoenixApi.Services;
using PhoenixApi.Data;
using PhoenixApi.Repositories;
using PhoenixApi.UnitofWork;

Thread.Sleep(10000);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IHubRepository, HubRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
