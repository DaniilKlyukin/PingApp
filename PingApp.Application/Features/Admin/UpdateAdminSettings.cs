using MediatR;
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

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IGlobalSettingsRepository _settingsRepository;

        public Handler(IDeviceRepository deviceRepository, IGlobalSettingsRepository settingsRepository)
        {
            _deviceRepository = deviceRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
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
                        device.IsAllowedToPing = toggle.IsAllowedToPing;
                        device.IsVisibleToUsers = toggle.IsVisibleToUsers;
                    }
                }
                await _deviceRepository.UpdateAsync(device, cancellationToken);
            }

            return Unit.Value;
        }
    }
}