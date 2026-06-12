using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;

namespace PingApp.Application.Features.Users;

public class UserContext : IUserContext
{
    public UserId UserId { get; set; } = UserId.Empty;
    public string? Username { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsGuest { get; set; }
    public bool IsAuthenticated => UserId != UserId.Empty;
}
