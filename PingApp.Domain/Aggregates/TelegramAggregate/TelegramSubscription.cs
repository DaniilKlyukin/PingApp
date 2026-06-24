using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.TelegramAggregate;

public class TelegramSubscription : IAggregateRoot
{
    public int Id { get; private set; }
    public long ChatId { get; private set; }
    public string DeviceAddress { get; private set; }
    public string? Nickname { get; private set; }
    public DateTime SubscribedAtUtc { get; private set; }

    private TelegramSubscription() { }

    public TelegramSubscription(long chatId, string deviceAddress, string? nickname = null)
    {
        ChatId = chatId;
        DeviceAddress = deviceAddress;
        Nickname = nickname;
        SubscribedAtUtc = DateTime.UtcNow;
    }

    public void UpdateNickname(string? nickname)
    {
        Nickname = nickname;
    }
}