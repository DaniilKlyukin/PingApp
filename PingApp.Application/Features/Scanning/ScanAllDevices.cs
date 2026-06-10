using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Interfaces;
using PingApp.Domain.Enums;

namespace PingApp.Application.Features.Scanning;

public static class ScanAllDevices
{
    public record Command : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;
        private readonly IPingService _pingService;
        private readonly IMediator _mediator;
        private readonly IScanConfiguration _config;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IDeviceRepository repository,
            IPingService pingService,
            IMediator mediator,
            IScanConfiguration config,
            ILogger<Handler> _logger)
        {
            _repository = repository;
            _pingService = pingService;
            _mediator = mediator;
            _config = config;
            this._logger = _logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var devices = await _repository.GetAllWithStatusesAsync(cancellationToken);

            var tasks = devices.Select(async device =>
            {
                try
                {
                    var isOnline = await _pingService.PingHostAsync(device.Address, cancellationToken);

                    if (_config.SaveToDatabase)
                    {
                        var transition = device.AddStatus(DateTime.Now, isOnline);
                        await _repository.UpdateAsync(device, cancellationToken);

                        if (transition != DeviceStatusTransition.None)
                        {
                            await _mediator.Publish(new DeviceStatusChanged.Notification(
                                device.Address,
                                device.Nickname,
                                transition == DeviceStatusTransition.LoggedIn,
                                DateTime.Now
                            ), cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении опроса устройства {Address}", device.Address);
                }
            });

            await Task.WhenAll(tasks);
            return Unit.Value;
        }
    }
}
