using Microsoft.AspNetCore.Authorization;
using Demo.Domain.Abstractions;
using System.Security.Claims;

namespace Web.Policies;

public class ActiveSellerHandler : AuthorizationHandler<ActiveSellerRequirement>
{
    private readonly ISellerProfileRepository _sellerProfileRepository;

    public ActiveSellerHandler(ISellerProfileRepository sellerProfileRepository)
    {
        _sellerProfileRepository = sellerProfileRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveSellerRequirement requirement)
    {
        if (!context.User.IsInRole("Seller"))
            return;

        var userGuidStr = context.User.FindFirstValue("identity_guid");
        if (!Guid.TryParse(userGuidStr, out var userGuid))
            return;

        var result = await _sellerProfileRepository.ReadAsync(userGuid);

        if (!result.IsSuccess)
            return;

        if (result.Value.SellerProfileId != default && result.Value.SellerStatus == "active")
            context.Succeed(requirement);
    }
}
