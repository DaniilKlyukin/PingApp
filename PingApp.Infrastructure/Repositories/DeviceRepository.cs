using Microsoft.EntityFrameworkCore;
using PingApp.Application.Interfaces;
using PingApp.Domain.Common;
using PingApp.Domain.Entities;
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
        return await _context.UserDevices
            .Where(ud => ud.UserId == userId)
            .Include(ud => ud.Device)
                .ThenInclude(d => d.Statuses)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsSubscriptionAsync(UserId userId, DeviceAddress address, CancellationToken cancellationToken = default)
    {
        return await _context.UserDevices
            .AnyAsync(ud => ud.UserId == userId && ud.Device.Address == address, cancellationToken);
    }

    public async Task AddSubscriptionAsync(UserId userId, Device device, string? nickname, CancellationToken cancellationToken = default)
    {
        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.Address == device.Address, cancellationToken);
        if (existingDevice == null)
        {
            existingDevice = device;
            _context.Devices.Add(existingDevice);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var userDevice = new UserDevice
        {
            UserId = userId,
            DeviceId = existingDevice.Id,
            Nickname = nickname
        };

        _context.UserDevices.Add(userDevice);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSubscriptionAsync(UserId userId, DeviceAddress address, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.UserDevices
            .FirstOrDefaultAsync(ud => ud.UserId == userId && ud.Device.Address == address, cancellationToken);

        if (subscription != null)
        {
            _context.UserDevices.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
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
        _context.Statuses.RemoveRange(_context.Statuses);
        await _context.SaveChangesAsync(cancellationToken);
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