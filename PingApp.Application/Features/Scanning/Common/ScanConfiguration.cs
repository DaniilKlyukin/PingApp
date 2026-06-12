namespace PingApp.Application.Features.Scanning.Common;

/// <summary>
/// Потокобезопасная реализация конфигурации фонового сканирования.
/// </summary>
public class ScanConfiguration : IScanConfiguration
{
    private readonly object _lock = new();
    private TimeSpan _interval = TimeSpan.FromSeconds(10);

    public TimeSpan Interval
    {
        get { lock (_lock) return _interval; }
        set { lock (_lock) _interval = value; }
    }
}