using PingApp.Domain.Enums;

namespace PingApp.Domain.Entities;

public class Device
{
    public int Id { get; set; }
    public required string Address { get; set; }
    public bool IsAllowedToPing { get; set; } = true;
    public List<StatusRecord> Statuses { get; set; } = [];
    public List<UserDevice> UserDevices { get; set; } = [];

    public DeviceStatusTransition AddStatus(DateTime dateTime, bool atWork)
    {
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

        var newStatus = new StatusRecord
        {
            DateTime = utcDateTime,
            AtWork = atWork,
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
            lastStatus.DateTime = utcDateTime;
            return DeviceStatusTransition.None;
        }

        Statuses.Add(newStatus);
        return atWork ? DeviceStatusTransition.LoggedIn : DeviceStatusTransition.LoggedOut;
    }
}