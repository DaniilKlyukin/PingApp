using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Devices;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Devices;

public class AddDeviceHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly ILogger<AddDevice.Handler> _loggerMock;
    private readonly AddDevice.Handler _sut;

    public AddDeviceHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _loggerMock = Substitute.For<ILogger<AddDevice.Handler>>();

        _sut = new AddDevice.Handler(
            _repositoryMock,
            _userContextMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAddressIsInvalid()
    {
        var command = new AddDevice.Command("invalid-ip-address#", "My PC");

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();

        await _repositoryMock.DidNotReceiveWithAnyArgs().GetByAddressAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeviceDoesNotExistInDatabase()
    {
        var ip = "192.168.1.15";
        var command = new AddDevice.Command(ip, "My PC");
        var address = DeviceAddress.Create(ip).Value;

        _repositoryMock.GetByAddressAsync(address, Arg.Any<CancellationToken>())
            .Returns((Device?)null);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeviceIsHiddenFromUsersByAdmin()
    {
        var ip = "192.168.1.15";
        var command = new AddDevice.Command(ip, "My PC");
        var address = DeviceAddress.Create(ip).Value;

        var deviceInDb = Device.Create(
            address,
            isAllowedToPing: true,
            isVisibleToUsers: false);

        _repositoryMock.GetByAddressAsync(address, Arg.Any<CancellationToken>())
            .Returns(deviceInDb);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeviceErrors.NotFound);

        await _repositoryMock.DidNotReceiveWithAnyArgs().AddSubscriptionAsync(default!, default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldAddSubscriptionAndReturnSuccess_WhenDeviceIsValidAndAllowed()
    {
        var ip = "192.168.1.15";
        var command = new AddDevice.Command(ip, "My PC");
        var address = DeviceAddress.Create(ip).Value;
        var userId = UserId.New();

        var deviceInDb = Device.Create(
            address,
            isAllowedToPing: true,
            isVisibleToUsers: true);

        _userContextMock.UserId.Returns(userId);

        _repositoryMock.GetByAddressAsync(address, Arg.Any<CancellationToken>())
            .Returns(deviceInDb);

        _repositoryMock.ExistsSubscriptionAsync(userId, address, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        await _repositoryMock.Received(1).AddSubscriptionAsync(
            userId,
            deviceInDb,
            DeviceNickname.Create("My PC").Value,
            Arg.Any<CancellationToken>());
    }
}
