# GestCom - Commercial Management System

## 🏗️ Architecture Overview

This is a modern, full-stack commercial management application built with **Clean Architecture** principles, implementing a complete ERP solution for Tunisian businesses.

### Technology Stack

#### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server
- **Authentication**: JWT + ASP.NET Core Identity
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI
- **PDF Generation**: QuestPDF
- **Architecture Pattern**: CQRS with MediatR

#### Frontend
- **Framework**: Angular 17+
- **State Management**: NgRx
- **UI Framework**: Angular Material
- **Reactive Programming**: RxJS
- **Build Tool**: Angular CLI

---

## 📁 Solution Structure

```
gestCom/
├── GestCom.sln
├── src/
│   ├── GestCom.Domain/              # Core business entities & interfaces
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── IHasEntreprise.cs    # Multi-tenancy interface
│   │   │   └── IAuditable.cs
│   │   ├── Entities/
│   │   │   ├── Entreprise.cs
│   │   │   ├── Client.cs
│   │   │   ├── Fournisseur.cs
│   │   │   ├── Produit.cs
│   │   │   ├── FactureClient.cs
│   │   │   ├── CommandeVente.cs
│   │   │   └── ... (37 entities total)
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       ├── IUnitOfWork.cs
│   │       └── IRepositories.cs     # Specific repositories
│   │
│   ├── GestCom.Application/         # Business logic & use cases
│   │   ├── Features/
│   │   │   ├── Ventes/             # Sales module
│   │   │   │   ├── Clients/
│   │   │   │   │   ├── Commands/   # CQRS Commands
│   │   │   │   │   ├── Queries/    # CQRS Queries
│   │   │   │   │   ├── DTOs/
│   │   │   │   │   └── Validators/
│   │   │   │   ├── Devis/
│   │   │   │   ├── Commandes/
│   │   │   │   ├── BonsLivraison/
│   │   │   │   ├── Factures/
│   │   │   │   └── Reglements/
│   │   │   │
│   │   │   ├── Achats/             # Purchase module
│   │   │   │   ├── Fournisseurs/
│   │   │   │   ├── DemandesPrix/
│   │   │   │   ├── CommandesAchat/
│   │   │   │   ├── BonsReception/
│   │   │   │   ├── FacturesFournisseur/
│   │   │   │   └── ReglementsFournisseur/
│   │   │   │
│   │   │   ├── Stock/              # Inventory module
│   │   │   │   ├── Produits/
│   │   │   │   ├── Categories/
│   │   │   │   ├── Magasins/
│   │   │   │   └── MouvementsStock/
│   │   │   │
│   │   │   ├── Configuration/      # Configuration module
│   │   │   │   ├── Entreprises/
│   │   │   │   ├── Devises/
│   │   │   │   ├── TVA/
│   │   │   │   └── ModesPayement/
│   │   │   │
│   │   │   └── Reporting/          # Reports & Analytics
│   │   │       ├── Ventes/
│   │   │       ├── Achats/
│   │   │       └── Stock/
│   │   │
│   │   ├── Common/
│   │   │   ├── Behaviors/          # MediatR pipelines
│   │   │   ├── Mappings/           # AutoMapper profiles
│   │   │   └── Services/           # Application services
│   │   │
│   │   └── Interfaces/
│   │       ├── IPdfService.cs
│   │       ├── IEmailService.cs
│   │       └── ICurrentUserService.cs
│   │
│   ├── GestCom.Infrastructure/      # Data access & external services
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/     # EF Core configurations
│   │   │   │   ├── EntrepriseConfiguration.cs
│   │   │   │   ├── ClientConfiguration.cs
│   │   │   │   ├── ProduitConfiguration.cs
│   │   │   │   └── ... (for all entities)
│   │   │   └── Migrations/         # EF Core migrations
│   │   │
│   │   ├── Repositories/
│   │   │   ├── Repository.cs       # Generic repository
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── MainRepositories.cs
│   │   │   └── SpecificRepositories.cs
│   │   │
│   │   ├── Identity/
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── JwtService.cs
│   │   │   └── IdentityService.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── PdfService.cs       # QuestPDF implementation
│   │   │   ├── EmailService.cs
│   │   │   └── CurrentUserService.cs
│   │   │
│   │   └── DependencyInjection.cs  # Service registration
│   │
│   ├── GestCom.WebAPI/              # API Layer
│   │   ├── Controllers/
│   │   │   ├── Ventes/
│   │   │   │   ├── ClientsController.cs
│   │   │   │   ├── DevisController.cs
│   │   │   │   ├── CommandesVenteController.cs
│   │   │   │   ├── BonsLivraisonController.cs
│   │   │   │   ├── FacturesClientController.cs
│   │   │   │   └── ReglementsController.cs
│   │   │   │
│   │   │   ├── Achats/
│   │   │   │   ├── FournisseursController.cs
│   │   │   │   ├── CommandesAchatController.cs
│   │   │   │   └── FacturesFournisseurController.cs
│   │   │   │
│   │   │   ├── Stock/
│   │   │   │   ├── ProduitsController.cs
│   │   │   │   ├── CategoriesController.cs
│   │   │   │   └── StockController.cs
│   │   │   │
│   │   │   ├── Configuration/
│   │   │   │   ├── EntreprisesController.cs
│   │   │   │   └── ParametresController.cs
│   │   │   │
│   │   │   ├── Reporting/
│   │   │   │   └── ReportsController.cs
│   │   │   │
│   │   │   └── AuthController.cs   # Authentication
│   │   │
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── TenantMiddleware.cs # Multi-tenancy
│   │   │   └── RequestLoggingMiddleware.cs
│   │   │
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   │
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   └── GestCom.Shared/              # Cross-cutting concerns
│       ├── Common/
│       │   ├── Result.cs           # Operation result pattern
│       │   └── PagedResult.cs      # Pagination
│       ├── Constants/
│       │   └── AppConstants.cs
│       ├── Exceptions/
│       │   ├── BusinessException.cs
│       │   ├── NotFoundException.cs
│       │   └── ValidationException.cs
│       └── Extensions/
│           └── StringExtensions.cs
│
├── frontend/                         # Angular Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/               # Singleton services
│   │   │   │   ├── auth/
│   │   │   │   ├── interceptors/
│   │   │   │   └── services/
│   │   │   │
│   │   │   ├── shared/             # Shared components
│   │   │   │   ├── components/
│   │   │   │   ├── directives/
│   │   │   │   └── pipes/
│   │   │   │
│   │   │   ├── features/           # Feature modules
│   │   │   │   ├── ventes/
│   │   │   │   │   ├── clients/
│   │   │   │   │   ├── devis/
│   │   │   │   │   ├── commandes/
│   │   │   │   │   ├── factures/
│   │   │   │   │   └── store/     # NgRx state
│   │   │   │   │
│   │   │   │   ├── achats/
│   │   │   │   ├── stock/
│   │   │   │   ├── config/
│   │   │   │   └── reporting/
│   │   │   │
│   │   │   ├── layout/
│   │   │   └── app.routes.ts
│   │   │
│   │   ├── environments/
│   │   └── assets/
│   │
│   ├── angular.json
│   ├── package.json
│   └── tsconfig.json
│
└── tests/
    ├── GestCom.Application.Tests/
    ├── GestCom.Infrastructure.Tests/
    └── GestCom.WebAPI.Tests/
```

