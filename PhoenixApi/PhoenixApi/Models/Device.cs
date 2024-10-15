namespace PhoenixApi.Models
{
    public class Device
    {
        public Guid DeviceId { get; set; }
        public string? Name { get; set; }
        //    public DeviceType DeviceType { get; set; } //Maybe Class
        //}

        //public enum DeviceType
        //{
        //    Hub,
        //    Alarm
        //}
    }
}