using Microsoft.EntityFrameworkCore;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;
using Demo.Infrastructure.Identity;

namespace Demo.Infrastructure.Repository;

public class BuyerProfileRepository : IBuyerProfileRepository
{
    private readonly IDbContextFactory<DemoIdentityDbContext> _dbContextFactory;

    public BuyerProfileRepository(IDbContextFactory<DemoIdentityDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<BuyerProfile>> ReadAsync(Guid userGuid)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var userId = await context.Users
                .Where(u => u.UserGuid == userGuid)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId is null)
                return Result.Success(new BuyerProfile());

            var entity = await context.BuyerProfiles
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (entity is null)
                return Result.Success(new BuyerProfile { UserId = userId });

            return Result.Success(new BuyerProfile
            {
                BuyerProfileId = entity.BuyerProfileId,
                UserId = entity.UserId,
                PreferredCurrency = entity.PreferredCurrency,
                BuyerLevel = entity.BuyerLevel,
                TrustScore = entity.TrustScore,
                TotalPurchases = entity.TotalPurchases,
                AvgOrderValue = entity.AvgOrderValue,
                TotalReturns = entity.TotalReturns,
                OpenDisputes = entity.OpenDisputes,
                DisputeRate = entity.DisputeRate,
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<BuyerProfile>("ReadFail", ex.Message);
        }
    }

    public async Task<Result> UpsertAsync(Guid userGuid, BuyerProfileUpdate update)
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

            var entity = await context.BuyerProfiles
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (entity is null)
            {
                entity = new BuyerProfileEntity { UserId = userId };
                context.BuyerProfiles.Add(entity);
            }

            entity.PreferredCurrency = update.PreferredCurrency;

            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("UpsertFail", ex.Message);
        }
    }
}
