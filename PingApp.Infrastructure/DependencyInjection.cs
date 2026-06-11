using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PingApp.Application.Interfaces;
using PingApp.Infrastructure.Data;
using PingApp.Infrastructure.Repositories;
using PingApp.Infrastructure.Services;

namespace PingApp.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрация инфраструктурных служб, БД и репозиториев.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PingDbContext>(options =>
            options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGlobalSettingsRepository, GlobalSettingsRepository>();

        services.AddSingleton<IPingService, PingService>();

        return services;
    }
}