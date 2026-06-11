using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Admin;

public static class UpdateAdminSettings
{
    public record Command(List<DeviceToggleDto> Toggles, int ScanIntervalSeconds, bool AllowAll) : IRequest<Unit>;

    public record DeviceToggleDto(string Address, bool IsAllowed);

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
                if (request.AllowAll)
                {
                    device.IsAllowedToPing = true;
                }
                else
                {
                    var toggle = request.Toggles.FirstOrDefault(t => t.Address == device.Address);
                    if (toggle != null)
                    {
                        device.IsAllowedToPing = toggle.IsAllowed;
                    }
                }
                await _deviceRepository.UpdateAsync(device, cancellationToken);
            }

            return Unit.Value;
        }
    }
}