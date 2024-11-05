namespace PhoenixApi.Models.DtoIn
{
    public class DeviceDataDto
    {
        public int DeviceId { get; set; }
        public required string Value { get; set; }
        public required string Unit { get; set; }
        public required string DataType { get; set; }
    }
}
