using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PingApp.Application;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Data;
using Serilog;
using Serilog.Exceptions;

namespace PingApp.WinForms;

internal static class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        InitializeBootstrapLogger();

        try
        {
            ApplicationConfiguration.Initialize();

            var host = BuildHost(args);

            await MigrateAndConfigureDatabaseAsync(host);

            await RunApplicationLoopAsync(host);
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

    /// <summary>
    /// Инициализация базового логгера и самодиагностики Serilog.
    /// </summary>
    private static void InitializeBootstrapLogger()
    {
        Directory.CreateDirectory("logs");
        Serilog.Debugging.SelfLog.Enable(msg =>
            File.AppendAllText("logs/serilog-selflog.txt", $"{DateTime.Now:G} [SelfLog] {msg}{Environment.NewLine}"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    /// <summary>
    /// Построение и конфигурация IoC-контейнера и хоста приложения.
    /// </summary>
    private static IHost BuildHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) => ConfigureServices(services))
            .UseSerilog((context, loggerConfiguration) => ConfigureSerilog(context.Configuration, loggerConfiguration))
            .Build();
    }

    /// <summary>
    /// Регистрация сервисов в DI-контейнере.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=pingapp_db;Username=postgres;Password=your_password";

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

    /// <summary>
    /// Настройка логирования в файлы и Elasticsearch.
    /// </summary>
    private static void ConfigureSerilog(IConfiguration configuration, LoggerConfiguration loggerConfiguration)
    {
        var elasticUri = configuration["ElasticConfiguration:Uri"] ?? "http://localhost:9200";

        loggerConfiguration
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", "PingApp.WinForms")
            .WriteTo.Console()
            .WriteTo.File("logs/pingapp-log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Elasticsearch(new[] { new Uri(elasticUri) }, opts =>
            {
                opts.DataStream = new DataStreamName("logs", "pingapp-winforms", "local");
                opts.BootstrapMethod = Elastic.Ingest.Elasticsearch.BootstrapMethod.Silent;
            });
    }

    /// <summary>
    /// Применение миграций БД и первичная настройка интервала сканирования.
    /// </summary>
    private static async Task MigrateAndConfigureDatabaseAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();

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

    /// <summary>
    /// Главный жизненный цикл приложения (авторизация, сессии, смена пользователей).
    /// </summary>
    private static async Task RunApplicationLoopAsync(IHost host)
    {
        var cts = new CancellationTokenSource();
        _ = host.StartAsync(cts.Token); // Запуск фоновых служб в фоне

        bool keepRunning = true;

        while (keepRunning)
        {
            using var scope = host.Services.CreateScope();
            var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
                System.Windows.Forms.Application.Run(mainForm);

                var userContext = host.Services.GetRequiredService<IUserContext>();

                // Удаление гостевого аккаунта при завершении сессии
                if (userContext.IsGuest && userContext.UserId != UserId.Empty)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(new DeleteUser.Command(userContext.UserId.Value));
                }

                if (mainForm.LogoutRequested)
                {
                    ResetUserContext(userContext);
                    keepRunning = true;
                }
                else
                {
                    keepRunning = false;
                }
            }
            else
            {
                keepRunning = false;
            }
        }

        cts.Cancel();
        await host.StopAsync();
    }

    /// <summary>
    /// Очистка сессии текущего пользователя.
    /// </summary>
    private static void ResetUserContext(IUserContext userContext)
    {
        userContext.UserId = UserId.Empty;
        userContext.Username = null;
        userContext.IsAdmin = false;
        userContext.IsGuest = false;
    }
}