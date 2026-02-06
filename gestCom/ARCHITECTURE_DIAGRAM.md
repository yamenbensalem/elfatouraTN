# GestCom Architecture Diagram

## 🏗️ Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────────────┐
│                         PRESENTATION LAYER                           │
│  ┌───────────────────────┐      ┌──────────────────────────────┐   │
│  │   Angular Frontend    │      │      ASP.NET Core WebAPI     │   │
│  │                       │◄────►│                               │   │
│  │  - Components         │ HTTP │  - Controllers                │   │
│  │  - Services           │ REST │  - Middleware                 │   │
│  │  - NgRx Store         │ JSON │  - Filters                    │   │
│  │  - Angular Material   │      │  - Swagger/OpenAPI            │   │
│  └───────────────────────┘      └──────────────┬────────────────┘   │
└─────────────────────────────────────────────────┼────────────────────┘
                                                  │
                                                  │ IMediator
                                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                             │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    MediatR CQRS Handlers                      │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────┐ │   │
│  │  │  Commands   │  │   Queries   │  │   DTOs & Mappings    │ │   │
│  │  │             │  │             │  │                      │ │   │
│  │  │ - Create    │  │ - GetAll    │  │  - ClientDto        │ │   │
│  │  │ - Update    │  │ - GetById   │  │  - ProduitDto       │ │   │
│  │  │ - Delete    │  │ - Search    │  │  - FactureDto       │ │   │
│  │  └─────────────┘  └─────────────┘  │  - AutoMapper        │ │   │
│  │                                     │  - FluentValidation  │ │   │
│  │                                     └──────────────────────┘ │   │
│  │                                                               │   │
│  │  Business Modules:                                           │   │
│  │  - Ventes (Sales)  - Achats (Purchases)  - Stock (Inventory)│   │
│  │  - Configuration   - Reporting                               │   │
│  └──────────────────────────┬────────────────────────────────────┘   │
└─────────────────────────────┼──────────────────────────────────────┘
                              │
                              │ IUnitOfWork / IRepository<T>
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       INFRASTRUCTURE LAYER                           │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Data Access (EF Core)                      │   │
│  │  ┌───────────────────┐      ┌──────────────────────────────┐ │   │
│  │  │  DbContext        │      │    Repository Pattern         │ │   │
│  │  │                   │      │                              │ │   │
│  │  │ - DbSet<Client>   │◄────►│  - Repository<T>            │ │   │
│  │  │ - DbSet<Produit>  │      │  - ClientRepository         │ │   │
│  │  │ - DbSet<Facture>  │      │  - ProduitRepository        │ │   │
│  │  │ - Identity        │      │  - FactureRepository        │ │   │
│  │  │ - Audit Trail     │      │  - UnitOfWork               │ │   │
│  │  │ - Multi-Tenancy   │      │  - Transaction Management   │ │   │
│  │  └───────────────────┘      └──────────────────────────────┘ │   │
│  │                                                               │   │
│  │  External Services:                                          │   │
│  │  - QuestPDF (PDF Generation)  - Email Service  - JWT Service│   │
│  └──────────────────────────┬────────────────────────────────────┘   │
└─────────────────────────────┼──────────────────────────────────────┘
                              │
                              │ SQL Queries
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          DATABASE LAYER                              │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    SQL Server Database                        │   │
│  │                                                               │   │
│  │  Tables:                                                      │   │
│  │  ┌────────────┐  ┌────────────┐  ┌──────────────────────┐   │   │
│  │  │  Client    │  │  Produit   │  │  FactureClient       │   │   │
│  │  │  (Clients) │  │  (Products)│  │  (Invoices)          │   │   │
│  │  └────────────┘  └────────────┘  └──────────────────────┘   │   │
│  │                                                               │   │
│  │  ┌────────────┐  ┌────────────┐  ┌──────────────────────┐   │   │
│  │  │ Fournisseur│  │ Entreprise │  │  CommandeVente       │   │   │
│  │  │ (Suppliers)│  │ (Company)  │  │  (Orders)            │   │   │
│  │  └────────────┘  └────────────┘  └──────────────────────┘   │   │
│  │                                                               │   │
│  │  + 30 more tables for complete commercial management         │   │
│  └───────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                          DOMAIN LAYER (Core)                         │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Domain Entities (Pure)                     │   │
│  │  - Client         - Produit        - FactureClient            │   │
│  │  - Fournisseur    - CommandeVente  - BonLivraison            │   │
│  │  - Entreprise     - Devise         - 30 more entities...      │   │
│  │                                                               │   │
│  │                    Domain Interfaces                          │   │
│  │  - IRepository<T>       - IClientRepository                  │   │
│  │  - IUnitOfWork          - IProduitRepository                 │   │
│  │  - IHasEntreprise       - 15 more specific interfaces...     │   │
│  └───────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                       SHARED / COMMON LAYER                          │
│  - Result<T>        - PagedResult<T>    - Exceptions                 │
│  - Constants        - Extensions         - Utilities                 │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Data Flow Example: Create Invoice

