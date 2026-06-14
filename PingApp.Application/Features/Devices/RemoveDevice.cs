using MediatR;
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

        public Handler(IDeviceRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var addressResult = DeviceAddress.Create(request.Address);

            if (addressResult.IsFailure)
            {
                return Result.Failure(addressResult.Error);
            }

            await _repository.RemoveSubscriptionAsync(
                _userContext.UserId,
                addressResult.Value,
                cancellationToken);

            return Result.Success();
        }
    }
}