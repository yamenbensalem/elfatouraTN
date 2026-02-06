using System.Diagnostics;

namespace GestCom.WebAPI.Middleware;

/// <summary>
/// Middleware pour logger les requêtes HTTP
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        // Log début de requête
        _logger.LogInformation(
            "[{RequestId}] {Method} {Path}{QueryString} - Début",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Déterminer le niveau de log en fonction du status code et de la durée
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "[{RequestId}] {Method} {Path} - {StatusCode} - {Duration}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    duration);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "[{RequestId}] {Method} {Path} - {StatusCode} - {Duration}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    duration);
            }
            else if (duration > 1000)
            {
                _logger.LogWarning(
                    "[{RequestId}] {Method} {Path} - {StatusCode} - {Duration}ms (SLOW)",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    duration);
            }
            else
            {
                _logger.LogInformation(
                    "[{RequestId}] {Method} {Path} - {StatusCode} - {Duration}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    duration);
            }
        }
    }
}

/// <summary>
/// Extension method pour enregistrer le middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
