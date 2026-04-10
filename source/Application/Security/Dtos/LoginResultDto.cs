namespace Demo.Application.Security.Dtos;

public class LoginResultDto
{
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}
