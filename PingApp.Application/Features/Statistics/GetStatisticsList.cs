using MediatR;
using PingApp.Application.Features.Statistics.Common;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Statistics;

public static class GetStatisticsList
{
    public record Query : IRequest<List<UserStatistics>>;

    public class Handler : IRequestHandler<Query, List<UserStatistics>>
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

            return userDevices
                .Where(ud => ud.Device.IsVisibleToUsers && ud.Device.IsAllowedToPing)
                .Select(ud =>
                {
                    var d = ud.Device;
                    return new UserStatistics
                    {
                        Address = d.Address,
                        Nickname = ud.Nickname,
                        Statuses = d.Statuses.Select(s => new WorkStatus(s.DateTime, s.AtWork)).ToList()
                    };
                }).ToList();
        }
    }
}