# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Web GestCom** is a Blazor Server web application (.NET 8) for commercial/business management targeting French-speaking markets. It handles sales, purchasing, inventory, invoicing, and financial management with multi-tenant support and role-based access control.

## Build & Run

Run from this folder (`GestCommercial/Web_GestCom/`):

```bash
# Restore / build
dotnet restore Web_GestCom.sln
dotnet build Web_GestCom.sln

# Run the application (http://localhost:5087 with the "http" profile)
dotnet run --project Web_GestCom.csproj --launch-profile http

# Run all tests (test project is a sibling folder)
dotnet test ../Web_GestCom.Tests

# Run a single test class
dotnet test ../Web_GestCom.Tests --filter "FullyQualifiedName~ClassName"
```

Stop the running app before building/testing — it locks the DLLs.

Default credentials: `admin` / `admin123`. Login path: `/compte/connexion`.

## Architecture

**Layered Blazor Server architecture**, split across two projects: the web host
(`Web_GestCom/` — Blazor components, Razor Pages, `Auth/`, `Program.cs`) and the class library
`../Web_GestCom.Core/` (EF Core entities, `AppDbContext`, and almost all services), referenced by
the host.

```
Components/Pages (Blazor UI)
    ↓
Services/ (business logic via interfaces)
    ↓
Data/AppDbContext (EF Core 8, SQL Server)
    ↓
Data/Models/ (26 EF Core entities)
```

### Key layers

**`Web_GestCom.Core/Data/Models/`** — EF Core entities. All multi-tenant entities implement `ITenantOwned` (filtered automatically at DbContext level). Key entities: `Client`, `Produit`, `Fournisseur`, `FactureClient`, `FactureFournisseur`, `BonLivraison`, `BonReception`, `DevisClient`, `CommandeVente`, `CommandeAchat`, `Utilisateur`, `JournalActivite`.

**`Web_GestCom.Core/Data/AppDbContext.cs`** — Single context with global query filters for tenant isolation. Uses `EnsureCreated()` + raw SQL migrations. Seeds reference data (currencies, TVA rates, payment modes, units, categories) and the default admin user on first run.

**`Web_GestCom.Core/Services/`** — One service interface + implementation per entity/domain area. All services receive `IHttpContextAccessor` to resolve the current tenant from claims. Stock mutations (increments/decrements) happen inside `BonLivraisonService`, `BonReceptionService`, `FactureClientService`, and `FactureFournisseurService` — not in the DbContext.

**`Auth/`** — Custom RBAC: `PermissionClaimsTransformation` loads permissions into claims on each request. `PermissionPolicyProvider` resolves `[Authorize(Policy = "perm:feature.action")]` at runtime via `PermissionAuthorizationHandler`. Permission checks live in both routes/pages (`PermissionAuthorizationHandler`) and services (`ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "feature.action")` as the first line of every mutating method). **Security-critical rule, do not regress**: `Admin` bypasses these checks entirely within its own company; `SuperAdmin` never does — it falls through to `HasPermissionAsync` and only holds platform-scoped permissions (`tenants`, `users-global`, `roles-global`, `journal-global`). SuperAdmin is a platform-management role with zero business-data access by design, enforced at three independent layers: the permission guard above, `AppDbContext`'s two distinct tenant query filters (`ShouldApplyTenantFilter` — active even for SuperAdmin, which naturally excludes all its rows since it has no `CompanyId`; vs. `ShouldApplyTenantFilterToAuthenticatedUsers` — SuperAdmin *does* bypass this one, but it only governs the `Utilisateur` entity, for the global cross-company users dashboard), and `ApplyTenantOwnershipRules` on the write side.

**`Components/Pages/`** — Feature-based organization. Each major entity has its own subdirectory with list, add/edit, and detail pages. Shared UI components (notification toasts, confirm dialogs, print layout) are in `Components/Shared/`.

**`Pages/Compte/`** — Razor Pages (not Blazor) for everything reachable anonymously: login/logout,
`Inscription`, `MotDePasseOublie`, `AccesRefuse`, and the public subscription form
`DemandeAbonnement` (`/demande-abonnement`, linked from the pricing section of `Home.razor`).
Subscription requests are stored in the `abonnement` table via `AbonnementService` and handled
manually by the SuperAdmin at `/admin/demandes-abonnement` — no online payment is wired up.

## Document Numbering Convention

All commercial documents use sequential codes: `{Prefix}{YYYYMM}{###}` (e.g., `FC20240100001`). The generation logic is centralized in `DocumentNumberService.Next*Async` — always use it rather than computing codes inline. Simple reference entities (`Client`/`Produit`/`Fournisseur`, codes like `CL00001`) use a separate, simpler generator, `AppDbContextSaveExtensions.GenerateNextCodeAsync(existingCodes, prefix, numberLength)` — both this and `DocumentNumberService` compute the next number from the **MAX existing number**, never `COUNT(*)` (see Known Pitfalls below).

