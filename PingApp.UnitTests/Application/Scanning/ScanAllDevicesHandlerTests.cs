using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Scanning;

public class ScanAllDevicesHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly IPingService _pingServiceMock;
    private readonly IMediator _mediatorMock;
    private readonly ILogger<ScanAllDevices.Handler> _loggerMock;
    private readonly ScanAllDevices.Handler _sut;

    public ScanAllDevicesHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _pingServiceMock = Substitute.For<IPingService>();
        _mediatorMock = Substitute.For<IMediator>();
        _loggerMock = Substitute.For<ILogger<ScanAllDevices.Handler>>();

        _sut = new ScanAllDevices.Handler(
            _repositoryMock,
            _pingServiceMock,
            _mediatorMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatusAndPublishNotification_WhenDeviceStatusChangesToOnline()
    {
        var ip = "192.168.1.100";
        var device = Device.Create(DeviceAddress.Create(ip).Value, isAllowedToPing: true, isVisibleToUsers: true);

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        _pingServiceMock.PingHostAsync(ip, Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.Handle(new ScanAllDevices.Command(), CancellationToken.None);

        device.Statuses.Should().HaveCount(1);
        device.Statuses.Single().AtWork.Should().BeTrue();

        await _repositoryMock.Received(1).UpdateAsync(device, Arg.Any<CancellationToken>());

        await _mediatorMock.Received(1).Publish(
            Arg.Is<DeviceStatusChanged.Notification>(n => n.Address == ip && n.AtWork == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotPublishNotification_WhenDeviceStatusIsUnchanged()
    {
        var ip = "192.168.1.100";
        var device = Device.Create(DeviceAddress.Create(ip).Value, isAllowedToPing: true, isVisibleToUsers: true);
        device.AddStatus(DateTime.UtcNow.AddMinutes(-5), atWork: true); // Устройство уже было онлайн

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        _pingServiceMock.PingHostAsync(ip, Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.Handle(new ScanAllDevices.Command(), CancellationToken.None);

        device.Statuses.Should().HaveCount(1);

        await _repositoryMock.Received(1).UpdateAsync(device, Arg.Any<CancellationToken>());
        await _mediatorMock.DidNotReceiveWithAnyArgs().Publish(default!, default);
    }
}
