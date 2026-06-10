using PingApp.Domain.Enums;

namespace PingApp.Domain.Entities;

public class Device
{
    public int Id { get; set; }
    public required string Address { get; set; }
    public string? Nickname { get; set; }
    public List<StatusRecord> Statuses { get; set; } = [];

    public DeviceStatusTransition AddStatus(DateTime dateTime, bool atWork)
    {
        var newStatus = new StatusRecord
        {
            DateTime = dateTime,
            AtWork = atWork,
            Device = this
        };

        if (Statuses.Count < 2)
        {
            Statuses.Add(newStatus);
            return DeviceStatusTransition.None;
        }

        var orderedStatuses = Statuses.OrderBy(s => s.DateTime).ToList();
        var lastStatus = orderedStatuses[^1];
        var preLastStatus = orderedStatuses[^2];

        DeviceStatusTransition transition = DeviceStatusTransition.None;

        if (preLastStatus.AtWork == atWork)
        {
            lastStatus.DateTime = dateTime;
        }
        else
        {
            Statuses.Add(newStatus);

            if (!lastStatus.AtWork && atWork)
            {
                transition = DeviceStatusTransition.LoggedIn;
            }
            else if (lastStatus.AtWork && !atWork)
            {
                transition = DeviceStatusTransition.LoggedOut;
            }
        }

        return transition;
    }
}