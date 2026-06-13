using FluentAssertions;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.UnitTests.Domain.ValueObjects;

public class NicknameTests
{
    [Fact]
    public void Create_ShouldSucceedWithNull_WhenNullIsProvided()
    {
        var result = DeviceNickname.Create(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().BeNull();
    }

    [Theory]
    [InlineData("N")]
    [InlineData("A trimmed nickname")]
    public void Create_ShouldSucceed_ForValidNicknames(string value)
    {
        var result = DeviceNickname.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenNicknameIsTooShortAfterTrim(string value)
    {
        var result = DeviceNickname.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserDeviceErrors.DeviceNicknameTooShort);
    }

    [Fact]
    public void Create_ShouldFail_WhenNicknameIsTooLong()
    {
        var longNickname = new string('n', DeviceNickname.MaxLength + 1);

        var result = DeviceNickname.Create(longNickname);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserDeviceErrors.DeviceNicknameTooLong);
    }
}