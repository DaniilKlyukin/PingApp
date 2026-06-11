using MediatR;
using PingApp.Application.Interfaces;

namespace PingApp.Application.Features.Devices;

public static class GetAllowedDevices
{
    public record Query : IRequest<List<string>>;

    public class Handler : IRequestHandler<Query, List<string>>
    {
        private readonly IDeviceRepository _repository;

        public Handler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowed = await _repository.GetAllowedDevicesAsync(cancellationToken);
            return allowed.Select(d => d.Address).ToList();
        }
    }
}