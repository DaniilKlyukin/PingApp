using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Devices;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.UnitTests.Application.Devices;

public class GetAllowedDevicesHandlerTests
{
    private readonly IDeviceRepository _repositoryMock;
    private readonly GetAllowedDevices.Handler _sut;

    public GetAllowedDevicesHandlerTests()
    {
        _repositoryMock = Substitute.For<IDeviceRepository>();
        _sut = new GetAllowedDevices.Handler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllowedDeviceAddresses_WhenDevicesExist()
    {
        var query = new GetAllowedDevices.Query();

        var devices = new List<Device>
        {
            Device.Create(DeviceAddress.Create("192.168.1.1").Value, isAllowedToPing: true, isVisibleToUsers: true),
            Device.Create(DeviceAddress.Create("192.168.1.2").Value, isAllowedToPing: true, isVisibleToUsers: true),
            Device.Create(DeviceAddress.Create("example.com").Value, isAllowedToPing: true, isVisibleToUsers: true)
        };

        _repositoryMock.GetAllowedDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devices);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().ContainInOrder("192.168.1.1", "192.168.1.2", "example.com");

        await _repositoryMock.Received(1).GetAllowedDevicesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAllowedDevicesExist()
    {
        var query = new GetAllowedDevices.Query();

        _repositoryMock.GetAllowedDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Device>());

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        await _repositoryMock.Received(1).GetAllowedDevicesAsync(Arg.Any<CancellationToken>());
    }
}
