namespace Demo.Domain.Models
{
    public class ExternalUserProvisionResult
    {
        public Guid UserGuid { get; set; }
        public IList<string> Roles { get; set; } = [];
        public string? ApiKey { get; set; }
    }
}
