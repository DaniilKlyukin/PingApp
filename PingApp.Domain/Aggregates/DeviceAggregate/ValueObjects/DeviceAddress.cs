using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Common;
using System.Net;

namespace PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

public class DeviceAddress : ValueObject<string>
{
    private DeviceAddress(string value) : base(value)
    {
    }

    public static Result<DeviceAddress> Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return DeviceErrors.EmptyAddress;

        var trimmed = address.Trim();

        if (IPAddress.TryParse(trimmed, out _))
        {
            return Result.Success(new DeviceAddress(trimmed));
        }

        var hostType = Uri.CheckHostName(trimmed);
        if (hostType == UriHostNameType.Dns)
        {
            var isAllDigitsAndDots = trimmed.All(c => char.IsDigit(c) || c == '.');
            if (isAllDigitsAndDots)
            {
                return DeviceErrors.InvalidAddress;
            }

            return Result.Success(new DeviceAddress(trimmed));
        }

        return DeviceErrors.InvalidAddress;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}
