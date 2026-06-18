using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.EntityFrameworkCore;
using PingApp.Application;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Interfaces;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Data;
using PingApp.Worker.BackgroundServices;
using Serilog;
using Serilog.Exceptions;

namespace PingApp.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog-Internal] {msg}"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, loggerConfiguration) =>
                {
                    var elasticUri = hostContext.Configuration["ElasticConfiguration:Uri"]
                                     ?? "http://elasticsearch:9200";

                    loggerConfiguration
                        .MinimumLevel.Debug()
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .Enrich.WithProperty("Application", "PingApp.Worker")
                        .WriteTo.Console()
                        .WriteTo.Elasticsearch(new[] { new Uri(elasticUri) }, opts =>
                        {
                            opts.DataStream = new DataStreamName("logs", "pingapp-worker", "production");
                            opts.BootstrapMethod = Elastic.Ingest.Elasticsearch.BootstrapMethod.Silent;
                        });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    string connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection")
                        ?? "Host=pingapp_db;Database=pingapp_db;Username=postgres;Password=your_password";

                    services.AddApplication();
                    services.AddInfrastructure(connectionString);

                    services.AddHostedService<DeviceScanBackgroundService>();
                    services.AddHostedService<GuestCleanupBackgroundService>();
                })
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<PingDbContext>();
                await db.Database.MigrateAsync();

                await SeedAdminUserAsync(services);
            }

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Ошибка запуска фонового воркера в Docker");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Инициализация учетной записи администратора в БД, если она отсутствует.
    /// </summary>
    private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        var adminUsernameRaw = configuration["AdminSettings:Username"];
        var adminPasswordRaw = configuration["AdminSettings:Password"];

        if (string.IsNullOrWhiteSpace(adminUsernameRaw) || string.IsNullOrWhiteSpace(adminPasswordRaw))
        {
            logger.LogWarning("Параметры конфигурации AdminSettings:Username или AdminSettings:Password не заданы. Пропуск инициализации администратора.");
            return;
        }

        var usernameResult = Username.Create(adminUsernameRaw);
        var passwordResult = Password.Create(adminPasswordRaw);

        if (usernameResult.IsFailure || passwordResult.IsFailure)
        {
            logger.LogError("Не удалось инициализировать администратора воркера: данные конфигурации не соответствуют доменным требованиям.");
            return;
        }

        var adminUsername = usernameResult.Value;
        var existingUser = await userRepository.GetUserByUsernameAsync(adminUsername);

        if (existingUser == null)
        {
            var adminUser = User.Create(adminUsername, isGuest: false, isAdmin: true);
            adminUser.SetPassword(passwordResult.Value, passwordHasher);

            await userRepository.AddUserAsync(adminUser);
            logger.LogInformation("Учетная запись администратора успешно создана в базе данных из фонового воркера (User: {Username}).", adminUsername.Value);
        }
        else
        {
            if (!existingUser.IsAdmin)
            {
                existingUser.IsAdmin = true;
                await userRepository.UpdateUserAsync(existingUser);
                logger.LogInformation("Права существующего пользователя {Username} повышены до администратора силами воркера.", adminUsername.Value);
            }
        }
    }
}