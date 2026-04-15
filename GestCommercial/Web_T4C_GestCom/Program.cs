using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Auth;
using Web_T4C_GestCom.Components;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core – SQL Server (pool factory for services that need CreateDbContextAsync)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath       = "/compte/connexion";
        options.LogoutPath      = "/compte/deconnexion";
        options.AccessDeniedPath = "/compte/connexion";
        options.ExpireTimeSpan  = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// RBAC
builder.Services.AddScoped<ITenantService,      TenantService>();
builder.Services.AddScoped<IPermissionService,  PermissionService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler,          PermissionAuthorizationHandler>();
builder.Services.AddScoped<IClaimsTransformation,          PermissionClaimsTransformation>();

// RazorPages (for Login/Logout pages)
builder.Services.AddRazorPages();

// Application services
builder.Services.AddSingleton<AppConfigService>();
builder.Services.AddScoped<DocumentNumberService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddScoped<IJournalActiviteService, JournalActiviteService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProduitService, ProduitService>();
builder.Services.AddScoped<IFournisseurService, FournisseurService>();
builder.Services.AddScoped<IFactureClientService, FactureClientService>();
builder.Services.AddScoped<IDevisClientService, DevisClientService>();
builder.Services.AddScoped<ICommandeVenteService, CommandeVenteService>();
builder.Services.AddScoped<IBonLivraisonService, BonLivraisonService>();
builder.Services.AddScoped<ICommandeAchatService, CommandeAchatService>();
builder.Services.AddScoped<IBonReceptionService, BonReceptionService>();
builder.Services.AddScoped<IFactureFournisseurService, FactureFournisseurService>();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// French culture
var frCulture = new System.Globalization.CultureInfo("fr-FR");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture   = frCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = frCulture;

var app = builder.Build();

