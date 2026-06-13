using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.Common;

public static class UserDeviceErrors
{
    public static readonly Error DeviceNicknameTooShort = new ValidationError(
        "UserDevice.DeviceNicknameTooShort", $"Имя пользователя должно содержать не менее {DeviceNickname.MinLength} символов.");

    public static readonly Error DeviceNicknameTooLong = new ValidationError(
        "UserDevice.DeviceNicknameTooLong", $"Имя пользователя не должно превышать {DeviceNickname.MaxLength} символов.");
}
