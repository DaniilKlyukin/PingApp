using MediatR;
using PingApp.Application.Interfaces;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Telegram;

public static class UnsubscribeTelegram
{
    public record Command(long ChatId, string DeviceAddress) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ITelegramSubscriptionRepository _subscriptionRepository;

        public Handler(ITelegramSubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            await _subscriptionRepository.RemoveAsync(request.ChatId, request.DeviceAddress, cancellationToken);
            return Result.Success();
        }
    }
}