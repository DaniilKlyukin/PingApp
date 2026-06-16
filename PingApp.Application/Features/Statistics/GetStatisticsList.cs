using MediatR;
using PingApp.Application.Features.Statistics.Common;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Statistics;

public static class GetStatisticsList
{
    public record Query : IRequest<List<UserStatistics>>;

    public sealed class Handler : IRequestHandler<Query, List<UserStatistics>>
    {
        private readonly IDeviceRepository _repository;
        private readonly IUserContext _userContext;

        public Handler(IDeviceRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<List<UserStatistics>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userDevices = await _repository.GetUserDevicesAsync(_userContext.UserId, cancellationToken);
            var allDevices = await _repository.GetAllDevicesNoTrackingAsync(cancellationToken);

            var activeDevices = userDevices
                .Join(
                    allDevices,
                    ud => ud.DeviceId,
                    d => d.Id,
                    (ud, d) => new { UserDevice = ud, Device = d })
                .Where(ud => ud.Device.IsVisibleToUsers && ud.Device.IsAllowedToPing)
                .ToList();

            var deviceIds = activeDevices.Select(x => x.Device.Id).ToList();

            var history = await _repository.GetStatusHistoryAsync(deviceIds, cancellationToken);

            return activeDevices.Select(ud =>
            {
                var d = ud.Device;
                var deviceHistory = history.Where(h => h.DeviceId == d.Id).ToList();

                return new UserStatistics
                {
                    Address = d.Address.Value,
                    Nickname = ud.UserDevice.DeviceNickname?.Value,
                    Statuses = deviceHistory.Select(s => new WorkStatus(s.DateTime, s.AtWork)).ToList()
                };
            }).ToList();
        }
    }
}