```
1. USER ACTION (Angular)
   │
   ├─► Component calls service.createFacture(data)
   │
   └─► HTTP POST /api/v1/factures/clients
       │
       ▼

2. WEB API LAYER
   │
   ├─► FacturesClientController.Create(command)
   │   - Authorize user
   │   - Validate request
   │   - Extract tenant (CodeEntreprise)
   │
   └─► IMediator.Send(CreateFactureClientCommand)
       │
       ▼

3. APPLICATION LAYER
   │
   ├─► CreateFactureClientCommandValidator
   │   - Validate client exists
   │   - Validate products
   │   - Validate amounts
   │
   ├─► CreateFactureClientCommandHandler
   │   - Map DTO to Entity
   │   - Calculate totals (HT, TVA, TTC, RAS)
   │   - Set defaults (status, date, numero)
   │   - Call IUnitOfWork
   │
   └─► IUnitOfWork.FacturesClient.AddAsync(facture)
       │
       ▼

4. INFRASTRUCTURE LAYER
   │
   ├─► FactureClientRepository.AddAsync(facture)
   │   - DbContext.Factures.Add(facture)
   │   - Auto-set CodeEntreprise (multi-tenancy)
   │   - Auto-set DateCreation (audit)
   │
   └─► UnitOfWork.SaveChangesAsync()
       │
       ▼

5. DATABASE LAYER
   │
   └─► SQL Server
       - INSERT INTO FactureClient (...)
       - INSERT INTO LigneFactureClient (...)
       - Transaction committed
       │
       ▼

6. RESPONSE
   │
   ├─► Return FactureClientDto
   │   - Map entity to DTO
   │   - Return to API controller
   │
   └─► HTTP 201 Created
       - Location header: /api/v1/factures/clients/{numero}
       - Body: FactureClientDto (JSON)
       │
       ▼

7. ANGULAR
   │
   └─► Component receives response
       - Update NgRx store
       - Navigate to invoice detail
       - Show success notification
```

---

## 🗂️ Project Structure Visual

