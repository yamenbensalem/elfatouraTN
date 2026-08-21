# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository shape

This is a **monorepo of independent projects**, not a single application. Each top-level
directory is its own codebase with its own stack, its own build, and (in most cases) its own
`CLAUDE.md`. There is no shared build system, no shared package manager workspace, and no
code shared between the sub-projects — treat each one in isolation and `cd` into it before
running any command.

Before working in a sub-project, check whether it has its own `CLAUDE.md` and read that first —
it is more specific and takes precedence over the generic notes below.

| Path | Stack | Status | Own CLAUDE.md? |
|---|---|---|---|
| `GestCommercial/Web_T4C_GestCom/` | Blazor Server, .NET 8, EF Core, SQL Server | **Active** — deployed to `http://gestioncom.tijaraflow.fr/` | ✅ [Web_T4C_GestCom/CLAUDE.md](GestCommercial/Web_T4C_GestCom/CLAUDE.md) |
| `gestCom/` | ASP.NET Core Web API, .NET, Clean Architecture (Domain/Application/Infrastructure/WebAPI/Shared), MediatR/CQRS, Angular frontend | In-progress rewrite of the legacy WinForms app | ❌ (see `LEGACY_T4C_REWORK_PLAYBOOK.md`, `ARCHITECTURE_DIAGRAM.md` in that folder) |
| `ClaudePrjt/TunisianEInvoice/` | ASP.NET Core 9 Web API, Clean Architecture, EF Core + SQL Server, QuestPDF, QRCoder, TTN SOAP client | Tunisian e-invoice (El Fatoora / TTN) backend | ❌ (see root [README.md](README.md), [Schema.md](Schema.md)) |
| `ClaudePrjt/einvoice-frontend/` | Angular 20 | Frontend for `TunisianEInvoice` above | ❌ |
| `Compta/`, `InvoiceApp/`, `Paie/` | — | **Empty placeholders** for future modules (accounting, generic invoicing, payroll) | — |

`GestCommercial/CLAUDE.md` and `GestCommercial/.claude/` (rules for Clean Architecture, Angular,
Blazor, C# style, security, testing) are generic boilerplate templates — the top-level
`GestCommercial/CLAUDE.md` in particular describes a WinForms project (`T4C_Commercial_Project`)
that does not exist in this repo; **ignore it** and use
[GestCommercial/Web_T4C_GestCom/CLAUDE.md](GestCommercial/Web_T4C_GestCom/CLAUDE.md) instead,
which accurately documents the actual Blazor app in that folder.

The legacy WinForms original (raw ODBC/Access, no ORM, French UI) is not present in this repo,
but both `GestCommercial/Web_T4C_GestCom/` and `gestCom/` are rewrites of it — they model the
same commercial-management domain (clients, produits, factures, bons de livraison/réception,
devis, commandes) with different architectures. Don't assume changes in one apply to the other.

## Domain language (French)

All three commercial-management projects (`Web_T4C_GestCom`, `gestCom`) use French domain
terms throughout entities, UI, and DB:

| French | English |
|---|---|
| Facture | Invoice |
| Devis | Quote |
| Bon de livraison | Delivery note |
| Bon de réception | Receiving note |
| Commande (Vente/Achat) | Order (Sales/Purchase) |
| Fournisseur | Supplier |
| Client | Customer |
| Règlement | Payment |
| Retenue (à la source) | Tax withholding |
| TVA | VAT |
| Remise | Discount |
| Avoir | Credit note |
| Timbre fiscal | Fiscal stamp |

## Build & run per sub-project

### GestCommercial/Web_T4C_GestCom (active Blazor app)
```bash
cd GestCommercial/Web_T4C_GestCom
dotnet restore Web_T4C_GestCom.sln
dotnet run --project Web_T4C_GestCom
dotnet test ../Web_T4C_GestCom.Tests
```
Default login: `admin` / `admin123` at `/compte/connexion`. Full architecture details, document
numbering conventions, stock/payment logic, and testing conventions are in its own
[CLAUDE.md](GestCommercial/Web_T4C_GestCom/CLAUDE.md) — read that before making changes here.

Docker/VPS deployment for this app lives in `GestCommercial/Web_T4C_GestCom/deploy/prod/` (see
`DEPLOY.md` there) and follows the shared OVH VPS Docker conventions in the user's global
instructions (nginx-proxy + acme-companion already running on the VPS; never redefine them,
always declare networks as `external: true` except in `docker-compose.infra.yml`).

### gestCom (Clean Architecture rewrite, WebAPI + Angular)
```bash
cd gestCom
dotnet build GestCom.sln
cd frontend && npm install && npm start
```
Projects: `GestCom.Domain`, `GestCom.Application`, `GestCom.Infrastructure`, `GestCom.WebAPI`,
`GestCom.Shared` (dependencies flow inward: WebAPI → Application → Domain, Infrastructure
implements Domain interfaces). See `gestCom/ARCHITECTURE_DIAGRAM.md` and
`gestCom/LEGACY_T4C_REWORK_PLAYBOOK.md` for the rationale and migration plan from the legacy app.

### ClaudePrjt/TunisianEInvoice (e-invoice backend)
```bash
cd ClaudePrjt/TunisianEInvoice
dotnet restore TunisianEInvoice.sln
dotnet run --project src/TunisianEInvoice.API   # http://localhost:5230
dotnet test tests/TunisianEInvoice.UnitTests
dotnet test tests/TunisianEInvoice.IntegrationTests
```
Projects: `TunisianEInvoice.Domain`, `.Application`, `.Infrastructure`, `.API`. Key pieces:
`QrCodeService` (El Fatoora QR format `MF|NumFac|Date|TotalTTC`), `PdfGeneratorService`
(QuestPDF), `TtnService` (SOAP client for TTN invoice submission/validation), `EInvoiceDbContext`
(EF Core / SQL Server). See root [README.md](README.md) for the API surface and
[Schema.md](Schema.md) for the entity relationships (Enterprises → Users/Products,
Clients → InvoiceRecords).

### ClaudePrjt/einvoice-frontend (Angular 20)
```bash
cd ClaudePrjt/einvoice-frontend
npm install
npm start   # http://localhost:4200
```

## VPS deployment conventions (applies to any Dockerized sub-project)

When generating deployment scripts, Dockerfiles, or docker-compose files targeting the shared
OVH VPS (`vps-bf0b3440.vps.ovh.net`), follow the global rules already codified in the user's
`~/.claude/CLAUDE.md`: `nginx-proxy` + `nginx-letsencrypt` are pre-existing global containers
owning ports 80/443 — never recreate them; app-level compose files declare their network as
`external: true`; only `docker-compose.infra.yml` defines the network itself; use
`$http_upgrade` (never `$connection_upgrade`) in vhost WebSocket config; inject vhost files via
`docker cp` + `docker kill --signal=HUP nginx-proxy`. `GestCommercial/Web_T4C_GestCom/deploy/prod/`
is the reference implementation of these conventions in this repo.
