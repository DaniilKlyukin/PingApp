using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Interfaces;
using PingApp.Worker.BackgroundServices;

namespace PingApp.UnitTests.Application.Scanning;

public class DeviceScanBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldSendScanCommand_AndLoadIntervalFromSettings()
    {
        var scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        var scopeMock = Substitute.For<IServiceScope>();
        var serviceProviderMock = Substitute.For<IServiceProvider>();

        var mediatorMock = Substitute.For<IMediator>();
        var settingsRepoMock = Substitute.For<IGlobalSettingsRepository>();
        var configMock = Substitute.For<IScanConfiguration>();
        var loggerMock = Substitute.For<ILogger<DeviceScanBackgroundService>>();

        scopeFactoryMock.CreateScope().Returns(scopeMock);
        scopeMock.ServiceProvider.Returns(serviceProviderMock);

        serviceProviderMock.GetService(typeof(IMediator)).Returns(mediatorMock);
        serviceProviderMock.GetService(typeof(IGlobalSettingsRepository)).Returns(settingsRepoMock);

        settingsRepoMock.GetSettingAsync("ScanIntervalSeconds", "10", Arg.Any<CancellationToken>())
            .Returns("5");

        configMock.Interval.Returns(TimeSpan.FromMilliseconds(50));

        var sut = new DeviceScanBackgroundService(scopeFactoryMock, configMock, loggerMock);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        var runTask = sut.StartAsync(linkedCts.Token);
        await Task.Delay(150, TestContext.Current.CancellationToken);
        linkedCts.Cancel();
        await runTask;

        await mediatorMock.Received().Send(Arg.Any<ScanAllDevices.Command>(), Arg.Any<CancellationToken>());
        configMock.Received().Interval = TimeSpan.FromSeconds(5);
    }
}