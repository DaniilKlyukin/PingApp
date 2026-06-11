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
        private readonly IUserContext _userContext;

        public Handler(IDeviceRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<List<DeviceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userDevices = await _repository.GetUserDevicesAsync(_userContext.UserId, cancellationToken);

            return userDevices.Select(ud =>
            {
                var d = ud.Device;
                var lastStatus = d.Statuses.OrderByDescending(s => s.DateTime).FirstOrDefault();

                return new DeviceDto(
                    d.Address,
                    ud.Nickname,
                    lastStatus?.AtWork ?? false,
                    lastStatus != null
                        ? (lastStatus.AtWork ? "В сети" : $"Не в сети (с {lastStatus.DateTime:g})")
                        : "Нет данных"
                );
            }).ToList();
        }
    }
}