using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Common;
using PingApp.Domain.ValueObjects;

namespace PingApp.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
             .HasConversion(
                 id => id.Value,
                 value => new DeviceId(value)
             );

        builder.Property(d => d.Address)
            .HasConversion(
                address => address.Value,
                value => DeviceAddress.Create(value).Value
            )
            .IsRequired();

        builder.HasIndex(d => d.Address)
            .IsUnique();

        builder.Property(d => d.IsAllowedToPing)
            .HasDefaultValue(true);

        builder.Property(d => d.IsVisibleToUsers)
            .HasDefaultValue(false);

        builder.HasMany(d => d.Statuses)
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}