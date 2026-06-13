using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.DeviceAggregate.Entities;

namespace PingApp.Infrastructure.Data.Configurations;

public class StatusRecordConfiguration : IEntityTypeConfiguration<StatusRecord>
{
    public void Configure(EntityTypeBuilder<StatusRecord> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DeviceId)
            .HasConversion(
                id => id.Value,
                value => new DeviceId(value)
            );

        builder.Property(s => s.DateTime)
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );
    }
}
