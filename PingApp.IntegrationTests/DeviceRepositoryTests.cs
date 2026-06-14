using Microsoft.EntityFrameworkCore;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Infrastructure.Data;
using PingApp.Infrastructure.Repositories;
using FluentAssertions;

namespace PingApp.IntegrationTests;

public class DeviceRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DbContextOptions<PingDbContext> _contextOptions;

    public DeviceRepositoryTests(DatabaseFixture fixture)
    {
        _contextOptions = new DbContextOptionsBuilder<PingDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
    }

    [Fact]
    public async Task AddDeviceAsync_ShouldPersistDeviceInRealPostgres()
    {
        using var context = new PingDbContext(_contextOptions);
        var repository = new DeviceRepository(context);
        var device = Device.Create(DeviceAddress.Create("192.168.1.55").Value);

        await repository.AddDeviceAsync(device);

        using var checkContext = new PingDbContext(_contextOptions);
        var dbDevice = await checkContext.Devices.FirstOrDefaultAsync(d => d.Address == device.Address);

        dbDevice.Should().NotBeNull();
        dbDevice!.Id.Should().Be(device.Id);
    }
}