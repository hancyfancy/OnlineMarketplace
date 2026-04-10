using Demo.Application.Security.Dtos;

namespace Demo.Application.Security.Abstractions;

public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAt) CreateToken(TokenDto user);
}
