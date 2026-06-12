using PingApp.Domain.Common;

namespace PingApp.Domain.Entities;

public class StatusRecord
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public bool AtWork { get; set; }
    public DeviceId DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}