namespace PhoenixApi.Models
{
    public class Device
    {
        public Guid DeviceId { get; set; }
        public string? Name { get; set; }
        public Guid HubId { get; set; }
        public required Hub Hub { get; set; }
        public int TypeId { get; set; }
        public required DeviceTypeEnum Type { get; set; }
        public ICollection<DeviceData> Data { get; set; } = [];

    }
    public class DeviceTypeEnum
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}