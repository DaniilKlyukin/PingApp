using MediatR;
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
            if (device == null || !device.IsVisibleToUsers)
                return DeviceErrors.NotFound;

            if (!device.IsAllowedToPing)
                return DeviceErrors.NotAllowedToPing;

            var alreadySubscribed = await _repository.ExistsSubscriptionAsync(_userContext.UserId, address, cancellationToken);
            if (alreadySubscribed)
                return DeviceErrors.AlreadySubscribed;

            var nicknameResult = DeviceNickname.Create(request.Nickname);

            if (nicknameResult.IsFailure)
                return Result.Failure(nicknameResult.Error);

            await _repository.AddSubscriptionAsync(_userContext.UserId, device, nicknameResult.Value, cancellationToken);
            return Result.Success();
        }
    }
}