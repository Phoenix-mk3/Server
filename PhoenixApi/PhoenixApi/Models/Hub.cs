namespace PhoenixApi.Models
{
    public class Hub
    {
        public Guid HubId { get; set; }
        public string? Name { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public ICollection<Device>? Devices { get; set; }

    }
}
