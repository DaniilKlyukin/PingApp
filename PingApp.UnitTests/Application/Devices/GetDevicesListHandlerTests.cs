using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Devices;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Devices;

public class GetDevicesListHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly GetDevicesList.Handler _sut;

    public GetDevicesListHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _sut = new GetDevicesList.Handler(_repositoryMock, _userContextMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnDevicesWithOnlineStatus_WhenDeviceIsOnline()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        var device = Device.Create(DeviceAddress.Create("192.168.1.10").Value, isAllowedToPing: true, isVisibleToUsers: true);
        device.AddStatus(DateTime.UtcNow, atWork: true); // Добавляем статус "В сети"

        var userDevice = new UserDevice(userId, device.Id, DeviceNickname.Create("My Laptop").Value);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice> { userDevice });

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        var result = await _sut.Handle(new GetDevicesList.Query(), CancellationToken.None);

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.Address.Should().Be("192.168.1.10");
        dto.Nickname.Should().Be("My Laptop");
        dto.AtWork.Should().BeTrue();
        dto.StatusString.Should().Be("В сети");
    }

    [Fact]
    public async Task Handle_ShouldReturnDevicesWithOfflineStatus_WhenDeviceIsOffline()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        var device = Device.Create(DeviceAddress.Create("192.168.1.10").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var offlineTime = DateTime.UtcNow.AddHours(-1);
        device.AddStatus(offlineTime, atWork: false);

        var userDevice = new UserDevice(userId, device.Id, DeviceNickname.Create("My Laptop").Value);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice> { userDevice });

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        var result = await _sut.Handle(new GetDevicesList.Query(), CancellationToken.None);

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.AtWork.Should().BeFalse();
        dto.StatusString.Should().Be($"Не в сети (с {offlineTime.ToLocalTime():g})");
    }

    [Fact]
    public async Task Handle_ShouldReturnNoDataStatus_WhenDeviceHasNoStatuses()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        var device = Device.Create(DeviceAddress.Create("192.168.1.10").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var userDevice = new UserDevice(userId, device.Id, DeviceNickname.Create("My Laptop").Value);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice> { userDevice });

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        var result = await _sut.Handle(new GetDevicesList.Query(), CancellationToken.None);

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.AtWork.Should().BeFalse();
        dto.StatusString.Should().Be("Нет данных");
    }

    [Fact]
    public async Task Handle_ShouldFilterOutDevices_WhenTheyAreHiddenOrNotAllowedToPing()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        var hiddenDevice = Device.Create(DeviceAddress.Create("192.168.1.10").Value, isAllowedToPing: true, isVisibleToUsers: false);
        var userDeviceHidden = new UserDevice(userId, hiddenDevice.Id, DeviceNickname.Create("Hidden").Value);

        var forbiddenDevice = Device.Create(DeviceAddress.Create("192.168.1.20").Value, isAllowedToPing: false, isVisibleToUsers: true);
        var userDeviceForbidden = new UserDevice(userId, forbiddenDevice.Id, DeviceNickname.Create("Forbidden").Value);

        var validDevice = Device.Create(DeviceAddress.Create("192.168.1.30").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var userDeviceValid = new UserDevice(userId, validDevice.Id, DeviceNickname.Create("Valid").Value);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice> { userDeviceHidden, userDeviceForbidden, userDeviceValid });

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { hiddenDevice, forbiddenDevice, validDevice });

        var result = await _sut.Handle(new GetDevicesList.Query(), CancellationToken.None);

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.Address.Should().Be("192.168.1.30");
        dto.Nickname.Should().Be("Valid");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoSubscriptions()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice>());

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device>());

        var result = await _sut.Handle(new GetDevicesList.Query(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
