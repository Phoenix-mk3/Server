using PhoenixApi.Models.Security;

namespace PhoenixApi.Models.Dto
{
    public class SubjectDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string ClientSecret { get; set; }
        public AuthRole Role { get; set; }
    }
}
