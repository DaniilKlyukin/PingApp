using PingApp.Domain.Common;
using PingApp.Domain.Enums;
using PingApp.Domain.ValueObjects;

namespace PingApp.Domain.Entities;

public class Device
{
    public DeviceId Id { get; set; } = DeviceId.New();
    public required DeviceAddress Address { get; set; }
    public bool IsAllowedToPing { get; set; } = true;
    public bool IsVisibleToUsers { get; set; } = false;
    public List<StatusRecord> Statuses { get; set; } = [];
    public List<UserDevice> UserDevices { get; set; } = [];

    public DeviceStatusTransition AddStatus(DateTime dateTime, bool atWork)
    {
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

        var newStatus = new StatusRecord
        {
            DateTime = utcDateTime,
            AtWork = atWork,
            DeviceId = Id,
            Device = this
        };

        if (Statuses.Count == 0)
        {
            Statuses.Add(newStatus);
            return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.None;
        }

        var lastStatus = Statuses.OrderByDescending(s => s.DateTime).First();

        if (lastStatus.AtWork == atWork)
        {
            return DeviceStatusTransition.None;
        }

        Statuses.Add(newStatus);
        return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.LoggedOut;
    }
}