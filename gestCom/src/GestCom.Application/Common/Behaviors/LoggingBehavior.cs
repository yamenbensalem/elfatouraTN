using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestCom.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior pour le logging des requêtes MediatR
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "[START] {RequestName} [{Guid}]",
            requestName,
            requestGuid);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} [{Guid}] - Durée: {ElapsedMilliseconds}ms",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds);

            // Avertir si la requête est lente
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "[SLOW] {RequestName} [{Guid}] - Durée: {ElapsedMilliseconds}ms",
                    requestName,
                    requestGuid,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "[ERROR] {RequestName} [{Guid}] - Durée: {ElapsedMilliseconds}ms - Erreur: {Message}",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
