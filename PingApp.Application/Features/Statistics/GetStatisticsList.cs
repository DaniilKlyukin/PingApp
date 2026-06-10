using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Statistics;

public static class GetStatisticsList
{
    public record Query : IRequest<List<UserStatistics>>;

    public class Handler : IRequestHandler<Query, List<UserStatistics>>
    {
        private readonly IDeviceRepository _repository;

        public Handler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserStatistics>> Handle(Query request, CancellationToken cancellationToken)
        {
            var devices = await _repository.GetAllWithStatusesAsync(cancellationToken);

            return devices.Select(d => new UserStatistics
            {
                Address = d.Address,
                Nickname = d.Nickname,
                Statuses = d.Statuses.Select(s => new WorkStatus(s.DateTime, s.AtWork)).ToList()
            }).ToList();
        }
    }
}

public class UserStatistics
{
    public required string Address { get; init; }
    public string? Nickname { get; init; }
    public required List<WorkStatus> Statuses { get; init; }
}

public class WorkStatus
{
    public DateTime DateTime { get; set; }
    public bool AtWork { get; set; }

    public WorkStatus(DateTime dateTime, bool atWork)
    {
        DateTime = dateTime;
        AtWork = atWork;
    }
}