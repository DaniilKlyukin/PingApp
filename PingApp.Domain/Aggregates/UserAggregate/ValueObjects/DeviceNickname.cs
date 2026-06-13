using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

public class DeviceNickname : ValueObject<string?>
{
    public const int MinLength = 1;
    public const int MaxLength = 50;

    private DeviceNickname(string? value) : base(value) { }

    public static Result<DeviceNickname> Create(string? nickname)
    {
        if (nickname == null)
            return new DeviceNickname(null);

        nickname = nickname.Trim();

        if (nickname.Length < MinLength)
            return UserDeviceErrors.DeviceNicknameTooShort;

        if (nickname.Length > MaxLength)
            return UserDeviceErrors.DeviceNicknameTooLong;

        return new DeviceNickname(nickname);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}