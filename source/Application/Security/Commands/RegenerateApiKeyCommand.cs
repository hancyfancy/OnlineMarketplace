using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands;

public class RegenerateApiKeyCommand : ICommand<Result<string>>
{
    public RegenerateApiKeyCommand(string email)
    {
        Email = email;
    }

    public string Email { get; }
}
