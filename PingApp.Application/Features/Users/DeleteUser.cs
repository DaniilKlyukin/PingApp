using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Common;

namespace PingApp.Application.Features.Users;

public static class DeleteUser
{
    public record Command(Guid UserId) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IUserRepository userRepository, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = new UserId(request.UserId);

            if (userId == UserId.Empty)
            {
                _logger.LogWarning("Попытка удаления пустого или некорректного UserId.");
                return Result.Success();
            }

            _logger.LogInformation("Запуск процедуры удаления учетной записи {UserId}", userId.Value);

            await _userRepository.DeleteUserAsync(userId, cancellationToken);

            _logger.LogInformation("Учетная запись {UserId} успешно удалена из базы данных", userId.Value);
            return Result.Success();
        }
    }
}