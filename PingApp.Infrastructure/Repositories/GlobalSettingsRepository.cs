using PingApp.Application.Interfaces;
using PingApp.Domain.Entities;
using PingApp.Infrastructure.Data;

namespace PingApp.Infrastructure.Repositories;

public class GlobalSettingsRepository : IGlobalSettingsRepository
{
    private readonly PingDbContext _context;

    public GlobalSettingsRepository(PingDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue, CancellationToken cancellationToken = default)
    {
        var setting = await _context.GlobalSettings.FindAsync([key], cancellationToken);
        return setting?.Value ?? defaultValue;
    }

    public async Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = await _context.GlobalSettings.FindAsync([key], cancellationToken);
        if (setting == null)
        {
            setting = new GlobalSetting { Key = key, Value = value };
            _context.GlobalSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}