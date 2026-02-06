using System.Security.Claims;
using GestCom.Application.Common.Interfaces;

namespace GestCom.WebAPI.Middleware;

/// <summary>
/// Middleware pour extraire le tenant (CodeEntreprise) du token JWT
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extraire le CodeEntreprise des claims JWT
            var codeEntreprise = context.User.FindFirstValue("CodeEntreprise");
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = context.User.FindFirstValue(ClaimTypes.Name);

            if (!string.IsNullOrEmpty(codeEntreprise))
            {
                tenantContext.SetTenant(codeEntreprise, userId, userName);
                _logger.LogDebug("Tenant défini: {CodeEntreprise} pour l'utilisateur {UserName}", 
                    codeEntreprise, userName);
            }
            else
            {
                _logger.LogWarning("Utilisateur authentifié sans CodeEntreprise: {UserName}", userName);
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Interface pour le contexte du tenant
/// </summary>
public interface ITenantContext
{
    string? CodeEntreprise { get; }
    string? UserId { get; }
    string? UserName { get; }
    void SetTenant(string codeEntreprise, string? userId, string? userName);
}

/// <summary>
/// Implémentation du contexte du tenant (scoped)
/// </summary>
public class TenantContext : ITenantContext
{
    public string? CodeEntreprise { get; private set; }
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }

    public void SetTenant(string codeEntreprise, string? userId, string? userName)
    {
        CodeEntreprise = codeEntreprise;
        UserId = userId;
        UserName = userName;
    }
}

/// <summary>
/// Extension method pour enregistrer le middleware
/// </summary>
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenant(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }

    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }
}
