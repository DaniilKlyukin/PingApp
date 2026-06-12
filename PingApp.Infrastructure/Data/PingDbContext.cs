using Microsoft.EntityFrameworkCore;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.SettingsAggregate;
using PingApp.Domain.Aggregates.UserAggregate;

namespace PingApp.Infrastructure.Data;

public class PingDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<User> Users => Set<User>();
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();

    public PingDbContext(DbContextOptions<PingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PingDbContext).Assembly);
    }
}