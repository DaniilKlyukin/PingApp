namespace PingApp.Application.Features.Scanning.Common;

/// <summary>
/// Контракт управления параметрами фонового сканирования.
/// </summary>
public interface IScanConfiguration
{
    bool IsEnabled { get; set; }
    TimeSpan Interval { get; set; }
    bool SaveToDatabase { get; set; }
}
