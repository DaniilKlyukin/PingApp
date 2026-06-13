using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.DeviceAggregate.Common;

public static class DeviceErrors
{
    public static readonly Error InvalidAddress = new ValidationError(
        "Device.InvalidAddress", "Некорректный IP-адрес или доменное имя.");

    public static readonly Error EmptyAddress = new ValidationError(
        "Device.EmptyAddress", "Адрес устройства не должен быть пустым.");

    public static readonly Error NotFound = new NotFoundError(
        "Device.NotFound", "Данное устройство еще не обнаружено в сети.");

    public static readonly Error NotAllowedToPing = new DomainError(
        "Device.TrackingRestricted", "Отслеживание этого устройства ограничено администратором.");

    public static readonly Error AlreadySubscribed = new DomainError(
        "Device.AlreadySubscribed", "Вы уже отслеживаете это устройство.");
}
