using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries;

public class IssueTokenQueryHandler : IQueryHandler<IssueTokenQuery, Result<LoginResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public IssueTokenQueryHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResultDto>> Handle(IssueTokenQuery query, CancellationToken cancellationToken)
    {
        var result = await _userRepository.FindByApiKeyAsync(query.Email, query.ApiKey);

        if (result.IsFailure)
            return Result.Failure<LoginResultDto>(result.Error);

        var user = result.Value;

        if (!user.IsActive)
            return Result.Failure<LoginResultDto>("Forbidden", "Account is not active.");

        var (accessToken, expiresAt) = _jwtTokenService.CreateToken(new TokenDto
        {
            UserId = user.Id.ToString(),
            UserGuid = user.UserGuid,
            Email = user.Email,
            Roles = [user.Role]
        });

        return Result.Success(new LoginResultDto
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        });
    }
}
