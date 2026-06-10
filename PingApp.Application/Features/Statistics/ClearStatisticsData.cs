using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Statistics;

public static class ClearStatisticsData
{
    public record Command : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;

        public Handler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await _repository.ClearAllStatusesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}