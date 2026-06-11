using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Entities;

namespace PingApp.Infrastructure.Data.Configurations;

public class StatusRecordConfiguration : IEntityTypeConfiguration<StatusRecord>
{
    public void Configure(EntityTypeBuilder<StatusRecord> builder)
    {
        builder.HasKey(s => s.Id);
    }
}
