using Microsoft.AspNetCore.Identity;

namespace Demo.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<string>
{
    public Guid UserGuid { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string AccountStatus { get; set; } = "active";
    public DateTime? LastPasswordChange { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string PreferredLanguage { get; set; } = "en-US";
    public string Timezone { get; set; } = "UTC";
    public bool DataProcessingConsent { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastActiveAt { get; set; }

    // MFA properties
    public bool IsMfaEnabled { get; set; } = false;
    public string? AuthenticatorKey { get; set; }
    public string? EmailMfaSecret { get; set; }
    public DateTime? EmailMfaCodeExpiresAt { get; set; }
    public bool IsEmailMfaEnabled { get; set; } = false;
    public bool IsAuthenticatorAppEnabled { get; set; } = false;
    public DateTime? MfaSetupAt { get; set; }
    public DateTime? LastMfaVerificationAt { get; set; }

    public string? ApiKey { get; set; }
}