---

## 🔑 Key Features Implemented

### 1. **Multi-Tenancy Support**
- `IHasEntreprise` interface for entity-level filtering
- Global query filters in EF Core
- Automatic `CodeEntreprise` injection in SaveChanges
- Tenant middleware for request-level context

### 2. **Clean Domain Models**
- ✅ 37 refactored entities (removed all database logic)
- ✅ Navigation properties for relationships
- ✅ Proper encapsulation with properties (not public fields)
- ✅ French naming preserved for database compatibility

### 3. **Repository Pattern & Unit of Work**
- ✅ Generic repository with common CRUD operations
- ✅ Specific repositories with business-specific queries
- ✅ Unit of Work for transaction management
- ✅ Async/await throughout

### 4. **Infrastructure Layer**
- ✅ EF Core DbContext with Identity integration
- ✅ Fluent API configurations (type mapping, relationships)
- ✅ Repository implementations
- ✅ Multi-tenancy global filters
- ✅ Audit trail (auto-set creation/modification dates)

---

## 📊 Business Modules

### Module 1: Gestion des Ventes (Sales Management)
**Entities**: Client, DevisClient, CommandeVente, BonLivraison, FactureClient, ReglementFacture

**Workflow**:
```
Client → Devis (Quote) → Commande (Order) → Bon Livraison (Delivery) → Facture (Invoice) → Règlement (Payment)
```

