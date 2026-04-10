using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Domain.Abstractions
{
    public interface IBuyerProfileRepository
    {
        Task<Result<BuyerProfile>> ReadAsync(Guid userGuid);
        Task<Result> UpsertAsync(Guid userGuid, BuyerProfileUpdate update);
    }
}
