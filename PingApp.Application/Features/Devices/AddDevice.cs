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

            RuleFor(x => x.Nickname)
                .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов.");
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

        public Handler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var addressTrimmed = request.Address.Trim();
            var exists = await _repository.ExistsAsync(addressTrimmed, cancellationToken);
            if (exists) return Unit.Value;

            var device = new Device
            {
                Address = addressTrimmed,
                Nickname = request.Nickname?.Trim()
            };

            await _repository.AddAsync(device, cancellationToken);
            return Unit.Value;
        }
    }
}