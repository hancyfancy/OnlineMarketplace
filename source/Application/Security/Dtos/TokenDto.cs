namespace Demo.Application.Security.Dtos;

public class TokenDto
{
    public string UserId { get; set; } = "";
    public Guid UserGuid { get; set; }
    public string Email { get; set; } = "";
    public IList<string> Roles { get; set; } = [];
}
