using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.Domain.Aggregates.UserAggregate.Entities;

public class UserDevice
{
    public UserId UserId { get; private set; }

    public DeviceId DeviceId { get; private set; }

    public DeviceNickname DeviceNickname { get; private set; }

    private UserDevice() { }

    public UserDevice(UserId userId, DeviceId deviceId, DeviceNickname deviceNickname)
    {
        UserId = userId;
        DeviceId = deviceId;
        DeviceNickname = deviceNickname;
    }
}