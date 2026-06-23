using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Devices;

public static class GetGlobalDeviceStatuses
{
    public record Query : IRequest<List<DeviceStatusDto>>;

    public record DeviceStatusDto(string Address, bool AtWork, string StatusString);

    public sealed class Handler : IRequestHandler<Query, List<DeviceStatusDto>>
    {
        private readonly IDeviceRepository _deviceRepository;

        public Handler(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<List<DeviceStatusDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var devices = await _deviceRepository.GetAllowedDevicesAsync(cancellationToken);

            return devices.Select(d =>
            {
                var atWork = d.LastKnownStatus ?? false;
                string statusString;

                if (d.LastStatusChangedUtc.HasValue)
                {
                    statusString = atWork
                        ? "В сети"
                        : $"Не в сети (с {d.LastStatusChangedUtc.Value.ToLocalTime():g})";
                }
                else
                {
                    statusString = "Нет данных о проверках";
                }

                return new DeviceStatusDto(
                    d.Address.Value,
                    atWork,
                    statusString
                );
            }).ToList();
        }
    }
}