using PingApp.Domain.Aggregates.DeviceAggregate.Common;

namespace PingApp.Domain.Aggregates.DeviceAggregate.Entities;

public class StatusRecord
{
    public int Id { get; private set; }
    public DateTime DateTime { get; init; }
    public bool AtWork { get; init; }
    public DeviceId DeviceId { get; init; }
    public Device Device { get; init; } = null!;
}