using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Identity;

public class DemoIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public DbSet<BuyerProfileEntity> BuyerProfiles { get; set; }
    public DbSet<SellerProfileEntity> SellerProfiles { get; set; }

    public DemoIdentityDbContext(DbContextOptions<DemoIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("users", "security");
            b.HasKey(u => u.Id);

            // Map all the custom properties
            b.Property(u => u.UserGuid).HasColumnName("user_guid");
            b.Property(u => u.FirstName).HasColumnName("first_name");
            b.Property(u => u.LastName).HasColumnName("last_name");
            b.Property(u => u.DateOfBirth).HasColumnName("date_of_birth");
            b.Property(u => u.AccountStatus).HasColumnName("account_status");
            b.Property(u => u.LastPasswordChange).HasColumnName("last_password_change");
            b.Property(u => u.FailedLoginAttempts).HasColumnName("failed_login_attempts");
            b.Property(u => u.LockedUntil).HasColumnName("locked_until");
            b.Property(u => u.PreferredLanguage).HasColumnName("preferred_language");
            b.Property(u => u.Timezone).HasColumnName("timezone");
            b.Property(u => u.DataProcessingConsent).HasColumnName("data_processing_consent");
            b.Property(u => u.MarketingConsent).HasColumnName("marketing_consent");
            b.Property(u => u.CreatedAt).HasColumnName("created_at");
            b.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            b.Property(u => u.LastLoginAt).HasColumnName("last_login_at");
            b.Property(u => u.LastActiveAt).HasColumnName("last_active_at");

            // Map primary key
            b.Property(u => u.Id).HasColumnName("user_id");

            // Configure standard Identity properties
            b.Property(u => u.UserName).HasColumnName("username");
            b.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name");
            b.Property(u => u.Email).HasColumnName("email");
            b.Property(u => u.NormalizedEmail).HasColumnName("normalized_email");
            b.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
            b.Property(u => u.PasswordHash).HasColumnName("password_hash");
            b.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
            b.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            b.Property(u => u.PhoneNumber).HasColumnName("phone_number");
            b.Property(u => u.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
            b.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            b.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
            b.Property(u => u.LockoutEnabled).HasColumnName("lockout_enabled");
            b.Property(u => u.AccessFailedCount).HasColumnName("access_failed_count");

            // Map MFA properties
            b.Property(u => u.IsMfaEnabled).HasColumnName("is_mfa_enabled");
            b.Property(u => u.AuthenticatorKey).HasColumnName("authenticator_key");
            b.Property(u => u.EmailMfaSecret).HasColumnName("email_mfa_secret");
            b.Property(u => u.EmailMfaCodeExpiresAt).HasColumnName("email_mfa_code_expires_at");
            b.Property(u => u.IsEmailMfaEnabled).HasColumnName("is_email_mfa_enabled");
            b.Property(u => u.IsAuthenticatorAppEnabled).HasColumnName("is_authenticator_app_enabled");
            b.Property(u => u.MfaSetupAt).HasColumnName("mfa_setup_at");
            b.Property(u => u.LastMfaVerificationAt).HasColumnName("last_mfa_verification_at");
            b.Property(u => u.ApiKey).HasColumnName("api_key");

            // Configure indexes
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("email_index");
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("user_name_index");
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("roles", "security");
            b.HasKey(r => r.Id);

            // Map custom properties
            b.Property(r => r.Description).HasColumnName("description");
            b.Property(r => r.RoleType).HasColumnName("role_type");
            b.Property(r => r.IsActive).HasColumnName("is_active");
            b.Property(r => r.Priority).HasColumnName("priority");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            b.Property(r => r.CreatedBy).HasColumnName("created_by");
            b.Property(r => r.UpdatedBy).HasColumnName("updated_by");

            // Map primary key
            b.Property(r => r.Id).HasColumnName("role_id");

            // Configure standard Identity properties
            b.Property(r => r.Name).HasColumnName("name");
            b.Property(r => r.NormalizedName).HasColumnName("normalized_name");
            b.Property(r => r.ConcurrencyStamp).HasColumnName("concurrency_stamp");

            // Configure indexes
            b.HasIndex(r => r.NormalizedName).HasDatabaseName("role_name_index");
        });

        builder.Entity<UserRole>(b =>
        {
            b.ToTable("user_roles", "security");
            b.Property(ur => ur.UserId).HasColumnName("user_id");
            b.Property(ur => ur.RoleId).HasColumnName("role_id");
        });

        builder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.ToTable("user_claims", "security");
            b.Property(c => c.Id).HasColumnName("id");
            b.Property(c => c.UserId).HasColumnName("user_id");
            b.Property(c => c.ClaimType).HasColumnName("claim_type");
            b.Property(c => c.ClaimValue).HasColumnName("claim_value");
        });

        builder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.ToTable("user_logins", "security");
            b.Property(l => l.LoginProvider).HasColumnName("login_provider");
            b.Property(l => l.ProviderKey).HasColumnName("provider_key");
            b.Property(l => l.ProviderDisplayName).HasColumnName("provider_display_name");
            b.Property(l => l.UserId).HasColumnName("user_id");
        });

        builder.Entity<IdentityUserToken<string>>(b =>
        {
            b.ToTable("user_tokens", "security");
            b.Property(t => t.UserId).HasColumnName("user_id");
            b.Property(t => t.LoginProvider).HasColumnName("login_provider");
            b.Property(t => t.Name).HasColumnName("name");
            b.Property(t => t.Value).HasColumnName("value");
        });

        builder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.ToTable("role_claims", "security");
            b.Property(c => c.Id).HasColumnName("id");
            b.Property(c => c.RoleId).HasColumnName("role_id");
            b.Property(c => c.ClaimType).HasColumnName("claim_type");
            b.Property(c => c.ClaimValue).HasColumnName("claim_value");
        });

        builder.Entity<BuyerProfileEntity>(b =>
        {
            b.ToTable("buyer_profiles", "security");
            b.HasKey(x => x.BuyerProfileId);
            b.Property(x => x.BuyerProfileId).HasColumnName("buyer_profile_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.PreferredCurrency).HasColumnName("preferred_currency");
            b.Property(x => x.BuyerLevel).HasColumnName("buyer_level");
            b.Property(x => x.TrustScore).HasColumnName("trust_score");
            b.Property(x => x.TotalPurchases).HasColumnName("total_purchases");
            b.Property(x => x.AvgOrderValue).HasColumnName("avg_order_value");
            b.Property(x => x.TotalReturns).HasColumnName("total_returns");
            b.Property(x => x.OpenDisputes).HasColumnName("open_disputes");
            b.Property(x => x.DisputeRate).HasColumnName("dispute_rate");
        });

        builder.Entity<SellerProfileEntity>(b =>
        {
            b.ToTable("seller_profiles", "security");
            b.HasKey(s => s.SellerProfileId);
            b.Property(s => s.SellerProfileId).HasColumnName("seller_profile_id");
            b.Property(s => s.UserId).HasColumnName("user_id");
            b.Property(s => s.BusinessName).HasColumnName("business_name");
            b.Property(s => s.BusinessType).HasColumnName("business_type");
            b.Property(s => s.BusinessTaxId).HasColumnName("business_tax_id");
            b.Property(s => s.BusinessRegistrationNumber).HasColumnName("business_registration_number");
            b.Property(s => s.BusinessRegistrationCountry).HasColumnName("business_registration_country");
            b.Property(s => s.VatNumber).HasColumnName("vat_number");
            b.Property(s => s.SellerTier).HasColumnName("seller_tier");
            b.Property(s => s.SellerStatus).HasColumnName("seller_status");
            b.Property(s => s.HandlingTimeDays).HasColumnName("handling_time_days");
            b.Property(s => s.ReturnPolicyDays).HasColumnName("return_policy_days");
            b.Property(s => s.RefundPolicy).HasColumnName("refund_policy");
            b.Property(s => s.PayoutSchedule).HasColumnName("payout_schedule");
        });
    }
}
