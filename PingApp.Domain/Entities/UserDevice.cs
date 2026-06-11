namespace PingApp.Domain.Entities;

public class UserDevice
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string? Nickname { get; set; }
}