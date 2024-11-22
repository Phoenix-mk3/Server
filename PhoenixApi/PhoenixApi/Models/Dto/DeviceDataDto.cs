namespace PhoenixApi.Models.DtoIn
{
    public class DeviceDataDto
    {
        public Guid DeviceId { get; set; }
        public Guid HubId { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
