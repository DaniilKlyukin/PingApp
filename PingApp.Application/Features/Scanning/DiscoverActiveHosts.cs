using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using MediatR;

namespace PingApp.Application.Features.Scanning;

/// <summary>
/// Бизнес-сценарий: Автоматическое обнаружение активных хостов в локальной подсети.
/// </summary>
public static class DiscoverActiveHosts
{
    public record Query : IRequest<List<string>>;

    public class Handler : IRequestHandler<Query, List<string>>
    {
        public async Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var localIp = GetLocalIpAddress();
            if (localIp == null) return [];

            var ipBytes = localIp.GetAddressBytes();

            var baseSubnetIp = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}";

            var tasks = new List<Task<(string Ip, bool IsActive)>>();

            for (int i = 1; i <= 254; i++)
            {
                var targetIp = $"{baseSubnetIp}.{i}";
                tasks.Add(PingHostAsync(targetIp, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);

            return results
                .Where(r => r.IsActive)
                .Select(r => r.Ip)
                .OrderBy(ip => ip)
                .ToList();
        }

        private IPAddress? GetLocalIpAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) return null;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            return host.AddressList.FirstOrDefault(ip =>
                ip.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ip));
        }

        private async Task<(string Ip, bool IsActive)> PingHostAsync(string ip, CancellationToken cancellationToken)
        {
            using var ping = new Ping();
            try
            {
                if (!IPAddress.TryParse(ip, out var ipAddress))
                {
                    return (ip, false);
                }

                var reply = await ping.SendPingAsync(
                    ipAddress,
                    TimeSpan.FromMilliseconds(500),
                    cancellationToken: cancellationToken);

                return (ip, reply.Status == IPStatus.Success);
            }
            catch
            {
                return (ip, false);
            }
        }
    }
}