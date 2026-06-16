namespace PingApp.Application.Interfaces;

public interface IPingService
{
    Task<bool> PingHostAsync(string address, CancellationToken cancellationToken = default);
}
