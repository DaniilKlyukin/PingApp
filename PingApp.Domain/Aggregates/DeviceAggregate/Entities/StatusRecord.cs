using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;

namespace PingApp.Domain.Aggregates.DeviceAggregate.Entities;

public class StatusRecord
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public bool AtWork { get; set; }
    public DeviceId DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}