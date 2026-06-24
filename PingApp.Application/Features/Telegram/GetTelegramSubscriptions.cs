using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Telegram;

public static class GetTelegramSubscriptions
{
    public record Query(long ChatId) : IRequest<List<SubscriptionDto>>;

    public record SubscriptionDto(string Address, string? Nickname);

    public sealed class Handler : IRequestHandler<Query, List<SubscriptionDto>>
    {
        private readonly ITelegramSubscriptionRepository _subscriptionRepository;

        public Handler(ITelegramSubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<List<SubscriptionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var subs = await _subscriptionRepository.GetSubscriptionsByChatIdAsync(request.ChatId, cancellationToken);
            return subs.Select(s => new SubscriptionDto(s.DeviceAddress, s.Nickname)).ToList();
        }
    }
}