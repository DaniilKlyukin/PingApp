namespace PingApp.Domain.Common;

public readonly record struct DeviceId(Guid Value)
{
    public static DeviceId New() => new(Guid.NewGuid());
    public static DeviceId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}