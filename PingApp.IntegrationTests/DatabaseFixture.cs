using Microsoft.EntityFrameworkCore;
using PingApp.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace PingApp.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("pingapp_test_db")
        .WithUsername("postgres")
        .WithPassword("password")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<PingDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new PingDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}
