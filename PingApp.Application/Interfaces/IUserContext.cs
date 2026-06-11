namespace PingApp.Application.Interfaces;

public interface IUserContext
{
    int UserId { get; set; }
    string? Username { get; set; }
    bool IsAdmin { get; set; }
    bool IsGuest { get; set; }
    bool IsAuthenticated { get; }
}