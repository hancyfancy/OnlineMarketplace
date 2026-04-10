using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class BuyerProfileUpdateCommandHandler : ICommandHandler<BuyerProfileUpdateCommand, Result>
    {
        private readonly IBuyerProfileRepository _buyerProfileRepository;

        public BuyerProfileUpdateCommandHandler(IBuyerProfileRepository buyerProfileRepository)
        {
            _buyerProfileRepository = buyerProfileRepository;
        }

        public async Task<Result> Handle(BuyerProfileUpdateCommand request, CancellationToken cancellationToken)
        {
            var update = new BuyerProfileUpdate
            {
                PreferredCurrency = request.Model.PreferredCurrency,
            };

            return await _buyerProfileRepository.UpsertAsync(request.UserGuid, update);
        }
    }
}
