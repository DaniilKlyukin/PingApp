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
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Interfaces;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Data;
using Serilog;
using Serilog.Exceptions;

namespace PingApp.WinForms;

public class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        InitializeBootstrapLogger();

        try
        {
            ApplicationConfiguration.Initialize();

            var host = BuildHost(args);

            await ConfigureDatabaseAsync(host);

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

        services.AddApplication();
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
    /// Первичная настройка интервала сканирования и инициализация администратора.
    /// </summary>
    private static async Task ConfigureDatabaseAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<PingDbContext>();
        if (!await db.Database.CanConnectAsync())
        {
            MessageBox.Show(
                "Не удалось подключиться к серверу базы данных. Пожалуйста, убедитесь, что служба запущена.",
                "Ошибка подключения",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var settingsRepository = scope.ServiceProvider.GetRequiredService<IGlobalSettingsRepository>();
        var intervalStr = await settingsRepository.GetSettingAsync("ScanIntervalSeconds", "10");

        if (int.TryParse(intervalStr, out var intervalSeconds))
        {
            var scanConfig = scope.ServiceProvider.GetRequiredService<IScanConfiguration>();
            scanConfig.Interval = TimeSpan.FromSeconds(intervalSeconds);
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Program");

        await SeedAdminUserAsync(configuration, userRepository, passwordHasher, logger);
    }

    /// <summary>
    /// Безопасное создание учетной записи администратора на основе конфигурации.
    /// </summary>
    private static async Task SeedAdminUserAsync(
        IConfiguration configuration,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        var adminUsernameRaw = configuration["AdminSettings:Username"];
        var adminPasswordRaw = configuration["AdminSettings:Password"];

        if (string.IsNullOrWhiteSpace(adminUsernameRaw) || string.IsNullOrWhiteSpace(adminPasswordRaw))
        {
            logger.LogWarning("Настройки AdminSettings:Username или AdminSettings:Password не заданы. Пропуск инициализации администратора.");
            return;
        }

        var usernameResult = Username.Create(adminUsernameRaw);
        var passwordResult = Password.Create(adminPasswordRaw);

        if (usernameResult.IsFailure || passwordResult.IsFailure)
        {
            logger.LogError("Не удалось инициализировать администратора: имя пользователя или пароль не соответствуют правилам валидации.");
            return;
        }

        var adminUsername = usernameResult.Value;
        var existingUser = await userRepository.GetUserByUsernameAsync(adminUsername);

        if (existingUser == null)
        {
            var adminUser = User.Create(adminUsername, isGuest: false, isAdmin: true);
            adminUser.SetPassword(passwordResult.Value, passwordHasher);

            await userRepository.AddUserAsync(adminUser);
            logger.LogInformation("Учетная запись администратора успешно создана в базе данных (User: {Username}).", adminUsername.Value);
        }
        else
        {
            if (!existingUser.IsAdmin)
            {
                existingUser.IsAdmin = true;
                await userRepository.UpdateUserAsync(existingUser);
                logger.LogInformation("Существующему пользователю {Username} повышены права до администратора.", adminUsername.Value);
            }
        }
    }

    /// <summary>
    /// Главный жизненный цикл приложения (авторизация, сессии, смена пользователей).
    /// </summary>
    private static async Task RunApplicationLoopAsync(IHost host)
    {
        var cts = new CancellationTokenSource();
        _ = host.StartAsync(cts.Token);

        var keepRunning = true;

        while (keepRunning)
        {
            using var scope = host.Services.CreateScope();
            var loginForm = scope.ServiceProvider.GetRequiredService<LoginForm>();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
                System.Windows.Forms.Application.Run(mainForm);

                var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();

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