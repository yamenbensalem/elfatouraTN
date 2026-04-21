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
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var principal = context.Principal;
                if (principal is null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var userId = principal.GetUserId();
                if (!userId.HasValue)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                await using var scope = context.HttpContext.RequestServices.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var user = await db.Utilisateurs
                    .AsNoTracking()
                    .Where(u => u.Id == userId.Value)
                    .Select(u => new
                    {
                        u.Actif,
                        u.Role,
                        u.CompanyId,
                        u.IsSuperAdmin,
                        u.SecurityStamp,
                        u.PermissionsVersion
                    })
                    .FirstOrDefaultAsync();

                if (user is null || !user.Actif)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var claimStamp = principal.GetSecurityStamp();
                var claimPermVersion = principal.GetPermissionsVersion();
                var claimCompanyId = principal.GetCompanyId();

                var expectedRole = user.IsSuperAdmin
                    ? RoleNameMapper.SuperAdmin
                    : RoleNameMapper.NormalizeKnownRoleName(user.Role);

                var roleMismatch = !principal.IsInRole(expectedRole);
                var stampMismatch = string.IsNullOrWhiteSpace(claimStamp) ||
                                    !string.Equals(claimStamp, user.SecurityStamp, StringComparison.Ordinal);
                var permissionVersionMismatch = !claimPermVersion.HasValue || claimPermVersion.Value != user.PermissionsVersion;
                var tenantMismatch = user.IsSuperAdmin
                    ? claimCompanyId.HasValue
                    : !user.CompanyId.HasValue || claimCompanyId != user.CompanyId;

                if (roleMismatch || stampMismatch || permissionVersionMismatch || tenantMismatch)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<LoginProtectionOptions>(builder.Configuration.GetSection("Security:LoginProtection"));
builder.Services.AddSingleton<ILoginProtectionService, LoginProtectionService>();

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
                datecreation_utilisateur DATETIME2     NOT NULL DEFAULT GETDATE(),
                is_superadmin_utilisateur BIT          NOT NULL DEFAULT 0,
                securitystamp_utilisateur NVARCHAR(64) NOT NULL DEFAULT CONVERT(NVARCHAR(64), NEWID()),
                permissionsversion_utilisateur INT     NOT NULL DEFAULT 1
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
                detail_journal       NVARCHAR(255) NULL,
                company_id_journal   INT NULL
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

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'journalactivite' AND COLUMN_NAME = 'company_id_journal'
        )
        BEGIN
            ALTER TABLE journalactivite ADD company_id_journal INT NULL
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'client' AND COLUMN_NAME = 'company_id_client'
        )
        BEGIN
            ALTER TABLE client ADD company_id_client INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'fournisseur' AND COLUMN_NAME = 'company_id_fournisseur'
        )
        BEGIN
            ALTER TABLE fournisseur ADD company_id_fournisseur INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'produit' AND COLUMN_NAME = 'company_id_produit'
        )
        BEGIN
            ALTER TABLE produit ADD company_id_produit INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'devisClient' AND COLUMN_NAME = 'company_id_devis'
        )
        BEGIN
            ALTER TABLE devisClient ADD company_id_devis INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'commandevente' AND COLUMN_NAME = 'company_id_commandevente'
        )
        BEGIN
            ALTER TABLE commandevente ADD company_id_commandevente INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'bonlivraison' AND COLUMN_NAME = 'company_id_bonlivraison'
        )
        BEGIN
            ALTER TABLE bonlivraison ADD company_id_bonlivraison INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'factureclient' AND COLUMN_NAME = 'company_id_factureclient'
        )
        BEGIN
            ALTER TABLE factureclient ADD company_id_factureclient INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'commandeachat' AND COLUMN_NAME = 'company_id_commandeachat'
        )
        BEGIN
            ALTER TABLE commandeachat ADD company_id_commandeachat INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'bonreception' AND COLUMN_NAME = 'company_id_bonreception'
        )
        BEGIN
            ALTER TABLE bonreception ADD company_id_bonreception INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'facturefournisseur' AND COLUMN_NAME = 'company_id_facturefournisseur'
        )
        BEGIN
            ALTER TABLE facturefournisseur ADD company_id_facturefournisseur INT NULL REFERENCES company(id_company)
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'utilisateurs' AND COLUMN_NAME = 'is_superadmin_utilisateur'
        )
        BEGIN
            ALTER TABLE utilisateurs ADD is_superadmin_utilisateur BIT NOT NULL DEFAULT 0
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'utilisateurs' AND COLUMN_NAME = 'securitystamp_utilisateur'
        )
        BEGIN
            ALTER TABLE utilisateurs ADD securitystamp_utilisateur NVARCHAR(64) NULL
        END
        """);

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'utilisateurs' AND COLUMN_NAME = 'permissionsversion_utilisateur'
        )
        BEGIN
            ALTER TABLE utilisateurs ADD permissionsversion_utilisateur INT NOT NULL DEFAULT 1
        END
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE utilisateurs
        SET securitystamp_utilisateur = CONVERT(NVARCHAR(64), NEWID())
        WHERE securitystamp_utilisateur IS NULL OR LTRIM(RTRIM(securitystamp_utilisateur)) = ''
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE utilisateurs
        SET permissionsversion_utilisateur = 1
        WHERE permissionsversion_utilisateur IS NULL OR permissionsversion_utilisateur < 1
        """);

    // ── RBAC seed ──────────────────────────────────────────────────────────
    // Default company (let IDENTITY assign the PK — don't specify id_company)
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM company WHERE slug_company = 'default')
            INSERT INTO company (name_company, slug_company, plan_company)
            VALUES ('Entreprise Défaut', 'default', 'Standard')
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE u
        SET u.company_id_utilisateur = c.id_company
        FROM utilisateurs u
        CROSS APPLY (
            SELECT TOP 1 id_company
            FROM company
            ORDER BY id_company
        ) c
        WHERE u.is_superadmin_utilisateur = 0
          AND u.company_id_utilisateur IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_client = c.id_company
        FROM client t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_client IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_fournisseur = c.id_company
        FROM fournisseur t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_fournisseur IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_produit = c.id_company
        FROM produit t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_produit IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_devis = c.id_company
        FROM devisClient t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_devis IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_commandevente = c.id_company
        FROM commandevente t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_commandevente IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_bonlivraison = c.id_company
        FROM bonlivraison t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_bonlivraison IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_factureclient = c.id_company
        FROM factureclient t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_factureclient IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_commandeachat = c.id_company
        FROM commandeachat t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_commandeachat IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_bonreception = c.id_company
        FROM bonreception t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_bonreception IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE t
        SET company_id_facturefournisseur = c.id_company
        FROM facturefournisseur t
        CROSS APPLY (SELECT TOP 1 id_company FROM company ORDER BY id_company) c
        WHERE t.company_id_facturefournisseur IS NULL
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE utilisateurs
        SET is_superadmin_utilisateur = 1
        WHERE role_utilisateur = 'SuperAdmin'
        """);

    db.Database.ExecuteSqlRaw("""
        UPDATE utilisateurs
        SET role_utilisateur = 'SuperAdmin',
            company_id_utilisateur = NULL
        WHERE is_superadmin_utilisateur = 1
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

    // Ensure global governance permissions exist (SuperAdmin only).
    db.Database.ExecuteSqlRaw("""
        INSERT INTO permission (feature_permission, action_permission)
        SELECT v.feature_permission, v.action_permission
        FROM (VALUES
            ('tenants','view'),('tenants','create'),('tenants','update'),('tenants','delete'),
            ('users-global','view'),('users-global','create'),('users-global','update'),('users-global','delete'),
            ('roles-global','view'),('roles-global','create'),('roles-global','update'),('roles-global','delete'),
            ('journal-global','view'),('journal-global','create'),('journal-global','update'),('journal-global','delete')
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

    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM app_role WHERE name_role = 'SuperAdmin' AND company_id_role IS NULL)
        BEGIN
            INSERT INTO app_role (name_role, company_id_role)
            VALUES ('SuperAdmin', NULL)
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
        WHERE p.feature_permission IN (
            'clients', 'factures', 'devis', 'commandes-vente', 'bons-livraison',
            'fournisseurs', 'commandes-achat', 'bons-reception', 'factures-fournisseur', 'produits'
        )
          AND p.action_permission IN ('view', 'create')
          AND NOT EXISTS (
              SELECT 1
              FROM role_permission rp
              WHERE rp.role_id = 3 AND rp.permission_id = p.id_permission
          )
        """);

    // Enforce employee policy: no update/delete and no global governance permissions.
    db.Database.ExecuteSqlRaw("""
        DELETE rp
        FROM role_permission rp
        INNER JOIN permission p ON p.id_permission = rp.permission_id
        WHERE rp.role_id = 3
          AND (
              p.action_permission IN ('update', 'delete')
              OR p.feature_permission IN ('tenants', 'users-global', 'roles-global', 'journal-global')
          )
        """);

    // SuperAdmin receives governance/global permissions only.
    db.Database.ExecuteSqlRaw("""
        INSERT INTO role_permission (role_id, permission_id)
        SELECT r.id_role, p.id_permission
        FROM app_role r
        CROSS JOIN permission p
        WHERE r.name_role = 'SuperAdmin'
          AND r.company_id_role IS NULL
          AND p.feature_permission IN ('tenants', 'users-global', 'roles-global', 'journal-global')
          AND NOT EXISTS (
              SELECT 1
              FROM role_permission rp
              WHERE rp.role_id = r.id_role AND rp.permission_id = p.id_permission
          )
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
