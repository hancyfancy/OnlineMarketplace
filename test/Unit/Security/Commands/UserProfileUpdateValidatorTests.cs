using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using FluentValidation.TestHelper;

namespace Unit.Security.Commands;

public class UserProfileUpdateValidatorTests
{
    private readonly UserProfileUpdateValidator _sut = new();

    [Theory]
    [InlineData("+61411111111")]
    [InlineData("+61298765432")]
    [InlineData("0411111111")]
    public void PhoneNumber_ShouldPass_WhenValidAuNumber(string phone)
    {
        var result = _sut.TestValidate(BuildCommand(phoneNumber: phone));

        result.ShouldNotHaveValidationErrorFor(x => x.Model.PhoneNumber);
    }

    [Theory]
    [InlineData("+14155552671")]
    [InlineData("+447911123456")]
    public void PhoneNumber_ShouldFail_WhenNonAuNumber(string phone)
    {
        var result = _sut.TestValidate(BuildCommand(phoneNumber: phone));

        result.ShouldHaveValidationErrorFor(x => x.Model.PhoneNumber)
              .WithErrorMessage("Invalid AU Phone Number");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PhoneNumber_ShouldFail_WhenEmptyOrWhitespace(string phone)
    {
        var result = _sut.TestValidate(BuildCommand(phoneNumber: phone));

        result.ShouldHaveValidationErrorFor(x => x.Model.PhoneNumber)
              .WithErrorMessage("Invalid AU Phone Number");
    }

    [Fact]
    public void PhoneNumber_ShouldFail_WhenGibberish()
    {
        var result = _sut.TestValidate(BuildCommand(phoneNumber: "abc123"));

        result.ShouldHaveValidationErrorFor(x => x.Model.PhoneNumber)
              .WithErrorMessage("Invalid AU Phone Number");
    }

    private static UserProfileUpdateCommand BuildCommand(
        string username = "test@example.com",
        string firstName = "John",
        string lastName = "Doe",
        string phoneNumber = "+61411111111")
    {
        return new UserProfileUpdateCommand(username, new UserProfileUpdateDto
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber
        });
    }
}
