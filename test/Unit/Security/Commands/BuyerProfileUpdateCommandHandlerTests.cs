using Moq;
using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Unit.Security.Commands;

public class BuyerProfileUpdateCommandHandlerTests
{
    private readonly Mock<IBuyerProfileRepository> _repoMock = new();
    private readonly BuyerProfileUpdateCommandHandler _sut;

    public BuyerProfileUpdateCommandHandlerTests()
    {
        _sut = new BuyerProfileUpdateCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositorySucceeds()
    {
        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<BuyerProfileUpdate>()))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenRepositoryFails()
    {
        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<BuyerProfileUpdate>()))
            .ReturnsAsync(Result.Failure("UpsertFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("UpsertFail", result.Error.Code);
        Assert.Equal("Database error", result.Error.Description);
    }

    [Fact]
    public async Task Handle_MapsPreferredCurrency_ToRepository()
    {
        BuyerProfileUpdate? captured = null;
        Guid? capturedGuid = null;

        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<BuyerProfileUpdate>()))
            .Callback<Guid, BuyerProfileUpdate>((guid, update) =>
            {
                capturedGuid = guid;
                captured = update;
            })
            .ReturnsAsync(Result.Success());

        var userGuid = Guid.NewGuid();
        var command = BuildCommand(userGuid, "AUD");

        await _sut.Handle(command, CancellationToken.None);

        Assert.Equal(userGuid, capturedGuid);
        Assert.Equal("AUD", captured!.PreferredCurrency);
    }

    [Fact]
    public async Task Handle_CallsUpsertAsync_ExactlyOnce()
    {
        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<BuyerProfileUpdate>()))
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<BuyerProfileUpdate>()), Times.Once);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("AUD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public async Task Handle_PassesThrough_DifferentCurrencies(string currency)
    {
        BuyerProfileUpdate? captured = null;

        _repoMock
            .Setup(r => r.UpsertAsync(It.IsAny<Guid>(), It.IsAny<BuyerProfileUpdate>()))
            .Callback<Guid, BuyerProfileUpdate>((_, update) => captured = update)
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(currency: currency), CancellationToken.None);

        Assert.Equal(currency, captured!.PreferredCurrency);
    }

    private static BuyerProfileUpdateCommand BuildCommand(
        Guid? userGuid = null,
        string currency = "USD")
    {
        return new BuyerProfileUpdateCommand(
            userGuid ?? Guid.NewGuid(),
            new BuyerProfileUpdateDto { PreferredCurrency = currency });
    }
}
