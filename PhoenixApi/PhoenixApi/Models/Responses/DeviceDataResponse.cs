using PhoenixApi.Models.Lookups;

namespace PhoenixApi.Models.Responses
{
    public class DeviceDataResponse
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; }
        public string Value { get; set; }
        public UnitLookup Unit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DataTypeLookup Type { get; set; }
        public DataCategoryLookup Category { get; set; }
    }
}
