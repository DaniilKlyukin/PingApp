using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Попытка входа пользователя: {Username}", request.Username);

            var usernameResult = Username.Create(request.Username);
            if (usernameResult.IsFailure)
            {
                _logger.LogWarning("Неудачный вход: некорректный формат имени пользователя {Username}", request.Username);
                return Result.Failure<Response>(usernameResult.Error);
            }

            var username = usernameResult.Value;

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
            {
                _logger.LogWarning("Неудачный вход: некорректный формат пароля для {Username}", request.Username);
                return Result.Failure<Response>(passwordResult.Error);
            }

            var password = passwordResult.Value;

            var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
            var adminPass = _configuration["AdminSettings:Password"] ?? "admin";

            if (username.Value.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
            {
                if (password.Value != adminPass)
                {
                    _logger.LogWarning("Попытка входа под администратором {Username} отклонена: неверный пароль", request.Username);
                    return UserErrors.InvalidCredentials;
                }

                var dbAdmin = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);
                if (dbAdmin == null)
                {
                    dbAdmin = User.Create(username, isGuest: false, isAdmin: true);
                    await _userRepository.AddUserAsync(dbAdmin, cancellationToken);
                    _logger.LogInformation("Создана новая учетная запись администратора {Username} в БД", username.Value);
                }
                else if (!dbAdmin.IsAdmin)
                {
                    dbAdmin.IsAdmin = true;
                    await _userRepository.UpdateUserAsync(dbAdmin, cancellationToken);
                }

                _logger.LogInformation("Администратор {Username} успешно авторизован. ID: {UserId}", username.Value, dbAdmin.Id.Value);
                return new Response(dbAdmin.Id.Value, dbAdmin.Username.Value, dbAdmin.IsAdmin, dbAdmin.IsGuest);
            }

            var user = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);

            if (user == null ||
                user.IsGuest ||
                string.IsNullOrEmpty(user.PasswordHash) ||
                !_passwordHasher.VerifyPassword(password.Value, user.PasswordHash))
            {
                _logger.LogWarning("Неудачный вход: неверные учетные данные для {Username}", request.Username);
                return UserErrors.InvalidCredentials;
            }

            _logger.LogInformation("Пользователь {Username} успешно авторизован. ID: {UserId}", user.Username.Value, user.Id.Value);
            return new Response(user.Id.Value, user.Username.Value, user.IsAdmin, user.IsGuest);
        }
    }
}