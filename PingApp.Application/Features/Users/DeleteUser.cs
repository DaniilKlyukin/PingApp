using MediatR;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Users;

public static class DeleteUser
{
    public record Command(Guid UserId) : IRequest<Result>;

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = new UserId(request.UserId);

            if (userId == UserId.Empty)
            {
                return Result.Success();
            }

            await _userRepository.DeleteUserAsync(userId, cancellationToken);
            return Result.Success();
        }
    }
}