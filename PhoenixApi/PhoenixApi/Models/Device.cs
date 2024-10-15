namespace PhoenixApi.Models
{
    public abstract class Device
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public DeviceType DeviceType { get; set; } //Maybe Class
    }

    public enum DeviceType
    {
        Hub,
        Alarm
    }
}