**Features**:
- Client management with credit limits
- Quote generation and tracking
- Sales order processing
- Delivery note generation
- Invoicing with RAS (tax withholding)
- Payment tracking
- Customer statements

### Module 2: Gestion des Achats (Purchase Management)
**Entities**: Fournisseur, DemandePrix, CommandeAchat, BonReception, FactureFournisseur, ReglementFournisseur

**Workflow**:
```
Fournisseur → Demande Prix (RFQ) → Commande Achat (PO) → Bon Réception (GRN) → Facture → Règlement
```

**Features**:
- Supplier management
- RFQ (Request for Quotation)
- Purchase order creation
- Goods receipt
- Supplier invoicing
- Payment to suppliers
- Supplier ledger

### Module 3: Gestion de Stock (Inventory Management)
**Entities**: Produit, CategorieProduit, UniteProduit, MagasinProduit

**Features**:
- Product catalog
- Category hierarchy
- Multi-warehouse support
- Stock tracking
- Low stock alerts
- Price management (purchase/sale, margins)
- FODEC & TVA calculations

### Module 4: Paramétrage (Configuration)
**Entities**: Entreprise, Devise, TvaProduit, ModePayement, RetenuSource

**Features**:
- Company profile
- Multi-currency support
- Tax rates configuration
- Payment methods
- User & role management

### Module 5: Reporting & Analytics
**Features** (To be implemented):
- Sales reports & statistics
- Purchase reports
- Stock valuation
- Revenue analysis (Chiffre d'affaires)
- Customer receivables (Créances)
- Supplier payables (Dettes)

---

## 🚀 Next Steps for Implementation

### Completed ✅
1. ✅ Solution structure with 6 projects
2. ✅ Domain layer with 37 refactored entities
3. ✅ Shared layer with common utilities
4. ✅ Infrastructure DbContext & configurations
5. ✅ Repository pattern & Unit of Work
6. ✅ All repository implementations

### Remaining Tasks 🔨

#### 1. Application Layer (CQRS/MediatR)
Create for each entity (e.g., Client):

**Commands**:
```csharp
Features/Ventes/Clients/Commands/
├── CreateClient/
│   ├── CreateClientCommand.cs
│   ├── CreateClientCommandHandler.cs
│   └── CreateClientCommandValidator.cs
├── UpdateClient/
├── DeleteClient/
└── DTOs/
    ├── ClientDto.cs
    └── CreateClientDto.cs
```

**Queries**:
```csharp
Features/Ventes/Clients/Queries/
├── GetClientById/
│   ├── GetClientByIdQuery.cs
│   └── GetClientByIdQueryHandler.cs
├── GetAllClients/
├── GetClientsByEntreprise/
└── SearchClients/
```

**AutoMapper Profiles**:
```csharp
Common/Mappings/
└── ClientMappingProfile.cs
```

#### 2. WebAPI Controllers
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClientDto>>> GetAll([FromQuery] GetAllClientsQuery query)
    
    [HttpGet("{code}")]
    public async Task<ActionResult<ClientDto>> GetById(string code)
    
    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientCommand command)
    
    [HttpPut("{code}")]
    public async Task<ActionResult> Update(string code, [FromBody] UpdateClientCommand command)
    
    [HttpDelete("{code}")]
    public async Task<ActionResult> Delete(string code)
}
```

#### 3. WebAPI Configuration (Program.cs)
```csharp
// Add services
builder.Services.AddApplicationServices();      // MediatR, AutoMapper, FluentValidation
builder.Services.AddInfrastructureServices(      // EF Core, Repositories, Identity
    builder.Configuration);
builder.Services.AddWebAPIServices();             // Swagger, CORS, JWT

// Configure pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
```

#### 4. Database Migration
```bash
# Create initial migration
cd src/GestCom.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../GestCom.WebAPI

# Update database
dotnet ef database update --startup-project ../GestCom.WebAPI
```

#### 5. Angular Frontend
```bash
# Create Angular workspace
ng new gestcom-frontend
cd gestcom-frontend

# Install dependencies
npm install @ngrx/store @ngrx/effects @ngrx/entity @ngrx/store-devtools
npm install @angular/material @angular/cdk
npm install rxjs

