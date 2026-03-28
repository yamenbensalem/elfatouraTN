# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**T4C GestCom** is a Windows Forms desktop application for commercial/business management (gestion commerciale), targeting French-speaking markets. It handles sales, purchasing, inventory, invoicing, and financial management.

- **Language**: C# targeting .NET Framework 4.8
- **Platform**: x86 (32-bit) WinForms executable
- **UI Framework**: Windows Forms
- **Database**: ODBC (database-agnostic; SQL Server supported via System.Data.SqlClient)
- **No ORM**: Raw SQL via ODBC queries

## Build & Run

Open and build via **Visual Studio** (solution file):
```
T4C_Commercial_Project/T4C_Commercial_Project.sln
```

Or with MSBuild from the repo root:
```bash
msbuild T4C_Commercial_Project/T4C_Commercial_Project.sln /p:Configuration=Debug /p:Platform=x86
msbuild T4C_Commercial_Project/T4C_Commercial_Project.sln /p:Configuration=Release /p:Platform=x86
```

Run the built executable:
```
T4C_Commercial_Project/bin/Debug/T4C_Commercial_Project.exe
T4C_Commercial_Project/bin/Release/T4C_Commercial_Project.exe
```

Build configurations: `Debug`, `Release`, `CD_ROM`, `DVD-5`, `SingleImage`

No automated test suite exists in this project.

## Architecture

Three-layer architecture within `T4C_Commercial_Project/`:

```
Forms (UI)  ──►  Entity (CRUD methods)  ──►  DataBaseConnexion  ──►  ODBC / Access DB
                                                     ▲
                                          DataBaseSQLQuery (query strings)
                                          DataBaseTableName (table constants)
```

### Startup Flow (`Program.cs`)

1. `DataBaseConnexion.readConfig()` — reads app.config (connection string, fiscal settings, logo path)
2. `DataBaseConnexion.initializer()` — opens the single static ODBC connection
3. `ParametreDecimales.getFormatDecimaleForPrix/Quantites()` — initializes decimal formats
4. `Application.Run(new Accueil())` — launches main MDI container

Login and multi-enterprise selection exist in `Program.cs` but are **commented out**.

### Global State (`DAL/VariablesGlobales.cs`)

Central static class holding application-wide state. Key fields:
- `DataBaseFileName`, `DataBasePassWord`, `DataBaseFilePath` — connection parameters
- `TimbreValue`, `TauxRetenu` — fiscal configuration loaded from app.config
- `WindowLanguage`, `PrintLanguage` — always `"Français"` in current config
- `FormatDecimalForAllPrice`, `FormatDecimalForAllQuantites` — `"0.##0"` (max 3 decimals)
- `currentPrintDocument` — the active `PrintedDocumentChild` instance shared during printing
- Entity status constants: `EntityActif`, `EntityOuvert`, `EntityFacture`, `EntityLivre`, etc.
- Payment mode constants: `ModePayementEspece`, `ModePayementCheque`, `ModePayementVirement`, etc.

### Database Connection (`DAL/DataBaseConnexion.cs`)

Single **static** ODBC connection opened at startup and reused for the lifetime of the app:

```csharp
// Connection string (Microsoft Access .mdb via ODBC)
"Driver={Microsoft Access Driver (*.mdb)};Dbq=" + DataBaseFilePath + DataBaseFileName + ";Uid=;Pwd=" + DataBasePassWord + ";"
```

Key methods:
- `addOrUpdateElementInDataBase(string request, string errorMessage)` — `ExecuteNonQuery()` wrapper for INSERT/UPDATE
- `deleteElementFromDataBase(string request, string errorMessage)` — DELETE wrapper
- `updateDataGridView(ref BindingSource bs, ref DataGridView dgv, string query, string table)` — fills a DataGridView via `OdbcDataAdapter`
- `updateComboData(string table, ref BindingSource bs)` — fills a ComboBox binding source
- `getNewCodeEntity(string date, string table, string pk)` — generates sequential document numbers in `YYYYMM###` format

All queries use **string concatenation** (no parameterized queries). Escape single quotes with `.Replace("'", "''")` as the existing pattern throughout the code.

### SQL Queries (`DAL/DataBaseSQLQuery.cs`)

Static string fields — one per query. Naming convention: `requete[Entity][Variant]`.
Examples: `requeteClients`, `requeteFactureClient`, `requeteLigneFactureClient(string code)`, `requeteStockProduit`.

Always use these constants rather than writing inline SQL in forms.

### Table Names (`DAL/DataBaseTableName.cs`)

