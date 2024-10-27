namespace PhoenixApi.Models
{
    public class Device
    {
        public Guid DeviceId { get; set; }
        public string? Name { get; set; }
        public Guid HubId { get; set; }
        public Hub Hub { get; set; } = null!;
        public Type Type { get; set; }

    }
    public enum Type
    {
        Unknown = 0
    }
}