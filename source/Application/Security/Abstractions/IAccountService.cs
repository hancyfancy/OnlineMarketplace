using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using Demo.Application.Security.Queries;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Abstractions
{
    public interface IAccountService
    {
        Task<List<RoleDto>> GetAvailableRolesAsync(AvailableRolesQuery query);

        Task<Result<UserProfileDto>> GetUserProfileAsync(UserProfileQuery query);

        Task<Result> UpdateUserProfileAsync(UserProfileUpdateCommand command);

        Task<Result<SellerProfileDto>> GetSellerProfileAsync(SellerProfileQuery query);

        Task<Result> UpdateSellerProfileAsync(SellerProfileUpdateCommand command);

        Task<Result<BuyerProfileDto>> GetBuyerProfileAsync(BuyerProfileQuery query);

        Task<Result> UpdateBuyerProfileAsync(BuyerProfileUpdateCommand command);

        Task<Result<ExternalUserProvisionDto>> ProvisionExternalUserAsync(ProvisionExternalUserCommand command);

        Task<Result<LoginResultDto>> IssueTokenAsync(IssueTokenQuery query);

        Task<Result<string>> RegenerateApiKeyAsync(RegenerateApiKeyCommand command);
    }
}
