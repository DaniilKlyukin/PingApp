using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Enums;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

namespace PingApp.Application.Features.Scanning;

public static class ScanAllDevices
{
    public record Command : IRequest<Unit>;

    public sealed class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;
        private readonly IPingService _pingService;
        private readonly ILocalNetworkProvider _networkProvider;
        private readonly IMediator _mediator;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IDeviceRepository repository,
            IPingService pingService,
            ILocalNetworkProvider networkProvider,
            IMediator mediator,
            ILogger<Handler> logger)
        {
            _repository = repository;
            _pingService = pingService;
            _networkProvider = networkProvider;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запуск фонового сканирования подсети...");

            var localIp = _networkProvider.GetLocalIpAddress();
            var activeIps = new List<string>();

            if (localIp != null)
            {
                var ipBytes = localIp.GetAddressBytes();
                var baseSubnetIp = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}";
                var discoveryTasks = new List<Task<(string Ip, bool IsActive)>>();

                for (int i = 1; i <= 254; i++)
                {
                    var targetIp = $"{baseSubnetIp}.{i}";
                    discoveryTasks.Add(_networkProvider.PingHostForDiscoveryAsync(targetIp, cancellationToken));
                }

                var results = await Task.WhenAll(discoveryTasks);
                activeIps = results
                    .Where(r => r.IsActive)
                    .Select(r => r.Ip)
                    .ToList();
            }

            foreach (var ip in activeIps)
            {
                var addressResult = DeviceAddress.Create(ip);
                if (addressResult.IsSuccess)
                {
                    var existing = await _repository.GetByAddressAsync(addressResult.Value, cancellationToken);
                    if (existing == null)
                    {
                        var newDevice = Device.Create(
                            addressResult.Value,
                            isAllowedToPing: true,
                            isVisibleToUsers: false);

                        await _repository.AddDeviceAsync(newDevice, cancellationToken);
                    }
                }
            }

            var allDevices = await _repository.GetAllDevicesAsync(cancellationToken);
            var devicesToPing = allDevices.Where(d => d.IsAllowedToPing).ToList();

            _logger.LogInformation("Пингуем разрешенные устройства (активных к опросу: {Count} из {Total})", devicesToPing.Count, allDevices.Count);

            var pingTasks = devicesToPing.Select(async device =>
            {
                try
                {
                    var isOnline = await _pingService.PingHostAsync(device.Address.Value, cancellationToken);
                    return new PingResult(device, isOnline, Success: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при опросе устройства {Address}", device.Address.Value);
                    return new PingResult(device, IsOnline: false, Success: false);
                }
            });

            var resultsList = await Task.WhenAll(pingTasks);

            foreach (var result in resultsList)
            {
                if (!result.Success) continue;

                try
                {
                    var device = result.Device;
                    var transition = device.AddStatus(DateTime.Now, result.IsOnline);

                    await _repository.UpdateAsync(device, cancellationToken);

                    if (transition != DeviceStatusTransition.None)
                    {
                        _logger.LogInformation("Устройство {Address} изменило сетевой статус. Доступно: {IsOnline} (Событие: {Transition})",
                            device.Address.Value, result.IsOnline, transition.ToString());

                        await _mediator.Publish(new DeviceStatusChanged.Notification(
                             device.Address.Value,
                             transition == DeviceStatusTransition.LoggedIn,
                             DateTime.Now
                         ), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении статуса устройства {Address} в базу данных", result.Device.Address.Value);
                }
            }

            return Unit.Value;
        }

        private record PingResult(Device Device, bool IsOnline, bool Success);
    }
}