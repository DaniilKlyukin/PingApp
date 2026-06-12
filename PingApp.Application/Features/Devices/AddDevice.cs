using MediatR;
using PingApp.Application.Interfaces;
using PingApp.Domain.Common;
using PingApp.Domain.ValueObjects;

namespace PingApp.Application.Features.Devices;

public static class AddDevice
{
    public record Command(string Address, string? Nickname) : IRequest<Result>;

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IDeviceRepository _repository;
        private readonly IUserContext _userContext;

        public Handler(IDeviceRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var addressResult = DeviceAddress.Create(request.Address);

            if (addressResult.IsFailure)
                return Result.Failure(addressResult.Error);

            var address = addressResult.Value;

            var device = await _repository.GetByAddressAsync(address, cancellationToken);
            if (device == null)
                return Result.Failure("Данное устройство еще не обнаружено в сети.");

            if (!device.IsAllowedToPing)
                return Result.Failure("Отслеживание этого устройства ограничено администратором.");

            var alreadySubscribed = await _repository.ExistsSubscriptionAsync(_userContext.UserId, address, cancellationToken);
            if (alreadySubscribed)
                return Result.Failure("Вы уже отслеживаете это устройство.");

            await _repository.AddSubscriptionAsync(_userContext.UserId, device, request.Nickname?.Trim(), cancellationToken);
            return Result.Success();
        }
    }
}