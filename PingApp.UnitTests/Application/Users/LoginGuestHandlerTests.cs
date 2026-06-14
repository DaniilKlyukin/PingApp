using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;

namespace PingApp.UnitTests.Application.Users;

public class LoginGuestHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly LoginGuest.Handler _sut;

    public LoginGuestHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _sut = new LoginGuest.Handler(_userRepositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndSaveGuestUser_AndReturnCredentials()
    {
        var command = new LoginGuest.Command();

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsGuest.Should().BeTrue();
        result.Value.IsAdmin.Should().BeFalse();
        result.Value.Username.Should().StartWith("Guest_");

        await _userRepositoryMock.Received(1).AddUserAsync(
            Arg.Is<User>(u => u.IsGuest == true && u.Username.Value.StartsWith("Guest_")),
            Arg.Any<CancellationToken>());
    }
}
