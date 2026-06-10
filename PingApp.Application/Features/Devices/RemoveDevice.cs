using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Devices;

public static class RemoveDevice
{
    public record Command(string Address) : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;

        public Handler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(request.Address, cancellationToken);
            return Unit.Value;
        }
    }
}