using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.SettingsAggregate;

public class GlobalSetting : IAggregateRoot
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}
