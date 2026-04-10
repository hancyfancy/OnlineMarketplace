using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Moq;
using Demo.Domain.Primitives;

namespace Unit.Security.Commands;

public class UserProfileUpdateCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly UserProfileUpdateCommandHandler _sut;

    public UserProfileUpdateCommandHandlerTests()
    {
        _sut = new UserProfileUpdateCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositorySucceeds()
    {
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UserProfileUpdate>()))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserNotFound()
    {
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UserProfileUpdate>()))
            .ReturnsAsync(Result.Failure("NotFound", "User not found"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error.Code);
        Assert.Equal("User not found", result.Error.Description);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenNoChangesDetected()
    {
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UserProfileUpdate>()))
            .ReturnsAsync(Result.Failure("NoChanges", "No changes were saved"));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("NoChanges", result.Error.Code);
    }

    [Fact]
    public async Task Handle_MapsCommandFieldsCorrectly_ToRepository()
    {
        UserProfileUpdate? captured = null;
        string? capturedUsername = null;

        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UserProfileUpdate>()))
            .Callback<string, UserProfileUpdate>((username, update) =>
            {
                capturedUsername = username;
                captured = update;
            })
            .ReturnsAsync(Result.Success());

        var command = BuildCommand("user@example.com", "Jane", "Smith", "+61411111111");

        await _sut.Handle(command, CancellationToken.None);

        Assert.Equal("user@example.com", capturedUsername);
        Assert.Equal("Jane", captured!.FirstName);
        Assert.Equal("Smith", captured.LastName);
        Assert.Equal("+61411111111", captured.PhoneNumber);
    }

    [Fact]
    public async Task Handle_CallsRepository_ExactlyOnce()
    {
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UserProfileUpdate>()))
            .ReturnsAsync(Result.Success());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<UserProfileUpdate>()), Times.Once);
    }

    private static UserProfileUpdateCommand BuildCommand(
        string username = "test@example.com",
        string firstName = "John",
        string lastName = "Doe",
        string phoneNumber = "+61400000000")
    {
        return new UserProfileUpdateCommand(username, new UserProfileUpdateDto
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber
        });
    }
}
