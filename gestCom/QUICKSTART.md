# GestCom - Quick Start Guide

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+ or SQL Server Express
- Node.js 18+ and npm (for Angular frontend)
- Visual Studio 2022 or VS Code
- Git

---

## 📋 Step-by-Step Setup

### 1. Clone & Build Backend

```powershell
# Navigate to project directory
cd c:\Projects\elfatouraTN\elfatouraTN\gestCom

# Restore NuGet packages
dotnet restore GestCom.sln

# Build solution
dotnet build GestCom.sln

# Verify build
dotnet build --configuration Release
```

### 2. Database Setup

```powershell
# Update connection string in appsettings.json
# File: src/GestCom.WebAPI/appsettings.json

# Run migrations
cd src/GestCom.WebAPI
dotnet ef database update --project ../GestCom.Infrastructure

# Or create migration if needed
dotnet ef migrations add InitialCreate --project ../GestCom.Infrastructure --startup-project .
```

### 3. Run the API

```powershell
cd src/GestCom.WebAPI
dotnet run

# API will be available at:
# https://localhost:7001
# http://localhost:5001
# Swagger UI: https://localhost:7001/swagger
```

### 4. Setup Angular Frontend

```powershell
# Create Angular workspace
cd c:\Projects\elfatouraTN\elfatouraTN\gestCom
ng new frontend --routing --style=scss

cd frontend

# Install dependencies
npm install @ngrx/store @ngrx/effects @ngrx/entity @ngrx/store-devtools
npm install @angular/material @angular/cdk @angular/animations
npm install rxjs

# Run development server
ng serve

# Frontend will be available at:
# http://localhost:4200
```

---

## 🔧 Next Implementation Steps

### Priority 1: Complete Application Layer (Week 1-2)

#### A. Create MediatR Handlers for Clients Module

1. **Create Command: CreateClient**

```powershell
# Create folder structure
mkdir -p src/GestCom.Application/Features/Ventes/Clients/Commands/CreateClient
mkdir -p src/GestCom.Application/Features/Ventes/Clients/Queries/GetAllClients
mkdir -p src/GestCom.Application/Features/Ventes/Clients/DTOs
```

Files to create:
- `CreateClientCommand.cs`
- `CreateClientCommandHandler.cs`
- `CreateClientCommandValidator.cs`
- `ClientDto.cs`
- `ClientMappingProfile.cs`

2. **Repeat for other entities**:
- Produits (Products)
- FacturesClient (Customer Invoices)
- CommandesVente (Sales Orders)

#### B. Setup Dependency Injection

File: `src/GestCom.Application/DependencyInjection.cs`

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
```

File: `src/GestCom.Infrastructure/DependencyInjection.cs`

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // JWT
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => { /* JWT config */ });

        return services;
    }
}
```

### Priority 2: WebAPI Controllers (Week 2-3)

Create controllers for each module following RESTful principles.

