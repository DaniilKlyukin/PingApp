using PingApp.Domain.Common;
using PingApp.Domain.ValueObjects;

namespace PingApp.Domain.Entities;

public class User
{
    public UserId Id { get; set; } = UserId.New();
    public required Username Username { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsGuest { get; set; }
    public bool IsAdmin { get; set; }
    public List<UserDevice> UserDevices { get; set; } = [];
}
