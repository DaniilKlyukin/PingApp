using Microsoft.Extensions.DependencyInjection;
using PingApp.Application.Common.Behaviors;
using PingApp.Application.Features.Devices;
using PingApp.Application.Features.Scanning.Background;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Features.Security;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Interfaces;

namespace PingApp.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрация прикладных служб и сценариев использования.
    /// </summary>
    /// <param name="registerBackgroundScanner">Определяет, нужно ли этому хосту запускать фоновый воркер сканирования.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services, bool registerBackgroundScanner = false)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AddDevice.Command).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddSingleton<IScanConfiguration, ScanConfiguration>();
        services.AddSingleton<IUiEventBridge, UiEventBridge>();
        services.AddSingleton<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        if (registerBackgroundScanner)
        {
            services.AddHostedService<DeviceScanBackgroundService>();
        }

        return services;
    }
}