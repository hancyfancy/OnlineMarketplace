using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class ProvisionExternalUserCommand : ICommand<Result<ExternalUserProvisionDto>>
    {
        public ProvisionExternalUserCommand(
            string email,
            string? givenName,
            string? familyName,
            string defaultRole,
            string? loginProvider = null,
            string? providerKey = null,
            string? accessToken = null,
            string? refreshToken = null)
        {
            Email = email;
            GivenName = givenName;
            FamilyName = familyName;
            DefaultRole = defaultRole;
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string Email { get; }
        public string? GivenName { get; }
        public string? FamilyName { get; }
        public string DefaultRole { get; }
        public string? LoginProvider { get; }
        public string? ProviderKey { get; }
        public string? AccessToken { get; }
        public string? RefreshToken { get; }
    }
}
