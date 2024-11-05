namespace PhoenixApi.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public ICollection<Hub> Hubs { get; } = [];
    }
}
