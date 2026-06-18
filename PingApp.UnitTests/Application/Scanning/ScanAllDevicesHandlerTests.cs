using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
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
    private readonly FakeLocalNetworkProvider _networkProviderFake;
    private readonly IMediator _mediatorMock;
    private readonly ILogger<ScanAllDevices.Handler> _loggerMock;
    private readonly IConfiguration _configurationMock;
    private readonly ScanAllDevices.Handler _sut;

    public ScanAllDevicesHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _pingServiceMock = Substitute.For<IPingService>();
        _networkProviderFake = new FakeLocalNetworkProvider();
        _mediatorMock = Substitute.For<IMediator>();
        _loggerMock = Substitute.For<ILogger<ScanAllDevices.Handler>>();
        _configurationMock = Substitute.For<IConfiguration>();

        _sut = new ScanAllDevices.Handler(
            _repositoryMock,
            _pingServiceMock,
            _networkProviderFake,
            _mediatorMock,
            _configurationMock,
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

        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        device.LastKnownStatus.Should().BeTrue();

        await _repositoryMock.Received(1).UpdateAsync(device, Arg.Any<CancellationToken>());

        await _repositoryMock.Received(1).AddStatusRecordAsync(
            Arg.Is<StatusRecord>(r => r.DeviceId == device.Id && r.AtWork == true),
            Arg.Any<CancellationToken>());

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
        _networkProviderFake.LocalIpAddress = IPAddress.Parse("192.168.1.5");
        _networkProviderFake.ActiveIpChecker = ip => ip == "192.168.1.10";

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device>());

        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        await _repositoryMock.Received(1).AddDevicesRangeAsync(
            Arg.Is<IEnumerable<Device>>(devices =>
                devices.Count() == 1 &&
                devices.First().Address.Value == "192.168.1.10" &&
                !devices.First().IsVisibleToUsers &&
                devices.First().IsAllowedToPing),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotAddDevice_WhenAlreadyExistsInDatabase()
    {
        _networkProviderFake.LocalIpAddress = IPAddress.Parse("192.168.1.5");
        _networkProviderFake.ActiveIpChecker = ip => ip == "192.168.1.10";

        var existingDevice = Device.Create(DeviceAddress.Create("192.168.1.10").Value);

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { existingDevice });

        await _sut.Handle(new ScanAllDevices.Command(), TestContext.Current.CancellationToken);

        await _repositoryMock.DidNotReceive().AddDevicesRangeAsync(
            Arg.Any<IEnumerable<Device>>(),
            Arg.Any<CancellationToken>());
    }

    #region Потокобезопасный ручной Stub для сетевого провайдера

    private class FakeLocalNetworkProvider : ILocalNetworkProvider
    {
        public IPAddress? LocalIpAddress { get; set; }
        public Func<string, bool>? ActiveIpChecker { get; set; }

        public IPAddress? GetLocalIpAddress() => LocalIpAddress;

        public Task<(string Ip, bool IsActive)> PingHostForDiscoveryAsync(string ip, CancellationToken cancellationToken)
        {
            var isActive = ActiveIpChecker?.Invoke(ip) ?? false;
            return Task.FromResult((ip, isActive));
        }
    }

    #endregion
}