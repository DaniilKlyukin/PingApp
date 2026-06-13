using FluentAssertions;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.UnitTests.Domain;

public class DeviceAddressTests
{
    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("2001:db8::ff00:42:8329")]
    [InlineData("localhost")]
    [InlineData("example.com")]
    [InlineData("  my.domain.local  ")]
    public void Create_ShouldSucceed_ForValidAddresses(string rawAddress)
    {
        var result = DeviceAddress.Create(rawAddress);

        result.IsSuccess.Should().BeTrue();  
        result.Value.Value.Should().Be(rawAddress.Trim());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_ShouldFail_WhenAddressIsEmpty(string? rawAddress)
    {
        var result = DeviceAddress.Create(rawAddress!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeviceErrors.EmptyAddress);
    }

    [Theory]
    [InlineData("invalid_address#")]
    [InlineData("http://google.com")]
    [InlineData("192.168.1.300")]
    public void Create_ShouldFail_WhenAddressIsInvalidFormat(string rawAddress)
    {
        var result = DeviceAddress.Create(rawAddress);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeviceErrors.InvalidAddress);
    }

    [Fact]
    public void Equality_ShouldBeCaseInsensitive()
    {
        var address1 = DeviceAddress.Create("GOOGLE.COM").Value;
        var address2 = DeviceAddress.Create("google.com").Value;

        address1.Should().Be(address2);
    }
}
