using Microsoft.EntityFrameworkCore;
using PingApp.Application.Interfaces;
using PingApp.Domain.Entities;
using PingApp.Infrastructure.Data;

namespace PingApp.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly PingDbContext _context;

    public DeviceRepository(PingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Device>> GetAllWithStatusesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Statuses)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device?> GetByAddressWithStatusesAsync(string address, CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .Include(d => d.Statuses)
            .FirstOrDefaultAsync(d => d.Address == address, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string address, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.AnyAsync(d => d.Address == address, cancellationToken);
    }

    public async Task AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string address, CancellationToken cancellationToken = default)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Address == address, cancellationToken);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ClearAllStatusesAsync(CancellationToken cancellationToken = default)
    {
        _context.Statuses.RemoveRange(_context.Statuses);
        await _context.SaveChangesAsync(cancellationToken);
    }
}