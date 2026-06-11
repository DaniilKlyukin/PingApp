namespace PingApp.Application.Interfaces;

public interface IGlobalSettingsRepository
{
    Task<string> GetSettingAsync(string key, string defaultValue, CancellationToken cancellationToken = default);
    Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default);
}