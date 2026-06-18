using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<Login.Handler> _loggerMock;
    private readonly Login.Handler _sut;

    public LoginHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();
        _loggerMock = Substitute.For<ILogger<Login.Handler>>();

        _sut = new Login.Handler(
            _userRepositoryMock,
            _passwordHasherMock,
            _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalidFormat()
    {
        var command = new Login.Command("", "pass");

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidUsernameEmpty);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAdminExistsInDbAndCredentialsAreCorrect()
    {
        var command = new Login.Command("admin", "correctAdminPassword");
        var username = Username.Create("admin").Value;

        var adminInDb = User.Create(username, isGuest: false, isAdmin: true);

        _passwordHasherMock.HashPassword("correctAdminPassword").Returns("admin_hashed_val");
        adminInDb.SetPassword(Password.Create("correctAdminPassword").Value, _passwordHasherMock);

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns(adminInDb);

        _passwordHasherMock.VerifyPassword("correctAdminPassword", "admin_hashed_val")
            .Returns(true);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("admin");
        result.Value.IsAdmin.Should().BeTrue();
        result.Value.IsGuest.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var command = new Login.Command("unknown_user", "password");
        var username = Username.Create("unknown_user").Value;

        _userRepositoryMock.GetUserByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

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

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("regular_user");
        result.Value.IsAdmin.Should().BeFalse();
        result.Value.IsGuest.Should().BeFalse();
    }
}