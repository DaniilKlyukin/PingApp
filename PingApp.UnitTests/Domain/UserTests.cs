using FluentAssertions;
using NSubstitute;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Interfaces;

namespace PingApp.UnitTests.Domain;

public class UserTests
{
    private readonly Username _testUsername = Username.Create("TestUser").Value;
    private readonly DeviceId _deviceId = DeviceId.New();
    private readonly DeviceNickname _nickname = DeviceNickname.Create("My Router").Value;

    [Fact]
    public void Create_ShouldInitializeUserCorrectly()
    {
        var user = User.Create(_testUsername, isGuest: true, isAdmin: false);

        user.Id.Value.Should().NotBeEmpty();
        user.Username.Should().Be(_testUsername);
        user.IsGuest.Should().BeTrue();
        user.IsAdmin.Should().BeFalse();
        user.UserDevices.Should().BeEmpty();
    }

    [Fact]
    public void AddSubscription_ShouldAddDeviceToUserDevices()
    {
        var user = User.Create(_testUsername);

        user.AddSubscription(_deviceId, _nickname);

        user.UserDevices.Should().HaveCount(1);
        var userDevice = user.UserDevices.Single();
        userDevice.DeviceId.Should().Be(_deviceId);
        userDevice.DeviceNickname.Should().Be(_nickname);
        userDevice.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void AddSubscription_ShouldNotAddDuplicateDevice()
    {
        var user = User.Create(_testUsername);
        user.AddSubscription(_deviceId, _nickname);

        user.AddSubscription(_deviceId, DeviceNickname.Create("Another Name").Value);

        user.UserDevices.Should().HaveCount(1);
        user.UserDevices.Single().DeviceNickname.Should().Be(_nickname);
    }

    [Fact]
    public void RemoveSubscription_ShouldRemoveDevice_WhenSubscriptionExists()
    {
        var user = User.Create(_testUsername);
        user.AddSubscription(_deviceId, _nickname);

        user.RemoveSubscription(_deviceId);

        user.UserDevices.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSubscription_ShouldDoNothing_WhenSubscriptionDoesNotExist()
    {
        var user = User.Create(_testUsername);

        user.RemoveSubscription(_deviceId);

        user.UserDevices.Should().BeEmpty();
    }

    [Fact]
    public void SetPassword_ShouldUseHasherAndStoreHash()
    {
        var user = User.Create(_testUsername);
        var password = Password.Create("Secret123").Value;

        var mockHasher = Substitute.For<IPasswordHasher>();
        mockHasher.HashPassword(password.Value).Returns("hashed_value");

        user.SetPassword(password, mockHasher);

        user.PasswordHash.Should().Be("hashed_value");
        mockHasher.Received(1).HashPassword(password.Value);
    }
}