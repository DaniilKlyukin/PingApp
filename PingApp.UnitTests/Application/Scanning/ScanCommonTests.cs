using FluentAssertions;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Features.Scanning.Common;

namespace PingApp.UnitTests.Application.Scanning;

public class ScanCommonTests
{
    [Fact]
    public void ScanConfiguration_ShouldGetAndSetInterval_WithThreadSafety()
    {
        var config = new ScanConfiguration();
        var targetInterval = TimeSpan.FromSeconds(45);

        config.Interval = targetInterval;
        config.Interval.Should().Be(targetInterval);

        var tasks = new List<Task>();
        for (int i = 1; i <= 10; i++)
        {
            var intervalSec = i;
            tasks.Add(Task.Run(() =>
            {
                config.Interval = TimeSpan.FromSeconds(intervalSec);
                _ = config.Interval;
            }, TestContext.Current.CancellationToken));
        }

        Action act = () => Task.WaitAll(tasks.ToArray());
        act.Should().NotThrow("Блокировка внутри ScanConfiguration должна предотвращать гонку потоков.");
    }

    [Fact]
    public void UiEventBridge_ShouldPublishEvents_ToSubscribers()
    {
        var bridge = new UiEventBridge();
        DeviceStatusChanged.Notification? receivedNotification = null;

        bridge.DeviceStatusChanged += (notification) => receivedNotification = notification;

        var expected = new DeviceStatusChanged.Notification("192.168.1.1", true, DateTime.UtcNow);

        bridge.PublishStatusChanged(expected);

        receivedNotification.Should().NotBeNull();
        receivedNotification!.Address.Should().Be("192.168.1.1");
        receivedNotification.AtWork.Should().BeTrue();
    }
}
