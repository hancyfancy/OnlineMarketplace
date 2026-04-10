using Moq;
using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Unit.Security.Commands;

public class SellerProfileUpdateCommandHandlerTests
{
    private readonly Mock<ISellerProfileRepository> _repoMock = new();
    private readonly SellerProfileUpdateCommandHandler _sut;

    public SellerProfileUpdateCommandHandlerTests()
    {
        _sut = new SellerProfileUpdateCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositorySucceeds()
    {
        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<SellerProfileUpdate>()))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenRepositoryFails()
    {
        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<SellerProfileUpdate>()))
            .ReturnsAsync(Result.Failure("UpsertFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("UpsertFail", result.Error.Code);
        Assert.Equal("Database error", result.Error.Description);
    }

    [Fact]
    public async Task Handle_MapsAllFields_ToRepository()
    {
        SellerProfileUpdate? captured = null;
        Guid? capturedGuid = null;

        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<SellerProfileUpdate>()))
            .Callback<Guid, SellerProfileUpdate>((guid, update) =>
            {
                capturedGuid = guid;
                captured = update;
            })
            .ReturnsAsync(Result.Success());

        var userGuid = Guid.NewGuid();
        var command = BuildCommand(
            userGuid: userGuid,
            businessName: "Acme Corp",
            businessType: "company",
            businessTaxId: "ABN123",
            registrationNumber: "REG456",
            registrationCountry: "AU",
            vatNumber: "VAT789",
            handlingTimeDays: 3,
            returnPolicyDays: 14,
            refundPolicy: "partial",
            payoutSchedule: "monthly");

        await _sut.Handle(command, CancellationToken.None);

        Assert.Equal(userGuid, capturedGuid);
        Assert.Equal("Acme Corp", captured!.BusinessName);
        Assert.Equal("company", captured.BusinessType);
        Assert.Equal("ABN123", captured.BusinessTaxId);
        Assert.Equal("REG456", captured.BusinessRegistrationNumber);
        Assert.Equal("AU", captured.BusinessRegistrationCountry);
        Assert.Equal("VAT789", captured.VatNumber);
        Assert.Equal(3, captured.HandlingTimeDays);
        Assert.Equal(14, captured.ReturnPolicyDays);
        Assert.Equal("partial", captured.RefundPolicy);
        Assert.Equal("monthly", captured.PayoutSchedule);
    }

    [Fact]
    public async Task Handle_CallsUpsertAsync_ExactlyOnce()
    {
        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<SellerProfileUpdate>()))
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<SellerProfileUpdate>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PassesUserGuid_ToRepository()
    {
        Guid? capturedGuid = null;

        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<SellerProfileUpdate>()))
            .Callback<Guid, SellerProfileUpdate>((guid, _) => capturedGuid = guid)
            .ReturnsAsync(Result.Success());

        var expectedGuid = Guid.NewGuid();
        await _sut.Handle(BuildCommand(userGuid: expectedGuid), CancellationToken.None);

        Assert.Equal(expectedGuid, capturedGuid);
    }

    private static SellerProfileUpdateCommand BuildCommand(
        Guid? userGuid = null,
        string? businessName = "Test Business",
        string businessType = "individual",
        string? businessTaxId = null,
        string? registrationNumber = null,
        string? registrationCountry = null,
        string? vatNumber = null,
        int handlingTimeDays = 2,
        int returnPolicyDays = 30,
        string refundPolicy = "full",
        string payoutSchedule = "weekly")
    {
        return new SellerProfileUpdateCommand(
            userGuid ?? Guid.NewGuid(),
            new SellerProfileUpdateDto
            {
                BusinessName = businessName,
                BusinessType = businessType,
                BusinessTaxId = businessTaxId,
                BusinessRegistrationNumber = registrationNumber,
                BusinessRegistrationCountry = registrationCountry,
                VatNumber = vatNumber,
                HandlingTimeDays = handlingTimeDays,
                ReturnPolicyDays = returnPolicyDays,
                RefundPolicy = refundPolicy,
                PayoutSchedule = payoutSchedule
            });
    }
}
