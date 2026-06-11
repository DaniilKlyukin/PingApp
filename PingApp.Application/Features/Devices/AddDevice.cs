using FluentValidation;
using MediatR;
using PingApp.Application.Interfaces;
using PingApp.Domain.Entities;

namespace PingApp.Application.Features.Devices;

public static class AddDevice
{
    public record Command(string Address, string? Nickname) : IRequest<Unit>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Адрес устройства не должен быть пустым.")
                .Must(BeValidHostOrIp).WithMessage("Некорректный IP-адрес или доменное имя.");
        }

        private bool BeValidHostOrIp(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return false;
            var hostType = Uri.CheckHostName(address.Trim());
            return hostType is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6;
        }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;
        private readonly IUserContext _userContext;

        public Handler(IDeviceRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var addressTrimmed = request.Address.Trim();

            var alreadySubscribed = await _repository.ExistsSubscriptionAsync(_userContext.UserId, addressTrimmed, cancellationToken);
            if (alreadySubscribed) return Unit.Value;

            var device = new Device { Address = addressTrimmed };

            await _repository.AddSubscriptionAsync(_userContext.UserId, device, request.Nickname?.Trim(), cancellationToken);
            return Unit.Value;
        }
    }
}
