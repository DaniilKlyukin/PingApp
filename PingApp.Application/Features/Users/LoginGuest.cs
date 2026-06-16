using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Users;

public static class LoginGuest
{
    public record Command : IRequest<Result<Login.Response>>;

    public sealed class Handler : IRequestHandler<Command, Result<Login.Response>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IUserRepository userRepository, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result<Login.Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rawGuestUsername = $"Guest_{Guid.NewGuid().ToString()[..8]}";

            _logger.LogInformation("Запрос на гостевой вход. Сгенерировано имя пользователя: {Username}", rawGuestUsername);

            var usernameResult = Username.Create(rawGuestUsername);
            if (usernameResult.IsFailure)
            {
                _logger.LogWarning("Не удалось сформировать имя гостевого пользователя: {Error}", usernameResult.Error.Message);
                return Result.Failure<Login.Response>(usernameResult.Error);
            }

            var guestUser = User.Create(usernameResult.Value, isGuest: true, isAdmin: false);

            await _userRepository.AddUserAsync(guestUser, cancellationToken);

            _logger.LogInformation(
                "Успешная регистрация гостевого сеанса. ID пользователя: {UserId}, Имя: {Username}",
                guestUser.Id.Value,
                guestUser.Username.Value);

            return new Login.Response(
                guestUser.Id.Value,
                guestUser.Username.Value,
                guestUser.IsAdmin,
                guestUser.IsGuest);
        }
    }
}