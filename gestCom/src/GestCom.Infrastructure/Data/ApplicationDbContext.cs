using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Entities;
using GestCom.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestCom.Infrastructure.Data;

/// <summary>
/// Contexte de base de données principal
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantContext? _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext? tenantContext = null) 
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    private string? CurrentEntrepriseCode => _tenantContext?.CodeEntreprise;

    // Identity & Auth
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Ventes
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<DevisClient> DevisClients => Set<DevisClient>();
    public DbSet<LigneDevisClient> LignesDevisClient => Set<LigneDevisClient>();
    public DbSet<CommandeVente> CommandesVente => Set<CommandeVente>();
    public DbSet<LigneCommandeVente> LignesCommandeVente => Set<LigneCommandeVente>();
    public DbSet<BonLivraison> BonsLivraison => Set<BonLivraison>();
    public DbSet<LigneBonLivraison> LignesBonLivraison => Set<LigneBonLivraison>();
    public DbSet<FactureClient> FacturesClient => Set<FactureClient>();
    public DbSet<LigneFactureClient> LignesFactureClient => Set<LigneFactureClient>();
    public DbSet<ReglementFacture> ReglementsFacture => Set<ReglementFacture>();
    public DbSet<BonLivraison_Facture> BonsLivraison_Factures => Set<BonLivraison_Facture>();

    // Achats
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<DemandePrix> DemandesPrix => Set<DemandePrix>();
    public DbSet<LigneDemandePrix> LignesDemandesPrix => Set<LigneDemandePrix>();
    public DbSet<CommandeAchat> CommandesAchat => Set<CommandeAchat>();
    public DbSet<LigneCommandeAchat> LignesCommandeAchat => Set<LigneCommandeAchat>();
    public DbSet<BonReception> BonsReception => Set<BonReception>();
    public DbSet<LigneBonReception> LignesBonReception => Set<LigneBonReception>();
    public DbSet<FactureFournisseur> FacturesFournisseur => Set<FactureFournisseur>();
    public DbSet<LigneFactureFournisseur> LignesFactureFournisseur => Set<LigneFactureFournisseur>();
    public DbSet<ReglementFournisseur> ReglementsFournisseur => Set<ReglementFournisseur>();

    // Stock & Produits
    public DbSet<Produit> Produits => Set<Produit>();
    public DbSet<CategorieProduit> CategoriesProduit => Set<CategorieProduit>();
    public DbSet<UniteProduit> UnitesProduit => Set<UniteProduit>();
    public DbSet<MagasinProduit> MagasinsProduit => Set<MagasinProduit>();
    public DbSet<FabriquantProduit> FabriquantsProduit => Set<FabriquantProduit>();
    public DbSet<PaysProduit> PaysProduit => Set<PaysProduit>();
    public DbSet<DouaneProduit> DouanesProduit => Set<DouaneProduit>();

    // Configuration
    public DbSet<Entreprise> Entreprises => Set<Entreprise>();
    public DbSet<Devise> Devises => Set<Devise>();
    public DbSet<TvaProduit> TVAProduit => Set<TvaProduit>();
    public DbSet<ModePayement> ModesPayement => Set<ModePayement>();
    public DbSet<RetenuSource> RetenusSource => Set<RetenuSource>();
    public DbSet<ParametresDecimales> ParametresDecimales => Set<ParametresDecimales>();
    public DbSet<Licence> Licences => Set<Licence>();
    public DbSet<PathLogo> PathLogos => Set<PathLogo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for multi-tenancy
        ApplyGlobalFilters(builder);
    }

    private void ApplyGlobalFilters(ModelBuilder builder)
    {
        // Apply filter to all entities that implement IHasEntreprise
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            
            if (typeof(Domain.Common.IHasEntreprise).IsAssignableFrom(clrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyEntrepriseFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(clrType);
                
                method?.Invoke(this, new object[] { builder });
            }
        }
    }

    private void ApplyEntrepriseFilter<TEntity>(ModelBuilder builder) where TEntity : class, Domain.Common.IHasEntreprise
    {
        builder.Entity<TEntity>().HasQueryFilter(e => 
            string.IsNullOrEmpty(CurrentEntrepriseCode) || e.CodeEntreprise == CurrentEntrepriseCode);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set audit fields
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DateCreation = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.DateModification = DateTime.UtcNow;
                    break;
            }
        }

        // Auto-set CodeEntreprise for multi-tenancy
        if (!string.IsNullOrEmpty(CurrentEntrepriseCode))
        {
            foreach (var entry in ChangeTracker.Entries<Domain.Common.IHasEntreprise>())
            {
                if (entry.State == EntityState.Added && string.IsNullOrEmpty(entry.Entity.CodeEntreprise))
                {
                    entry.Entity.CodeEntreprise = CurrentEntrepriseCode;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
