# 🧱 [NomDuProjet] — Clean Architecture stricte (version opinionated)

---

## 🧭 Préférences globales

* Langue : **français**
* Toujours expliquer les choix techniques et architecturaux
* Toujours demander confirmation avant modification de fichiers existants
* **async/await obligatoire** (`.Result` et `.Wait()` interdits)
* `CancellationToken` obligatoire sur toutes les méthodes async
* `<Nullable>enable</Nullable>` obligatoire

---

## 🏗️ Architecture (Clean Architecture stricte)

### Règle fondamentale

➡️ **Les dépendances pointent UNIQUEMENT vers le Domain**

```
API → Application → Domain
         ↓
   Infrastructure (implémentations)
```

### Interdictions

* ❌ Domain → aucune dépendance externe
* ❌ Application → ne dépend PAS d’Infrastructure
* ❌ Controllers → aucune logique métier
* ❌ Pas d’accès direct à EF Core depuis Application

---

## 📦 Organisation des projets

```
src/
 ├── MyApp.Domain/
 │    ├── Entities/
 │    ├── ValueObjects/
 │    ├── Enums/
 │    ├── Events/
 │    └── Interfaces/ (Repository, Services abstraits)
 │
 ├── MyApp.Application/
 │    ├── Abstractions/
 │    ├── Features/
 │    │    └── [FeatureName]/
 │    │         ├── Commands/
 │    │         ├── Queries/
 │    │         ├── DTOs/
 │    │         ├── Validators/
 │    │         └── Handlers/
 │    └── Behaviors/ (Pipeline MediatR)
 │
 ├── MyApp.Infrastructure/
 │    ├── Persistence/
 │    │    ├── DbContext/
 │    │    ├── Configurations/
 │    │    └── Repositories/
 │    ├── Services/
 │    └── Migrations/
 │
 ├── MyApp.API/
 │    ├── Controllers/
 │    ├── Middleware/
 │    └── Configuration/
 │
 └── MyApp.Web/
      └── Frontend
```

---

## ⚙️ Stack imposée

* .NET 8 / C# 12
* ASP.NET Core Web API
* EF Core 8
* MediatR (**obligatoire**)
* FluentValidation
* xUnit + FluentAssertions + Moq

---

## 🧠 Patterns obligatoires

### CQRS (strict)

* Une action = un **Command** ou une **Query**
* Aucun mélange lecture/écriture

### MediatR

* Tous les use cases passent par MediatR
* Controllers = simple proxy vers MediatR

### Exemple

```csharp
public record CreateOrderCommand(string CustomerId) : IRequest<Guid>;
```

---

## 🔁 Pipeline Behaviors (obligatoires)

Ordre d’exécution :

1. Validation (FluentValidation)
2. Logging
3. Performance
4. Transaction (si écriture)

---

## 🗃️ Accès aux données

* Repositories définis dans **Domain**
* Implémentation dans **Infrastructure**
* EF Core UNIQUEMENT dans Infrastructure

### Interdictions

* ❌ Pas de `DbContext` dans Application
* ❌ Pas de LINQ complexe dans Controllers

---

## 📐 Règles Domain

* Entités riches (logique métier dedans)
* Pas d’anemic domain model
* Value Objects dès que possible
* Invariants protégés (constructeurs/factories)

---

## 📡 API (ultra fine)

### Controllers = 100% minimal

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateOrderCommand command, CancellationToken ct)
{
    var result = await _mediator.Send(command, ct);
    return Ok(result);
}
```

### Interdictions

* ❌ Pas de logique
* ❌ Pas de mapping complexe
* ❌ Pas d’accès direct aux services

---

## 🔐 Sécurité

* JWT obligatoire
* Policies pour autorisation
* Validation systématique
* Jamais de secrets en dur

---

## 📏 Règles de qualité

* Méthode > 20 lignes → refactor
* Classe > 300 lignes → découper
* > 3 paramètres → objet
* Imbrication >3 → guard clauses

---

## 🧪 Tests (obligatoires)

### Unit tests

* Domain + Application

### Integration tests

* Infrastructure + DB

### Règles

* Pas de mocks dans Domain
* FluentAssertions obligatoire

---

## 🚨 Règles strictes

* DTO obligatoire (jamais d’entités exposées)
* DI obligatoire (pas de `new`)
* Logging via `ILogger`
* Gestion des erreurs via middleware global

---

## 🔄 Workflow

1. Domain (modèle métier)
2. Application (use cases)
3. Infrastructure
4. API
5. Tests
6. Refactor

---

## 🧠 Bonnes pratiques

* Favoriser immuabilité
* Eviter sur-ingénierie
* Mapper via Mapster ou AutoMapper
* Centraliser les constantes
* Feature-based organization (pas technique)

---

## ❗ Règle Claude

* Toujours expliquer AVANT coder
* Ne jamais modifier sans validation
* Poser des questions si doute
* Code prêt à compiler uniquement

---

## ⚖️ Philosophie

> Clean Architecture = découplage + testabilité
> ❌ Pas = complexité inutile

---

## 🚀 Option avancée (si projet complexe)

* Domain Events
* Outbox Pattern
* Caching (Redis)
* Feature Flags
* Observabilité (OpenTelemetry)

---
