using Microsoft.EntityFrameworkCore;
using PhoenixApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Device> Devices { get; set; }
    }
    public static class Extensions
    {
        public static void CreateDbIfNotExists(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApiDbContext>();
            try
            {
                context.Database.EnsureCreated();
                DbInit.Initialize(context);
            }
            catch (Exception ex)
            {
            }
        }
    }
    //FOR TESTING;;; REMOVE
    public static class DbInit
    {
        public static void Initialize(ApiDbContext context)
        {
            if (context.Hubs.Any())
                return;

            Hub newHub = new()
            {
                HubId = Guid.Parse("cbb69446-b121-4549-a4eb-b8d7384072c2"),
                ClientId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ClientSecret = HashSecret("testing1")
            };

            context.Add(newHub);

            context.SaveChanges();
        }

        private static string HashSecret(string secret)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            var hashedSecret = Convert.ToBase64String(hashedBytes);
            return hashedSecret;
        }
    }
}
