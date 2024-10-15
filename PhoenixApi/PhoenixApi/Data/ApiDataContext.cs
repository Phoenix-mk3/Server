using Microsoft.EntityFrameworkCore;
using PhoenixApi.Models;
using System.Data.Common;

namespace PhoenixApi.Data
{
    public class ApiDataContext : DbContext
    {
        public ApiDataContext(DbContextOptions<ApiDataContext> options) : base(options) { }

        public DbSet<WeatherForecast> WeatherForecast { get; set; } = default!;

    }

        public static class DbInitializer
        {
            public static void Initialize(ApiDataContext context)
            {
                if (context.WeatherForecast.Any())
                    return;

                var forecasts = new List<WeatherForecast>
        {
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 20, Summary = "Sunny" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)), TemperatureC = 18, Summary = "Partly Cloudy" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)), TemperatureC = 15, Summary = "Rainy" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(4)), TemperatureC = 22, Summary = "Hot" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(5)), TemperatureC = 17, Summary = "Windy" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(6)), TemperatureC = 19, Summary = "Cloudy" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(7)), TemperatureC = 16, Summary = "Stormy" }
        };

                context.AddRange(forecasts);

                context.SaveChanges();
            }
        }
    
}
