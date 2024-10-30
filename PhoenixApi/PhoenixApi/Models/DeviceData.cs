namespace PhoenixApi.Models
{
    public class DeviceData
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; }
        public required Device Device { get; set; }
        public required string Value { get; set; }
        public int UnitId { get; set; }
        public required UnitEnum Unit { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TypeId { get; set; }
        public required DataTypeEnum Type { get; set; }
        public int CategoryId { get; set; }
        public required DataCategoryEnum Category { get; set; }

    }

    public class UnitEnum
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ShortName { get; set; }
    }
    public class DataTypeEnum
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
    public class DataCategoryEnum
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
