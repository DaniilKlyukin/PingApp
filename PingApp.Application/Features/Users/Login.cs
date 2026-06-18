using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;
using PingApp.Domain.Interfaces;

namespace PingApp.Application.Features.Users;

public static class Login
{
    public record Command(string Username, string Password) : IRequest<Result<Response>>;

    public record Response(Guid UserId, string Username, bool IsAdmin, bool IsGuest);

    public sealed class Handler : IRequestHandler<Command, Result<Response>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Попытка входа пользователя: {Username}", request.Username);

            var usernameResult = Username.Create(request.Username);
            if (usernameResult.IsFailure)
            {
                _logger.LogWarning("Неудачный вход: неверный формат имени пользователя");
                return Result.Failure<Response>(usernameResult.Error);
            }

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
            {
                _logger.LogWarning("Неудачный вход: неверный формат пароля");
                return Result.Failure<Response>(passwordResult.Error);
            }

            var username = usernameResult.Value;
            var password = passwordResult.Value;

            var user = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);

            const string dummyHash = "600000:YmFkc2FsdGJhZHNhbHQ=:YmFkaGFzaGJhZGhhc2hiYWRoYXNoYmFkaGFzaA==";
            var hashToVerify = (user != null && !user.IsGuest && !string.IsNullOrEmpty(user.PasswordHash))
                ? user.PasswordHash
                : dummyHash;

            var isPasswordValid = _passwordHasher.VerifyPassword(password.Value, hashToVerify);

            if (user == null || user.IsGuest || !isPasswordValid)
            {
                _logger.LogWarning("Неудачный вход: неверные учетные данные для {Username}", request.Username);
                return UserErrors.InvalidCredentials;
            }

            _logger.LogInformation("Пользователь {Username} успешно авторизован. ID: {UserId}", user.Username.Value, user.Id.Value);
            return new Response(user.Id.Value, user.Username.Value, user.IsAdmin, user.IsGuest);
        }
    }
}