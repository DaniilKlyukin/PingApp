using PingApp.Domain.Aggregates.UserAggregate.Common;

namespace PingApp.Application.Interfaces;

public interface IUserContext
{
    UserId UserId { get; set; }
    string? Username { get; set; }
    bool IsAdmin { get; set; }
    bool IsGuest { get; set; }
    bool IsAuthenticated { get; }
}