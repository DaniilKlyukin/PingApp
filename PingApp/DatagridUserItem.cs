namespace PingApp
{
    public class DataGridUserItem
    {
        public required string Address { get; init; }
        public string? NickName { get; set; }
        public required string StatusString { get; init; }
        public required bool AtWork { get; init; }
    }
}
