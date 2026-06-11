namespace PingApp.Application.Features.Statistics.Common;

public class WorkStatus
{
    public DateTime DateTime { get; set; }
    public bool AtWork { get; set; }

    public WorkStatus(DateTime dateTime, bool atWork)
    {
        DateTime = dateTime;
        AtWork = atWork;
    }
}