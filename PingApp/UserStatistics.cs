namespace PingApp
{
    public class UserStatistics
    {
        public required string Address { get; init; }
        public required string? Nickname { get; init; }
        public required List<WorkStatus> Statuses { get; init; }
    }
}
