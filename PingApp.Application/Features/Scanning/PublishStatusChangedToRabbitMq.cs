using MassTransit;
using MediatR;
using PingApp.Application.Contracts;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Scanning;

public class PublishStatusChangedToRabbitMq : INotificationHandler<DeviceStatusChanged.Notification>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITelegramSubscriptionRepository _subscriptionRepository;

    public PublishStatusChangedToRabbitMq(IPublishEndpoint publishEndpoint, ITelegramSubscriptionRepository subscriptionRepository)
    {
        _publishEndpoint = publishEndpoint;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task Handle(DeviceStatusChanged.Notification notification, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetSubscriptionsByAddressAsync(notification.Address, cancellationToken);

        if (subscriptions.Count == 0)
        {
            return;
        }

        var targetChatIdsWithNicknames = subscriptions.ToDictionary(s => s.ChatId, s => s.Nickname);

        await _publishEndpoint.Publish(new DeviceStatusChangedIntegrationEvent(
            notification.Address,
            notification.AtWork,
            targetChatIdsWithNicknames,
            notification.DateTime
        ), cancellationToken);
    }
}