```
gestCom/
│
├─── src/
│    ├─── GestCom.Domain/              (No dependencies)
│    │    │
│    │    ├─── Common/
│    │    │    ├─── BaseEntity.cs
│    │    │    ├─── IHasEntreprise.cs
│    │    │    └─── IAuditable.cs
│    │    │
│    │    ├─── Entities/               (37 entities)
│    │    │    ├─── Client.cs
│    │    │    ├─── Produit.cs
│    │    │    ├─── FactureClient.cs
│    │    │    └─── ...
│    │    │
│    │    └─── Interfaces/
│    │         ├─── IRepository.cs
│    │         ├─── IUnitOfWork.cs
│    │         └─── IRepositories.cs
│    │
│    ├─── GestCom.Application/         (Depends on: Domain)
│    │    │
│    │    ├─── Features/
│    │    │    ├─── Ventes/
│    │    │    │    ├─── Clients/
│    │    │    │    │    ├─── Commands/
│    │    │    │    │    │    └─── CreateClient/
│    │    │    │    │    │         ├─── CreateClientCommand.cs
│    │    │    │    │    │         ├─── CreateClientCommandHandler.cs
│    │    │    │    │    │         └─── CreateClientCommandValidator.cs
│    │    │    │    │    ├─── Queries/
│    │    │    │    │    │    ├─── GetAllClients/
│    │    │    │    │    │    └─── GetClientById/
│    │    │    │    │    ├─── DTOs/
│    │    │    │    │    └─── Mappings/
│    │    │    │    │
│    │    │    │    ├─── Devis/
│    │    │    │    ├─── Commandes/
│    │    │    │    ├─── Factures/
│    │    │    │    └─── ...
│    │    │    │
│    │    │    ├─── Achats/
│    │    │    ├─── Stock/
│    │    │    ├─── Configuration/
│    │    │    └─── Reporting/
│    │    │
│    │    └─── Common/
│    │         ├─── Behaviors/
│    │         ├─── Mappings/
│    │         └─── Services/
│    │
│    ├─── GestCom.Infrastructure/     (Depends on: Application, Domain)
│    │    │
│    │    ├─── Data/
│    │    │    ├─── ApplicationDbContext.cs
│    │    │    ├─── Configurations/   (EF Core Fluent API)
│    │    │    │    ├─── ClientConfiguration.cs
│    │    │    │    ├─── ProduitConfiguration.cs
│    │    │    │    └─── ...
│    │    │    └─── Migrations/
│    │    │
│    │    ├─── Repositories/
│    │    │    ├─── Repository.cs
│    │    │    ├─── UnitOfWork.cs
│    │    │    ├─── MainRepositories.cs
│    │    │    └─── SpecificRepositories.cs
│    │    │
│    │    ├─── Identity/
│    │    │    ├─── ApplicationUser.cs
│    │    │    ├─── JwtService.cs
│    │    │    └─── IdentityService.cs
│    │    │
│    │    └─── Services/
│    │         ├─── PdfService.cs
│    │         ├─── EmailService.cs
│    │         └─── CurrentUserService.cs
│    │
│    ├─── GestCom.WebAPI/             (Depends on: Infrastructure, Application)
│    │    │
│    │    ├─── Controllers/
│    │    │    ├─── Ventes/
│    │    │    │    ├─── ClientsController.cs
│    │    │    │    ├─── FacturesClientController.cs
│    │    │    │    └─── ...
│    │    │    ├─── Achats/
│    │    │    ├─── Stock/
│    │    │    └─── Configuration/
│    │    │
│    │    ├─── Middleware/
│    │    │    ├─── ExceptionHandlingMiddleware.cs
│    │    │    ├─── TenantMiddleware.cs
│    │    │    └─── RequestLoggingMiddleware.cs
│    │    │
│    │    ├─── Program.cs
│    │    └─── appsettings.json
│    │
│    └─── GestCom.Shared/             (No dependencies)
│         │
│         ├─── Common/
│         │    ├─── Result.cs
│         │    └─── PagedResult.cs
│         │
│         ├─── Constants/
│         │    └─── AppConstants.cs
│         │
│         ├─── Exceptions/
│         │    ├─── BusinessException.cs
│         │    ├─── NotFoundException.cs
│         │    └─── ValidationException.cs
│         │
│         └─── Extensions/
│              └─── StringExtensions.cs
│
├─── frontend/                         (Angular 17 Application)
│    └─── src/app/
│         ├─── core/
│         │    ├─── auth/
│         │    ├─── interceptors/
│         │    └─── services/
│         │
│         ├─── shared/
│         │    ├─── components/
│         │    ├─── directives/
│         │    └─── pipes/
│         │
│         └─── features/
│              ├─── ventes/
│              ├─── achats/
│              ├─── stock/
│              ├─── config/
│              └─── reporting/
│
├─── tests/
│    ├─── GestCom.Application.Tests/
│    ├─── GestCom.Infrastructure.Tests/
│    └─── GestCom.WebAPI.Tests/
│
├─── GestCom.sln
├─── README.md
├─── QUICKSTART.md
└─── IMPLEMENTATION_SUMMARY.md
```

