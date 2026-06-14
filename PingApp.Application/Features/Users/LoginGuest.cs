using MediatR;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Users;

public static class LoginGuest
{
    public record Command : IRequest<Result<Login.Response>>;

    public class Handler : IRequestHandler<Command, Result<Login.Response>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<Login.Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rawGuestUsername = $"Guest_{Guid.NewGuid().ToString()[..8]}";

            var usernameResult = Username.Create(rawGuestUsername);
            if (usernameResult.IsFailure)
                return Result.Failure<Login.Response>(usernameResult.Error);

            var guestUser = User.Create(usernameResult.Value, isGuest: true, isAdmin: false);

            await _userRepository.AddUserAsync(guestUser, cancellationToken);

            return new Login.Response(
                guestUser.Id.Value,
                guestUser.Username.Value,
                guestUser.IsAdmin,
                guestUser.IsGuest);
        }
    }
}