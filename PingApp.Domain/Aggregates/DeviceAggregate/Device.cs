using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.Enums;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.DeviceAggregate;

public class Device : IAggregateRoot
{
    public DeviceId Id { get; private set; } = DeviceId.New();
    public DeviceAddress Address { get; private set; }
    public bool IsAllowedToPing { get; set; } = true;
    public bool IsVisibleToUsers { get; set; } = false;

    public bool? LastKnownStatus { get; private set; }
    public DateTime? LastStatusChangedUtc { get; private set; }

    private readonly List<UserDevice> _userDevices = [];
    public IReadOnlyCollection<UserDevice> UserDevices => _userDevices.AsReadOnly();

    private Device() { }

    public static Device Create(DeviceAddress address, bool isAllowedToPing = true, bool isVisibleToUsers = false)
    {
        return new Device
        {
            Id = DeviceId.New(),
            Address = address,
            IsAllowedToPing = isAllowedToPing,
            IsVisibleToUsers = isVisibleToUsers
        };
    }

    public DeviceStatusTransition UpdateStatus(DateTime dateTime, bool atWork)
    {
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

        if (LastKnownStatus == null)
        {
            LastKnownStatus = atWork;
            LastStatusChangedUtc = utcDateTime;
            return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.None;
        }

        if (LastKnownStatus == atWork)
        {
            return DeviceStatusTransition.None;
        }

        LastKnownStatus = atWork;
        LastStatusChangedUtc = utcDateTime;

        return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.LoggedOut;
    }
}