---

## 🔐 Security Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    SECURITY LAYERS                            │
└──────────────────────────────────────────────────────────────┘

1. AUTHENTICATION
   │
   ├─► JWT Token (Bearer)
   │   - Username + Password → Token
   │   - Token expiration (60 min)
   │   - Refresh token mechanism
   │
   └─► ASP.NET Core Identity
       - Password hashing
       - User management
       - Role management

2. AUTHORIZATION
   │
   ├─► Role-Based (RBAC)
   │   - Admin: Full access
   │   - Manager: Read/Write
   │   - User: Read only
   │
   └─► Policy-Based
       - RequireAdminRole
       - RequireManagerRole

3. MULTI-TENANCY
   │
   ├─► TenantMiddleware
   │   - Extract CodeEntreprise from JWT claims
   │   - Set in current context
   │
   └─► Global Query Filters
       - Automatic WHERE CodeEntreprise = @currentTenant
       - Row-level security

4. DATA PROTECTION
   │
   ├─► Input Validation
   │   - FluentValidation rules
   │   - Request validation filter
   │
   ├─► SQL Injection Prevention
   │   - EF Core parameterized queries
   │   - No raw SQL (except for reports)
   │
   └─► XSS Protection
       - Angular sanitization
       - Content Security Policy

5. API SECURITY
   │
   ├─► HTTPS Enforcement
   │   - Redirect HTTP → HTTPS
   │   - HSTS headers
   │
   ├─► CORS Configuration
   │   - Allowed origins
   │   - Allowed methods
   │
   └─── Rate Limiting
       - Per user/IP
       - DDoS protection
```

---

## 📊 Performance Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                PERFORMANCE OPTIMIZATION                       │
└──────────────────────────────────────────────────────────────┘

1. DATABASE OPTIMIZATION
   │
   ├─► Indexing Strategy
   │   - Primary keys
   │   - Foreign keys
   │   - Frequently queried columns (CodeClient, NumeroFacture)
   │
   ├─► Query Optimization
   │   - .Include() for eager loading
   │   - Pagination (avoid SELECT *)
   │   - Compiled queries for hot paths
   │
   └─► Connection Pooling
       - EF Core default pooling
       - Async/await throughout

2. APPLICATION CACHING
   │
   ├─► Memory Cache
   │   - Reference data (Devises, TVA, Categories)
   │   - Short-lived (5-15 min)
   │
   └─► Distributed Cache (Redis)
       - User sessions
       - Frequently accessed reports

3. API OPTIMIZATION
   │
   ├─► Response Compression
   │   - Gzip/Brotli
   │   - Automatic for JSON
   │
   ├─► Response Caching
   │   - GET endpoints
   │   - Cache-Control headers
   │
   └─► Async/Await
       - Non-blocking I/O
       - Better scalability

4. FRONTEND OPTIMIZATION
   │
   ├─► Lazy Loading
   │   - Feature modules loaded on demand
   │   - Reduces initial bundle size
   │
   ├─► Change Detection
   │   - OnPush strategy
   │   - Reduces unnecessary renders
   │
   └─► Virtual Scrolling
       - Large lists (products, clients)
       - Only render visible items
```

---

**This architecture diagram provides a comprehensive visual representation of the GestCom application structure and data flow.**