# Generate modules
ng g module features/ventes --routing
ng g module features/achats --routing
ng g module features/stock --routing
ng g module core
ng g module shared

# Generate services
ng g service core/services/api
ng g service core/auth/auth
ng g service core/services/client

# Generate components
ng g c features/ventes/clients/client-list
ng g c features/ventes/clients/client-form
ng g c features/ventes/clients/client-detail
```

#### 6. JWT Authentication Setup
- ApplicationUser model ✅
- JwtService implementation needed
- Login/Register endpoints
- Token refresh mechanism
- Role-based authorization

#### 7. PDF Generation (QuestPDF)
```csharp
public interface IPdfService
{
    byte[] GenerateFacturePdf(FactureClient facture);
    byte[] GenerateDevisPdf(DevisClient devis);
    byte[] GenerateBonLivraisonPdf(BonLivraison bon);
}
```

#### 8. Reporting Services
- Sales statistics
- Purchase statistics
- Stock reports
- Financial reports
- Dashboard KPIs

---

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=GestComDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyForJWTTokenGeneration",
    "Issuer": "GestComAPI",
    "Audience": "GestComClients",
    "ExpiryMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "Logs/log-.txt", "rollingInterval": "Day" }
      }
    ]
  }
}
```

---

## 📦 NuGet Packages Reference

### GestCom.Domain
- No external dependencies (Pure domain)

### GestCom.Application
- `MediatR` (12.2.0)
- `AutoMapper` (13.0.1)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (13.0.1)
- `FluentValidation` (11.9.0)
- `FluentValidation.DependencyInjectionExtensions` (11.9.0)

### GestCom.Infrastructure
- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.0)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.0)
- `QuestPDF` (2024.1.3)

### GestCom.WebAPI
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0)
- `Swashbuckle.AspNetCore` (6.5.0)
- `Serilog.AspNetCore` (8.0.1)
- `Serilog.Sinks.Console` (5.0.1)
- `Serilog.Sinks.File` (5.0.0)

---

## 🗂️ Database Schema Notes

### Multi-Tenancy Strategy
- **Single Database** with `CodeEntreprise` column
- All tenant-specific entities implement `IHasEntreprise`
- Global query filters automatically filter by tenant
- Row-level security at application level

### Naming Convention
- **Database columns**: French (e.g., `matriculefiscale_client`)
- **C# Properties**: PascalCase French (e.g., `MatriculeFiscale`)
- **Maintained compatibility** with existing schema

### Key Relationships
- **Client → FactureClient** (One-to-Many)
- **FactureClient → LigneFactureClient** (One-to-Many, Cascade Delete)
- **Produit → Multiple Lines** (One-to-Many, Restrict Delete)
- **BonLivraison ↔ FactureClient** (Many-to-Many via BonLivraison_Facture)

---

## 🎯 API Endpoint Examples

```
POST   /api/v1/auth/login
POST   /api/v1/auth/register

GET    /api/v1/clients?pageNumber=1&pageSize=20
GET    /api/v1/clients/{code}
POST   /api/v1/clients
PUT    /api/v1/clients/{code}
DELETE /api/v1/clients/{code}
GET    /api/v1/clients/{code}/factures
GET    /api/v1/clients/{code}/stats

GET    /api/v1/produits?search=laptop
GET    /api/v1/produits/stock-faible
POST   /api/v1/produits

GET    /api/v1/factures/clients
POST   /api/v1/factures/clients
GET    /api/v1/factures/clients/{numero}/pdf
POST   /api/v1/factures/clients/{numero}/email

GET    /api/v1/reports/ventes/chiffre-affaires?dateDebut=2024-01-01&dateFin=2024-12-31
GET    /api/v1/reports/stock/valuation
```

---

## 👥 Contributors

**Architecture Design**: Clean Architecture / Onion Architecture  
**Database Strategy**: Multi-tenant single database  
**Naming Convention**: French (database compatibility)  
**PDF Library**: QuestPDF

---

## 📝 License

This project is part of elfatouraTN commercial management system.

---

## 🔗 Additional Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Angular Documentation](https://angular.io/docs)
- [NgRx Documentation](https://ngrx.io/)
- [QuestPDF Documentation](https://www.questpdf.com/)

---

**Status**: 🟢 Infrastructure & Domain layers complete | 🟡 Application & API layers in progress
