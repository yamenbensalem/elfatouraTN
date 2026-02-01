# =============================================================================
# GUIDE DE CRÉATION DU PROJET TUNISIAN E-INVOICE API
# =============================================================================
# Copiez et exécutez ces commandes dans votre terminal

# 1. Créer la structure du projet
mkdir TunisianEInvoice
cd TunisianEInvoice

# Créer la solution
dotnet new sln -n TunisianEInvoice

# Créer les projets
dotnet new classlib -n TunisianEInvoice.Domain -o src/TunisianEInvoice.Domain
dotnet new classlib -n TunisianEInvoice.Application -o src/TunisianEInvoice.Application
dotnet new classlib -n TunisianEInvoice.Infrastructure -o src/TunisianEInvoice.Infrastructure
dotnet new webapi -n TunisianEInvoice.API -o src/TunisianEInvoice.API
dotnet new xunit -n TunisianEInvoice.UnitTests -o tests/TunisianEInvoice.UnitTests
dotnet new xunit -n TunisianEInvoice.IntegrationTests -o tests/TunisianEInvoice.IntegrationTests

# Ajouter les projets à la solution
dotnet sln add src/TunisianEInvoice.Domain/TunisianEInvoice.Domain.csproj
dotnet sln add src/TunisianEInvoice.Application/TunisianEInvoice.Application.csproj
dotnet sln add src/TunisianEInvoice.Infrastructure/TunisianEInvoice.Infrastructure.csproj
dotnet sln add src/TunisianEInvoice.API/TunisianEInvoice.API.csproj
dotnet sln add tests/TunisianEInvoice.UnitTests/TunisianEInvoice.UnitTests.csproj
dotnet sln add tests/TunisianEInvoice.IntegrationTests/TunisianEInvoice.IntegrationTests.csproj

# Ajouter les références entre projets
dotnet add src/TunisianEInvoice.Application/TunisianEInvoice.Application.csproj reference src/TunisianEInvoice.Domain/TunisianEInvoice.Domain.csproj
dotnet add src/TunisianEInvoice.Infrastructure/TunisianEInvoice.Infrastructure.csproj reference src/TunisianEInvoice.Application/TunisianEInvoice.Application.csproj
dotnet add src/TunisianEInvoice.API/TunisianEInvoice.API.csproj reference src/TunisianEInvoice.Application/TunisianEInvoice.Application.csproj
dotnet add src/TunisianEInvoice.API/TunisianEInvoice.API.csproj reference src/TunisianEInvoice.Infrastructure/TunisianEInvoice.Infrastructure.csproj

# 2. Installer les packages NuGet

# Application layer
cd src/TunisianEInvoice.Application
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package FluentValidation
dotnet add package FluentValidation.AspNetCore
dotnet add package Microsoft.Extensions.Configuration.Abstractions
cd ../..

# Infrastructure layer
cd src/TunisianEInvoice.Infrastructure
dotnet add package itext7
dotnet add package QRCoder
dotnet add package System.Security.Cryptography.Xml
cd ../..

# API layer
cd src/TunisianEInvoice.API
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
dotnet add package Serilog.AspNetCore
cd ../..

# Tests
cd tests/TunisianEInvoice.UnitTests
dotnet add package Moq
dotnet add package FluentAssertions
cd ../..

# 3. Créer les dossiers nécessaires
mkdir -p src/TunisianEInvoice.Domain/Entities
mkdir -p src/TunisianEInvoice.Domain/Enums
mkdir -p src/TunisianEInvoice.Domain/ValueObjects
mkdir -p src/TunisianEInvoice.Application/DTOs
mkdir -p src/TunisianEInvoice.Application/Interfaces
mkdir -p src/TunisianEInvoice.Application/Services
mkdir -p src/TunisianEInvoice.Application/Mappings
mkdir -p src/TunisianEInvoice.Application/Validators
mkdir -p src/TunisianEInvoice.Infrastructure/Services
mkdir -p src/TunisianEInvoice.Infrastructure/Resources/Schemas
mkdir -p src/TunisianEInvoice.Infrastructure/Certificates
mkdir -p src/TunisianEInvoice.API/Controllers
mkdir -p src/TunisianEInvoice.API/Middleware