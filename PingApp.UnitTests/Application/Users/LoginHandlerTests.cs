using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Interfaces;

namespace PingApp.UnitTests.Application.Users;

public class LoginHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly IConfiguration _configuration;
    private readonly Login.Handler _sut;

    public LoginHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();

        var inMemorySettings = new Dictionary<string, string> {
            {"AdminSettings:Username", "admin"},
            {"AdminSettings:Password", "adminPass"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new Login.Handler(_userRepositoryMock, _passwordHasherMock, _configuration);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalidFormat()
    {
        var command = new Login.Command("", "pass");
        
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidUsernameEmpty);
    }

    [Fact]
    public async Task Handle_ShouldCreateAdminInDb_WhenAdminLogsInFirstTime()
    {
        var command = new Login.Command("admin", "adminPass");
        var username = Username.Create("admin").Value;

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("admin");
        result.Value.IsAdmin.Should().BeTrue();

        await _userRepositoryMock.Received(1).AddUserAsync(
            Arg.Is<User>(u => u.Username == username && u.IsAdmin && !u.IsGuest),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserToAdmin_WhenAdminLogsInButNotMarkedAsAdminInDb()
    {
        var command = new Login.Command("admin", "adminPass");
        var username = Username.Create("admin").Value;
        var existingNonAdminUser = User.Create(username, isGuest: false, isAdmin: false);

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns(existingNonAdminUser);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingNonAdminUser.IsAdmin.Should().BeTrue();

        await _userRepositoryMock.Received(1).UpdateUserAsync(existingNonAdminUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var command = new Login.Command("unknown_user", "password");
        var username = Username.Create("unknown_user").Value;

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRegularUserLogsInWithCorrectPassword()
    {
        var command = new Login.Command("regular_user", "correctPassword");
        var username = Username.Create("regular_user").Value;

        var userInDb = User.Create(username, isGuest: false, isAdmin: false);

        _passwordHasherMock.HashPassword("correctPassword").Returns("some_hashed_value");
        userInDb.SetPassword(Password.Create("correctPassword").Value, _passwordHasherMock);

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns(userInDb);

        _passwordHasherMock.VerifyPassword("correctPassword", "some_hashed_value")
            .Returns(true);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("regular_user");
        result.Value.IsAdmin.Should().BeFalse();
        result.Value.IsGuest.Should().BeFalse();
    }
}