36 static string constants. Always reference these — never hardcode table names.
Key tables: `client`, `fournisseur`, `produit`, `entreprise`, `factureclient`, `bonlivraison`, `lignebonlivraison`, `reglementfactureclient`, `modepayement`, `devise`, `ParametresDecimales`.

### Entity/ — Business Models

38 entity classes. Each entity owns its own CRUD via instance/static methods:
- `ajouter[Entity]()` — INSERT (instance method, builds SQL from `this`)
- `modifier[Entity]()` — UPDATE (instance method)
- `supprimer[Entity](string code)` — DELETE (static or instance)
- `get[Entity](string code)` — SELECT one, returns hydrated object (static factory)
- `getALL[Entity]s()` — SELECT all, returns `ArrayList`

Key entities and their notable fields:
- **`Client`**: `code_client`, `nom_client`, `matriculefiscale_client`, `maxcredit_client`, `exonore_client` (VAT exempt), `code_devise`
- **`Produit`**: `prixventeHT_produit`, `prixventeTTC_produit`, `prixachatTTC_produit`, `tauxmarge_produit`, `quantite_produit` (stock), `fodec_produit`, `remisemaximale_produit`
- **`FactureClient`**: links to `Client` + list of `LigneFactureClient` + `ReglementFactureClient`
- **`BonLivraison`**: linked to `CommandeVente` and/or `FactureClient` via junction tables

Document line-item entities (`Ligne*`) always carry a reference to their parent document code plus product, quantity, unit price, discount, TVA rate.

### Forms/ — Windows Forms UI (157 forms)

`Accueil.cs` is the **MDI parent** container. It lazy-loads all major child forms as public fields (`clientForm`, `factureClientForm`, `devisForm`, etc.) — forms are instantiated on first access and reused.

Naming conventions:
- `AddOrUpdate[Entity].cs` — modal create/edit dialog
- `Select[Entity].cs` — modal picker/lookup (returns a selected entity to caller)
- `[Entity]s.cs` — list/grid management screen
- `Etats*.cs` — reporting and statement screens

Typical form data flow:
```csharp
// 1. Fill combo/grid from DB
DataBaseConnexion.updateComboData(TableClient, ref clientBindingSource);
DataBaseConnexion.updateDataGridView(ref bs, ref dataGridView, query, tableName);

// 2. On select/action: hydrate entity
FactureClient f = FactureClient.getFactureClient(selectedCode);

// 3. Persist via entity method
f.modifierFactureClient();
```

On DB error, many forms call `Environment.Exit(0)` — this is the existing pattern (not a bug).

### Print Subsystem (`DAL/Impression.cs`)

Printing uses static fields on `Impression` to pass data between the caller and the `PrintDocument.PrintPage` event handler:
```csharp
Impression.mClient = selectedClient;
Impression.mTypDocument = DocumentType.Facture;
Impression.mTableaufacturePrint = lineItemsList;
// then trigger PrintDocument.Print()
```

`PrintedDocumentChild.cs` drives the actual `PrintPage` event. `Impression.PrintEntrepriseData()` renders the company header (logo + info) at the top of every document.

`ExcludedForms/` contains deprecated forms — do not use or reference.

## Key Configuration (app.config)

```
TraceLevel        - Logging verbosity (0=off, 1=on)
TimbreFiscal      - Fiscal stamp rate (e.g., 0.6)
TauxRetenue       - Tax withholding rate (e.g., 1.5)
DisplayRemise     - Show/hide discount columns (Yes/No)
DisplayTVA        - Show/hide VAT columns (Yes/No)
PathLogo          - Path to company logo (./logoApp.png)
```

## Domain Language (French)

All UI, entities, and database objects use French terminology:
- **Facture** = Invoice | **Devis** = Quote | **Bon de livraison** = Delivery note
- **Bon de réception** = Receiving note | **Commande** = Order
- **Fournisseur** = Supplier | **Client** = Customer | **Produit** = Product
- **Règlement** = Payment | **Retenue** = Tax withholding | **TVA** = VAT
- **Remise** = Discount | **Devise** = Currency | **Stock** = Inventory

## Important Notes

- Multi-currency support: products carry `code_devise` for pricing in different currencies
- The `Retenu/` subfolder in Forms/ contains tax-withholding specific screens
- License validation logic lives in `DAL/SendingInfosLicence.cs`
- Login and multi-enterprise support exist in `Program.cs` but are currently commented out
- Decimal separator handling is culture-sensitive — use `VariablesGlobales` helpers rather than direct `ToString()` on decimals
