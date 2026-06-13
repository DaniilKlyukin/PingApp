using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PingApp.Domain.Aggregates.DeviceAggregate;
using PingApp.Domain.Aggregates.DeviceAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.Entities;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Infrastructure.Data.Converters;

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

        builder.Property(ud => ud.DeviceNickname)
            .HasConversion<NicknameValueConverter>()
            .IsRequired(false)
            .HasMaxLength(DeviceNickname.MaxLength);

        builder.HasOne<User>()
            .WithMany(u => u.UserDevices)
            .HasForeignKey(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Device>()
            .WithMany(d => d.UserDevices)
            .HasForeignKey(ud => ud.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
