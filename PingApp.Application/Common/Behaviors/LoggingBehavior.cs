using MediatR;
using Microsoft.Extensions.Logging;
using PingApp.Application.Interfaces;
using System.Diagnostics;

namespace PingApp.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IUserContext _userContext;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IUserContext userContext)
    {
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _userContext.UserId.Value;

        _logger.LogInformation(
            "Начало выполнения запроса {RequestName} для пользователя {UserId}",
            requestName, userId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Успешно обработан запрос {RequestName} за {ElapsedMs} мс",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Сбой при выполнении запроса {RequestName} после {ElapsedMs} мс. Ошибка: {ErrorMessage}",
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}