using PingApp.Application.Interfaces;

namespace PingApp.Worker.BackgroundServices;

public class GuestCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GuestCleanupBackgroundService> _logger;
    private readonly TimeSpan _checkPeriod = TimeSpan.FromHours(1);
    private readonly TimeSpan _guestLifetime = TimeSpan.FromDays(1);

    public GuestCleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<GuestCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновая служба очистки гостей запущена в контейнере воркера.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var thresholdTime = DateTime.UtcNow - _guestLifetime;
                _logger.LogInformation("Запуск очистки гостей, созданных ранее {Threshold}", thresholdTime);

                await userRepository.DeleteExpiredGuestsAsync(thresholdTime, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка во время очистки устаревших гостевых сессий.");
            }

            try
            {
                await Task.Delay(_checkPeriod, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Фоновая служба очистки гостей остановлена.");
    }
}