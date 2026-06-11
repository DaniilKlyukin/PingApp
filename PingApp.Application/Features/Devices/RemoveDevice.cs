using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Devices;

public static class RemoveDevice
{
    public record Command(string Address) : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;
        private readonly IUserContext _userContext;

        public Handler(IDeviceRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await _repository.RemoveSubscriptionAsync(_userContext.UserId, request.Address, cancellationToken);
            return Unit.Value;
        }
    }
}