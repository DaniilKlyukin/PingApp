using MediatR;
using PingApp.Application.Features.Scanning.Common;

namespace PingApp.Application.Features.Scanning;

/// <summary>
/// Реакция системы на изменение статуса устройства.
/// </summary>
public static class DeviceStatusChanged
{
    public record Notification(
        string Address,
        bool AtWork,
        DateTime DateTime) : INotification;

    public class UiBridgeHandler : INotificationHandler<Notification>
    {
        private readonly IUiEventBridge _bridge;

        public UiBridgeHandler(IUiEventBridge bridge)
        {
            _bridge = bridge;
        }

        public Task Handle(Notification notification, CancellationToken cancellationToken)
        {
            _bridge.PublishStatusChanged(notification);
            return Task.CompletedTask;
        }
    }
}