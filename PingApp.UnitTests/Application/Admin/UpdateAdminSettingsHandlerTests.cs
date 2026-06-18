using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Admin;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Admin;

public class UpdateAdminSettingsHandlerTests
{
    private readonly IDeviceRepository _deviceRepositoryMock;
    private readonly IGlobalSettingsRepository _settingsRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<UpdateAdminSettings.Handler> _loggerMock;
    private readonly UpdateAdminSettings.Handler _sut;

    public UpdateAdminSettingsHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<UpdateAdminSettings.Handler>>();
        _deviceRepositoryMock = Substitute.For<IDeviceRepository>();
        _settingsRepositoryMock = Substitute.For<IGlobalSettingsRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        _sut = new UpdateAdminSettings.Handler(
            _deviceRepositoryMock,
            _settingsRepositoryMock,
            _unitOfWorkMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldUpdateSpecificDevices_WhenIndividualTogglesAreProvided()
    {
        var device1 = Device.Create(DeviceAddress.Create("192.168.1.1").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var device2 = Device.Create(DeviceAddress.Create("192.168.1.2").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var devicesInDb = new List<Device> { device1, device2 };

        _deviceRepositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devicesInDb);

        var toggles = new List<UpdateAdminSettings.DeviceToggleDto>
        {
            new("192.168.1.1", IsAllowedToPing: false, IsVisibleToUsers: false)
        };

        var command = new UpdateAdminSettings.Command(toggles, ScanIntervalSeconds: 30);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.Should().Be(Unit.Value);

        await _settingsRepositoryMock.Received(1).SaveSettingAsync("ScanIntervalSeconds", "30", Arg.Any<CancellationToken>());

        device1.IsAllowedToPing.Should().BeFalse();
        device1.IsVisibleToUsers.Should().BeFalse();

        device2.IsAllowedToPing.Should().BeTrue();
        device2.IsVisibleToUsers.Should().BeTrue();

        _deviceRepositoryMock.Received(1).Update(device1);
        _deviceRepositoryMock.DidNotReceive().Update(device2);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDenyPingForAllDevices_WhenBulkActionIsDenyAllPing()
    {
        var device1 = Device.Create(DeviceAddress.Create("192.168.1.1").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var device2 = Device.Create(DeviceAddress.Create("192.168.1.2").Value, isAllowedToPing: true, isVisibleToUsers: false);
        var devicesInDb = new List<Device> { device1, device2 };

        _deviceRepositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devicesInDb);

        var command = new UpdateAdminSettings.Command(
            Toggles: new List<UpdateAdminSettings.DeviceToggleDto>(),
            ScanIntervalSeconds: 10,
            BulkAction: UpdateAdminSettings.BulkActionType.DenyAllPing);

        await _sut.Handle(command, TestContext.Current.CancellationToken);

        device1.IsAllowedToPing.Should().BeFalse();
        device2.IsAllowedToPing.Should().BeFalse();

        _deviceRepositoryMock.Received(1).Update(device1);
        _deviceRepositoryMock.Received(1).Update(device2);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMakeAllDevicesVisible_WhenBulkActionIsAllowAllVisible()
    {
        var device1 = Device.Create(DeviceAddress.Create("192.168.1.1").Value, isAllowedToPing: true, isVisibleToUsers: false);
        var device2 = Device.Create(DeviceAddress.Create("192.168.1.2").Value, isAllowedToPing: false, isVisibleToUsers: false);
        var devicesInDb = new List<Device> { device1, device2 };

        _deviceRepositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devicesInDb);

        var command = new UpdateAdminSettings.Command(
            Toggles: new List<UpdateAdminSettings.DeviceToggleDto>(),
            ScanIntervalSeconds: 10,
            BulkAction: UpdateAdminSettings.BulkActionType.AllowAllVisible);

        await _sut.Handle(command, TestContext.Current.CancellationToken);

        device1.IsVisibleToUsers.Should().BeTrue();
        device2.IsVisibleToUsers.Should().BeTrue();

        _deviceRepositoryMock.Received(1).Update(device1);
        _deviceRepositoryMock.Received(1).Update(device2);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}