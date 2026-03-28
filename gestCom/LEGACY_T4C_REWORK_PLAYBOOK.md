# Legacy T4C Rework Playbook For gestCom

## 1. Objective

This document is the handoff baseline for reworking gestCom using the legacy desktop codebase at:

- c:\Projects\T4C_Commercial_Project

Use this file at the start of each rework session to avoid repeating reverse engineering.


## 2. Legacy System Snapshot

### 2.1 Runtime and packaging

- Application type: WinForms desktop app (MDI style)
- Framework: .NET Framework 4.8, old csproj format, x86 target
- Startup object: T4C_Commercial_Project.DAL.Program
- NuGet packages: EASendMail, log4net, Newtonsoft.Json, System.Security.Cryptography.Pkcs

### 2.2 Data layer

- Data store: Microsoft Access (Jet) via ODBC
- Connection string pattern: Driver={Microsoft Access Driver (*.mdb)}
- Data file used by default: T4C_Commercial_N.config (binary signature contains Standard Jet DB)
- Data access style: raw SQL string concatenation, static helper methods, mostly no parameterization

### 2.3 Source inventory metrics

- Total .cs files: 204
- Non-designer .cs files: 144
- Designer .cs files: 60
- Heavy areas:
  - Forms: 105
  - Entity: 36
  - DAL: 20
  - Repositories: 12


## 3. Main Architecture Pattern (As Implemented)

### 3.1 Effective layering in practice

The codebase presents 3 overlapping styles:

- Legacy active path (dominant):
  - Forms directly call Entity classes
  - Entity classes issue SQL through DataBaseConnexion static methods
- Utility-driven global state:
  - VariablesGlobales + Program + GlobalMessages + Tools are shared everywhere
- Partial newer repository/model path:
  - Repositories and Common/Models exist but are incomplete and not the dominant execution path

### 3.2 High-coupling anchors

- DataBaseConnexion is called very widely (high static coupling)
- MessageBox.Show appears extensively in forms, entities, and DAL logic
- Business rules, DB mutations, stock movements, and UI concerns are often in same methods


## 4. Core Functional Coverage Found

### 4.1 Sales side

- Clients management
- Products and product parameters (TVA, unite, categorie, fabriquant, etc.)
- Devis client and lines
- Bon de livraison and lines
- Client invoices:
  - Libre invoices
  - BL-based invoices
  - Avoir invoices (isFactureAvoir workflow)
- Client payment regulations
- Avoir settlement workflow (solderAvoir)

### 4.2 Purchase side

- Suppliers management
- Bon de reception and lines
- Supplier invoices and lines
- Supplier payment regulations

### 4.3 Admin and cross-cutting

- Entreprise profile and logo
- Licensing and activation workflow
- Printing engine for documents
- Monetary formatting and decimal parameter forms
- Authentication panel (admin-oriented)

### 4.4 Present but incomplete/partially wired

- CommandeAchat, CommandeVente, DemandePrix entities exist
- Full UI workflow for these appears incomplete/limited compared to BL/facture/devis flows
- Repository layer has multiple NotImplementedException paths and broken SQL methods


## 5. Workflow Map (Legacy Behavior)

### 5.1 Typical sales flow

1. Create/maintain product and client master data
2. Create Devis OR create BL directly
3. Convert BL into Facture client OR create Facture libre
4. For each line operation, stock is adjusted immediately
5. Apply reglements and update invoice status/montant restant
6. Optionally create/solder avoirs

### 5.2 Typical purchase flow

1. Create/maintain supplier and product data
2. Create Bon Reception
3. Convert BR into supplier invoice
4. Apply supplier reglements and update statuses


## 6. Data and State Rules Observed

### 6.1 Numbering strategy

- Many document numbers are date-driven string codes using dd/MM/yyyy decomposition
- Some optional separator-based invoice numbering exists

### 6.2 Stock behavior

- Stock updates happen at document creation/update/delete time
- Quantity rollback/reapply logic is scattered across forms and entities
- This is high-risk during migration if done without transactional boundaries

### 6.3 Status semantics (important to preserve)

- Examples: Ouvert, Comptabilisee, Livre, Non Livre, Facture, Non Facture
- Status combinations drive UI actions and allowed modifications


## 7. Build and Tooling Reality Check

### 7.1 Build attempt result in this environment

- dotnet build on legacy csproj fails with:
  - MSB4803 ResolveComReference not supported in .NET Core MSBuild
- msbuild (full .NET Framework toolchain) not available in current shell path

### 7.2 Implication

- Local analysis is source-based and behavior inferred from code paths
- Full executable validation requires Visual Studio Build Tools / full MSBuild with .NET Framework support


## 8. Risk Register For gestCom Rework

### High risk

