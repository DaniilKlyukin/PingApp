using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PingApp.Application.Features.Scanning.Common;

namespace PingApp.Application.Features.Scanning.Background;

/// <summary>
/// Фоновая служба непрерывного циклического мониторинга сетевых устройств.
/// </summary>
public class DeviceScanBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScanConfiguration _config;
    private readonly ILogger<DeviceScanBackgroundService> _logger;

    public DeviceScanBackgroundService(
        IServiceScopeFactory scopeFactory,
        IScanConfiguration config,
        ILogger<DeviceScanBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис мониторинга устройств успешно запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_config.IsEnabled)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new ScanAllDevices.Command(), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Во время выполнения фонового опроса устройств произошло исключение.");
            }

            try
            {
                await Task.Delay(_config.Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Фоновый сервис мониторинга устройств завершил работу.");
    }
}