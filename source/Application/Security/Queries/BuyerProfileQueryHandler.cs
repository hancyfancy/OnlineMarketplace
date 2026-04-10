using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries
{
    public class BuyerProfileQueryHandler : IQueryHandler<BuyerProfileQuery, Result<BuyerProfileDto>>
    {
        private readonly IBuyerProfileRepository _buyerProfileRepository;

        public BuyerProfileQueryHandler(IBuyerProfileRepository buyerProfileRepository)
        {
            _buyerProfileRepository = buyerProfileRepository;
        }

        public async Task<Result<BuyerProfileDto>> Handle(BuyerProfileQuery request, CancellationToken cancellationToken)
        {
            var result = await _buyerProfileRepository.ReadAsync(request.UserGuid);

            if (result.IsFailure)
                return Result.Failure<BuyerProfileDto>(result.Error);

            var b = result.Value;
            return Result.Success(new BuyerProfileDto
            {
                BuyerProfileId = b.BuyerProfileId,
                PreferredCurrency = b.PreferredCurrency,
                BuyerLevel = b.BuyerLevel,
                TrustScore = b.TrustScore,
                TotalPurchases = b.TotalPurchases,
                AvgOrderValue = b.AvgOrderValue,
                TotalReturns = b.TotalReturns,
                OpenDisputes = b.OpenDisputes,
                DisputeRate = b.DisputeRate,
            });
        }
    }
}
