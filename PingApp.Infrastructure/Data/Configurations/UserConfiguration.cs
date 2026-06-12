using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Common;
using PingApp.Domain.Entities;
using PingApp.Domain.ValueObjects;

namespace PingApp.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

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
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();
    }
}
