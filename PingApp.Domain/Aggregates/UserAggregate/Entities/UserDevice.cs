using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.Entities;

public class UserDevice
{
    public UserId UserId { get; set; }
    public User User { get; set; } = null!;

    public DeviceId DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string? Nickname { get; set; }
}