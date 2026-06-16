using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Admin;

public static class UpdateAdminSettings
{
    public record Command(
        List<DeviceToggleDto> Toggles,
        int ScanIntervalSeconds,
        BulkActionType? BulkAction = null) : IRequest<Unit>;

    public record DeviceToggleDto(string Address, bool IsAllowedToPing, bool IsVisibleToUsers);

    public enum BulkActionType
    {
        AllowAllPing,
        DenyAllPing,
        AllowAllVisible,
        DenyAllVisible
    }

    public sealed class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IGlobalSettingsRepository _settingsRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IDeviceRepository deviceRepository,
            IGlobalSettingsRepository settingsRepository,
            ILogger<Handler> logger)
        {
            _deviceRepository = deviceRepository;
            _settingsRepository = settingsRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Изменение глобальных настроек. Интервал сканирования: {Interval} секунд. Массовое действие: {BulkAction}",
                request.ScanIntervalSeconds, request.BulkAction?.ToString() ?? "нет");

            await _settingsRepository.SaveSettingAsync("ScanIntervalSeconds", request.ScanIntervalSeconds.ToString(), cancellationToken);

            var allDevices = await _deviceRepository.GetAllDevicesAsync(cancellationToken);
            foreach (var device in allDevices)
            {
                if (request.BulkAction.HasValue)
                {
                    switch (request.BulkAction.Value)
                    {
                        case BulkActionType.AllowAllPing:
                            device.IsAllowedToPing = true;
                            break;
                        case BulkActionType.DenyAllPing:
                            device.IsAllowedToPing = false;
                            break;
                        case BulkActionType.AllowAllVisible:
                            device.IsVisibleToUsers = true;
                            break;
                        case BulkActionType.DenyAllVisible:
                            device.IsVisibleToUsers = false;
                            break;
                    }
                }
                else
                {
                    var toggle = request.Toggles.FirstOrDefault(t => t.Address == device.Address.Value);
                    if (toggle != null)
                    {
                        if (device.IsAllowedToPing != toggle.IsAllowedToPing || device.IsVisibleToUsers != toggle.IsVisibleToUsers)
                        {
                            _logger.LogDebug("Обновление флагов устройства {Address}: Разрешен Пинг={IsAllowed}, Видим={IsVisible}",
                                device.Address.Value, toggle.IsAllowedToPing, toggle.IsVisibleToUsers);
                        }
                        device.IsAllowedToPing = toggle.IsAllowedToPing;
                        device.IsVisibleToUsers = toggle.IsVisibleToUsers;
                    }
                }
                await _deviceRepository.UpdateAsync(device, cancellationToken);
            }

            _logger.LogInformation("Глобальные параметры успешно сохранены. Обработано устройств: {Count}", allDevices.Count);
            return Unit.Value;
        }
    }
}