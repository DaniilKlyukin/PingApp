using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.Application.Interfaces;

public interface IUserContext
{
    UserId UserId { get; set; }
    Username? Username { get; set; }
    bool IsAdmin { get; set; }
    bool IsGuest { get; set; }
    bool IsAuthenticated { get; }
}