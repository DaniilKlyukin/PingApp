namespace PingApp.Application.Contracts;

public record DeviceStatusChangedIntegrationEvent(
    string Address,
    bool IsOnline,
    Dictionary<long, string?> TargetChatIdsWithNicknames,
    DateTime Timestamp
);