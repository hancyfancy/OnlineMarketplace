using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;
using Demo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Demo.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<ExternalUserProvisionResult>> ProvisionExternalUserAsync(
        string email, string? givenName, string? familyName, string defaultRole,
        string? loginProvider = null, string? providerKey = null,
        string? accessToken = null, string? refreshToken = null)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var plainKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                var newUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    FirstName = givenName,
                    LastName = familyName,
                    EmailConfirmed = true,
                    UserGuid = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,
                    ApiKey = HashApiKey(plainKey)
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    return Result.Failure<ExternalUserProvisionResult>("ProvisionFail", errors);
                }

                await _userManager.AddToRoleAsync(newUser, defaultRole);

                if (!string.IsNullOrEmpty(loginProvider) && !string.IsNullOrEmpty(providerKey))
                    await _userManager.AddLoginAsync(newUser, new UserLoginInfo(loginProvider, providerKey, loginProvider));

                var roles = await _userManager.GetRolesAsync(newUser);

                await SaveExternalTokensAsync(newUser, loginProvider, accessToken, refreshToken);

                return Result.Success(new ExternalUserProvisionResult { UserGuid = newUser.UserGuid, Roles = roles, ApiKey = plainKey });
            }
            else
            {
                existingUser.LastLoginAt = DateTime.UtcNow;
                existingUser.LastActiveAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(existingUser.NormalizedEmail))
                    existingUser.NormalizedEmail = email.ToUpperInvariant();
                if (string.IsNullOrEmpty(existingUser.NormalizedUserName))
                    existingUser.NormalizedUserName = email.ToUpperInvariant();

                await _userManager.UpdateAsync(existingUser);

                if (!string.IsNullOrEmpty(loginProvider) && !string.IsNullOrEmpty(providerKey))
                {
                    var existingLogins = await _userManager.GetLoginsAsync(existingUser);
                    var hasLogin = existingLogins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
                    if (!hasLogin)
                        await _userManager.AddLoginAsync(existingUser, new UserLoginInfo(loginProvider, providerKey, loginProvider));
                }

                var roles = await _userManager.GetRolesAsync(existingUser);

                await SaveExternalTokensAsync(existingUser, loginProvider, accessToken, refreshToken);

                return Result.Success(new ExternalUserProvisionResult { UserGuid = existingUser.UserGuid, Roles = roles });
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<ExternalUserProvisionResult>("ProvisionFail", ex.Message);
        }
    }

    public async Task<Result<UserProfile>> ReadAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower().Trim());

            if (user is null)
                return Result.Failure<UserProfile>("NotFound", "User not found");

            var roleNames = await _userManager.GetRolesAsync(user);
            var currentRoleName = roleNames.FirstOrDefault();
            var currentRole = currentRoleName is not null
                ? await _roleManager.FindByNameAsync(currentRoleName)
                : null;

            var userProfile = new UserProfile
            {
                Id = Guid.Parse(user.Id),
                UserGuid = user.UserGuid,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PasswordHash = user.PasswordHash ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = currentRoleName ?? string.Empty,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Result.Success(userProfile);
        }
        catch (Exception)
        {
            return Result.Failure<UserProfile>("NotFound", "User not found");
        }
    }

    public async Task<Result<string>> RegenerateApiKeyAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower().Trim());
            if (user is null)
                return Result.Failure<string>("NotFound", "User not found.");

            var plainKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.ApiKey = HashApiKey(plainKey);
            await _userManager.UpdateAsync(user);
            return Result.Success(plainKey);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>("UpdateFail", ex.Message);
        }
    }

    public async Task<Result<UserProfile>> FindByApiKeyAsync(string email, string apiKey)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower().Trim());
            var hashedKey = HashApiKey(apiKey);

            if (user is null || user.ApiKey != hashedKey)
                return Result.Failure<UserProfile>("Unauthorized", "Invalid email or API key.");

            var roleNames = await _userManager.GetRolesAsync(user);

            return Result.Success(new UserProfile
            {
                Id = Guid.Parse(user.Id),
                UserGuid = user.UserGuid,
                Email = user.Email ?? string.Empty,
                Role = roleNames.FirstOrDefault() ?? string.Empty,
                IsActive = user.AccountStatus == "active",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }
        catch (Exception)
        {
            return Result.Failure<UserProfile>("Unauthorized", "Invalid email or API key.");
        }
    }

    private async Task SaveExternalTokensAsync(
        ApplicationUser user, string? loginProvider, string? accessToken, string? refreshToken)
    {
        if (string.IsNullOrEmpty(loginProvider))
            return;

        if (!string.IsNullOrEmpty(accessToken))
            await _userManager.SetAuthenticationTokenAsync(user, loginProvider, "access_token", accessToken);

        if (!string.IsNullOrEmpty(refreshToken))
            await _userManager.SetAuthenticationTokenAsync(user, loginProvider, "refresh_token", refreshToken);
    }

    private static string HashApiKey(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();

    public async Task<Result> UpdateAsync(string username, UserProfileUpdate userProfileUpdate)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(username.ToLower().Trim());

            if (user is null)
                return Result.Failure("NotFound", "User not found");

            bool hasChanges = user.FirstName != userProfileUpdate.FirstName ||
                              user.LastName != userProfileUpdate.LastName ||
                              user.PhoneNumber != userProfileUpdate.PhoneNumber;

            var currentRoleNames = await _userManager.GetRolesAsync(user);
            ApplicationRole? newRole = null;
            if (!string.IsNullOrEmpty(userProfileUpdate.Role))
                newRole = await _roleManager.FindByNameAsync(userProfileUpdate.Role);

            bool roleChanged = newRole is not null &&
                               newRole.Name is not null &&
                               !currentRoleNames.Contains(newRole.Name);

            //if (!hasChanges && !roleChanged)
            //return Result.Failure("NoChanges", "No changes were saved");

            if (hasChanges)
            {
                user.FirstName = userProfileUpdate.FirstName;
                user.LastName = userProfileUpdate.LastName;
                user.PhoneNumber = userProfileUpdate.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            if (roleChanged && newRole?.Name is not null)
            {
                if (currentRoleNames.Count > 0)
                    await _userManager.RemoveFromRolesAsync(user, currentRoleNames);
                await _userManager.AddToRoleAsync(user, newRole.Name);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("UpdateFail", ex.Message);
        }
    }
}
