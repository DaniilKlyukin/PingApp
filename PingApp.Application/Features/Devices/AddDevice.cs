using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Devices;

public static class AddDevice
{
    public record Command(string Address, string? Nickname) : IRequest<Result>;

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
            _logger.LogInformation("Пользователь {UserId} пытается добавить устройство {Address} (Имя: {Nickname})",
                _userContext.UserId, request.Address, request.Nickname ?? "нет");

            var addressResult = DeviceAddress.Create(request.Address);
            if (addressResult.IsFailure)
            {
                _logger.LogWarning("Отклонено добавление устройства: неверный формат адреса {Address}. Ошибка: {Error}",
                    request.Address, addressResult.Error.Message);
                return Result.Failure(addressResult.Error);
            }

            var address = addressResult.Value;

            var device = await _repository.GetByAddressAsync(address, cancellationToken);
            if (device == null || !device.IsVisibleToUsers)
            {
                _logger.LogWarning("Отклонено добавление устройства: {Address} отсутствует в пуле разрешенных администратором", address.Value);
                return DeviceErrors.NotFound;
            }

            if (!device.IsAllowedToPing)
            {
                _logger.LogWarning("Отклонено добавление устройства: мониторинг устройства {Address} запрещен администратором", address.Value);
                return DeviceErrors.NotAllowedToPing;
            }

            var alreadySubscribed = await _repository.ExistsSubscriptionAsync(_userContext.UserId, address, cancellationToken);
            if (alreadySubscribed)
            {
                _logger.LogWarning("Отклонено добавление устройства: пользователь {UserId} уже отслеживает {Address}", _userContext.UserId, address.Value);
                return DeviceErrors.AlreadySubscribed;
            }

            var nicknameResult = DeviceNickname.Create(request.Nickname);
            if (nicknameResult.IsFailure)
            {
                _logger.LogWarning("Отклонено добавление устройства: некорректное имя {Nickname}. Ошибка: {Error}", request.Nickname, nicknameResult.Error.Message);
                return Result.Failure(nicknameResult.Error);
            }

            await _repository.AddSubscriptionAsync(_userContext.UserId, device, nicknameResult.Value, cancellationToken);

            _logger.LogInformation("Устройство {Address} успешно добавлено в список отслеживания пользователя {UserId}", address.Value, _userContext.UserId);
            return Result.Success();
        }
    }
}