| Prefix | Document |
|--------|----------|
| `CL` | Client |
| `FO` | Fournisseur |
| `DV` | Devis |
| `CV` | Commande Vente |
| `BL` | Bon de Livraison |
| `FC` | Facture Client |
| `CA` | Commande Achat |
| `BR` | Bon de Réception |
| `FF` | Facture Fournisseur |

## Stock & Financial Logic

- **Stock is modified** only when a `BonLivraison` or `FactureClient` (sales) or `BonReception` / `FactureFournisseur` (purchases) is created or deleted — not on order/quote creation.
- **Payment state** (`NonPayee`, `PartielPayee`, `Payee`) is computed from `ReglementFactureClient` / `ReglementFactureFournisseur` records, not stored directly.
- **`IsAvoir`** flag on `FactureClient` marks credit notes (there is no supplier credit note — `FactureFournisseur` has no such flag) — these reverse stock movements. An avoir is stored with **positive** amounts copied from its source invoice (`MontantHT`, `MontantRetenue`...): any report that totals invoices must negate avoirs itself (see `Rapports/RetenueRecap.razor`), and must never add sales and purchase withholdings together (retenue subie vs. opérée).
- Cloning a document generates a new code and resets payment/status fields.

## Testing

- **Framework**: xUnit + bUnit (Blazor components) + Moq
- **Test project**: `Web_GestCom.Tests/`
- **Helpers**: `TestDbContextFactory` (EF Core InMemory), `NoOpJournalService`
- Tests are organized by layer: `Components/`, `Services/`, `Data/`
- Use `InMemoryDatabase` for service tests; use bUnit `TestContext` for component rendering

## Configuration

`appsettings.json` key entries:

```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "AppSettings": {
    "TimbreFiscal": 0.6,
    "TauxRetenue": 1.5,
    "DisplayRemise": "Yes",
    "DisplayTVA": "Yes",
    "PathLogo": "./logoApp.png"
  }
}
```

Secrets and environment-specific overrides go in `appsettings.Development.json` (gitignored for passwords).

**Tarifs** (`Tarifs` section, `TarifsOptions` in `Web_GestCom.Core/Services/TarifsOptions.cs`): single
source for every public price (subscriptions Standard/Pro in `Annuel`/`Mensuel`, Desktop formulas),
the trial length (`JoursEssai`) and the included onboarding hours (`InitiationHeures`). Never hardcode a price or "30
jours" in a page — `Home.razor`, `DemandeAbonnement` and `Admin/AbonnementsList.razor` all read these
options. Promo codes live in the `code_promo` table (`CodePromo` entity, seeded with `FONDATEUR2026` by
`Program.cs`); `AbonnementService.CreateDemandeAsync` always recomputes `PrixCatalogue`/`PrixApplique`
server-side (never trusts the form) and a promo "place" is only consumed by a subscription in status
`Essai`/`Active`, not by a mere request. The price shown live on `/demande-abonnement` comes from an
inline script, so links to that page carry `data-enhance-nav="false"` (a script reached through Blazor
enhanced navigation never runs).

**Desktop licence offers** (`Tarifs:Desktop`): three one-time purchases shown in the third tab of the
pricing toggle (Annuel | Mensuel | Application Desktop) and sold through the same `/demande-abonnement`
form — plans `DesktopEssentiel` (licence only, short warranty, no updates), `DesktopSerenite` and
`DesktopEquipe` (one year of updates and support included, then an optional yearly maintenance).
`TarifsOptions.EstAchatUnique(plan)` is the single switch for everything that differs from a
subscription: no annual/monthly cycle, promo codes ignored, and on activation **no web company,
account or confirmation email** (the software runs on the customer's PC; the licence file is produced
by hand with `GestCom_LicenseGenerator`). Display a plan with `TarifsOptions.NomPlan(plan)`.
**All prices are HT** and must be labelled so ("HT", "TVA en sus") wherever one is shown.
The "Clients Fondateurs" discount was withdrawn on 2026-10-08: no banner, no promo field on the form,
and `Program.cs` expires the `FONDATEUR2026` row. The promo mechanism itself (`code_promo` table,
`AbonnementService.GetOffrePromoAsync`, `Tarifs:CodePromoMisEnAvant`) is kept, unused, for a later campaign.

**Subscription activation** (`AbonnementActivationService`, used by `Admin/AbonnementsList.razor`): saving
a request in status `Essai`/`Active` is what gives the customer real access — in one transaction it
creates the `Company` if none is linked, creates an `Admin` user if that company has no user yet (login
suggested from the contact name, password generated, both editable in the dialog), then emails the
customer (credentials on first activation, a plain "mis à jour" email afterwards; the SuperAdmin can
untick it). Never call `AbonnementService.UpdateAsync` directly from that screen again — it would skip
all of this. Links in customer emails use `Email:AppUrl`.

