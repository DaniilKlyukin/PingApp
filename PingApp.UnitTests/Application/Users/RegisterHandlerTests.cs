using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Interfaces;

namespace PingApp.UnitTests.Application.Users;

public class RegisterHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly ILogger<Register.Handler> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly Register.Handler _sut;

    public RegisterHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();
        _loggerMock = Substitute.For<ILogger<Register.Handler>>();

        var inMemorySettings = new Dictionary<string, string> {
            {"AdminSettings:Username", "admin"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new Register.Handler(
            _userRepositoryMock,
            _passwordHasherMock,
            _configuration,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameIsTooShort()
    {
        var command = new Register.Command(new string('a', Username.MinLength - 1), "securePassword123");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UsernameTooShort);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameIsReservedAdminName()
    {
        var command = new Register.Command("admin", "securePassword123");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.ReservedName);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameAlreadyExists()
    {
        var command = new Register.Command("existing_user", "securePassword123");
        var username = Username.Create("existing_user").Value;
        var existingUser = User.Create(username);

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.DuplicateUsername);
    }

    [Fact]
    public async Task Handle_ShouldRegisterAndSaveUser_WhenRequestIsValid()
    {
        var command = new Register.Command("new_user", "securePassword123");
        var username = Username.Create("new_user").Value;

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _passwordHasherMock.HashPassword("securePassword123").Returns("hashed_secure_pass");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _userRepositoryMock.Received(1).AddUserAsync(
            Arg.Is<User>(u => u.Username == username && u.PasswordHash == "hashed_secure_pass" && !u.IsAdmin && !u.IsGuest),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultAdminName_WhenConfigurationIsEmpty()
    {
        var emptyConfig = new ConfigurationBuilder().Build();
        var handler = new Register.Handler(
            _userRepositoryMock,
            _passwordHasherMock,
            emptyConfig,
            _loggerMock);
        var command = new Register.Command("admin", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.ReservedName);
    }
}