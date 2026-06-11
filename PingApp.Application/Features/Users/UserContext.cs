using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Users;

public class UserContext : IUserContext
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsGuest { get; set; }
    public bool IsAuthenticated => UserId > 0;
}
