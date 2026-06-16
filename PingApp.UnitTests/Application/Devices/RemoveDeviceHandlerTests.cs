using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Devices;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.Common;

namespace PingApp.UnitTests.Application.Devices;

public class RemoveDeviceHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly ILogger<RemoveDevice.Handler> _loggerMock;
    private readonly RemoveDevice.Handler _sut;

    public RemoveDeviceHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _loggerMock = Substitute.For<ILogger<RemoveDevice.Handler>>();

        _sut = new RemoveDevice.Handler(
            _repositoryMock,
            _userContextMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAddressIsInvalid()
    {
        var command = new RemoveDevice.Command("invalid_address#");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeviceErrors.InvalidAddress);

        await _repositoryMock.DidNotReceiveWithAnyArgs().RemoveSubscriptionAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_ShouldRemoveSubscriptionAndReturnSuccess_WhenAddressIsValid()
    {
        var ip = "192.168.1.15";
        var command = new RemoveDevice.Command(ip);
        var userId = UserId.New();

        _userContextMock.UserId.Returns(userId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _repositoryMock.Received(1).RemoveSubscriptionAsync(
            userId,
            Arg.Is<DeviceAddress>(addr => addr.Value == ip),
            Arg.Any<CancellationToken>());
    }
}