namespace PingApp.Application.Features.Scanning.Common;

/// <summary>
/// Потокобезопасная реализация моста сопряжения.
/// </summary>
public class UiEventBridge : IUiEventBridge
{
    public event Action<DeviceStatusChanged.Notification>? DeviceStatusChanged;

    public void PublishStatusChanged(DeviceStatusChanged.Notification notification)
    {
        DeviceStatusChanged?.Invoke(notification);
    }
}