using PingApp.Application.Interfaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PingApp.Infrastructure.Services;

public class LocalNetworkProvider : ILocalNetworkProvider
{
    public IPAddress? GetLocalIpAddress()
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

    public async Task<(string Ip, bool IsActive)> PingHostForDiscoveryAsync(string ip, CancellationToken cancellationToken)
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