using FluentAssertions;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.UnitTests.Domain.ValueObjects;

public class PasswordTests
{
    [Theory]
    [InlineData("1234")]
    [InlineData("some-secure-password")]
    public void Create_ShouldSucceed_ForValidPasswords(string value)
    {
        var result = Password.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenPasswordIsEmpty(string? value)
    {
        var result = Password.Create(value!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmptyPassword);
    }

    [Fact]
    public void Create_ShouldFail_WhenPasswordIsTooShort()
    {
        var result = Password.Create(new string('a', Password.MinLength - 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.PasswordTooShort);
    }

    [Fact]
    public void Create_ShouldFail_WhenPasswordIsTooLong()
    {
        var longPassword = new string('a', Password.MaxLength + 1);

        var result = Password.Create(longPassword);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.PasswordTooLong);
    }
}
