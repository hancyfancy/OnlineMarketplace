using Microsoft.AspNetCore.Authorization;
using Moq;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;
using System.Security.Claims;
using Web.Policies;

namespace Unit.Web.Policies;

public class ActiveSellerHandlerTests
{
    private readonly Mock<ISellerProfileRepository> _repoMock = new();
    private readonly ActiveSellerHandler _sut;

    public ActiveSellerHandlerTests()
    {
        _sut = new ActiveSellerHandler(_repoMock.Object);
    }

    [Fact]
    public async Task HandleRequirement_Succeeds_WhenSellerIsActive()
    {
        var userGuid = Guid.NewGuid();
        _repoMock
            .Setup(r => r.ReadAsync(userGuid))
            .ReturnsAsync(Result.Success<SellerProfile>(new SellerProfile
            {
                SellerProfileId = Guid.NewGuid(),
                SellerStatus = "active"
            }));

        var context = BuildContext(role: "Seller", identityGuid: userGuid);
        await _sut.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_DoesNotSucceed_WhenUserIsBuyer()
    {
        var context = BuildContext(role: "Buyer", identityGuid: Guid.NewGuid());
        await _sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        _repoMock.Verify(r => r.ReadAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirement_DoesNotSucceed_WhenSellerStatusIsNotActive()
    {
        var userGuid = Guid.NewGuid();
        _repoMock
            .Setup(r => r.ReadAsync(userGuid))
            .ReturnsAsync(Result.Success<SellerProfile>(new SellerProfile
            {
                SellerProfileId = Guid.NewGuid(),
                SellerStatus = "pending_verification"
            }));

        var context = BuildContext(role: "Seller", identityGuid: userGuid);
        await _sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_DoesNotSucceed_WhenNoSellerProfileExists()
    {
        var userGuid = Guid.NewGuid();
        _repoMock
            .Setup(r => r.ReadAsync(userGuid))
            .ReturnsAsync(Result.Failure<SellerProfile>("NotFound", "Seller profile not found"));

        var context = BuildContext(role: "Seller", identityGuid: userGuid);
        await _sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirement_DoesNotSucceed_WhenIdentityGuidClaimIsMissing()
    {
        var context = BuildContext(role: "Seller", identityGuid: null);
        await _sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        _repoMock.Verify(r => r.ReadAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirement_DoesNotSucceed_WhenIdentityGuidIsInvalid()
    {
        var context = BuildContext(role: "Seller", identityGuidOverride: "not-a-valid-guid");
        await _sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        _repoMock.Verify(r => r.ReadAsync(It.IsAny<Guid>()), Times.Never);
    }

    private static AuthorizationHandlerContext BuildContext(
        string role = "Seller",
        Guid? identityGuid = null,
        string? identityGuidOverride = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role)
        };

        if (identityGuidOverride != null)
            claims.Add(new Claim("identity_guid", identityGuidOverride));
        else if (identityGuid.HasValue)
            claims.Add(new Claim("identity_guid", identityGuid.Value.ToString()));

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return new AuthorizationHandlerContext(new[] { new ActiveSellerRequirement() }, user, null);
    }
}
