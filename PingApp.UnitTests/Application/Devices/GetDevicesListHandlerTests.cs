using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Devices;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;

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

        var deviceWithStatus = new DeviceWithLastStatus(
            Address: "192.168.1.10",
            Nickname: "My Laptop",
            LastAtWork: true,
            LastStatusTime: DateTime.UtcNow
        );

        _repositoryMock.GetUserDevicesWithLastStatusAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceWithLastStatus> { deviceWithStatus });

        var result = await _sut.Handle(new GetDevicesList.Query(), TestContext.Current.CancellationToken);

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

        var offlineTime = DateTime.UtcNow.AddHours(-1);
        var deviceWithStatus = new DeviceWithLastStatus(
            Address: "192.168.1.10",
            Nickname: "My Laptop",
            LastAtWork: false,
            LastStatusTime: offlineTime
        );

        _repositoryMock.GetUserDevicesWithLastStatusAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceWithLastStatus> { deviceWithStatus });

        var result = await _sut.Handle(new GetDevicesList.Query(), TestContext.Current.CancellationToken);

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

        var deviceWithStatus = new DeviceWithLastStatus(
            Address: "192.168.1.10",
            Nickname: "My Laptop",
            LastAtWork: null,
            LastStatusTime: null
        );

        _repositoryMock.GetUserDevicesWithLastStatusAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceWithLastStatus> { deviceWithStatus });

        var result = await _sut.Handle(new GetDevicesList.Query(), TestContext.Current.CancellationToken);

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.AtWork.Should().BeFalse();
        dto.StatusString.Should().Be("Нет данных");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoSubscriptions()
    {
        var userId = UserId.New();
        _userContextMock.UserId.Returns(userId);

        _repositoryMock.GetUserDevicesWithLastStatusAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceWithLastStatus>());

        var result = await _sut.Handle(new GetDevicesList.Query(), TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}