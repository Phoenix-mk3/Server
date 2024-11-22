using PhoenixApi.Models.Lookups;

namespace PhoenixApi.Models.Responses
{
    //This approach would have been used to prevent circular references.
    //Not implemented due to time constraints.
    //The current implementation ensures no circular references exist.

    public class HubResponse
    {
        public Guid HubId { get; set; }
        public string? Name { get; set; }
        public Guid? ClientId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public ICollection<DeviceHubDto> DevicesDto { get; set; } = [];
        public ICollection<UserHubDto> UsersDto { get; } = [];
    }

    public class DeviceHubDto
    {

        public Guid DeviceId { get; set; }
        public string? Name { get; set; }
        public Guid HubId { get; set; }
        public int TypeId { get; set; }
        public required DeviceTypeLookup Type { get; set; }
        public ICollection<DeviceDataDeviceDto> Data { get; set; } = [];
    }

    public class UserHubDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? ClientId { get; set; }
        public bool IsActive { get; set; } = true;
        public required ICollection<int> HubIds { get; set; }
    }

    public class DeviceDataDeviceDto
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; }
        public required string Value { get; set; }
        public required UnitLookup Unit { get; set; }
        public DateTime CreatedAt { get; set; }
        public required DataTypeLookup Type { get; set; }
        public required DataCategoryLookup Category { get; set; }
    }
}
