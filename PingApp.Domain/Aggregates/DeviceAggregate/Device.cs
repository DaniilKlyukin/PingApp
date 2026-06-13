using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;
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

    private readonly List<StatusRecord> _statuses = [];
    private readonly List<UserDevice> _userDevices = [];

    public IReadOnlyCollection<StatusRecord> Statuses => _statuses.AsReadOnly();
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

    public DeviceStatusTransition AddStatus(DateTime dateTime, bool atWork)
    {
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

        var newStatus = new StatusRecord
        {
            DateTime = utcDateTime,
            AtWork = atWork,
            DeviceId = Id,
            Device = this
        };

        if (_statuses.Count == 0)
        {
            _statuses.Add(newStatus);
            return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.None;
        }

        var lastStatus = _statuses[^1];

        if (lastStatus.AtWork == atWork)
        {
            return DeviceStatusTransition.None;
        }

        _statuses.Add(newStatus);
        return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.LoggedOut;
    }
}