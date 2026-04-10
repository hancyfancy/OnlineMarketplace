using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries;

public class IssueTokenQuery : IQuery<Result<LoginResultDto>>
{
    public IssueTokenQuery(string email, string apiKey)
    {
        Email = email;
        ApiKey = apiKey;
    }

    public string Email { get; }
    public string ApiKey { get; }
}
