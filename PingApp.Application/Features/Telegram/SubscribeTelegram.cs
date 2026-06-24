using MediatR;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.TelegramAggregate;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Telegram;

public static class SubscribeTelegram
{
    public record Command(long ChatId, string DeviceAddress, string? Nickname = null) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ITelegramSubscriptionRepository _subscriptionRepository;
        private readonly IDeviceRepository _deviceRepository;

        public Handler(ITelegramSubscriptionRepository subscriptionRepository, IDeviceRepository deviceRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _deviceRepository = deviceRepository;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var addressResult = DeviceAddress.Create(request.DeviceAddress);
            if (addressResult.IsFailure)
            {
                return Result.Failure(addressResult.Error);
            }

            var device = await _deviceRepository.GetByAddressAsync(addressResult.Value, cancellationToken);
            if (device == null || !device.IsVisibleToUsers || !device.IsAllowedToPing)
            {
                return Result.Failure(new ValidationError("Telegram.DeviceUnavailable", "Устройство недоступно для мониторинга."));
            }

            var existingSubscription = await _subscriptionRepository.GetSubscriptionAsync(request.ChatId, request.DeviceAddress, cancellationToken);
            if (existingSubscription != null)
            {
                existingSubscription.UpdateNickname(request.Nickname);
                await _subscriptionRepository.UpdateAsync(existingSubscription, cancellationToken);
                return Result.Success();
            }

            var subscription = new TelegramSubscription(request.ChatId, request.DeviceAddress, request.Nickname);
            await _subscriptionRepository.AddAsync(subscription, cancellationToken);

            return Result.Success();
        }
    }
}
