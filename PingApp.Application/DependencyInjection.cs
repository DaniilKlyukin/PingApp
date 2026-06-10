using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PingApp.Application.Features.Devices;
using PingApp.Application.Features.Scanning.Background;
using PingApp.Application.Features.Scanning.Common;

namespace PingApp.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрация прикладных служб и сценариев использования (слайсов).
    /// </summary>
    /// <param name="registerBackgroundScanner">Определяет, нужно ли этому хосту запускать фоновый воркер сканирования.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services, bool registerBackgroundScanner = false)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddDevice.Command).Assembly));

        services.AddValidatorsFromAssemblyContaining<AddDevice.Validator>();

        services.AddSingleton<IScanConfiguration, ScanConfiguration>();
        services.AddSingleton<IUiEventBridge, UiEventBridge>();

        if (registerBackgroundScanner)
        {
            services.AddHostedService<DeviceScanBackgroundService>();
        }

        return services;
    }
}