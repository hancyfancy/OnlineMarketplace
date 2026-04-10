using MediatR;
using Demo.Application.Security.Abstractions;
using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using Demo.Application.Security.Queries;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Services
{
    public class AccountService : IAccountService
    {
        private readonly IMediator _mediator;

        public AccountService(
            IMediator mediator
        )
        {
            _mediator = mediator;
        }

        public async Task<List<RoleDto>> GetAvailableRolesAsync(AvailableRolesQuery query)
        {
            return await _mediator.Send(query);
        }

        public async Task<Result<UserProfileDto>> GetUserProfileAsync(UserProfileQuery query)
        {
            return await _mediator.Send(query);
        }

        public async Task<Result> UpdateUserProfileAsync(UserProfileUpdateCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<Result<SellerProfileDto>> GetSellerProfileAsync(SellerProfileQuery query)
        {
            return await _mediator.Send(query);
        }

        public async Task<Result> UpdateSellerProfileAsync(SellerProfileUpdateCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<Result<BuyerProfileDto>> GetBuyerProfileAsync(BuyerProfileQuery query)
        {
            return await _mediator.Send(query);
        }

        public async Task<Result> UpdateBuyerProfileAsync(BuyerProfileUpdateCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<Result<ExternalUserProvisionDto>> ProvisionExternalUserAsync(ProvisionExternalUserCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<Result<LoginResultDto>> IssueTokenAsync(IssueTokenQuery query)
        {
            return await _mediator.Send(query);
        }

        public async Task<Result<string>> RegenerateApiKeyAsync(RegenerateApiKeyCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
