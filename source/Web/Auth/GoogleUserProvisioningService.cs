using Microsoft.AspNetCore.Authentication.OAuth;
using Demo.Application.Security.Abstractions;
using Demo.Application.Security.Commands;
using System.Security.Claims;

namespace Web.Auth
{
    public class GoogleUserProvisioningService
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<GoogleUserProvisioningService> _logger;

        public GoogleUserProvisioningService(
            IAccountService accountService,
            ILogger<GoogleUserProvisioningService> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task ProvisionAsync(OAuthCreatingTicketContext context)
        {
            var email = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return;

            var givenName  = context.Identity?.FindFirst(ClaimTypes.GivenName)?.Value;
            var familyName = context.Identity?.FindFirst(ClaimTypes.Surname)?.Value;

            var providerKey = context.Identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _accountService.ProvisionExternalUserAsync(
                new ProvisionExternalUserCommand(email, givenName, familyName, "Seller",
                    loginProvider: "Google",
                    providerKey: providerKey,
                    accessToken: context.AccessToken,
                    refreshToken: context.RefreshToken));

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to provision Google user {Email}: {Error}", email, result.Error.Description);
                return;
            }

            context.Identity?.AddClaim(new Claim("identity_guid", result.Value.UserGuid.ToString()));
            foreach (var role in result.Value.Roles)
                context.Identity?.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }
}
