using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Domain.Abstractions
{
    public interface IUserRepository
    {
        Task<Result<UserProfile>> ReadAsync(string username);

        Task<Result<UserProfile>> FindByApiKeyAsync(string email, string apiKey);

        Task<Result<string>> RegenerateApiKeyAsync(string email);

        Task<Result> UpdateAsync(string username, UserProfileUpdate userProfileUpdate);

        Task<Result<ExternalUserProvisionResult>> ProvisionExternalUserAsync(string email, string? givenName, string? familyName, string defaultRole, string? loginProvider = null, string? providerKey = null, string? accessToken = null, string? refreshToken = null);
    }
}
