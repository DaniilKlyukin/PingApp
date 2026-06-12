using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Common;
using PingApp.Domain.Entities;

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
    }
}
