using PingApp.Domain.Entities;

namespace PingApp.Application.Interfaces;

public interface IDeviceRepository
{
    Task<List<Device>> GetAllWithStatusesAsync(CancellationToken cancellationToken = default);
    Task<Device?> GetByAddressWithStatusesAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string address, CancellationToken cancellationToken = default);
    Task AddAsync(Device device, CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
    Task DeleteAsync(string address, CancellationToken cancellationToken = default);
    Task ClearAllStatusesAsync(CancellationToken cancellationToken = default);
}