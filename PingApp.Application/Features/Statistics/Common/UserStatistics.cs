namespace PingApp.Application.Features.Statistics.Common;

public class UserStatistics
{
    public required string Address { get; init; }
    public string? Nickname { get; init; }
    public required List<WorkStatus> Statuses { get; init; }
}
