using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Admin;

public static class GetAdminData
{
    public record Query : IRequest<Response>;

    public record Response(List<DeviceAdminDto> Devices, int ScanIntervalSeconds);

    public record DeviceAdminDto(string Address, bool IsAllowed);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IGlobalSettingsRepository _settingsRepository;

        public Handler(IDeviceRepository deviceRepository, IGlobalSettingsRepository settingsRepository)
        {
            _deviceRepository = deviceRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var allDevices = await _deviceRepository.GetAllDevicesAsync(cancellationToken);
            var intervalStr = await _settingsRepository.GetSettingAsync("ScanIntervalSeconds", "10", cancellationToken);
            int.TryParse(intervalStr, out var interval);

            var dtos = allDevices.Select(d => new DeviceAdminDto(d.Address, d.IsAllowedToPing)).ToList();
            return new Response(dtos, interval);
        }
    }
}