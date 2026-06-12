using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using PingApp.Domain.Entities;
using PingApp.Domain.Enums;
using PingApp.Domain.ValueObjects;

namespace PingApp.Application.Features.Scanning;

public static class ScanAllDevices
{
    public record Command : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IDeviceRepository _repository;
        private readonly IPingService _pingService;
        private readonly IMediator _mediator;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IDeviceRepository repository,
            IPingService pingService,
            IMediator mediator,
            ILogger<Handler> logger)
        {
            _repository = repository;
            _pingService = pingService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запуск фонового сканирования подсети...");

            var localIp = GetLocalIpAddress();
            var activeIps = new List<string>();

            if (localIp != null)
            {
                var ipBytes = localIp.GetAddressBytes();
                var baseSubnetIp = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}";
                var discoveryTasks = new List<Task<(string Ip, bool IsActive)>>();

                for (int i = 1; i <= 254; i++)
                {
                    var targetIp = $"{baseSubnetIp}.{i}";
                    discoveryTasks.Add(PingHostForDiscoveryAsync(targetIp, cancellationToken));
                }

                var results = await Task.WhenAll(discoveryTasks);
                activeIps = results
                    .Where(r => r.IsActive)
                    .Select(r => r.Ip)
                    .ToList();
            }

            foreach (var ip in activeIps)
            {
                var addressResult = DeviceAddress.Create(ip);
                if (addressResult.IsSuccess)
                {
                    var existing = await _repository.GetByAddressAsync(addressResult.Value, cancellationToken);
                    if (existing == null)
                    {
                        var newDevice = new Device
                        {
                            Address = addressResult.Value,
                            IsAllowedToPing = true,
                            IsVisibleToUsers = false
                        };
                        await _repository.AddDeviceAsync(newDevice, cancellationToken);
                    }
                }
            }

            var allDevices = await _repository.GetAllDevicesAsync(cancellationToken);
            var devicesToPing = allDevices.Where(d => d.IsAllowedToPing).ToList();

            _logger.LogInformation("Пингуем разрешенные устройства (активных к опросу: {Count} из {Total})", devicesToPing.Count, allDevices.Count);

            var pingTasks = devicesToPing.Select(async device =>
            {
                try
                {
                    var isOnline = await _pingService.PingHostAsync(device.Address.Value, cancellationToken);
                    var transition = device.AddStatus(DateTime.Now, isOnline);

                    await _repository.UpdateAsync(device, cancellationToken);

                    if (transition != DeviceStatusTransition.None)
                    {
                        await _mediator.Publish(new DeviceStatusChanged.Notification(
                             device.Address.Value,
                             transition == DeviceStatusTransition.LoggedIn,
                             DateTime.Now
                         ), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при опросе устройства {Address}", device.Address.Value);
                }
            });

            await Task.WhenAll(pingTasks);
            return Unit.Value;
        }

        private IPAddress? GetLocalIpAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) return null;

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                                         ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ToList();

            foreach (var ni in interfaces)
            {
                var name = ni.Name.ToLowerInvariant();
                var description = ni.Description.ToLowerInvariant();

                if (name.Contains("docker") || name.Contains("vbox") || name.Contains("virtual") ||
                    name.Contains("wsl") || name.Contains("vmware") || name.Contains("hyper-v") || name.Contains("vnet") ||
                    description.Contains("docker") || description.Contains("vbox") || description.Contains("virtual") ||
                    description.Contains("wsl") || description.Contains("vmware") || description.Contains("hyper-v") || description.Contains("vnet"))
                {
                    continue;
                }

                var ipProps = ni.GetIPProperties();
                var ipv4 = ipProps.UnicastAddresses
                    .FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4 != null)
                {
                    return ipv4.Address;
                }
            }

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                return host.AddressList.FirstOrDefault(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip));
            }
            catch
            {
                return null;
            }
        }

        private async Task<(string Ip, bool IsActive)> PingHostForDiscoveryAsync(string ip, CancellationToken cancellationToken)
        {
            using var ping = new Ping();
            try
            {
                if (!IPAddress.TryParse(ip, out var ipAddress)) return (ip, false);
                var reply = await ping.SendPingAsync(ipAddress, TimeSpan.FromMilliseconds(500), cancellationToken: cancellationToken);
                return (ip, reply.Status == IPStatus.Success);
            }
            catch
            {
                return (ip, false);
            }
        }
    }
}