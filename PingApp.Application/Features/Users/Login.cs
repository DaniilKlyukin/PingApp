using MediatR;
using Microsoft.Extensions.Configuration;
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

        public Handler(IUserRepository userRepository, IPasswordHasher passwordHasher, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var usernameResult = Username.Create(request.Username);
            if (usernameResult.IsFailure)
                return Result.Failure<Response>(usernameResult.Error);

            var username = usernameResult.Value;

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
                return Result.Failure<Response>(passwordResult.Error);

            var password = passwordResult.Value;

            var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
            var adminPass = _configuration["AdminSettings:Password"] ?? "admin";

            if (username.Value.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
            {
                if (password.Value != adminPass)
                    return UserErrors.InvalidCredentials;

                var dbAdmin = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);

                if (dbAdmin == null)
                {
                    dbAdmin = User.Create(username, isGuest: false, isAdmin: true);
                    await _userRepository.AddUserAsync(dbAdmin, cancellationToken);
                }
                else if (!dbAdmin.IsAdmin)
                {
                    dbAdmin.IsAdmin = true;
                    await _userRepository.UpdateUserAsync(dbAdmin, cancellationToken);
                }

                return new Response(dbAdmin.Id.Value, dbAdmin.Username.Value, dbAdmin.IsAdmin, dbAdmin.IsGuest);
            }

            var user = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);

            if (user == null ||
                user.IsGuest ||
                string.IsNullOrEmpty(user.PasswordHash) ||
                !_passwordHasher.VerifyPassword(password.Value, user.PasswordHash))
            {
                return UserErrors.InvalidCredentials;
            }

            return new Response(user.Id.Value, user.Username.Value, user.IsAdmin, user.IsGuest);
        }
    }
}
