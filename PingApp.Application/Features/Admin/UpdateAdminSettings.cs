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
            _logger.LogInformation(
                "Запрос на изменение настроек. Новый интервал: {Interval} сек. Массовое действие: {BulkAction}",
                request.ScanIntervalSeconds, request.BulkAction?.ToString() ?? "нет");

            await _settingsRepository.SaveSettingAsync("ScanIntervalSeconds", request.ScanIntervalSeconds.ToString(), cancellationToken);

            var allDevices = await _deviceRepository.GetAllDevicesAsync(cancellationToken);
            int updatedDevicesCount = 0;

            foreach (var device in allDevices)
            {
                bool hasChanged = false;

                if (request.BulkAction.HasValue)
                {
                    var previousPing = device.IsAllowedToPing;
                    var previousVisibility = device.IsVisibleToUsers;

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

                    hasChanged = (previousPing != device.IsAllowedToPing) || (previousVisibility != device.IsVisibleToUsers);
                }
                else
                {
                    var toggle = request.Toggles.FirstOrDefault(t => t.Address == device.Address.Value);
                    if (toggle != null)
                    {
                        if (device.IsAllowedToPing != toggle.IsAllowedToPing || device.IsVisibleToUsers != toggle.IsVisibleToUsers)
                        {
                            device.IsAllowedToPing = toggle.IsAllowedToPing;
                            device.IsVisibleToUsers = toggle.IsVisibleToUsers;
                            hasChanged = true;
                        }
                    }
                }

                if (hasChanged)
                {
                    await _deviceRepository.UpdateAsync(device, cancellationToken);
                    updatedDevicesCount++;

                    _logger.LogInformation(
                        "Устройство {Address} обновлено администратором: Разрешен Пинг = {IsAllowed}, Видимость = {IsVisible}",
                        device.Address.Value, device.IsAllowedToPing, device.IsVisibleToUsers);
                }
            }

            _logger.LogInformation(
                "Глобальные параметры успешно применены. Общее число устройств в пуле: {TotalCount}. Фактически изменено: {UpdatedCount}",
                allDevices.Count, updatedDevicesCount);

            return Unit.Value;
        }
    }
}