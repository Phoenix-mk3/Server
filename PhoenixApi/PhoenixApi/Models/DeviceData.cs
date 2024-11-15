using PhoenixApi.Models.Lookups;

namespace PhoenixApi.Models
{
    public class DeviceData
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; }
        public required Device Device { get; set; }
        public required string Value { get; set; }
        public int UnitId { get; set; }
        public required UnitLookup Unit { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TypeId { get; set; }
        public required DataTypeLookup Type { get; set; }
        public int CategoryId { get; set; }
        public required DataCategoryLookup Category { get; set; }

    }
}
