using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using GestCom_Desktop.Session;
using Web_GestCom.Auth;
using Web_GestCom.Services;

namespace GestCom_Desktop;

/// <summary>
/// Composition root. Shares the bulk of its service graph with Web_GestCom's Program.cs via
/// <c>AddGestComServices</c> (Web_GestCom.Core), then registers Desktop's own
/// IExecutionContext/ICurrentUserService/ITenantService implementations — skipping the ASP.NET
/// Core-only pieces (HttpExecutionContext, PermissionPolicyProvider/Handler/ClaimsTransformation),
/// which only make sense inside an HTTP authorization pipeline. Desktop forms resolve services
/// from a fresh DI scope per operation via <see cref="CreateScope"/> — mirrors one AppDbContext
/// per unit of work, the same way ASP.NET Core gives one scope per HTTP request.
/// </summary>
public static class AppHost
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static UserSession Session { get; private set; } = null!;

    public static void Initialize()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        // Pas de secret dans cette chaîne (Trusted_Connection=True) — sûr à logger, et ça permet de
        // repérer immédiatement un "Server=" mal configuré pour la machine cible sans deviner.
        Log.Information("Chaîne de connexion configurée : {ConnectionString}", connectionString);

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();

        // EF Core, and every service shared with Web_GestCom's Program.cs — see
        // AddGestComServices's own doc comment for what's shared vs. host-specific.
        services.AddGestComServices(connectionString);

        services.AddSingleton<UserSession>();
        services.AddScoped<IExecutionContext, DesktopExecutionContext>();
        services.AddScoped<ICurrentUserService, DesktopCurrentUserService>();
        services.AddScoped<ITenantService, DesktopTenantService>();

        // ValidateOnBuild fails fast at startup if any service's dependency graph can't resolve,
        // instead of only surfacing the error the first time a particular screen touches it.
        Services = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
        Session = Services.GetRequiredService<UserSession>();
    }

    /// <summary>One scope = one unit of work = one AppDbContext, matching the per-HTTP-request lifetime in the web app.</summary>
    public static IServiceScope CreateScope() => Services.CreateScope();
}
