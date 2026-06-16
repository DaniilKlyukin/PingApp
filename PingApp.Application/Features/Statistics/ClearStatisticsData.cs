using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Statistics;

public static class ClearStatisticsData
{
    public record Command : IRequest<Unit>;

    public sealed class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;
        private readonly ILogger<Handler> _logger;

        public Handler(IDeviceRepository repository, ILogger<Handler> _logger)
        {
            _repository = repository;
            this._logger = _logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("ВНИМАНИЕ: Запущена очистка всей истории статусов устройств в базе данных!");

            await _repository.ClearAllStatusesAsync(cancellationToken);

            _logger.LogInformation("Вся история статусов устройств в БД успешно очищена.");
            return Unit.Value;
        }
    }
}