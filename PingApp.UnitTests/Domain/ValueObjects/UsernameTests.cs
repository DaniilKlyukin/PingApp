using FluentAssertions;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace PingApp.UnitTests.Domain.ValueObjects;

public class UsernameTests
{
    [Theory]
    [InlineData("usr")]
    [InlineData("ValidUser123")]
    [InlineData("  TrimmingUser  ")]
    public void Create_ShouldSucceed_ForValidUsernames(string value)
    {
        var result = Username.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Create_ShouldFail_WhenUsernameIsEmpty(string? value)
    {
        var result = Username.Create(value!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidUsernameEmpty);
    }

    [Fact]
    public void Create_ShouldFail_WhenUsernameIsTooShort()
    {
        var shortUsername = new string('a', Username.MinLength - 1);

        var result = Username.Create(shortUsername);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UsernameTooShort);
    }

    [Fact]
    public void Create_ShouldFail_WhenUsernameIsTooLong()
    {
        var longUsername = new string('a', Username.MaxLength + 1);

        var result = Username.Create(longUsername);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UsernameTooLong);
    }

    [Fact]
    public void Equality_ShouldBeCaseInsensitive()
    {
        var username1 = Username.Create("ADMIN").Value;
        var username2 = Username.Create("admin").Value;

        username1.Should().Be(username2);
    }
}
