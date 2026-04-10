namespace Demo.Application.Security.Dtos
{
    public class ExternalUserProvisionDto
    {
        public Guid UserGuid { get; set; }
        public IList<string> Roles { get; set; } = [];
    }
}
