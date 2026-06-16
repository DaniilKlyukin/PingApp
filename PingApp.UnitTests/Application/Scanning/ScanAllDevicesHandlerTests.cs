using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using System.Net;

namespace PingApp.UnitTests.Application.Scanning;

public class ScanAllDevicesHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly IPingService _pingServiceMock;
    private readonly ILocalNetworkProvider _networkProviderMock;
    private readonly IMediator _mediatorMock;
    private readonly ILogger<ScanAllDevices.Handler> _loggerMock;
    private readonly ScanAllDevices.Handler _sut;

    public ScanAllDevicesHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _pingServiceMock = Substitute.For<IPingService>();
        _networkProviderMock = Substitute.For<ILocalNetworkProvider>();
        _mediatorMock = Substitute.For<IMediator>();
        _loggerMock = Substitute.For<ILogger<ScanAllDevices.Handler>>();

        _sut = new ScanAllDevices.Handler(
            _repositoryMock,
            _pingServiceMock,
            _networkProviderMock,
            _mediatorMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatusAndPublishNotification_WhenDeviceStatusChangesToOnline()
    {
        // Arrange
        var ip = "192.168.1.100";
        var device = Device.Create(DeviceAddress.Create(ip).Value, isAllowedToPing: true, isVisibleToUsers: true);

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        _pingServiceMock.PingHostAsync(ip, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        // Assert
        // 1. Проверяем, что состояние самого устройства обновилось в памяти
        device.LastKnownStatus.Should().BeTrue();

        // 2. Проверяем, что агрегат устройства был сохранен в базу
        await _repositoryMock.Received(1).UpdateAsync(device, Arg.Any<CancellationToken>());

        // 3. Проверяем, что в базу была записана новая строка истории статуса (append-only лог)
        await _repositoryMock.Received(1).AddStatusRecordAsync(
            Arg.Is<StatusRecord>(r => r.DeviceId == device.Id && r.AtWork == true),
            Arg.Any<CancellationToken>());

        // 4. Проверяем публикацию события в шину MediatR
        await _mediatorMock.Received(1).Publish(
            Arg.Is<DeviceStatusChanged.Notification>(n => n.Address == ip && n.AtWork == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotPublishNotification_WhenDeviceStatusIsUnchanged()
    {
        var ip = "192.168.1.100";
        var device = Device.Create(DeviceAddress.Create(ip).Value, isAllowedToPing: true, isVisibleToUsers: true);

        device.UpdateStatus(DateTime.UtcNow.AddMinutes(-5), atWork: true);

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        _pingServiceMock.PingHostAsync(ip, Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        device.LastKnownStatus.Should().BeTrue();

        await _repositoryMock.Received(1).UpdateAsync(device, Arg.Any<CancellationToken>());

        await _repositoryMock.DidNotReceive().AddStatusRecordAsync(
            Arg.Any<StatusRecord>(),
            Arg.Any<CancellationToken>());

        await _mediatorMock.DidNotReceiveWithAnyArgs().Publish(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldDiscoverNewDevicesAndSaveThem_WhenLocalIpIsFound()
    {
        var localIp = IPAddress.Parse("192.168.1.5");
        _networkProviderMock.GetLocalIpAddress().Returns(localIp);

        _networkProviderMock.PingHostForDiscoveryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var ip = callInfo.Arg<string>();
                return Task.FromResult((ip, ip == "192.168.1.10"));
            });

        _repositoryMock.GetByAddressAsync(Arg.Is<DeviceAddress>(a => a.Value == "192.168.1.10"), Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device>());

        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        await _repositoryMock.Received(1).AddDeviceAsync(
            Arg.Is<Device>(d => d.Address.Value == "192.168.1.10" && !d.IsVisibleToUsers && d.IsAllowedToPing),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotAddDevice_WhenAlreadyExistsInDatabase()
    {
        var localIp = IPAddress.Parse("192.168.1.5");
        _networkProviderMock.GetLocalIpAddress().Returns(localIp);

        _networkProviderMock.PingHostForDiscoveryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => (callInfo.Arg<string>(), callInfo.Arg<string>() == "192.168.1.10"));

        var existingDevice = Device.Create(DeviceAddress.Create("192.168.1.10").Value);
        _repositoryMock.GetByAddressAsync(existingDevice.Address, Arg.Any<CancellationToken>())
            .Returns(existingDevice);

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { existingDevice });

        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        await _repositoryMock.DidNotReceive().AddDeviceAsync(Arg.Any<Device>(), Arg.Any<CancellationToken>());
    }
}
