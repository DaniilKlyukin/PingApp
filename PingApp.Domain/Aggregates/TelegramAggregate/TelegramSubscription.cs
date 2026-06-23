using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.TelegramAggregate;

public class TelegramSubscription : IAggregateRoot
{
    public int Id { get; private set; }
    public long ChatId { get; private set; }
    public string DeviceAddress { get; private set; }
    public DateTime SubscribedAtUtc { get; private set; }

    private TelegramSubscription() { }

    public TelegramSubscription(long chatId, string deviceAddress)
    {
        ChatId = chatId;
        DeviceAddress = deviceAddress;
        SubscribedAtUtc = DateTime.UtcNow;
    }
}
