using PingApp.Domain.Common;

namespace PingApp.Domain.ValueObjects;

public class DeviceAddress : ValueObject<string>
{
    private DeviceAddress(string value) : base(value)
    {
    }

    public static Result<DeviceAddress> Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure<DeviceAddress>("Адрес устройства не должен быть пустым.");

        var trimmed = address.Trim();
        var hostType = Uri.CheckHostName(trimmed);

        if (hostType is not (UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6))
            return Result.Failure<DeviceAddress>("Некорректный IP-адрес или доменное имя.");

        return Result.Success(new DeviceAddress(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}
