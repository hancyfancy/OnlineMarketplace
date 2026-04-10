using Microsoft.EntityFrameworkCore;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;
using Demo.Infrastructure.Identity;

namespace Demo.Infrastructure.Repository;

public class SellerProfileRepository : ISellerProfileRepository
{
    private readonly IDbContextFactory<DemoIdentityDbContext> _dbContextFactory;

    public SellerProfileRepository(IDbContextFactory<DemoIdentityDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<SellerProfile>> ReadAsync(Guid userGuid)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var userId = await context.Users
                .Where(u => u.UserGuid == userGuid)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId is null)
                return Result.Success(new SellerProfile());

            var entity = await context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (entity is null)
                return Result.Success(new SellerProfile { UserId = userId });

            return Result.Success(new SellerProfile
            {
                SellerProfileId = entity.SellerProfileId,
                UserId = entity.UserId,
                BusinessName = entity.BusinessName,
                BusinessType = entity.BusinessType,
                BusinessTaxId = entity.BusinessTaxId,
                BusinessRegistrationNumber = entity.BusinessRegistrationNumber,
                BusinessRegistrationCountry = entity.BusinessRegistrationCountry,
                VatNumber = entity.VatNumber,
                SellerTier = entity.SellerTier,
                SellerStatus = entity.SellerStatus,
                HandlingTimeDays = entity.HandlingTimeDays,
                ReturnPolicyDays = entity.ReturnPolicyDays,
                RefundPolicy = entity.RefundPolicy,
                PayoutSchedule = entity.PayoutSchedule,
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<SellerProfile>("ReadFail", ex.Message);
        }
    }

    public async Task<Result> UpsertAsync(Guid userGuid, SellerProfileUpdate update)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var userId = await context.Users
                .Where(u => u.UserGuid == userGuid)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId is null)
                return Result.Failure("UserNotFound", "No user found for the provided identifier.");

            var entity = await context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (entity is null)
            {
                entity = new SellerProfileEntity { UserId = userId };
                context.SellerProfiles.Add(entity);
            }

            entity.BusinessName = update.BusinessName;
            entity.BusinessType = update.BusinessType;
            entity.BusinessTaxId = update.BusinessTaxId;
            entity.BusinessRegistrationNumber = update.BusinessRegistrationNumber;
            entity.BusinessRegistrationCountry = update.BusinessRegistrationCountry;
            entity.VatNumber = update.VatNumber;
            entity.HandlingTimeDays = update.HandlingTimeDays;
            entity.ReturnPolicyDays = update.ReturnPolicyDays;
            entity.RefundPolicy = update.RefundPolicy;
            entity.PayoutSchedule = update.PayoutSchedule;
            entity.SellerStatus = "active"; // For testing purposes

            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("UpsertFail", ex.Message);
        }
    }
}
