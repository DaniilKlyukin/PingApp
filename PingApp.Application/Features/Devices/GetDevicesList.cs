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
            var devicesData = await _repository.GetUserDevicesWithLastStatusAsync(
                _userContext.UserId,
                cancellationToken);

            return devicesData.Select(d =>
            {
                var atWork = d.LastAtWork ?? false;
                string statusString;

                if (d.LastStatusTime.HasValue)
                {
                    statusString = atWork
                        ? "В сети"
                        : $"Не в сети (с {d.LastStatusTime.Value.ToLocalTime():g})";
                }
                else
                {
                    statusString = "Нет данных";
                }

                return new DeviceDto(
                    d.Address,
                    d.Nickname,
                    atWork,
                    statusString
                );
            }).ToList();
        }
    }
}