using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.Application.Interfaces;

public interface IDeviceRepository
{
    Task<List<UserDevice>> GetUserDevicesAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsSubscriptionAsync(UserId userId, DeviceAddress address, CancellationToken cancellationToken = default);
    Task AddSubscriptionAsync(UserId userId, Device device, DeviceNickname deviceNickname, CancellationToken cancellationToken = default);
    Task RemoveSubscriptionAsync(UserId userId, DeviceAddress address, CancellationToken cancellationToken = default);
    Task<List<DeviceWithLastStatus>> GetUserDevicesWithLastStatusAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllDevicesNoTrackingAsync(CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllowedDevicesAsync(CancellationToken cancellationToken = default);
    Task<List<Device>> GetAllTrackedDevicesAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
    Task ClearAllStatusesAsync(CancellationToken cancellationToken = default);
    Task<Device?> GetByAddressAsync(DeviceAddress address, CancellationToken cancellationToken = default);
    Task AddDeviceAsync(Device device, CancellationToken cancellationToken = default);
    Task AddStatusRecordAsync(StatusRecord record, CancellationToken cancellationToken = default);
    Task<List<StatusRecord>> GetStatusHistoryAsync(List<DeviceId> deviceIds, CancellationToken cancellationToken = default);
}

public record DeviceWithLastStatus(
    string Address,
    string? Nickname,
    bool? LastAtWork,
    DateTime? LastStatusTime
);