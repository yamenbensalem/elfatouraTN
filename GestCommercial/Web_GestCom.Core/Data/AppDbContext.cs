using Microsoft.EntityFrameworkCore;
using Web_GestCom.Auth;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Data;

public class AppDbContext : DbContext
{
    private readonly IExecutionContext? _executionContext;

    /// <summary>
    /// Primary DI constructor. Receives IExecutionContext so that tenant isolation works in
    /// both HTTP request scope (HttpExecutionContext) and background tasks
    /// (BackgroundExecutionContext).  Null = no active context = filters disabled (used in tests).
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options, IExecutionContext? executionContext = null)
        : base(options)
    {
        _executionContext = executionContext;
        ChangeTracker.Tracking += StampCompanyIdOnNewRows;
    }

    /// <summary>
    /// Renseigne CompanyId au moment où une nouvelle ligne métier entre dans le contexte (Add,
    /// AddRange ou découverte via une navigation), avant que EF n'en calcule la clé. Indispensable
    /// depuis que CompanyId fait partie de la clé primaire (CompanyId, code) : EF ne peut pas suivre
    /// une entité dont une partie de clé est inconnue, et ApplyTenantOwnershipRules (au
    /// SaveChanges) arrive trop tard. Une valeur déjà fournie par l'appelant n'est jamais écrasée
    /// ici — c'est ApplyTenantOwnershipRules qui rejette un CompanyId d'un autre tenant.
    /// (Un ValueGenerator EF ne convient pas : EF l'ignore sur une propriété qui est aussi une clé
    /// étrangère, ce qu'est CompanyId — vers company et, par les clés composites, vers le parent.)
    /// </summary>
    private void StampCompanyIdOnNewRows(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackingEventArgs e)
    {
        if (e.FromQuery || e.State != EntityState.Added) return;
        if (e.Entry.Entity is not ITenantOwned { CompanyId: null }) return;
        if (!TenantKeyedTypes.Contains(e.Entry.Metadata.ClrType)) return;

        var companyId = CompanyIdForNewRows;
        if (companyId.HasValue)
            e.Entry.Property(nameof(ITenantOwned.CompanyId)).CurrentValue = companyId;
    }

    private int?  CurrentCompanyId      => _executionContext?.CurrentCompanyId;
    private bool  CurrentIsSuperAdmin   => _executionContext?.IsSuperAdmin == true;
    private bool  CurrentIsAuthenticated => _executionContext?.IsAuthenticated == true;

    /// <summary>
    /// Entreprise attribuée à une nouvelle ligne métier (voir StampCompanyIdOnNewRows). Avec un
    /// contexte actif : le tenant courant (null pour SuperAdmin/anonyme — ils n'écrivent jamais de
    /// donnée métier, et EF refusera alors de suivre l'entité). Sans contexte (seed au démarrage,
    /// tests unitaires) : l'entreprise par défaut, celle à laquelle Program.cs rattache déjà toute
    /// ligne historique sans entreprise.
    /// </summary>
    /// <summary>Entreprise de l'utilisateur courant ; null hors contexte, pour un SuperAdmin ou un visiteur.</summary>
    internal int? TenantCompanyId
        => _executionContext?.HasActiveContext == true && !CurrentIsSuperAdmin ? CurrentCompanyId : null;

    internal int? CompanyIdForNewRows
        => _executionContext?.HasActiveContext == true ? CurrentCompanyId : Company.DefaultId;

    /// <summary>
    /// Tenant filters for business data engage whenever there is an active execution context —
    /// including for SuperAdmin. SuperAdmin is a platform-management role with zero business-data
    /// access by design (see PermissionAuthorizationHandler/ServicePermissionGuard for the matching
    /// read/write boundary at the permission-check level); since SuperAdmin has no CompanyId, this
    /// filter naturally excludes every business row for it rather than bypassing tenant isolation.
    /// Null context (unit tests, migrations) disables all filters.
    /// </summary>
    private bool ShouldApplyTenantFilter
        => _executionContext?.HasActiveContext == true;

    /// <summary>
    /// Governs the Utilisateur query filter only. Unlike ShouldApplyTenantFilter above, SuperAdmin
    /// DOES bypass this one — it needs cross-company visibility for the "Tous les utilisateurs"
    /// global dashboard (platform-management scope, not business data). Also requires an
    /// authenticated principal: HasActiveContext is true for the anonymous login POST too (it just
    /// means "an HTTP request exists"), and that request looks up Utilisateur by login before any
    /// principal/tenant exists — without this extra gate the filter would exclude every row
    /// (CurrentCompanyId is null pre-login) and login would always fail.
    /// </summary>
    private bool ShouldApplyTenantFilterToAuthenticatedUsers
        => (_executionContext?.HasActiveContext == true) && CurrentIsAuthenticated && !CurrentIsSuperAdmin;

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

    // ── SuperAdmin platform integrations (storage-only, no functional wiring) ──────────────
    public DbSet<ApiKey>  ApiKeys  => Set<ApiKey>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();

    // ── Subscription requests (manual follow-up — no payment gateway wired yet) ────────────
    public DbSet<Abonnement> Abonnements => Set<Abonnement>();
    public DbSet<CodePromo> CodesPromo => Set<CodePromo>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantOwnershipRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantOwnershipRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantOwnershipRules()
    {
        // The anonymous login POST can save changes too (AuthentifierAsync rehashes a legacy
        // password hash on successful login, before any principal/tenant exists) — skip ownership
        // enforcement in that case for the same reason the Utilisateur query filter does; there
        // is no tenant to stamp or check yet, and every other ITenantOwned entity is never
        // touched outside an authenticated request anyway, so this changes nothing for them.
        //
        // SuperAdmin is skipped too (unlike the read-side ShouldApplyTenantFilter above): its only
        // legitimate write path is managing Utilisateur rows across companies (platform-management
        // scope), which requires trusting an explicit, caller-supplied CompanyId rather than
        // stamping the current tenant (SuperAdmin has none). It has no service-layer path to write
        // genuine business entities at all — PermissionAuthorizationHandler/ServicePermissionGuard
        // reject those before SaveChanges is ever reached — so skipping the stamp/check here for
        // SuperAdmin does not reopen the business-data boundary those enforce.
        if (_executionContext?.HasActiveContext != true || CurrentIsSuperAdmin || !CurrentIsAuthenticated)
            return;

        if (!CurrentCompanyId.HasValue)
            throw new UnauthorizedAccessException("Aucun tenant actif dans le contexte de sécurité.");

        var tenantId = CurrentCompanyId.Value;

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is ITenantOwned &&
                                 (e.State == EntityState.Added ||
                                  e.State == EntityState.Modified ||
                                  e.State == EntityState.Deleted)))
        {
            var entity = (ITenantOwned)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                // Tenant ownership at creation time. CompanyId is normally already stamped by
                // StampCompanyIdOnNewRows; a different, caller-supplied value is a cross-tenant
                // write. It can't simply be overwritten any more: on business entities CompanyId
                // is part of the primary key, which EF forbids changing on a tracked entity.
                if (entity.CompanyId == tenantId)
                    continue;
                if (entry.Property(nameof(ITenantOwned.CompanyId)).Metadata.IsKey())
                    throw new UnauthorizedAccessException("Tentative d'accès cross-tenant détectée.");
                entity.CompanyId = tenantId;
                continue;
            }

            if (entity.CompanyId != tenantId)
                throw new UnauthorizedAccessException("Tentative d'accès cross-tenant détectée.");

            // (CompanyId == tenantId is guaranteed here — nothing to re-stamp.)
        }
    }

    /// <summary>
    /// Clés "par entreprise" des 10 tables métier : la clé primaire est (CompanyId, code), plus le
    /// code seul. Avant 2026-10 le code seul était la clé, globale à la base, alors que la
    /// numérotation repart de CL00001/FC2026... dans chaque entreprise : la deuxième entreprise ne
    /// pouvait créer ni client, ni produit, ni document (violation de PK_client, reproduit).
    /// Tous les liens vers ces tables portent donc deux colonnes (CompanyId, code) — CompanyId du
    /// dépendant étant partagé entre ses différents liens, un document ne peut référencer que des
    /// lignes de sa propre entreprise. La migration des bases existantes est dans TenantKeyMigration.
    /// </summary>
    private static void ConfigureTenantKeys(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().HasKey(e => new { e.CompanyId, e.CodeClient });
        modelBuilder.Entity<Fournisseur>().HasKey(e => new { e.CompanyId, e.CodeFournisseur });
        modelBuilder.Entity<Produit>().HasKey(e => new { e.CompanyId, e.CodeProduit });
        modelBuilder.Entity<DevisClient>().HasKey(e => new { e.CompanyId, e.NumeroDevis });
        modelBuilder.Entity<CommandeVente>().HasKey(e => new { e.CompanyId, e.NumeroCommandeVente });
        modelBuilder.Entity<BonLivraison>().HasKey(e => new { e.CompanyId, e.NumeroBonLivraison });
        modelBuilder.Entity<FactureClient>().HasKey(e => new { e.CompanyId, e.NumeroFactureClient });
        modelBuilder.Entity<CommandeAchat>().HasKey(e => new { e.CompanyId, e.NumeroCommandeAchat });
        modelBuilder.Entity<BonReception>().HasKey(e => new { e.CompanyId, e.NumeroBonReception });
        modelBuilder.Entity<FactureFournisseur>().HasKey(e => new { e.CompanyId, e.NumeroFactureFournisseur });

        // ── Tiers ──
        modelBuilder.Entity<Produit>().HasOne(e => e.Fournisseur).WithMany()
            .HasForeignKey(e => new { e.CompanyId, e.CodeFournisseur });
        modelBuilder.Entity<DevisClient>().HasOne(e => e.Client).WithMany(c => c.DevisClient)
            .HasForeignKey(e => new { e.CompanyId, e.CodeClient });
        modelBuilder.Entity<CommandeVente>().HasOne(e => e.Client).WithMany(c => c.CommandesVente)
            .HasForeignKey(e => new { e.CompanyId, e.CodeClient });
        modelBuilder.Entity<BonLivraison>().HasOne(e => e.Client).WithMany(c => c.BonsLivraison)
            .HasForeignKey(e => new { e.CompanyId, e.CodeClient });
        modelBuilder.Entity<FactureClient>().HasOne(e => e.Client).WithMany(c => c.FacturesClient)
            .HasForeignKey(e => new { e.CompanyId, e.CodeClient });
        modelBuilder.Entity<CommandeAchat>().HasOne(e => e.Fournisseur).WithMany(f => f.CommandesAchat)
            .HasForeignKey(e => new { e.CompanyId, e.CodeFournisseur });
        modelBuilder.Entity<BonReception>().HasOne(e => e.Fournisseur).WithMany(f => f.BonsReception)
            .HasForeignKey(e => new { e.CompanyId, e.CodeFournisseur });
        modelBuilder.Entity<FactureFournisseur>().HasOne(e => e.Fournisseur).WithMany(f => f.FacturesFournisseur)
            .HasForeignKey(e => new { e.CompanyId, e.CodeFournisseur });

        // ── Liens de traçabilité entre documents (optionnels) ──
        modelBuilder.Entity<BonLivraison>().HasOne(e => e.CommandeVente).WithMany(c => c.BonsLivraison)
            .HasForeignKey(e => new { e.CompanyId, e.NumeroCommandeVente });
        modelBuilder.Entity<BonReception>().HasOne(e => e.CommandeAchat).WithMany(c => c.BonsReception)
            .HasForeignKey(e => new { e.CompanyId, e.NumeroCommandeAchat });
        modelBuilder.Entity<FactureClient>().HasOne(e => e.BonLivraisonOrigine).WithMany()
            .HasForeignKey(e => new { e.CompanyId, e.NumeroBonLivraisonOrigine });

        // ── Lignes et règlements ──
        modelBuilder.Entity<LigneDevisClient>(b =>
        {
            b.HasOne(e => e.DevisClient).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroDevis });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<LigneCommandeVente>(b =>
        {
            b.HasOne(e => e.CommandeVente).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroCommandeVente });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<LigneBonLivraison>(b =>
        {
            b.HasOne(e => e.BonLivraison).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroBonLivraison });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<LigneFactureClient>(b =>
        {
            b.HasOne(e => e.FactureClient).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroFactureClient });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<ReglementFactureClient>()
            .HasOne(e => e.FactureClient).WithMany(d => d.Reglements).HasForeignKey(e => new { e.CompanyId, e.NumeroFactureClient });
        modelBuilder.Entity<LigneCommandeAchat>(b =>
        {
            b.HasOne(e => e.CommandeAchat).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroCommandeAchat });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<LigneBonReception>(b =>
        {
            b.HasOne(e => e.BonReception).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroBonReception });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<LigneFactureFournisseur>(b =>
        {
            b.HasOne(e => e.FactureFournisseur).WithMany(d => d.Lignes).HasForeignKey(e => new { e.CompanyId, e.NumeroFactureFournisseur });
            b.HasOne(e => e.Produit).WithMany().HasForeignKey(e => new { e.CompanyId, e.CodeProduit });
        });
        modelBuilder.Entity<ReglementFactureFournisseur>()
            .HasOne(e => e.FactureFournisseur).WithMany(d => d.Reglements).HasForeignKey(e => new { e.CompanyId, e.NumeroFactureFournisseur });

        // CompanyId : renseigné par StampCompanyIdOnNewRows, jamais généré par la base (pas
        // d'IDENTITY), et obligatoire sur toutes ces tables, filles comprises.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => TenantKeyedTypes.Contains(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property<int?>(nameof(ITenantOwned.CompanyId))
                .IsRequired()
                .ValueGeneratedNever();
        }
    }

    /// <summary>Les 10 tables métier à clé (CompanyId, code) et leurs 9 tables filles.</summary>
    public static readonly IReadOnlySet<Type> TenantKeyedTypes = new HashSet<Type>
    {
        typeof(Client), typeof(Fournisseur), typeof(Produit),
        typeof(DevisClient), typeof(CommandeVente), typeof(BonLivraison), typeof(FactureClient),
        typeof(CommandeAchat), typeof(BonReception), typeof(FactureFournisseur),
        typeof(LigneDevisClient), typeof(LigneCommandeVente), typeof(LigneBonLivraison),
        typeof(LigneFactureClient), typeof(ReglementFactureClient),
        typeof(LigneCommandeAchat), typeof(LigneBonReception),
        typeof(LigneFactureFournisseur), typeof(ReglementFactureFournisseur)
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Must run before the global Restrict loop below, which only sees relationships that
        // already exist in the model.
        ConfigureTenantKeys(modelBuilder);

        // Disable cascade delete globally (SQL Server multi-path restriction).
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        // Exceptions to the global Restrict above: these three FKs are purely informational
        // traceability links (which order a delivery/receipt note was generated from, which BL a
        // facture was generated from), not a financial record like a line item. Deleting the
        // source document must not be blocked by them — the link is cleared instead.
        // ClientSetNull, not SetNull: since the keys became (CompanyId, code), these FKs share the
        // dependent's non-nullable CompanyId column, and SQL Server refuses ON DELETE SET NULL on
        // such a constraint. The database constraint is therefore NO ACTION and the services clear
        // the link themselves before deleting (CommandeVenteService/CommandeAchatService/
        // BonLivraisonService.DeleteAsync) — EF then only nulls the nullable code column.
        modelBuilder.Entity<BonLivraison>()
            .HasOne(b => b.CommandeVente).WithMany(c => c.BonsLivraison)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<BonReception>()
            .HasOne(b => b.CommandeAchat).WithMany(c => c.BonsReception)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<FactureClient>()
            .HasOne(f => f.BonLivraisonOrigine).WithMany()
            .OnDelete(DeleteBehavior.ClientSetNull);

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

        // Business entities: strictly tenant-scoped for non-superadmin request contexts.
        modelBuilder.Entity<Client>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<Entreprise>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<Fournisseur>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<Produit>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<DevisClient>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<CommandeVente>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<BonLivraison>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<FactureClient>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<CommandeAchat>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<BonReception>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        modelBuilder.Entity<FactureFournisseur>()
            .HasQueryFilter(e =>
                !ShouldApplyTenantFilter ||
                (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        // Lignes et règlements : même cloisonnement que leur document parent (défense en profondeur —
        // ils n'étaient auparavant atteignables que via le parent).
        modelBuilder.Entity<LigneDevisClient>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<LigneCommandeVente>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<LigneBonLivraison>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<LigneFactureClient>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<ReglementFactureClient>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<LigneCommandeAchat>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<LigneBonReception>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<LigneFactureFournisseur>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));
        modelBuilder.Entity<ReglementFactureFournisseur>().HasQueryFilter(e => !ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId));

        // Utilisateur: same rule as the business entities above. A non-SuperAdmin session only
        // ever sees users of its own company (SuperAdmin rows have CompanyId == null, so they're
        // naturally excluded too). This is a defense-in-depth safety net alongside
        // UtilisateurService's own manual filtering in GetAllAsync/GetByIdAsync — it also closes a
        // gap those two methods couldn't cover: FindAsync (used by Activer/Desactiver/
        // ChangePassword) bypasses query filters by design, but ApplyTenantOwnershipRules below
        // still rejects the save if the loaded entity turns out to belong to another tenant.
        modelBuilder.Entity<Utilisateur>()
            .HasQueryFilter(u =>
                !ShouldApplyTenantFilterToAuthenticatedUsers ||
                (CurrentCompanyId.HasValue && u.CompanyId == CurrentCompanyId));

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

        // [Timestamp] RowVersion columns (7 document entities) are populated natively by SQL
        // Server (ROWVERSION auto-increments on every UPDATE) — real production databases need no
        // extra configuration. The InMemory provider used by unit tests has no equivalent, and
        // without a value generator it throws "Required properties '{RowVersion}' are missing" on
        // the very first insert. This block only runs against InMemory (never SQL Server) and
        // gives it a generator that produces a fresh value each time, so concurrency-token
        // behavior can be unit-tested without a real database.
        // String check instead of the Database.IsInMemory() extension: that extension lives in the
        // Microsoft.EntityFrameworkCore.InMemory package, which this shared Core project correctly
        // does not reference (it's a test-only provider, not something production code should
        // depend on).
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            foreach (var entityType in new[]
            {
                typeof(DevisClient), typeof(CommandeVente), typeof(BonLivraison), typeof(FactureClient),
                typeof(CommandeAchat), typeof(BonReception), typeof(FactureFournisseur)
            })
            {
                modelBuilder.Entity(entityType).Property("RowVersion").HasValueGenerator<InMemoryRowVersionGenerator>();
            }
        }
    }

    /// <summary>Only used against the InMemory test provider — see OnModelCreating above.</summary>
    private sealed class InMemoryRowVersionGenerator : Microsoft.EntityFrameworkCore.ValueGeneration.ValueGenerator<byte[]>
    {
        public override bool GeneratesTemporaryValues => false;
        public override byte[] Next(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) => Guid.NewGuid().ToByteArray();
    }
}
