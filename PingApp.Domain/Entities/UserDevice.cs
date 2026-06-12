using PingApp.Domain.Common;

namespace PingApp.Domain.Entities;

public class UserDevice
{
    public UserId UserId { get; set; }
    public User User { get; set; } = null!;

    public DeviceId DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string? Nickname { get; set; }
}