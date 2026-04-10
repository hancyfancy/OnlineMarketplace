using Moq;
using Demo.Application.Security.Commands;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Unit.Security.Commands;

public class RegenerateApiKeyCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly RegenerateApiKeyCommandHandler _sut;

    public RegenerateApiKeyCommandHandlerTests()
    {
        _sut = new RegenerateApiKeyCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositorySucceeds()
    {
        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("plaintext-api-key"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenRepositoryFails()
    {
        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("UpdateFail", "Database error"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("UpdateFail", result.Error.Code);
        Assert.Equal("Database error", result.Error.Description);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserNotFound()
    {
        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("NotFound", "User not found."));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_PassesEmail_ToRepository()
    {
        string? capturedEmail = null;

        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .Callback<string>(email => capturedEmail = email)
            .ReturnsAsync(Result.Success("plaintext-api-key"));

        await _sut.Handle(BuildCommand(email: "user@example.com"), CancellationToken.None);

        Assert.Equal("user@example.com", capturedEmail);
    }

    [Fact]
    public async Task Handle_CallsRegenerateApiKeyAsync_ExactlyOnce()
    {
        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("plaintext-api-key"));

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.RegenerateApiKeyAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnedKey_IsNotNullOrEmpty_OnSuccess()
    {
        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("plaintext-api-key"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsExactKey_FromRepository()
    {
        const string expectedKey = "abc123xyz";

        _repoMock
            .Setup(r => r.RegenerateApiKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(expectedKey));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(expectedKey, result.Value);
    }

    private static RegenerateApiKeyCommand BuildCommand(string email = "test@example.com")
    {
        return new RegenerateApiKeyCommand(email);
    }
}
