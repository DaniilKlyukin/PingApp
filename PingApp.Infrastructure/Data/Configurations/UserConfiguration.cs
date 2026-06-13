using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value)
            );

        builder.Property(u => u.Username)
            .HasConversion(
                username => username.Value,
                value => Username.Create(value).Value
            )
            .HasMaxLength(Username.MaxLength)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Navigation(u => u.UserDevices)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
