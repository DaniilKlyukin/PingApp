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

public static class Register
{
    public record Command(string Username, string Password) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
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

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запрос на регистрацию нового пользователя: {Username}", request.Username);

            var usernameResult = Username.Create(request.Username);
            if (usernameResult.IsFailure)
            {
                _logger.LogWarning("Регистрация отклонена: некорректное имя {Username}", request.Username);
                return Result.Failure(usernameResult.Error);
            }

            var username = usernameResult.Value;

            var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
            if (username.Value.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Регистрация отклонена: попытка зарегистрировать зарезервированное имя {Username}", request.Username);
                return UserErrors.ReservedName;
            }

            var existingUser = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Регистрация отклонена: имя пользователя {Username} уже занято", username.Value);
                return UserErrors.DuplicateUsername;
            }

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
            {
                _logger.LogWarning("Регистрация отклонена: пароль для {Username} не соответствует требованиям безопасности", username.Value);
                return passwordResult.Error;
            }

            var newUser = User.Create(username, isGuest: false, isAdmin: false);
            newUser.SetPassword(passwordResult.Value, _passwordHasher);

            await _userRepository.AddUserAsync(newUser, cancellationToken);

            _logger.LogInformation("Пользователь {Username} успешно зарегистрирован. ID: {UserId}", username.Value, newUser.Id.Value);
            return Result.Success();
        }
    }
}