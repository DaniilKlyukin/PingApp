using FluentAssertions;
using NSubstitute;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;

namespace PingApp.UnitTests.Application.Users;

public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly DeleteUser.Handler _sut;

    public DeleteUserHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _sut = new DeleteUser.Handler(_userRepositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessImmediately_WhenUserIdIsEmpty()
    {
        var command = new DeleteUser.Command(Guid.Empty);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _userRepositoryMock.DidNotReceiveWithAnyArgs().DeleteUserAsync(default!, default);
    }

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenUserIdIsValid()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteUser.Command(userId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _userRepositoryMock.Received(1).DeleteUserAsync(
            Arg.Is<UserId>(id => id.Value == userId),
            Arg.Any<CancellationToken>());
    }
}
