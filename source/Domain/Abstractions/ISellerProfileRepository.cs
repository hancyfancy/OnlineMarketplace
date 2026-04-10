using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Domain.Abstractions
{
    public interface ISellerProfileRepository
    {
        Task<Result<SellerProfile>> ReadAsync(Guid userGuid);

        Task<Result> UpsertAsync(Guid userGuid, SellerProfileUpdate update);
    }
}
