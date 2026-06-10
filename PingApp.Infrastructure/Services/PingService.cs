using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using System.Net.NetworkInformation;

namespace PingApp.Infrastructure.Services;

public class PingService : IPingService
{
    private readonly ILogger<PingService> _logger;

    public PingService(ILogger<PingService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> PingHostAsync(string address, CancellationToken cancellationToken = default)
    {
        using var ping = new Ping();
        try
        {
            var reply = await ping.SendPingAsync(address, TimeSpan.FromMilliseconds(3000), cancellationToken: cancellationToken);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось выполнить пинг хоста {Address}", address);
            return false;
        }
    }
}
