using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries
{
    public class SellerProfileQueryHandler : IQueryHandler<SellerProfileQuery, Result<SellerProfileDto>>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;

        public SellerProfileQueryHandler(ISellerProfileRepository sellerProfileRepository)
        {
            _sellerProfileRepository = sellerProfileRepository;
        }

        public async Task<Result<SellerProfileDto>> Handle(SellerProfileQuery request, CancellationToken cancellationToken)
        {
            var result = await _sellerProfileRepository.ReadAsync(request.UserGuid);

            if (result.IsFailure)
                return Result.Failure<SellerProfileDto>(result.Error);

            var s = result.Value;
            return Result.Success(new SellerProfileDto
            {
                SellerProfileId = s.SellerProfileId,
                BusinessName = s.BusinessName,
                BusinessType = s.BusinessType,
                BusinessTaxId = s.BusinessTaxId,
                BusinessRegistrationNumber = s.BusinessRegistrationNumber,
                BusinessRegistrationCountry = s.BusinessRegistrationCountry,
                VatNumber = s.VatNumber,
                SellerTier = s.SellerTier,
                SellerStatus = s.SellerStatus,
                HandlingTimeDays = s.HandlingTimeDays,
                ReturnPolicyDays = s.ReturnPolicyDays,
                RefundPolicy = s.RefundPolicy,
                PayoutSchedule = s.PayoutSchedule,
            });
        }
    }
}
