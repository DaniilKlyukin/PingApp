namespace PingApp.Application.Contracts;

public record DeviceStatusChangedIntegrationEvent(
    string Address,
    bool IsOnline,
    List<long> TargetChatIds,
    DateTime Timestamp
);
