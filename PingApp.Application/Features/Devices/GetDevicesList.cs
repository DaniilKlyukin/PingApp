using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Devices;

public static class GetDevicesList
{
    public record Query : IRequest<List<DeviceDto>>;

    public record DeviceDto(string Address, string? Nickname, bool AtWork, string StatusString);

    public sealed class Handler : IRequestHandler<Query, List<DeviceDto>>
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
            var allDevices = await _repository.GetAllDevicesAsync(cancellationToken);

            return userDevices
                .Join(
                    allDevices,
                    ud => ud.DeviceId,
                    d => d.Id,
                    (ud, d) => new { UserDevice = ud, Device = d })
                .Where(x => x.Device.IsVisibleToUsers && x.Device.IsAllowedToPing)
                .Select(x =>
                {
                    var d = x.Device;
                    var lastStatus = d.Statuses.OrderByDescending(s => s.DateTime).FirstOrDefault();

                    return new DeviceDto(
                        d.Address.Value,
                        x.UserDevice.DeviceNickname?.Value,
                        lastStatus?.AtWork ?? false,
                        lastStatus != null
                            ? (lastStatus.AtWork ? "В сети" : $"Не в сети (с {lastStatus.DateTime.ToLocalTime():g})")
                            : "Нет данных"
                    );
                }).ToList();
        }
    }
}