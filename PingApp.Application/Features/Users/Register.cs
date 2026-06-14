using MediatR;
using Microsoft.Extensions.Configuration;
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

    public class Handler : IRequestHandler<Command, Result>
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

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var usernameResult = Username.Create(request.Username);
            if (usernameResult.IsFailure)
                return Result.Failure(usernameResult.Error);

            var username = usernameResult.Value;

            var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
            if (username.Value.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
                return UserErrors.ReservedName;

            var existingUser = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);
            if (existingUser != null)
                return UserErrors.DuplicateUsername;

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
                return passwordResult.Error;

            var newUser = User.Create(username, isGuest: false, isAdmin: false);
            newUser.SetPassword(passwordResult.Value, _passwordHasher);

            await _userRepository.AddUserAsync(newUser, cancellationToken);
            return Result.Success();
        }
    }
}
