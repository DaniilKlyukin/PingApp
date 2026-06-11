using PingApp.Domain.Entities;

namespace PingApp.Application.Interfaces;

public interface IDeviceRepository
{
    Task<List<UserDevice>> GetUserDevicesAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsSubscriptionAsync(int userId, string address, CancellationToken cancellationToken = default);
    Task AddSubscriptionAsync(int userId, Device device, string? nickname, CancellationToken cancellationToken = default);
    Task RemoveSubscriptionAsync(int userId, string address, CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllowedDevicesAsync(CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllTrackedDevicesAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
    Task ClearAllStatusesAsync(CancellationToken cancellationToken = default);
}
