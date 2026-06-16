using FluentAssertions;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Enums;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.UnitTests.Domain;

public class DeviceTests
{
    private Device CreateSut(string ipAddress = "192.168.1.1")
    {
        return Device.Create(
            DeviceAddress.Create(ipAddress).Value,
            isAllowedToPing: true,
            isVisibleToUsers: true
        );
    }

    [Fact]
    public void UpdateStatus_ShouldSetFirstStatus_AndReturnLoggedIn_WhenDeviceIsOnline()
    {
        // Arrange
        var device = CreateSut();
        var time = DateTime.UtcNow;

        // Act
        var transition = device.UpdateStatus(time, atWork: true);

        // Assert
        transition.Should().Be(DeviceStatusTransition.LoggedIn);
        device.LastKnownStatus.Should().BeTrue();
        device.LastStatusChangedUtc.Should().Be(time);
    }

    [Fact]
    public void UpdateStatus_ShouldSetFirstStatus_AndReturnNone_WhenDeviceIsOffline()
    {
        // Arrange
        var device = CreateSut();
        var time = DateTime.UtcNow;

        // Act
        var transition = device.UpdateStatus(time, atWork: false);

        // Assert
        transition.Should().Be(DeviceStatusTransition.None);
        device.LastKnownStatus.Should().BeFalse();
        device.LastStatusChangedUtc.Should().Be(time);
    }

    [Fact]
    public void UpdateStatus_ShouldNotModifyStatus_WhenStateHasNotChanged()
    {
        // Arrange
        var device = CreateSut();
        var firstTime = DateTime.UtcNow.AddMinutes(-5);
        var secondTime = DateTime.UtcNow;

        device.UpdateStatus(firstTime, atWork: true);

        // Act
        var transition = device.UpdateStatus(secondTime, atWork: true);

        // Assert
        transition.Should().Be(DeviceStatusTransition.None);
        device.LastKnownStatus.Should().BeTrue();
        // Время изменения статуса должно остаться первоначальным, так как смены состояния не произошло
        device.LastStatusChangedUtc.Should().Be(firstTime);
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateStatus_AndReturnLoggedOut_WhenDeviceGoesOffline()
    {
        // Arrange
        var device = CreateSut();
        var timeOnline = DateTime.UtcNow.AddMinutes(-10);
        var timeOffline = DateTime.UtcNow;

        device.UpdateStatus(timeOnline, atWork: true);

        // Act
        var transition = device.UpdateStatus(timeOffline, atWork: false);

        // Assert
        transition.Should().Be(DeviceStatusTransition.LoggedOut);
        device.LastKnownStatus.Should().BeFalse();
        device.LastStatusChangedUtc.Should().Be(timeOffline);
    }

    [Fact]
    public void UpdateStatus_ShouldConvertDateTimeToUtc_WhenKindIsLocalOrUnspecified()
    {
        // Arrange
        var device = CreateSut();
        var localTime = DateTime.Now;

        // Act
        device.UpdateStatus(localTime, atWork: true);

        // Assert
        device.LastStatusChangedUtc.Should().NotBeNull();
        device.LastStatusChangedUtc!.Value.Kind.Should().Be(DateTimeKind.Utc);
        device.LastStatusChangedUtc.Should().Be(localTime.ToUniversalTime());
    }
}