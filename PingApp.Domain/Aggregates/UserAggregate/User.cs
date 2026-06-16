using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;
using PingApp.Domain.Interfaces;

namespace PingApp.Domain.Aggregates.UserAggregate;

public class User : IAggregateRoot
{
    public UserId Id { get; private set; } = UserId.New();
    public Username Username { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public bool IsGuest { get; set; }
    public bool IsAdmin { get; set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private readonly List<UserDevice> _userDevices = [];
    public IReadOnlyCollection<UserDevice> UserDevices => _userDevices.AsReadOnly();

    private User() { }

    public static User Create(Username username, bool isGuest = false, bool isAdmin = false)
    {
        return new User
        {
            Id = UserId.New(),
            Username = username,
            IsGuest = isGuest,
            IsAdmin = isAdmin,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void AddSubscription(DeviceId deviceId, DeviceNickname nickname)
    {
        if (_userDevices.Any(ud => ud.DeviceId == deviceId))
            return;

        var subscription = new UserDevice(Id, deviceId, nickname);
        _userDevices.Add(subscription);
    }

    public void RemoveSubscription(DeviceId deviceId)
    {
        var subscription = _userDevices.FirstOrDefault(ud => ud.DeviceId == deviceId);
        if (subscription != null)
        {
            _userDevices.Remove(subscription);
        }
    }

    public void SetPassword(Password password, IPasswordHasher passwordHasher)
    {
        PasswordHash = passwordHasher.HashPassword(password.Value);
    }
}