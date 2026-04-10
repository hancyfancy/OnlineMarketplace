using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands;

public class RegenerateApiKeyCommandHandler : ICommandHandler<RegenerateApiKeyCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;

    public RegenerateApiKeyCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<string>> Handle(RegenerateApiKeyCommand request, CancellationToken cancellationToken)
    {
        return await _userRepository.RegenerateApiKeyAsync(request.Email);
    }
}
