namespace PingApp.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsGuest { get; set; }
    public bool IsAdmin { get; set; }
    public List<UserDevice> UserDevices { get; set; } = [];
}
