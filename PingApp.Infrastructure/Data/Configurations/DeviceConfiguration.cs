using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;
using PingApp.Domain.Aggregates.DeviceAggregate.ValueObjects;

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
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(d => d.Address)
            .IsUnique();

        builder.Property(d => d.IsAllowedToPing)
            .HasDefaultValue(true);

        builder.Property(d => d.IsVisibleToUsers)
            .HasDefaultValue(false);

        builder.Property(d => d.LastKnownStatus);
        builder.Property(d => d.LastStatusChangedUtc);

        builder.HasMany<StatusRecord>()
            .WithOne(s => s.Device)
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.UserDevices)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}