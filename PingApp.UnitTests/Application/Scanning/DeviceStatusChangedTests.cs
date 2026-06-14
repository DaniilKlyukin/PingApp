using NSubstitute;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Features.Scanning.Common;

namespace PingApp.UnitTests.Application.Scanning;

public class DeviceStatusChangedTests
{
    [Fact]
    public async Task Handle_ShouldPublishToUiBridge_WhenNotificationIsReceived()
    {
        var uiEventBridgeMock = Substitute.For<IUiEventBridge>();
        var handler = new DeviceStatusChanged.UiBridgeHandler(uiEventBridgeMock);

        var notification = new DeviceStatusChanged.Notification(
            Address: "192.168.1.50",
            AtWork: true,
            DateTime: DateTime.UtcNow);

        await handler.Handle(notification, CancellationToken.None);

        uiEventBridgeMock.Received(1).PublishStatusChanged(notification);
    }
}