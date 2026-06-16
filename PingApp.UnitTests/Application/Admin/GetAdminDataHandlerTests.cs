using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Admin;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Admin;

public class GetAdminDataHandlerTests
{
    private readonly IDeviceRepository _deviceRepositoryMock;
    private readonly IGlobalSettingsRepository _settingsRepositoryMock;
    private readonly GetAdminData.Handler _sut;

    public GetAdminDataHandlerTests()
    {
        _deviceRepositoryMock = Substitute.For<IDeviceRepository>();
        _settingsRepositoryMock = Substitute.For<IGlobalSettingsRepository>();
        _sut = new GetAdminData.Handler(_deviceRepositoryMock, _settingsRepositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllDevicesAndScanInterval()
    {
        var query = new GetAdminData.Query();

        var devices = new List<Device>
        {
            Device.Create(DeviceAddress.Create("192.168.1.1").Value, isAllowedToPing: true, isVisibleToUsers: false),
            Device.Create(DeviceAddress.Create("192.168.1.2").Value, isAllowedToPing: false, isVisibleToUsers: true)
        };

        _deviceRepositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devices);

        _settingsRepositoryMock.GetSettingAsync("ScanIntervalSeconds", "10", Arg.Any<CancellationToken>())
            .Returns("15");

        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.ScanIntervalSeconds.Should().Be(15);
        result.Devices.Should().HaveCount(2);

        result.Devices[0].Address.Should().Be("192.168.1.1");
        result.Devices[0].IsAllowedToPing.Should().BeTrue();
        result.Devices[0].IsVisibleToUsers.Should().BeFalse();

        result.Devices[1].Address.Should().Be("192.168.1.2");
        result.Devices[1].IsAllowedToPing.Should().BeFalse();
        result.Devices[1].IsVisibleToUsers.Should().BeTrue();
    }
}
