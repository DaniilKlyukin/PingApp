using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PingApp.Application;
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
            }

            var cts = new CancellationTokenSource();
            var hostStartTask = host.StartAsync(cts.Token);

            var mainForm = host.Services.GetRequiredService<MainForm>();
            System.Windows.Forms.Application.Run(mainForm);

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

        string connectionString = "Host=localhost;Database=pingapp_db;Username=postgres;Password=your_password";

        services.AddApplication(registerBackgroundScanner: true); // WinForms может запускать сканер локально
        services.AddInfrastructure(connectionString);

        services.AddTransient<MainForm>();
        services.AddTransient<LocalAddressForm>();
        services.AddTransient<UserForm>();
        services.AddTransient<DiscoverHostsForm>();
    }
}