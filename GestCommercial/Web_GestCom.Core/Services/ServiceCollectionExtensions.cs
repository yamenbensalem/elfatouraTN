using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

/// <summary>
/// Registers the DbContext and every service shared between Web_GestCom's Program.cs and
/// GestCom_Desktop's AppHost.cs — one list instead of two that have to be kept in sync by hand.
/// Each host still registers its own host-specific pieces on top: the IExecutionContext/
/// ICurrentUserService/ITenantService implementation (HTTP-backed for Web, session-backed for
/// Desktop), and — for Web only — the ASP.NET Core authorization pipeline
/// (PermissionPolicyProvider/Handler/ClaimsTransformation).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGestComServices(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        // PermissionService and FeatureFlagService take IDbContextFactory<AppDbContext> directly —
        // without this they fail to resolve at runtime.
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(connectionString),
            ServiceLifetime.Scoped);

        services.AddSingleton<ILoginProtectionService, LoginProtectionService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();

        services.AddSingleton<AppConfigService>();
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

        services.AddScoped(typeof(IReferenceDataService<>), typeof(ReferenceDataService<>));
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
