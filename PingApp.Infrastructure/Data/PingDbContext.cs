using Microsoft.EntityFrameworkCore;
using PingApp.Domain.Entities;

namespace PingApp.Infrastructure.Data;

public class PingDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<StatusRecord> Statuses => Set<StatusRecord>();

    public PingDbContext(DbContextOptions<PingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.Address)
            .IsUnique();

        modelBuilder.Entity<Device>()
            .HasMany(d => d.Statuses)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