**Template Structure**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] GetAllClientsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetById(string code)
    {
        var query = new GetClientByIdQuery { CodeClient = code };
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] CreateClientCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { code = result.CodeClient }, result);
    }

    [HttpPut("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(string code, [FromBody] UpdateClientCommand command)
    {
        if (code != command.CodeClient)
            return BadRequest();

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string code)
    {
        var command = new DeleteClientCommand { CodeClient = code };
        await _mediator.Send(command);
        return NoContent();
    }
}
```

### Priority 3: Middleware & Global Error Handling (Week 3)

1. **ExceptionHandlingMiddleware.cs**
2. **TenantMiddleware.cs** (extract CodeEntreprise from claims)
3. **RequestLoggingMiddleware.cs**

### Priority 4: Authentication & Authorization (Week 3-4)

1. Create `AuthController` with Login/Register endpoints
2. Implement JWT token generation
3. Setup role-based authorization
4. Create user management endpoints

### Priority 5: Angular Frontend Development (Week 4-6)

1. **Setup project structure**
   - Core module (singleton services)
   - Shared module (components, directives, pipes)
   - Feature modules (lazy-loaded)

2. **Implement authentication**
   - Login/Register components
   - JWT interceptor
   - Auth guard

3. **Create CRUD interfaces for each module**
   - List view (with pagination, search, filter)
   - Create/Edit form
   - Detail view
   - Delete confirmation

4. **Implement NgRx state management**
   - Actions
   - Reducers
   - Effects
   - Selectors

### Priority 6: PDF Generation (Week 5)

Implement QuestPDF for document generation:
- Facture Client PDF
- Devis PDF
- Bon de Livraison PDF
- Purchase Order PDF

### Priority 7: Reporting Module (Week 6-7)

Create reporting endpoints and dashboards:
- Sales reports
- Purchase reports
- Stock reports
- Financial KPIs
- Dashboard charts

---

## 📊 Development Checklist

### Backend Development
- [ ] Complete Application layer (CQRS handlers)
- [ ] Create all API controllers
- [ ] Implement middleware
- [ ] Setup JWT authentication
- [ ] Create database migrations
- [ ] Seed initial data
- [ ] Implement PDF generation
- [ ] Create reporting services
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] API documentation (Swagger)

### Frontend Development
- [ ] Setup Angular project structure
- [ ] Configure NgRx store
- [ ] Implement authentication
- [ ] Create shared components
- [ ] Ventes module (Clients, Devis, Commandes, Factures)
- [ ] Achats module (Fournisseurs, Commandes Achat, Factures)
- [ ] Stock module (Produits, Categories, Stock management)
- [ ] Configuration module (Entreprise, Paramètres)
- [ ] Reporting module (Dashboards, Reports)
- [ ] Responsive design
- [ ] Form validation
- [ ] Error handling
- [ ] Loading states
- [ ] Unit tests
- [ ] E2E tests

### DevOps & Deployment
- [ ] Setup CI/CD pipeline
- [ ] Docker containerization
- [ ] Database backup strategy
- [ ] Monitoring & logging
- [ ] Performance optimization
- [ ] Security audit
- [ ] Documentation

---

## 🧪 Testing Strategy

### Unit Tests
```powershell
# Run unit tests
dotnet test src/GestCom.Application.Tests
dotnet test src/GestCom.Infrastructure.Tests

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Integration Tests
```powershell
dotnet test src/GestCom.WebAPI.Tests
```

### Frontend Tests
```powershell
cd frontend
npm run test           # Unit tests
npm run test:coverage  # With coverage
npm run e2e           # E2E tests
```

---

## 📈 Performance Optimization

### Database
- Index frequently queried columns
- Use pagination for large datasets
- Implement caching (Redis)
- Optimize N+1 queries with `.Include()`

### API
- Enable response compression
- Implement API versioning
- Use async/await throughout
- Add rate limiting

### Frontend
- Lazy load modules
- Implement virtual scrolling for large lists
- Use OnPush change detection
- Optimize bundle size

---

## 🔐 Security Checklist

- [ ] JWT token expiration
- [ ] Refresh token mechanism
- [ ] Password hashing (Identity default)
- [ ] SQL injection prevention (EF Core parameterized queries)
- [ ] XSS protection
- [ ] CSRF protection
- [ ] CORS configuration
- [ ] HTTPS enforcement
- [ ] Input validation (FluentValidation)
- [ ] Authorization policies
- [ ] Audit logging
- [ ] Rate limiting
- [ ] API key for external services

---

## 📚 Recommended Reading

1. **Clean Architecture** - Robert C. Martin
2. **Domain-Driven Design** - Eric Evans
3. **EF Core in Action** - Jon P Smith
4. **Angular Development** - Angular Team Documentation
5. **RESTful API Design** - Best Practices

---

## 🆘 Troubleshooting

### Common Issues

**Issue**: Migration fails
```powershell
# Solution: Delete Migrations folder and recreate
rm -r src/GestCom.Infrastructure/Data/Migrations
dotnet ef migrations add InitialCreate --project src/GestCom.Infrastructure --startup-project src/GestCom.WebAPI
```

**Issue**: CORS error in Angular
```csharp
// Solution: Add CORS in Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
app.UseCors("AllowAngular");
```

**Issue**: Authentication not working
- Check JWT configuration in appsettings.json
- Verify token is being sent in Authorization header
- Check token expiration

---

## 📞 Support

For questions or issues:
1. Check README.md for architecture details
2. Review QUICKSTART.md (this file)
3. Examine existing code examples
4. Refer to official documentation

---

**Happy Coding! 🚀**