- Massive SQL string concatenation (injection and escaping issues)
- No explicit transaction scope for multi-step document + stock mutations
- Business logic split across UI and entities (difficult parity verification)
- Date values stored and filtered as strings (format-sensitive behavior)
- Status logic spread in many files and duplicated

### Medium risk

- Inconsistent naming and table usage across modules (reglement naming variants)
- Partial repository layer may mislead migration if assumed production-ready
- Licensing behavior depends on local machine identifiers and email sending

### Low to medium risk

- Printing templates and formatting are deeply imperative; parity work may be expensive


## 9. Legacy To gestCom Mapping Matrix

### 9.1 Domain mapping

- Client/Fournisseur/Produit master data:
  - Legacy: Entity + Forms direct CRUD
  - gestCom target: Configuration aggregates + repository/unit-of-work + API endpoints

- Devis client:
  - Legacy semantics align most closely with gestCom proforma intent
  - Suggested mapping: Devis -> Facture Proforma aggregate + conversion pipeline

- BonLivraison and FactureClient:
  - Legacy uses tight coupling and stock side effects
  - Suggested mapping: separate command handlers with explicit transaction boundaries

- BonReception and FactureFournisseur:
  - Suggested mapping: Achats bounded workflow with explicit link tables/events

- Reglements:
  - Suggested mapping: dedicated payment aggregate, immutable payment entries, computed balances

- Retenue source:
  - Keep as a separate tax-withholding module, not mixed into invoice mutation methods

### 9.2 Features likely not worth direct porting

- Embedded licensing email internals as currently implemented
- Legacy repository skeleton code with unimplemented methods


## 10. Priority Rework Plan (Execution Order)

### Phase 0: Lock business invariants

- Freeze status vocabulary and allowed transitions
- Freeze document numbering expectations
- Freeze stock mutation rules by workflow (create/update/delete/cancel)

### Phase 1: Read-only parity

- Build query parity endpoints for core documents:
  - Devis/Proforma, BL, Factures client, BR, Factures fournisseur
- Validate totals and computed values against sample legacy data

### Phase 2: Write parity with safety

- Implement create/update/cancel commands with transaction boundaries
- Move all stock updates into deterministic domain/application services
- Add idempotency and guard clauses for duplicate execution

### Phase 3: Payment and settlement

- Implement reglements with auditable entries
- Implement avoir handling and settlement as separate domain flow

### Phase 4: Reporting and print parity

- Rebuild reports from normalized read models
- Keep print templates decoupled from data mutation


## 11. Suggested First Sprint Backlog

1. Create a status transition table (legacy to gestCom)
2. Build a shared number generator service matching required legacy format
3. Implement one complete flow end-to-end:
   - Create BL -> Convert to Facture client -> Apply reglement
4. Add integration tests for stock side effects in that flow
5. Add migration test fixtures from a real legacy DB sample


## 12. Hotspot Files Worth Re-Reading First Next Session

Use these first to quickly restore context:

- DAL\Program.cs
- DAL\DataBaseConnexion.cs
- DAL\DataBaseSQLQuery.cs
- DAL\VariablesGlobales.cs
- Forms\Accueil.cs
- Forms\FacturesClient.cs
- Forms\AddOrUpdateFactureLibreClient.cs
- Forms\AddOrUpdateFactureBLClient.cs
- Forms\BonsLivraison.cs
- Forms\AddOrUpdateBonLivraison.cs
- Entity\FactureClient.cs
- Entity\LigneFactureClient.cs
- Entity\BonLivraison.cs
- Entity\Produit.cs
- Forms\FacturesFournisseur.cs
- Entity\FactureFournisseur.cs
- Entity\BonReception.cs
- Forms\ReglementsFactureClient.cs


## 13. Known Legacy Defects Detected During Walkthrough

- Multiple SQL statements have obvious syntax fragility and quoting issues
- Some entity/repository methods appear unused or incorrect by construction
- Incomplete repository implementations (NotImplementedException)
- Mixed field naming between code and SQL table aliases in some modules
- Existing code updates in-memory fields without guaranteed persistence in certain paths


## 14. Next Session Kickoff Checklist

1. Re-open this playbook
2. Confirm the specific gestCom feature targeted for parity
3. Pick one legacy workflow and define acceptance criteria before coding
4. Implement with tests around status + stock invariants
5. Update this file with what was implemented and what remains


## 15. Recommended Prompt Template For Next Session

Use this starter prompt:

"Continue from LEGACY_T4C_REWORK_PLAYBOOK.md. Focus on [workflow]. Preserve legacy status semantics, numbering, and stock mutation rules. First create/refresh acceptance tests, then implement command/query handlers in gestCom, then report any behavior gaps between legacy and gestCom."
