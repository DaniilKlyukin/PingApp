using Microsoft.EntityFrameworkCore;
using PingApp.Domain.Entities;

namespace PingApp.Infrastructure.Data;

public class PingDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<StatusRecord> Statuses => Set<StatusRecord>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
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