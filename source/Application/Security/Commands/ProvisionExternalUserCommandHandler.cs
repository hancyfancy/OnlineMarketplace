using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class ProvisionExternalUserCommandHandler
        : ICommandHandler<ProvisionExternalUserCommand, Result<ExternalUserProvisionDto>>
    {
        private readonly IUserRepository _userRepository;

        public ProvisionExternalUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<ExternalUserProvisionDto>> Handle(
            ProvisionExternalUserCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _userRepository.ProvisionExternalUserAsync(
                request.Email,
                request.GivenName,
                request.FamilyName,
                request.DefaultRole,
                request.LoginProvider,
                request.ProviderKey,
                request.AccessToken,
                request.RefreshToken);

            if (!result.IsSuccess)
                return Result.Failure<ExternalUserProvisionDto>(result.Error.Code, result.Error.Description);

            return Result.Success(new ExternalUserProvisionDto
            {
                UserGuid = result.Value.UserGuid,
                Roles    = result.Value.Roles
            });
        }
    }
}
