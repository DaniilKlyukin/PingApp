using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.Infrastructure.Data.Converters;

public class NicknameValueConverter : ValueConverter<DeviceNickname, string?>
{
    public NicknameValueConverter() : base(
        nickname => nickname == null ? null : nickname.Value,
        value => DeviceNickname.Create(value).Value
    )
    {
    }

    public override bool ConvertsNulls => true;
}