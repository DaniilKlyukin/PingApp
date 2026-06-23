using MassTransit;
using PingApp.Api.Middleware;
using PingApp.Application;
using PingApp.Infrastructure;
using PingApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Критическая ошибка: Строка подключения 'DefaultConnection' не настроена в конфигурации среды.");

builder.Services.AddControllers();

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<PingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitUri = builder.Configuration["RabbitMq:Uri"] ?? "rabbitmq://localhost";
        cfg.Host(new Uri(rabbitUri), h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
    });
});

var app = builder.Build();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<ApiKeyMiddleware>();
});

app.UseRouting();

app.MapControllers();

app.Run();