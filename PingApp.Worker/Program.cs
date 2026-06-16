using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.EntityFrameworkCore;
using PingApp.Application;
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
                var db = scope.ServiceProvider.GetRequiredService<PingDbContext>();
                await db.Database.MigrateAsync();
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
}