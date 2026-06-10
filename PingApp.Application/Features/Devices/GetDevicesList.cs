using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Devices;

public static class GetDevicesList
{
    public record Query : IRequest<List<DeviceDto>>;

    public record DeviceDto(string Address, string? Nickname, bool AtWork, string StatusString);

    public class Handler : IRequestHandler<Query, List<DeviceDto>>
    {
        private readonly IDeviceRepository _repository;

        public Handler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DeviceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var devices = await _repository.GetAllWithStatusesAsync(cancellationToken);

            return devices.Select(d =>
            {
                var lastStatus = d.Statuses.OrderByDescending(s => s.DateTime).FirstOrDefault();
                return new DeviceDto(
                    d.Address,
                    d.Nickname,
                    lastStatus?.AtWork ?? false,
                    lastStatus != null
                        ? (lastStatus.AtWork ? "В сети" : $"Не в сети (с {lastStatus.DateTime:g})")
                        : "Нет данных"
                );
            }).ToList();
        }
    }
}