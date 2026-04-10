using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class SellerProfileUpdateCommandHandler : ICommandHandler<SellerProfileUpdateCommand, Result>
    {
        private readonly ISellerProfileRepository _sellerProfileRepository;

        public SellerProfileUpdateCommandHandler(ISellerProfileRepository sellerProfileRepository)
        {
            _sellerProfileRepository = sellerProfileRepository;
        }

        public async Task<Result> Handle(SellerProfileUpdateCommand request, CancellationToken cancellationToken)
        {
            var update = new SellerProfileUpdate
            {
                BusinessName = request.Model.BusinessName,
                BusinessType = request.Model.BusinessType,
                BusinessTaxId = request.Model.BusinessTaxId,
                BusinessRegistrationNumber = request.Model.BusinessRegistrationNumber,
                BusinessRegistrationCountry = request.Model.BusinessRegistrationCountry,
                VatNumber = request.Model.VatNumber,
                HandlingTimeDays = request.Model.HandlingTimeDays,
                ReturnPolicyDays = request.Model.ReturnPolicyDays,
                RefundPolicy = request.Model.RefundPolicy,
                PayoutSchedule = request.Model.PayoutSchedule,
            };

            return await _sellerProfileRepository.UpsertAsync(request.UserGuid, update);
        }
    }
}