// Create tables via raw SQL + seed data at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Create auth tables via raw SQL if not yet in migrations
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'utilisateurs')
        BEGIN
            CREATE TABLE utilisateurs (
                id_utilisateur           INT IDENTITY(1,1) PRIMARY KEY,
                login_utilisateur        NVARCHAR(50)  NOT NULL,
                passwordhash_utilisateur NVARCHAR(255) NOT NULL,
                prenom_utilisateur       NVARCHAR(50)  NOT NULL,
                nom_utilisateur          NVARCHAR(50)  NOT NULL,
                email_utilisateur        NVARCHAR(100) NULL,
                role_utilisateur         NVARCHAR(20)  NOT NULL DEFAULT 'Employé',
                actif_utilisateur        BIT           NOT NULL DEFAULT 1,
                datecreation_utilisateur DATETIME2     NOT NULL DEFAULT GETDATE()
            )
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'journalactivite')
        BEGIN
            CREATE TABLE journalactivite (
                id_journal           INT IDENTITY(1,1) PRIMARY KEY,
                login_journal        NVARCHAR(50)  NOT NULL,
                action_journal       NVARCHAR(100) NOT NULL,
                entite_journal       NVARCHAR(50)  NOT NULL,
                codeentite_journal   NVARCHAR(50)  NULL,
                dateheure_journal    DATETIME2     NOT NULL DEFAULT GETDATE(),
                detail_journal       NVARCHAR(255) NULL
            )
        END
        """);

    // ── RBAC tables ────────────────────────────────────────────────────────
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'company')
        BEGIN
            CREATE TABLE company (
                id_company       INT IDENTITY(1,1) PRIMARY KEY,
                name_company     NVARCHAR(100) NOT NULL,
                slug_company     NVARCHAR(50)  NULL,
                plan_company     NVARCHAR(50)  NOT NULL DEFAULT 'Standard',
                settings_company NVARCHAR(MAX) NULL
            )
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'app_role')
        BEGIN
            CREATE TABLE app_role (
                id_role         INT IDENTITY(1,1) PRIMARY KEY,
                name_role       NVARCHAR(100) NOT NULL,
                company_id_role INT NULL REFERENCES company(id_company)
            )
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'permission')
        BEGIN
            CREATE TABLE permission (
                id_permission      INT IDENTITY(1,1) PRIMARY KEY,
                feature_permission NVARCHAR(100) NOT NULL,
                action_permission  NVARCHAR(50)  NOT NULL
            )
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'user_role')
        BEGIN
            CREATE TABLE user_role (
                user_id INT NOT NULL REFERENCES utilisateurs(id_utilisateur),
                role_id INT NOT NULL REFERENCES app_role(id_role),
                PRIMARY KEY (user_id, role_id)
            )
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'role_permission')
        BEGIN
            CREATE TABLE role_permission (
                role_id       INT NOT NULL REFERENCES app_role(id_role),
                permission_id INT NOT NULL REFERENCES permission(id_permission),
                PRIMARY KEY (role_id, permission_id)
            )
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'feature_flag')
        BEGIN
            CREATE TABLE feature_flag (
                id_feature_flag   INT IDENTITY(1,1) PRIMARY KEY,
                company_id_flag   INT NOT NULL REFERENCES company(id_company),
                feature_name_flag NVARCHAR(100) NOT NULL,
                is_enabled_flag   BIT NOT NULL DEFAULT 1
            )
        END
        """);

    // Add company_id column to utilisateurs if missing
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'utilisateurs' AND COLUMN_NAME = 'company_id_utilisateur'
        )
        BEGIN
            ALTER TABLE utilisateurs ADD company_id_utilisateur INT NULL REFERENCES company(id_company)
        END
        """);

    // ── RBAC seed ──────────────────────────────────────────────────────────
    // Default company (let IDENTITY assign the PK — don't specify id_company)
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM company WHERE slug_company = 'default')
            INSERT INTO company (name_company, slug_company, plan_company)
            VALUES ('Entreprise Défaut', 'default', 'Standard')
        """);

    // Permissions (document modules) — seeded as one batch with IDENTITY_INSERT
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM permission WHERE id_permission = 1)
        BEGIN
            SET IDENTITY_INSERT permission ON;
            INSERT INTO permission (id_permission, feature_permission, action_permission) VALUES  (1,'factures','view'),(2,'factures','create'),(3,'factures','update'),(4,'factures','delete'),
             (5,'devis','view'),(6,'devis','create'),(7,'devis','update'),(8,'devis','delete'),
             (9,'commandes-vente','view'),(10,'commandes-vente','create'),(11,'commandes-vente','update'),(12,'commandes-vente','delete'),
             (13,'bons-livraison','view'),(14,'bons-livraison','create'),(15,'bons-livraison','update'),(16,'bons-livraison','delete'),
             (17,'commandes-achat','view'),(18,'commandes-achat','create'),(19,'commandes-achat','update'),(20,'commandes-achat','delete'),
             (21,'bons-reception','view'),(22,'bons-reception','create'),(23,'bons-reception','update'),(24,'bons-reception','delete'),
             (25,'factures-fournisseur','view'),(26,'factures-fournisseur','create'),(27,'factures-fournisseur','update'),(28,'factures-fournisseur','delete');
            SET IDENTITY_INSERT permission OFF;
        END
        """);

    // Ensure master-data permissions exist (clients, fournisseurs, produits)
    db.Database.ExecuteSqlRaw("""
        INSERT INTO permission (feature_permission, action_permission)
        SELECT v.feature_permission, v.action_permission
        FROM (VALUES
            ('clients','view'),('clients','create'),('clients','update'),('clients','delete'),
            ('fournisseurs','view'),('fournisseurs','create'),('fournisseurs','update'),('fournisseurs','delete'),
            ('produits','view'),('produits','create'),('produits','update'),('produits','delete')
        ) AS v(feature_permission, action_permission)
        WHERE NOT EXISTS (
            SELECT 1
            FROM permission p
            WHERE p.feature_permission = v.feature_permission
              AND p.action_permission = v.action_permission
        )
        """);

    // System roles
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM app_role WHERE id_role = 1)
        BEGIN
            SET IDENTITY_INSERT app_role ON;
            INSERT INTO app_role (id_role, name_role, company_id_role) VALUES (1, 'Admin',   NULL);
            INSERT INTO app_role (id_role, name_role, company_id_role) VALUES (2, 'Manager', NULL);
            INSERT INTO app_role (id_role, name_role, company_id_role) VALUES (3, 'Employé', NULL);
            SET IDENTITY_INSERT app_role OFF;
        END
        """);

    // RolePermissions: Admin=all document permissions, Manager=view+create+update, Employé=view+create
    for (int i = 1; i <= 28; i++)
        db.Database.ExecuteSqlRaw($"""
            IF NOT EXISTS (SELECT 1 FROM role_permission WHERE role_id=1 AND permission_id={i})
                INSERT INTO role_permission (role_id, permission_id) VALUES (1, {i})
            """);
    for (int m = 0; m < 7; m++)
        for (int a = 0; a < 3; a++)
        {
            int pid = m * 4 + a + 1;
            db.Database.ExecuteSqlRaw($"""
                IF NOT EXISTS (SELECT 1 FROM role_permission WHERE role_id=2 AND permission_id={pid})
                    INSERT INTO role_permission (role_id, permission_id) VALUES (2, {pid})
                """);
        }
    for (int m = 0; m < 7; m++)
        for (int a = 0; a < 2; a++) // view=0, create=1
        {
            int pid = m * 4 + a + 1;
            db.Database.ExecuteSqlRaw($"""
                IF NOT EXISTS (SELECT 1 FROM role_permission WHERE role_id=3 AND permission_id={pid})
                    INSERT INTO role_permission (role_id, permission_id) VALUES (3, {pid})
                """);
        }

    // Default grants for master-data modules (clients, fournisseurs, produits)
    db.Database.ExecuteSqlRaw("""
        INSERT INTO role_permission (role_id, permission_id)
        SELECT 1, p.id_permission
        FROM permission p
        WHERE p.feature_permission IN ('clients', 'fournisseurs', 'produits')
          AND NOT EXISTS (
              SELECT 1
              FROM role_permission rp
              WHERE rp.role_id = 1 AND rp.permission_id = p.id_permission
          )
        """);

    db.Database.ExecuteSqlRaw("""
        INSERT INTO role_permission (role_id, permission_id)
        SELECT 2, p.id_permission
        FROM permission p
        WHERE p.feature_permission IN ('clients', 'fournisseurs', 'produits')
          AND p.action_permission IN ('view', 'create', 'update')
          AND NOT EXISTS (
              SELECT 1
              FROM role_permission rp
              WHERE rp.role_id = 2 AND rp.permission_id = p.id_permission
          )
        """);

        db.Database.ExecuteSqlRaw("""
            INSERT INTO role_permission (role_id, permission_id)
            SELECT 3, p.id_permission
            FROM permission p
            WHERE p.action_permission IN ('view', 'create')
                AND NOT EXISTS (
                    SELECT 1
                    FROM role_permission rp
                    WHERE rp.role_id = 3 AND rp.permission_id = p.id_permission
                )
            """);

        // Enforce employee policy globally: create allowed, update/delete denied.
        db.Database.ExecuteSqlRaw("""
                DELETE rp
                FROM role_permission rp
                INNER JOIN permission p ON p.id_permission = rp.permission_id
                WHERE rp.role_id = 3
                    AND p.action_permission IN ('update', 'delete')
                """);

    // Seed: créer admin par défaut si aucun utilisateur n'existe
    if (!db.Utilisateurs.Any())
    {
        var utilisateurService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
        utilisateurService.AddAsync(new Utilisateur
        {
            Login  = "admin",
            Prenom = "Admin",
            Nom    = "Système",
            Role   = "Admin",
            Actif  = true
        }, "admin123").GetAwaiter().GetResult();
    }

    var seedMockData = app.Configuration.GetValue<bool>("MockData:Enabled") ||
                       string.Equals(Environment.GetEnvironmentVariable("SEED_MOCK_DATA"), "true", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(Environment.GetEnvironmentVariable("SEED_MOCK_DATA"), "1", StringComparison.OrdinalIgnoreCase);

    if (seedMockData)
    {
        var mockLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MockDataSeeder");
        MockDataSeeder.Seed(db, mockLogger);
    }

    // Backfill/sync legacy role_utilisateur values into user_role mapping table.
    var roleSyncService = scope.ServiceProvider.GetRequiredService<IUtilisateurService>();
    roleSyncService.SynchronizeLegacyRolesAsync().GetAwaiter().GetResult();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
