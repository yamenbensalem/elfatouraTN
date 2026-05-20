# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Web T4C GestCom** is a Blazor Server web application (.NET 8) for commercial/business management targeting French-speaking markets. It handles sales, purchasing, inventory, invoicing, and financial management with multi-tenant support and role-based access control.

## Build & Run

```bash
# Restore dependencies
dotnet restore Web_T4C_GestCom.sln

# Run the application
dotnet run --project Web_T4C_GestCom

# Run all tests
dotnet test Web_T4C_GestCom.Tests

# Run a single test class
dotnet test Web_T4C_GestCom.Tests --filter "FullyQualifiedName~ClassName"

# Build only
dotnet build Web_T4C_GestCom.sln
```

Default credentials: `admin` / `admin123`. Login path: `/compte/connexion`.

## Architecture

**Layered Blazor Server architecture:**

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

**`Data/Models/`** — 26 EF Core entities. All multi-tenant entities implement `ITenantOwned` (filtered automatically at DbContext level). Key entities: `Client`, `Produit`, `Fournisseur`, `FactureClient`, `FactureFournisseur`, `BonLivraison`, `BonReception`, `DevisClient`, `CommandeVente`, `CommandeAchat`, `Utilisateur`, `JournalActivite`.

**`Data/AppDbContext.cs`** — Single context with global query filters for tenant isolation. Uses `EnsureCreated()` + raw SQL migrations. Seeds reference data (currencies, TVA rates, payment modes, units, categories) and the default admin user on first run.

**`Services/`** — One service interface + implementation per entity/domain area. All services receive `IHttpContextAccessor` to resolve the current tenant from claims. Stock mutations (increments/decrements) happen inside `BonLivraisonService`, `BonReceptionService`, `FactureClientService`, and `FactureFournisseurService` — not in the DbContext.

**`Auth/`** — Custom RBAC: `PermissionClaimsTransformation` loads permissions into claims on each request. `DynamicAuthorizationPolicyProvider` resolves `[Authorize(Policy = "Permission.X")]` at runtime. Permission checks live in both services (service-level guard) and Blazor components (UI-level guard).

**`Components/Pages/`** — Feature-based organization. Each major entity has its own subdirectory with list, add/edit, and detail pages. Shared UI components (notification toasts, confirm dialogs, print layout) are in `Components/Shared/`.

**`Pages/`** — Razor Pages for authentication only (`Connexion.cshtml`, `Deconnexion.cshtml`).

## Document Numbering Convention

All commercial documents use sequential codes: `{Prefix}{YYYYMM}{###}` (e.g., `FC20240100001`). The generation logic is centralized — always use the existing `GetNextCode*` helper in the relevant service rather than computing codes inline.

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
- **`IsAvoir`** flag on `FactureClient` / `FactureFournisseur` marks credit notes — these reverse stock movements.
- Cloning a document generates a new code and resets payment/status fields.

## Testing

- **Framework**: xUnit + bUnit (Blazor components) + Moq
- **Test project**: `Web_T4C_GestCom.Tests/`
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
