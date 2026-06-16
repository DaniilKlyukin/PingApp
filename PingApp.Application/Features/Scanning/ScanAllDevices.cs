using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<Handler> _logger;

        private const int MaxParallelPings = 32;

        public Handler(
            IDeviceRepository repository,
            IPingService pingService,
            ILocalNetworkProvider networkProvider,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<Handler> logger)
        {
            _repository = repository;
            _pingService = pingService;
            _networkProvider = networkProvider;
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запуск фонового сканирования подсети...");

            var activeIps = await DiscoverActiveIpsAsync(cancellationToken);

            await RegisterNewDevicesAsync(activeIps, cancellationToken);

            await PingAndProcessDevicesAsync(cancellationToken);

            return Unit.Value;
        }

        #region Шаг 1: Обнаружение активных IP (Discovery)

        private async Task<List<string>> DiscoverActiveIpsAsync(CancellationToken cancellationToken)
        {
            var baseSubnetIp = ResolveBaseSubnetIp();
            if (baseSubnetIp == null)
            {
                _logger.LogWarning("Не удалось определить базовую подсеть для сканирования.");
                return [];
            }

            using var semaphore = new SemaphoreSlim(MaxParallelPings);
            var discoveryTasks = new List<Task<(string Ip, bool IsActive)>>();

            for (int i = 1; i <= 254; i++)
            {
                var targetIp = $"{baseSubnetIp}.{i}";
                discoveryTasks.Add(PingHostWithThrottleAsync(targetIp, semaphore, cancellationToken));
            }

            var results = await Task.WhenAll(discoveryTasks);
            return results
                .Where(r => r.IsActive)
                .Select(r => r.Ip)
                .ToList();
        }

        private string? ResolveBaseSubnetIp()
        {
            var subnetOverride = _configuration["Scanning:SubnetOverride"];
            if (!string.IsNullOrWhiteSpace(subnetOverride))
            {
                _logger.LogInformation("Используется переопределенная подсеть из конфигурации: {Subnet}.0/24", subnetOverride.Trim());
                return subnetOverride.Trim();
            }

            var localIp = _networkProvider.GetLocalIpAddress();
            if (localIp != null)
            {
                var ipBytes = localIp.GetAddressBytes();
                return $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}";
            }

            return null;
        }

        private async Task<(string Ip, bool IsActive)> PingHostWithThrottleAsync(
            string ip,
            SemaphoreSlim semaphore,
            CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await _networkProvider.PingHostForDiscoveryAsync(ip, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Шаг 2: Пакетная регистрация (Batch Registration)

        private async Task RegisterNewDevicesAsync(List<string> activeIps, CancellationToken cancellationToken)
        {
            var allDevices = await _repository.GetAllDevicesAsync(cancellationToken);
            var existingAddresses = allDevices.Select(d => d.Address.Value).ToHashSet();

            var newDevicesToRegister = new List<Device>();

            foreach (var ip in activeIps)
            {
                var addressResult = DeviceAddress.Create(ip);
                if (addressResult.IsSuccess && !existingAddresses.Contains(addressResult.Value.Value))
                {
                    var newDevice = Device.Create(
                        addressResult.Value,
                        isAllowedToPing: true,
                        isVisibleToUsers: false);

                    newDevicesToRegister.Add(newDevice);
                }
            }

            if (newDevicesToRegister.Count > 0)
            {
                await _repository.AddDevicesRangeAsync(newDevicesToRegister, cancellationToken);
                _logger.LogInformation("Автоматически зарегистрировано новых устройств в пуле: {Count}", newDevicesToRegister.Count);
            }
        }

        #endregion

        #region Шаг 3: Мониторинг и обработка переходов (Monitoring & Processing)

        private async Task PingAndProcessDevicesAsync(CancellationToken cancellationToken)
        {
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
                if (result.Success)
                {
                    await ProcessDeviceStatusAsync(result.Device, result.IsOnline, cancellationToken);
                }
            }
        }

        private async Task ProcessDeviceStatusAsync(Device device, bool isOnline, CancellationToken cancellationToken)
        {
            try
            {
                var transition = device.UpdateStatus(DateTime.Now, isOnline);
                await _repository.UpdateAsync(device, cancellationToken);

                if (transition == DeviceStatusTransition.None)
                {
                    return;
                }

                var historyRecord = new StatusRecord
                {
                    DateTime = DateTime.UtcNow,
                    AtWork = isOnline,
                    DeviceId = device.Id
                };
                await _repository.AddStatusRecordAsync(historyRecord, cancellationToken);

                _logger.LogInformation("Устройство {Address} изменило статус. Доступно: {IsOnline} (Событие: {Transition})",
                    device.Address.Value, isOnline, transition.ToString());

                await _mediator.Publish(new DeviceStatusChanged.Notification(
                     device.Address.Value,
                     transition == DeviceStatusTransition.LoggedIn,
                     DateTime.Now
                 ), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении статуса устройства {Address}", device.Address.Value);
            }
        }

        #endregion

        private record PingResult(Device Device, bool IsOnline, bool Success);
    }
}