**Email** (`Email` section, `EmailOptions` in `Web_GestCom.Core/Services/IEmailTransport.cs`):
`Program.cs` registers `BrevoEmailTransport` only when `Email:Provider` is `Brevo` **and**
`Email:BrevoApiKey` is set (user-secrets in dev, `Email__BrevoApiKey` env var in prod via
`deploy/prod/.env`); otherwise `NoOpEmailTransport`, which only logs. `AbonnementService` emails
the prospect and `AdminNotificationEmail` on each new request.

## Known Pitfalls (real production bugs — don't reintroduce these)

- **Blazor Server's `AppDbContext` lives for the whole circuit, not one request.** A service that
  reads with `AsNoTracking()` (`ClientService`, `ProduitService`, `FournisseurService`,
  `UtilisateurService`) must call `db.DetachStaleTrackedEntry(entity)` immediately before
  `Update()`/`Remove()`. Without it: add an entity, then edit that *same* entity later in the same
  browser session (a completely normal flow) → `InvalidOperationException: The instance of entity
  type 'X' cannot be tracked because another instance with the same key value ... is already being
  tracked`. Nulling navigation properties before `Update()` does **not** fix this — that's a
  different, narrower conflict (the loaded graph vs. a dropdown's tracked reference data), not the
  entity's own primary key colliding with a stale `Added`/`Unchanged` entry from earlier in the
  circuit. A regression test for this must **not** call `db.ChangeTracker.Clear()` after the initial
  `Add` — that artificially hides the bug a real circuit can't avoid.
- **Sequential code generation (`CodeClient`/`CodeProduit`/`CodeFournisseur`) must use MAX(existing
  number), never `COUNT(*)`.** `COUNT(*) + 1` recomputes an already-taken number as soon as any row
  is deleted (e.g. 3 rows, delete the 2nd → count=2 → next code collides with the still-present 3rd
  row) → `PRIMARY KEY` violation on insert. Use `AppDbContextSaveExtensions.GenerateNextCodeAsync`.
- **`SaveChangesGuardedAsync`'s generic `DbUpdateException` catch appends `InnerException.Message`**
  — don't remove this. EF Core's default message ("An error occurred while saving the entity
  changes...") never shows the real SQL error, which made a production bug much slower to diagnose
  than necessary before this was added. That full message is for the **logs only**, never the screen.
- **Never show `ex.Message` to the user in a `catch`.** Use `_notif.ShowError(ex)` /
  `_notif.ShowError(ex, "Erreur lors du clonage")`, `_notif.ShowDeleteError(ex, "<FK message>")`, or
  `_notif.DescribeDeleteError(...)` for pages showing the error in a dialog (`Components/Shared/Notification.razor`).
  It logs the full exception (SQL inner exception + stack) with a short reference, and displays
  `UserErrorMessage.Build(...)` (`Web_GestCom.Core/Services/UserErrorMessage.cs`): business messages
  (`InvalidOperationException`/`UnauthorizedAccessException` thrown from `Web_GestCom*` code,
  `ConcurrencyConflictException`) are shown as-is; anything technical becomes "Problème de
  sauvegarde…" / "Une erreur technique est survenue…" + "(réf. XXXXXXXX)". To diagnose a user report,
  grep the logs for that reference. Consequence for services: a message meant for the user must be
  thrown as `InvalidOperationException` from our own code — anything else will be masked.
- **Business keys are per company: `(CompanyId, code)`, never the code alone** (10 tables: client,
  fournisseur, produit and the 7 document tables; their 9 line/payment tables carry `CompanyId` too
  and are `ITenantOwned`). Until 2026-10 the code alone was a database-wide primary key while
  numbering restarts at `CL00001`/`FC2026...` in every company, so a second company could not create
  a single client (`PRIMARY KEY` violation on `CL00001`, reproduced). Consequences when writing code:
  - **Never `FindAsync(code)` on these sets** — it needs both key parts and bypasses the tenant
    filter. Use `FirstOrDefaultAsync(e => e.CodeX == code)`: the query filter supplies the company.
  - **`CompanyId` is stamped automatically** when a row enters the context
    (`AppDbContext.StampCompanyIdOnNewRows`, a `ChangeTracker.Tracking` handler — an EF
    `ValueGenerator` does not work here, EF ignores it on a property that is also a foreign key).
    Without an execution context (startup seed, unit tests) rows go to `Company.DefaultId`.
  - **Every link to these tables is two columns** and is declared in `AppDbContext.ConfigureTenantKeys`
    (no `[ForeignKey]` attribute). A new document/line type must be added there and to
    `AppDbContext.TenantKeyedTypes`.
  - **The three traceability links** (BL→commande vente, BR→commande achat, facture→BL d'origine) are
    `ClientSetNull`: SQL Server refuses `ON DELETE SET NULL` on a link sharing a `NOT NULL`
    `CompanyId`, so the services clear the link themselves before deleting the source document.
  - **Existing databases are upgraded by `TenantKeyMigration.Apply`** (called from `Program.cs`), driven
    by the EF model, in one transaction, idempotent. Never add or alter a foreign key on these tables
    with raw SQL in `Program.cs` again — a single-column FK would fail at startup.
- **Also run `dotnet test ../GestCom_Desktop.Tests`** after touching `Web_GestCom.Core`: the Desktop app
  shares it (it was broken once by a Core change nobody ran its tests against).
  `DeleteErrorMessageHelper.Build(ex, friendly)` (2 args) is the Desktop overload and shows the raw
  error; the 3-arg overload with a log reference is the Web one.
- **Still open: reference tables are shared by all companies.** `categorieproduit`, `fabriquantproduit`,
  `uniteproduit`, `tvaproduit`, `modepayement`, `devise` have no `CompanyId`, and any company's Admin
  can edit or delete them under `/parametres/*` for everyone.
- **`Entreprise` (company profile printed on every document) is `ITenantOwned` — one row per Company.**
  The 7 print pages and `/entreprise` all do `Db.Entreprises.FirstOrDefaultAsync()`; that is only
  correct because of the tenant query filter, so never add `IgnoreQueryFilters()` there. Its primary
  key `code_entreprise` is global too: new rows use `Entreprise.CodePourCompany(companyId)`
  (`ENT004`...), and `AbonnementActivationService` pre-creates the row for each new customer.
  `NomEntreprise` and `MatriculeFiscale` are mandatory **in the form** (`EntrepriseForm.SubmitAsync`),
  deliberately not via `[Required]` on the entity: the row is pre-created before the matricule is
  known. `IEntrepriseService.IsFicheIncompleteAsync` drives the Admin-only reminder on the dashboard.
  The logo is uploaded from that form and stored **in the database** as a data URI
  (`Entreprise.LogoImage`, validated by `Services/LogoImage`: PNG/JPEG/GIF/WebP by magic bytes, 200 KB
  max, no SVG) — never as a file under `wwwroot`, which is lost on every redeploy. Always render it
  with `Entreprise.LogoSource` (uploaded logo, else legacy `PathLogo`, else default).
- **Email failures are invisible to the user by design.** `AbonnementService` catches and logs
  notification errors, and `BrevoEmailTransport` only logs non-2xx responses — the page still says
  "Demande envoyée !". Always check the logs for `Échec d'envoi email via Brevo`. The Brevo account
  restricts API calls to **authorised IPs** (Brevo → Security → Authorised IPs): any new machine or
  server IP (IPv4 *and* IPv6) gets `401 unauthorized — unrecognised IP address` until it is added.
- **Antiforgery tokens are bound to the user identity at render time.** A Razor Page form loaded
  while anonymous and submitted after logging in (another tab, expired session) fails validation;
  the default Razor Pages behavior is an **empty 400** that browsers show as a blank error page
  (real prod report on `/demande-abonnement`, 2026-09-27, perceived as a "404"). Public forms should
  follow `DemandeAbonnementModel`: `[IgnoreAntiforgeryToken]` on the PageModel, then
  `IAntiforgery.IsRequestValidAsync(HttpContext)` at the top of the POST handler, returning `Page()`
  with a model error (the re-render issues a fresh token for the current user) instead of a 400.
  To find these in prod, grep nginx-proxy logs for the path — the app itself logs nothing (antiforgery
  failures log below the configured `Microsoft.AspNetCore` level).
- **A `<script>` tag written inside a `.razor` component's markup never executes** — Blazor Server
  inserts DOM via its own diffing, not `innerHTML`/`appendChild`, and browsers don't run scripts
  inserted that way. Put JS in a real `wwwroot/js/*.js` file referenced by `<script src="...">` in
  `App.razor`, and trigger it via `@inject IJSRuntime JS` + `JS.InvokeVoidAsync(...)` from
  `OnAfterRenderAsync(firstRender)`.

## Domain Language (French)

| French | English |
|--------|---------|
| Facture | Invoice |
| Devis | Quote |
| Bon de livraison | Delivery note |
| Bon de réception | Receiving note |
| Commande | Order |
| Fournisseur | Supplier |
| Règlement | Payment |
| Retenue | Tax withholding |
| TVA | VAT |
| Remise | Discount |
| Avoir | Credit note |
| Timbre fiscal | Fiscal stamp |
