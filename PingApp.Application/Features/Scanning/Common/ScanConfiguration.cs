namespace PingApp.Application.Features.Scanning.Common;

/// <summary>
/// Потокобезопасная реализация конфигурации фонового сканирования.
/// </summary>
public class ScanConfiguration : IScanConfiguration
{
    private readonly object _lock = new();
    private bool _isEnabled = false;
    private TimeSpan _interval = TimeSpan.FromSeconds(10);
    private bool _saveToDatabase = true;

    public bool IsEnabled
    {
        get { lock (_lock) return _isEnabled; }
        set { lock (_lock) _isEnabled = value; }
    }

    public TimeSpan Interval
    {
        get { lock (_lock) return _interval; }
        set { lock (_lock) _interval = value; }
    }

    public bool SaveToDatabase
    {
        get { lock (_lock) return _saveToDatabase; }
        set { lock (_lock) _saveToDatabase = value; }
    }
}