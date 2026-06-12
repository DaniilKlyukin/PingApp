using Microsoft.EntityFrameworkCore;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.ValueObjects;
using PingApp.Infrastructure.Data;

namespace PingApp.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly PingDbContext _context;

    public DeviceRepository(PingDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDevice>> GetUserDevicesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserDevices)
                .ThenInclude(ud => ud.Device)
                    .ThenInclude(d => d.Statuses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.UserDevices.ToList() ?? new List<UserDevice>();
    }

    public async Task<bool> ExistsSubscriptionAsync(UserId userId, DeviceAddress address, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserDevices)
                .ThenInclude(ud => ud.Device)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.UserDevices.Any(ud => ud.Device.Address == address) ?? false;
    }

    public async Task AddSubscriptionAsync(UserId userId, Device device, string? nickname, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserDevices)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"Пользователь с идентификатором {userId} не найден.");
        }

        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.Address == device.Address, cancellationToken);
        if (existingDevice == null)
        {
            existingDevice = device;
            _context.Devices.Add(existingDevice);
            await _context.SaveChangesAsync(cancellationToken);
        }

        user.AddSubscription(existingDevice.Id, nickname);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSubscriptionAsync(UserId userId, DeviceAddress address, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserDevices)
                .ThenInclude(ud => ud.Device)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user != null)
        {
            var subscription = user.UserDevices.FirstOrDefault(ud => ud.Device.Address == address);
            if (subscription != null)
            {
                user.RemoveSubscription(subscription.DeviceId);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task<List<Device>> GetAllTrackedDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Where(d => d.IsAllowedToPing && d.UserDevices.Any())
            .Include(d => d.Statuses)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAllStatusesAsync(CancellationToken cancellationToken = default)
    {
        await _context.Set<StatusRecord>().ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Statuses)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Device>> GetAllowedDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Where(d => d.IsAllowedToPing && d.IsVisibleToUsers)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByAddressAsync(DeviceAddress address, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Statuses)
            .FirstOrDefaultAsync(d => d.Address == address, cancellationToken);
    }

    public async Task AddDeviceAsync(Device device, CancellationToken cancellationToken = default)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);
    }
}