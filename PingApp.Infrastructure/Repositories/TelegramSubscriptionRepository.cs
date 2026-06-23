using Microsoft.EntityFrameworkCore;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.TelegramAggregate;
using PingApp.Infrastructure.Data;

namespace PingApp.Infrastructure.Repositories;

public class TelegramSubscriptionRepository : ITelegramSubscriptionRepository
{
    private readonly PingDbContext _context;

    public TelegramSubscriptionRepository(PingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(long chatId, string deviceAddress, CancellationToken cancellationToken = default)
    {
        return await _context.TelegramSubscriptions
            .AnyAsync(s => s.ChatId == chatId && s.DeviceAddress == deviceAddress, cancellationToken);
    }

    public async Task AddAsync(TelegramSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.TelegramSubscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(long chatId, string deviceAddress, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.TelegramSubscriptions
            .FirstOrDefaultAsync(s => s.ChatId == chatId && s.DeviceAddress == deviceAddress, cancellationToken);

        if (subscription != null)
        {
            _context.TelegramSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<long>> GetSubscribersByAddressAsync(string deviceAddress, CancellationToken cancellationToken = default)
    {
        return await _context.TelegramSubscriptions
            .Where(s => s.DeviceAddress == deviceAddress)
            .Select(s => s.ChatId)
            .ToListAsync(cancellationToken);
    }
}
