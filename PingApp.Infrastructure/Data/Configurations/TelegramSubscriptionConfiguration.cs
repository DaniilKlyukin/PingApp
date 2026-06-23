using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Aggregates.TelegramAggregate;

namespace PingApp.Infrastructure.Data.Configurations;

public class TelegramSubscriptionConfiguration : IEntityTypeConfiguration<TelegramSubscription>
{
    public void Configure(EntityTypeBuilder<TelegramSubscription> builder)
    {
        builder.ToTable("TelegramSubscriptions", "telegram");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ChatId)
            .IsRequired();

        builder.Property(s => s.DeviceAddress)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(s => new { s.ChatId, s.DeviceAddress })
            .IsUnique();
    }
}
