namespace PingApp.Application.Features.Scanning.Common;

/// <summary>
/// Интерфейс моста сопряжения для передачи событий из фонового слоя приложения в поток графического интерфейса.
/// </summary>
public interface IUiEventBridge
{
    event Action<DeviceStatusChanged.Notification>? DeviceStatusChanged;
    void PublishStatusChanged(DeviceStatusChanged.Notification notification);
}