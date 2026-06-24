using PingApp.Domain.Aggregates.TelegramAggregate;

namespace PingApp.Application.Interfaces;

public interface ITelegramSubscriptionRepository
{
    Task<bool> ExistsAsync(long chatId, string deviceAddress, CancellationToken cancellationToken = default);
    Task AddAsync(TelegramSubscription subscription, CancellationToken cancellationToken = default);
    Task RemoveAsync(long chatId, string deviceAddress, CancellationToken cancellationToken = default);
    Task<List<long>> GetSubscribersByAddressAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<List<TelegramSubscription>> GetSubscriptionsByAddressAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<List<TelegramSubscription>> GetSubscriptionsByChatIdAsync(long chatId, CancellationToken cancellationToken = default);
    Task<TelegramSubscription?> GetSubscriptionAsync(long chatId, string deviceAddress, CancellationToken cancellationToken = default);
    Task UpdateAsync(TelegramSubscription subscription, CancellationToken cancellationToken = default);
}
