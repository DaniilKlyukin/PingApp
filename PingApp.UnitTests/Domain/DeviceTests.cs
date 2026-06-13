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
    public void AddStatus_ShouldAddFirstStatus_AndReturnLoggedIn_WhenDeviceIsOnline()
    {
        var device = CreateSut();
        var time = DateTime.UtcNow;

        var transition = device.AddStatus(time, atWork: true);

        transition.Should().Be(DeviceStatusTransition.LoggedIn);
        device.Statuses.Should().HaveCount(1);

        var status = device.Statuses.Single();
        status.AtWork.Should().BeTrue();
        status.DateTime.Should().Be(time);
    }

    [Fact]
    public void AddStatus_ShouldAddFirstStatus_AndReturnNone_WhenDeviceIsOffline()
    {
        var device = CreateSut();
        var time = DateTime.UtcNow;

        var transition = device.AddStatus(time, atWork: false);

        transition.Should().Be(DeviceStatusTransition.None);
        device.Statuses.Should().HaveCount(1);
        device.Statuses.Single().AtWork.Should().BeFalse();
    }

    [Fact]
    public void AddStatus_ShouldNotAddDuplicateStatus_WhenStateHasNotChanged()
    {
        var device = CreateSut();
        var firstTime = DateTime.UtcNow.AddMinutes(-5);
        var secondTime = DateTime.UtcNow;

        device.AddStatus(firstTime, atWork: true);

        var transition = device.AddStatus(secondTime, atWork: true);

        transition.Should().Be(DeviceStatusTransition.None);
        device.Statuses.Should().HaveCount(1);
        device.Statuses.Single().DateTime.Should().Be(firstTime);
    }

    [Fact]
    public void AddStatus_ShouldAddNewStatus_AndReturnLoggedOut_WhenDeviceGoesOffline()
    {
        var device = CreateSut();
        var timeOnline = DateTime.UtcNow.AddMinutes(-10);
        var timeOffline = DateTime.UtcNow;

        device.AddStatus(timeOnline, atWork: true);

        var transition = device.AddStatus(timeOffline, atWork: false);

        transition.Should().Be(DeviceStatusTransition.LoggedOut);
        device.Statuses.Should().HaveCount(2);
        device.Statuses.Last().AtWork.Should().BeFalse();
        device.Statuses.Last().DateTime.Should().Be(timeOffline);
    }

    [Fact]
    public void AddStatus_ShouldConvertDateTimeToUtc_WhenKindIsLocalOrUnspecified()
    {
        var device = CreateSut();
        var localTime = DateTime.Now;

        device.AddStatus(localTime, atWork: true);

        device.Statuses.Single().DateTime.Kind.Should().Be(DateTimeKind.Utc);
        device.Statuses.Single().DateTime.Should().Be(localTime.ToUniversalTime());
    }
}