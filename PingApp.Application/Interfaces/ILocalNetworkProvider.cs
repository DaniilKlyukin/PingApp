using System.Net;

namespace PingApp.Application.Interfaces;

public interface ILocalNetworkProvider
{
    IPAddress? GetLocalIpAddress();
    Task<(string Ip, bool IsActive)> PingHostForDiscoveryAsync(string ip, CancellationToken cancellationToken);
}
