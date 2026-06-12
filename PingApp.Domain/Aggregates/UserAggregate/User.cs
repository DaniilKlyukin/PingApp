using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate;

public class User : IAggregateRoot
{
    public UserId Id { get; set; } = UserId.New();
    public required Username Username { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsGuest { get; set; }
    public bool IsAdmin { get; set; }
    public List<UserDevice> UserDevices { get; set; } = [];

    public void AddSubscription(DeviceId deviceId, string? nickname)
    {
        if (UserDevices.Any(ud => ud.DeviceId == deviceId))
            return;

        UserDevices.Add(new UserDevice
        {
            UserId = Id,
            DeviceId = deviceId,
            Nickname = nickname
        });
    }

    public void RemoveSubscription(DeviceId deviceId)
    {
        var subscription = UserDevices.FirstOrDefault(ud => ud.DeviceId == deviceId);
        if (subscription != null)
        {
            UserDevices.Remove(subscription);
        }
    }
}