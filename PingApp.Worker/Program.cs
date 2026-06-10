using Microsoft.EntityFrameworkCore;
using PingApp.Application;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Data;
using Serilog;

namespace PingApp.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    string connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection")
                        ?? "Host=localhost;Database=pingapp_db;Username=postgres;Password=your_password";

                    services.AddApplication(registerBackgroundScanner: true); // Воркер в докере ВСЕГДА выполняет сканирование
                    services.AddInfrastructure(connectionString);
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