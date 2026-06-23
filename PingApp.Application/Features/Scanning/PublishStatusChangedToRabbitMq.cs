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
        var subscriberChatIds = await _subscriptionRepository.GetSubscribersByAddressAsync(notification.Address, cancellationToken);

        if (subscriberChatIds.Count == 0)
        {
            return;
        }

        await _publishEndpoint.Publish(new DeviceStatusChangedIntegrationEvent(
            notification.Address,
            notification.AtWork,
            subscriberChatIds,
            notification.DateTime
        ), cancellationToken);
    }
}