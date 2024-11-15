using Microsoft.EntityFrameworkCore;
using PhoenixApi.Models;
using PhoenixApi.Models.Lookups;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixApi.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceTypeLookup> DeviceTypes { get; set; }
        public DbSet<DeviceData> DeviceDatas {  get; set; }
        public DbSet<DataTypeLookup> DataTypes { get; set; }
        public DbSet<UnitLookup> Units { get; set; }
        public DbSet<DataCategoryLookup> DataCategories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UnitLookup>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<DataTypeLookup>()
                .Property(d => d.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<DataCategoryLookup>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();


            modelBuilder.Entity<UnitLookup>().HasData(
                new UnitLookup { Id = 1, Name = "kilogram", ShortName = "kg" },
                new UnitLookup { Id = 2, Name = "meter", ShortName = "m" },
                new UnitLookup { Id = 3, Name = "hectopascal", ShortName = "hpa"}
            );

            modelBuilder.Entity<DataTypeLookup>().HasData(
                new DataTypeLookup { Id = 1, Name = "int" },
                new DataTypeLookup { Id = 2, Name = "string" },
                new DataTypeLookup { Id = 3, Name = "long" }
            );

            modelBuilder.Entity<DataCategoryLookup>().HasData(
                new DataCategoryLookup { Id = 1, Name = "sensor" },
                new DataCategoryLookup { Id = 2, Name = "setting" }
            );

        }
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

        public static void AddDeviceDataEnums(this IHost host)
        {
            
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

            DeviceTypeLookup dtEnum = new()
            {
                Id = 0,
                Name = "Alarm"
            };
            context.Add(dtEnum);


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
