using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using T4C_GestCom_Desktop.Session;
using Web_T4C_GestCom.Auth;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop;

/// <summary>
/// Composition root. Builds the same service graph as Web_T4C_GestCom's Program.cs, minus the
/// ASP.NET Core-only pieces (HttpExecutionContext, PermissionPolicyProvider/Handler/ClaimsTransformation,
/// which only make sense inside an HTTP authorization pipeline). Desktop forms resolve services
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

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddMemoryCache();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        // PermissionService and FeatureFlagService take IDbContextFactory<AppDbContext> directly
        // (see Web_T4C_GestCom's Program.cs) — without this they fail to resolve at runtime.
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);

        services.AddSingleton<UserSession>();
        services.AddScoped<IExecutionContext, DesktopExecutionContext>();
        services.AddScoped<ICurrentUserService, DesktopCurrentUserService>();
        services.AddScoped<ITenantService, DesktopTenantService>();

        services.AddSingleton<AppConfigService>();
        services.AddSingleton<ILoginProtectionService, LoginProtectionService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<DocumentNumberService>();

        services.AddScoped<IUtilisateurService, UtilisateurService>();
        services.AddScoped<IJournalActiviteService, JournalActiviteService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProduitService, ProduitService>();
        services.AddScoped<IFournisseurService, FournisseurService>();
        services.AddScoped<IFactureClientService, FactureClientService>();
        services.AddScoped<IDevisClientService, DevisClientService>();
        services.AddScoped<ICommandeVenteService, CommandeVenteService>();
        services.AddScoped<IBonLivraisonService, BonLivraisonService>();
        services.AddScoped<ICommandeAchatService, CommandeAchatService>();
        services.AddScoped<IBonReceptionService, BonReceptionService>();
        services.AddScoped<IFactureFournisseurService, FactureFournisseurService>();

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
