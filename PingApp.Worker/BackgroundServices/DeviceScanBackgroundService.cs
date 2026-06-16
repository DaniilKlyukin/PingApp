using MediatR;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Interfaces;

namespace PingApp.Worker.BackgroundServices;

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
        _logger.LogInformation("Фоновый сервис мониторинга устройств успешно запущен в контейнере воркера.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentInterval = _config.Interval;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var settingsRepository = scope.ServiceProvider.GetRequiredService<IGlobalSettingsRepository>();

                var intervalStr = await settingsRepository.GetSettingAsync("ScanIntervalSeconds", "10", stoppingToken);
                if (int.TryParse(intervalStr, out var intervalSeconds))
                {
                    _config.Interval = TimeSpan.FromSeconds(intervalSeconds);
                    currentInterval = _config.Interval;
                }

                await mediator.Send(new ScanAllDevices.Command(), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Во время выполнения фонового опроса устройств произошло исключение.");
            }

            try
            {
                await Task.Delay(currentInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Фоновый сервис мониторинга устройств завершил работу.");
    }
}