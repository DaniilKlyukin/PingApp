using Microsoft.EntityFrameworkCore;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.SettingsAggregate;
using PingApp.Domain.Aggregates.TelegramAggregate;
using PingApp.Domain.Aggregates.UserAggregate;

namespace PingApp.Infrastructure.Data;

public class PingDbContext : DbContext, IUnitOfWork
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<User> Users => Set<User>();
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();
    public DbSet<TelegramSubscription> TelegramSubscriptions => Set<TelegramSubscription>();
    public PingDbContext(DbContextOptions<PingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PingDbContext).Assembly);
    }
}