using System.Net;
using System.Text.Json;
using GestCom.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Middleware;

/// <summary>
/// Middleware pour la gestion globale des exceptions
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Erreur de validation",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "Une ou plusieurs erreurs de validation se sont produites.",
                    Extensions = { ["errors"] = validationEx.Errors }
                }),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Ressource non trouvée",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundEx.Message
                }),

            CreditLimitExceededException creditEx => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                    Title = "Limite de crédit dépassée",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Detail = creditEx.Message,
                    Extensions = 
                    {
                        ["errorCode"] = creditEx.ErrorCode,
                        ["codeClient"] = creditEx.CodeClient,
                        ["limiteCredit"] = creditEx.LimiteCredit,
                        ["soldeActuel"] = creditEx.SoldeActuel,
                        ["montantDemande"] = creditEx.MontantDemande
                    }
                }),

            InsufficientStockException stockEx => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                    Title = "Stock insuffisant",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Detail = stockEx.Message,
                    Extensions = 
                    {
                        ["errorCode"] = stockEx.ErrorCode,
                        ["codeProduit"] = stockEx.CodeProduit,
                        ["stockDisponible"] = stockEx.StockDisponible,
                        ["quantiteDemandee"] = stockEx.QuantiteDemandee
                    }
                }),

            BusinessException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                    Title = "Erreur métier",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Detail = businessEx.Message,
                    Extensions = { ["errorCode"] = businessEx.ErrorCode ?? "BUSINESS_ERROR" }
                }),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Title = "Non autorisé",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = "Vous n'êtes pas autorisé à accéder à cette ressource."
                }),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Opération invalide",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = invalidOpEx.Message
                }),

            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Erreur serveur",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = _env.IsDevelopment() 
                        ? exception.Message 
                        : "Une erreur inattendue s'est produite."
                })
        };

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erreur non gérée: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Exception gérée: {Type} - {Message}", exception.GetType().Name, exception.Message);
        }

        // Add trace identifier
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // Include stack trace in development
        if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }
}

/// <summary>
/// Extension method pour enregistrer le middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
