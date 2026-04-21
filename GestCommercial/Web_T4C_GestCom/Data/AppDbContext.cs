using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Auth;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>Constructeur DI principal (injection de IHttpContextAccessor pour le filtre tenant).</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>ID de l'entreprise courante, résolu depuis le claim "CompanyId" du cookie.</summary>
    private int? CurrentCompanyId
    {
        get
        {
            return _httpContextAccessor?.HttpContext?.User.GetCompanyId();
        }
    }

    // ── Reference data ─────────────────────────────────────────────────────
    public DbSet<Entreprise>        Entreprises         => Set<Entreprise>();
    public DbSet<Devise>            Devises             => Set<Devise>();
    public DbSet<CategorieProduit>  CategoriesProduit   => Set<CategorieProduit>();
    public DbSet<UniteProduit>      UnitesProduit       => Set<UniteProduit>();
    public DbSet<TvaProduit>        TvasProduit         => Set<TvaProduit>();
    public DbSet<FabriquantProduit> FabriquantsProduit  => Set<FabriquantProduit>();
    public DbSet<ModePayement>      ModesPayement       => Set<ModePayement>();

    // ── Master data ─────────────────────────────────────────────────────────
    public DbSet<Client>      Clients      => Set<Client>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<Produit>     Produits     => Set<Produit>();

    // ── Sales ────────────────────────────────────────────────────────────────
    public DbSet<DevisClient>              DevisClient              => Set<DevisClient>();
    public DbSet<LigneDevisClient>         LignesDevisClient        => Set<LigneDevisClient>();
    public DbSet<CommandeVente>            CommandesVente           => Set<CommandeVente>();
    public DbSet<LigneCommandeVente>       LignesCommandeVente      => Set<LigneCommandeVente>();
    public DbSet<BonLivraison>             BonsLivraison            => Set<BonLivraison>();
    public DbSet<LigneBonLivraison>        LignesBonLivraison       => Set<LigneBonLivraison>();
    public DbSet<FactureClient>            FacturesClient           => Set<FactureClient>();
    public DbSet<LigneFactureClient>       LignesFactureClient      => Set<LigneFactureClient>();
    public DbSet<ReglementFactureClient>   ReglementsFactureClient  => Set<ReglementFactureClient>();

    // ── Auth & Journal ───────────────────────────────────────────────────────
    public DbSet<Utilisateur>    Utilisateurs   => Set<Utilisateur>();
    public DbSet<JournalActivite> JournalActivites => Set<JournalActivite>();

    // ── Purchases ────────────────────────────────────────────────────────────
    public DbSet<CommandeAchat>               CommandesAchat              => Set<CommandeAchat>();
    public DbSet<LigneCommandeAchat>          LignesCommandeAchat         => Set<LigneCommandeAchat>();
    public DbSet<BonReception>                BonsReception               => Set<BonReception>();
    public DbSet<LigneBonReception>           LignesBonReception          => Set<LigneBonReception>();
    public DbSet<FactureFournisseur>          FacturesFournisseur         => Set<FactureFournisseur>();
    public DbSet<LigneFactureFournisseur>     LignesFactureFournisseur    => Set<LigneFactureFournisseur>();
    public DbSet<ReglementFactureFournisseur> ReglementsFactureFournisseur => Set<ReglementFactureFournisseur>();

    // ── RBAC ─────────────────────────────────────────────────────────────────
    public DbSet<Company>        Companies        => Set<Company>();
    public DbSet<AppRole>        AppRoles         => Set<AppRole>();
    public DbSet<Permission>     Permissions      => Set<Permission>();
    public DbSet<UserRole>       UserRoles        => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions  => Set<RolePermission>();
    public DbSet<FeatureFlag>    FeatureFlags     => Set<FeatureFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Disable cascade delete globally (SQL Server multi-path restriction).
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        // ── Composite PKs ──────────────────────────────────────────────────
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // ── Global query filters (tenant isolation) ────────────────────────
        // AppRole: null = système global, visible par tous ; valeur = rôle de cette entreprise seulement.
        modelBuilder.Entity<AppRole>()
            .HasQueryFilter(r =>
                r.CompanyId == null ||
                CurrentCompanyId == null ||
                r.CompanyId == CurrentCompanyId);

        // FeatureFlag: visible uniquement pour l'entreprise courante.
        modelBuilder.Entity<FeatureFlag>()
            .HasQueryFilter(ff =>
                CurrentCompanyId == null ||
                ff.CompanyId == CurrentCompanyId);

        // ── Reference data seed ────────────────────────────────────────────
        modelBuilder.Entity<Devise>().HasData(
            new Devise { CodeDevise = 1, NomDevise = "Dinar Tunisien", SymboleDevise = "TND", TauxDevise = 1.0 },
            new Devise { CodeDevise = 2, NomDevise = "Euro",           SymboleDevise = "EUR", TauxDevise = 3.3 },
            new Devise { CodeDevise = 3, NomDevise = "Dollar US",      SymboleDevise = "USD", TauxDevise = 3.1 }
        );

        modelBuilder.Entity<ModePayement>().HasData(
            new ModePayement { CodeModePayement = 1, NomModePayement = "Espèces"  },
            new ModePayement { CodeModePayement = 2, NomModePayement = "Chèque"   },
            new ModePayement { CodeModePayement = 3, NomModePayement = "Virement" },
            new ModePayement { CodeModePayement = 4, NomModePayement = "Effet"    },
            new ModePayement { CodeModePayement = 5, NomModePayement = "À terme"  }
        );

        modelBuilder.Entity<TvaProduit>().HasData(
            new TvaProduit { CodeTvaProduit = 1, NomTvaProduit = "TVA 19%",  TauxTvaProduit = 19 },
            new TvaProduit { CodeTvaProduit = 2, NomTvaProduit = "TVA 13%",  TauxTvaProduit = 13 },
            new TvaProduit { CodeTvaProduit = 3, NomTvaProduit = "TVA 7%",   TauxTvaProduit = 7  },
            new TvaProduit { CodeTvaProduit = 4, NomTvaProduit = "Exonéré",  TauxTvaProduit = 0  }
        );

        modelBuilder.Entity<UniteProduit>().HasData(
            new UniteProduit { CodeUniteProduit = 1, NomUniteProduit = "Unité"  },
            new UniteProduit { CodeUniteProduit = 2, NomUniteProduit = "Kg"     },
            new UniteProduit { CodeUniteProduit = 3, NomUniteProduit = "Litre"  },
            new UniteProduit { CodeUniteProduit = 4, NomUniteProduit = "Mètre"  },
            new UniteProduit { CodeUniteProduit = 5, NomUniteProduit = "Boîte"  }
        );

        modelBuilder.Entity<CategorieProduit>().HasData(
            new CategorieProduit { CodeCategorieProduit = 1, NomCategorieProduit = "Général" }
        );

        modelBuilder.Entity<FabriquantProduit>().HasData(
            new FabriquantProduit { CodeFabriquantProduit = 1, NomFabriquantProduit = "Divers" }
        );

        // ── RBAC seed ──────────────────────────────────────────────────────
        // Company par défaut
        modelBuilder.Entity<Company>().HasData(
            new Company { Id = 1, Name = "Entreprise Défaut", Slug = "default", Plan = "Standard" }
        );

        // Permissions: enterprise modules + global superadmin modules.
        var enterpriseModules = new[]
        {
            "clients",
            "factures",
            "devis",
            "commandes-vente",
            "bons-livraison",
            "fournisseurs",
            "commandes-achat",
            "bons-reception",
            "factures-fournisseur",
            "produits"
        };
        var superAdminModules = new[] { "tenants", "users-global", "roles-global", "journal-global" };
        var modules = enterpriseModules.Concat(superAdminModules).ToArray();
        var actions = new[] { "view", "create", "update", "delete" };
        var permissions = new List<Permission>();
        int permId = 1;
        foreach (var module in modules)
            foreach (var action in actions)
                permissions.Add(new Permission { Id = permId++, Feature = module, Action = action });
        modelBuilder.Entity<Permission>().HasData(permissions);

        // Rôles système (CompanyId null = global)
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "Admin",   CompanyId = null },
            new AppRole { Id = 2, Name = "Manager", CompanyId = null },
            new AppRole { Id = 3, Name = "Employé", CompanyId = null },
            new AppRole { Id = 4, Name = "SuperAdmin", CompanyId = null }
        );

        // RolePermissions
        var rp = new List<RolePermission>();
        foreach (var p in permissions.Where(p => enterpriseModules.Contains(p.Feature)))
            rp.Add(new RolePermission { RoleId = 1, PermissionId = p.Id });

        foreach (var p in permissions.Where(p => enterpriseModules.Contains(p.Feature) && p.Action != "delete"))
            rp.Add(new RolePermission { RoleId = 2, PermissionId = p.Id });

        foreach (var p in permissions.Where(p => enterpriseModules.Contains(p.Feature) && (p.Action == "view" || p.Action == "create")))
            rp.Add(new RolePermission { RoleId = 3, PermissionId = p.Id });

        foreach (var p in permissions.Where(p => superAdminModules.Contains(p.Feature)))
            rp.Add(new RolePermission { RoleId = 4, PermissionId = p.Id });

        modelBuilder.Entity<RolePermission>().HasData(rp);
    }
}
