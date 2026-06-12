using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Common;
using PingApp.Domain.Entities;

namespace PingApp.Infrastructure.Data.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(ud => new { ud.UserId, ud.DeviceId });

        builder.Property(ud => ud.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value)
            );

        builder.Property(ud => ud.DeviceId)
            .HasConversion(
                id => id.Value,
                value => new DeviceId(value)
            );

        builder.HasOne(ud => ud.User)
            .WithMany(u => u.UserDevices)
            .HasForeignKey(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ud => ud.Device)
            .WithMany(d => d.UserDevices)
            .HasForeignKey(ud => ud.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
