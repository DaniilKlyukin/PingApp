using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Devices;

public static class RemoveDevice
{
    public record Command(string Address) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IDeviceRepository _repository;
        private readonly IUserContext _userContext;
        private readonly ILogger<Handler> _logger;

        public Handler(IDeviceRepository repository, IUserContext userContext, ILogger<Handler> logger)
        {
            _repository = repository;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Пользователь {UserId} инициировал удаление устройства {Address}", _userContext.UserId, request.Address);

            var addressResult = DeviceAddress.Create(request.Address);
            if (addressResult.IsFailure)
            {
                _logger.LogWarning("Не удалось удалить устройство: некорректный адрес {Address}", request.Address);
                return Result.Failure(addressResult.Error);
            }

            await _repository.RemoveSubscriptionAsync(
                _userContext.UserId,
                addressResult.Value,
                cancellationToken);

            _logger.LogInformation("Устройство {Address} успешно удалено из подписок пользователя {UserId}", addressResult.Value.Value, _userContext.UserId);
            return Result.Success();
        }
    }
}