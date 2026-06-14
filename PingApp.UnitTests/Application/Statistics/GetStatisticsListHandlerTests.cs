using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Statistics;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Statistics;

public class GetStatisticsListHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly GetStatisticsList.Handler _sut;

    public GetStatisticsListHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _sut = new GetStatisticsList.Handler(_repositoryMock, _userContextMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserStatisticsWithStatuses_WhenSubscriptionsExist()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        var device = Device.Create(DeviceAddress.Create("192.168.1.50").Value, isAllowedToPing: true, isVisibleToUsers: true);

        var time1 = DateTime.UtcNow.AddMinutes(-10);
        var time2 = DateTime.UtcNow;
        device.AddStatus(time1, atWork: true);
        device.AddStatus(time2, atWork: false);

        var userDevice = new UserDevice(userId, device.Id, DeviceNickname.Create("Main Router").Value);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice> { userDevice });

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { device });

        var result = await _sut.Handle(new GetStatisticsList.Query(), CancellationToken.None);

        result.Should().HaveCount(1);

        var stats = result.Single();
        stats.Address.Should().Be("192.168.1.50");
        stats.Nickname.Should().Be("Main Router");

        stats.Statuses.Should().HaveCount(2);

        stats.Statuses[0].DateTime.Should().Be(time1);
        stats.Statuses[0].AtWork.Should().BeTrue();

        stats.Statuses[1].DateTime.Should().Be(time2);
        stats.Statuses[1].AtWork.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFilterOutInvisibleOrNotAllowedDevices()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        var hiddenDevice = Device.Create(DeviceAddress.Create("192.168.1.1").Value, isAllowedToPing: true, isVisibleToUsers: false);
        var userDeviceHidden = new UserDevice(userId, hiddenDevice.Id, DeviceNickname.Create("Hidden").Value);

        var forbiddenDevice = Device.Create(DeviceAddress.Create("192.168.1.2").Value, isAllowedToPing: false, isVisibleToUsers: true);
        var userDeviceForbidden = new UserDevice(userId, forbiddenDevice.Id, DeviceNickname.Create("Forbidden").Value);

        var validDevice = Device.Create(DeviceAddress.Create("192.168.1.3").Value, isAllowedToPing: true, isVisibleToUsers: true);
        var userDeviceValid = new UserDevice(userId, validDevice.Id, DeviceNickname.Create("Valid").Value);

        _repositoryMock.GetUserDevicesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDevice> { userDeviceHidden, userDeviceForbidden, userDeviceValid });

        _repositoryMock.GetAllDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device> { hiddenDevice, forbiddenDevice, validDevice });

        var result = await _sut.Handle(new GetStatisticsList.Query(), CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().Address.Should().Be("192.168.1.3");
        result.Single().Nickname.Should().Be("Valid");
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

        var result = await _sut.Handle(new GetStatisticsList.Query(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
