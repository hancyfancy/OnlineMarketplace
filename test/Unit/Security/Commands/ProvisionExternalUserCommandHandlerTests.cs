using Moq;
using Demo.Application.Security.Commands;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Unit.Security.Commands;

public class ProvisionExternalUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly ProvisionExternalUserCommandHandler _sut;

    public ProvisionExternalUserCommandHandlerTests()
    {
        _sut = new ProvisionExternalUserCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositorySucceeds()
    {
        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = Guid.NewGuid(),
                Roles    = ["Seller"]
            }));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenRepositoryFails()
    {
        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(Result.Failure<ExternalUserProvisionResult>("ProvisionFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("ProvisionFail", result.Error.Code);
        Assert.Equal("Database error", result.Error.Description);
    }

    [Fact]
    public async Task Handle_MapsUserGuidAndRoles_ToDto()
    {
        var expectedGuid = Guid.NewGuid();

        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = expectedGuid,
                Roles    = ["Seller"]
            }));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(expectedGuid, result.Value.UserGuid);
        Assert.Contains("Seller", result.Value.Roles);
    }

    [Fact]
    public async Task Handle_MapsMultipleRoles_ToDto()
    {
        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = Guid.NewGuid(),
                Roles    = ["Seller", "Buyer"]
            }));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(2, result.Value.Roles.Count);
        Assert.Contains("Seller", result.Value.Roles);
        Assert.Contains("Buyer", result.Value.Roles);
    }

    [Fact]
    public async Task Handle_PassesAllCommandFields_ToRepository()
    {
        string? capturedEmail      = null;
        string? capturedGivenName  = null;
        string? capturedFamilyName = null;
        string? capturedRole       = null;

        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Callback<string, string?, string?, string, string?, string?, string?, string?>(
                (email, given, family, role, _, _, _, _) =>
            {
                capturedEmail      = email;
                capturedGivenName  = given;
                capturedFamilyName = family;
                capturedRole       = role;
            })
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = Guid.NewGuid(),
                Roles    = ["Seller"]
            }));

        await _sut.Handle(
            BuildCommand(email: "jane@example.com", givenName: "Jane", familyName: "Doe", defaultRole: "Seller"),
            CancellationToken.None);

        Assert.Equal("jane@example.com", capturedEmail);
        Assert.Equal("Jane", capturedGivenName);
        Assert.Equal("Doe", capturedFamilyName);
        Assert.Equal("Seller", capturedRole);
    }

    [Fact]
    public async Task Handle_PassesNullGivenName_ToRepository()
    {
        string? capturedGivenName = "not-null";

        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Callback<string, string?, string?, string, string?, string?, string?, string?>(
                (_, given, _, _, _, _, _, _) => capturedGivenName = given)
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = Guid.NewGuid(),
                Roles    = ["Seller"]
            }));

        await _sut.Handle(BuildCommand(givenName: null), CancellationToken.None);

        Assert.Null(capturedGivenName);
    }

    [Fact]
    public async Task Handle_PassesNullFamilyName_ToRepository()
    {
        string? capturedFamilyName = "not-null";

        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Callback<string, string?, string?, string, string?, string?, string?, string?>(
                (_, _, family, _, _, _, _, _) => capturedFamilyName = family)
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = Guid.NewGuid(),
                Roles    = ["Seller"]
            }));

        await _sut.Handle(BuildCommand(familyName: null), CancellationToken.None);

        Assert.Null(capturedFamilyName);
    }

    [Fact]
    public async Task Handle_CallsProvisionExternalUserAsync_ExactlyOnce()
    {
        _repoMock
            .Setup(r => r.ProvisionExternalUserAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(Result.Success(new ExternalUserProvisionResult
            {
                UserGuid = Guid.NewGuid(),
                Roles    = ["Seller"]
            }));

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.ProvisionExternalUserAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Once);
    }

    private static ProvisionExternalUserCommand BuildCommand(
        string email       = "test@example.com",
        string? givenName  = "John",
        string? familyName = "Smith",
        string defaultRole = "Seller")
    {
        return new ProvisionExternalUserCommand(email, givenName, familyName, defaultRole);
    }
}
