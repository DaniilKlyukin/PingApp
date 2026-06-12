using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PingApp.Application;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Data;
using Serilog;

namespace PingApp.WinForms;

internal static class Program
{
    [STAThread]
    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/pingapp-log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            ApplicationConfiguration.Initialize();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .UseSerilog()
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PingDbContext>();
                await db.Database.MigrateAsync();

                var settingsRepository = scope.ServiceProvider.GetRequiredService<IGlobalSettingsRepository>();
                var intervalStr = await settingsRepository.GetSettingAsync("ScanIntervalSeconds", "10");
                if (int.TryParse(intervalStr, out var intervalSeconds))
                {
                    var scanConfig = scope.ServiceProvider.GetRequiredService<IScanConfiguration>();
                    scanConfig.Interval = TimeSpan.FromSeconds(intervalSeconds);
                }
            }

            var cts = new CancellationTokenSource();
            var hostStartTask = host.StartAsync(cts.Token);

            using (var scope = host.Services.CreateScope())
            {
                var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var mainForm = host.Services.GetRequiredService<MainForm>();
                    System.Windows.Forms.Application.Run(mainForm);

                    var userContext = host.Services.GetRequiredService<IUserContext>();
                    if (userContext.IsGuest && userContext.UserId != UserId.Empty)
                    {
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        await userRepository.DeleteUserAsync(userContext.UserId);
                    }
                }
            }

            cts.Cancel();
            await host.StopAsync();
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Критическая ошибка при запуске или работе приложения");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        var connectionString = "Host=localhost;Database=pingapp_db;Username=postgres;Password=your_password";

        services.AddDbContext<PingDbContext>(options =>
            options.UseNpgsql(connectionString), ServiceLifetime.Transient);

        services.AddApplication(registerBackgroundScanner: true);
        services.AddInfrastructure(connectionString);

        services.AddTransient<MainForm>();
        services.AddTransient<LocalAddressForm>();
        services.AddTransient<UserForm>();
        services.AddTransient<LoginForm>();
        services.AddTransient<AdminForm>();
    }
}