# Spécification agent Claude — Implémentation RBAC multi-tenant
**Stack cible :** C# / ASP.NET Core / Blazor / Entity Framework Core  
**Périmètre :** Application commerciale existante, ajout de la gestion des droits par rôle et par entreprise

---

## 1. Objectif

L'agent doit analyser le projet existant, puis générer et intégrer de façon autonome l'ensemble des artefacts nécessaires à un système de contrôle d'accès basé sur les rôles (RBAC) avec isolation multi-tenant (par entreprise). Il produit du code compilable, des migrations EF Core prêtes à l'emploi, et des composants Blazor fonctionnels.

---

## 2. Périmètre des tâches

### Phase 1 — Analyse du projet existant
- Lire la structure des fichiers du projet (`.csproj`, dossiers `Models/`, `Data/`, `Pages/`, `Services/`)
- Identifier le `DbContext` existant et les entités déjà présentes
- Détecter le système d'authentification en place (ASP.NET Identity, JWT, cookie, etc.)
- Identifier les pages/composants Blazor existants pour lesquels des droits doivent être appliqués
- Produire un **rapport d'analyse** avant toute modification

### Phase 2 — Modèle de données
Générer les entités C# suivantes (si elles n'existent pas déjà) :

```
Company          { Id, Name, Slug, Plan, Settings (jsonb) }
ApplicationUser  { + CompanyId (FK) }         ← étendre l'entité existante
Role             { Id, Name, CompanyId (FK) }
Permission       { Id, Feature, Action }       ← ex: "invoices.view"
UserRole         { UserId, RoleId }
RolePermission   { RoleId, PermissionId }
FeatureFlag      { Id, CompanyId, Feature, IsEnabled }
```

- Ajouter les relations et contraintes dans le `DbContext`
- Générer la migration EF Core (`dotnet ef migrations add AddRbacMultiTenant`)
- Générer le script de seed pour les rôles et permissions de base

### Phase 3 — Couche service (backend C#)
Générer les services suivants :

**`IPermissionService` / `PermissionService`**
```csharp
Task<bool> HasPermissionAsync(string userId, string permission);
Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
```
- Requête optimisée avec jointures EF Core
- Mise en cache avec `IMemoryCache` (TTL 5 min, invalidation sur changement de rôle)

**`IFeatureFlagService` / `FeatureFlagService`**
```csharp
Task<bool> IsEnabledAsync(Guid companyId, string feature);
```

**`ITenantService` / `TenantService`**
```csharp
Guid GetCurrentCompanyId();   // résolu depuis le claim JWT/cookie
string GetCurrentUserId();
```

- Enregistrer les services dans `Program.cs` avec les bons lifetimes (`Scoped`)
- Ajouter un filtre global EF Core pour isoler les données par `CompanyId`

### Phase 4 — Composants et directives Blazor

**Composant `<PermissionGuard>`**
```razor
<PermissionGuard Permission="invoices.create" Feature="invoices">
    <Authorized><button>Nouvelle facture</button></Authorized>
    <NotAuthorized><p>Accès refusé</p></NotAuthorized>
</PermissionGuard>
```

**`CustomAuthorizationHandler`** (ASP.NET Core Policy)
- Créer une policy par permission : `options.AddPolicy("invoices.view", ...)`
- Attribut `[Authorize(Policy = "invoices.view")]` utilisable sur les endpoints API

**`AuthorizeView` Blazor étendu**
- Injecter les permissions dans le `ClaimsPrincipal` au login
- Créer un `IClaimsTransformation` qui ajoute les claims de permissions au démarrage de session

**Page d'administration des droits** (`/admin/roles`)
- Tableau des rôles par entreprise
- Matrice permissions / rôles (checkbox)
- Gestion des feature flags par entreprise

---

## 3. Contraintes techniques

| Contrainte | Détail |
|---|---|
| Framework | .NET 8+ / Blazor Server ou WASM |
| ORM | Entity Framework Core 8 |
| Auth | Compatible ASP.NET Core Identity (existant) |
| Cache | `IMemoryCache` (pas de Redis requis par défaut) |
| Base de données | SQL Server ou PostgreSQL (détecter depuis `appsettings.json`) |
| Tests | Générer des tests unitaires xUnit pour `PermissionService` |
| Migrations | Ne pas écraser les migrations existantes |

---

## 4. Règles de comportement de l'agent

### Ce que l'agent DOIT faire
- Toujours lire les fichiers existants avant de les modifier
- Produire un plan d'action et attendre validation avant d'écrire du code
- Modifier les fichiers existants par blocs précis (pas de réécriture complète si possible)
- Respecter les conventions de nommage déjà présentes dans le projet
- Générer des commentaires XML (`///`) sur toutes les interfaces publiques
- Signaler explicitement chaque fichier créé ou modifié

### Ce que l'agent NE DOIT PAS faire
- Supprimer ou écraser des entités ou migrations existantes sans confirmation
- Changer le système d'authentification en place
- Ajouter des dépendances NuGet non nécessaires
- Créer des abstractions inutiles (YAGNI)

---

## 5. Séquence d'exécution attendue

```
1. Analyser le projet → produire rapport
2. Soumettre le plan détaillé → attendre confirmation
3. Créer / modifier les entités du modèle
4. Mettre à jour le DbContext
5. Générer la migration EF Core
6. Créer les services (PermissionService, FeatureFlagService, TenantService)
7. Enregistrer les services dans Program.cs
8. Créer le composant PermissionGuard
9. Créer les policies d'autorisation
10. Créer la page d'administration /admin/roles
11. Générer les tests unitaires
12. Produire un résumé des fichiers créés/modifiés
```

---

## 6. Livrables attendus

- [ ] Rapport d'analyse du projet existant
- [ ] Entités C# (`Company`, `Permission`, `Role`, `UserRole`, `RolePermission`, `FeatureFlag`)
- [ ] `DbContext` mis à jour avec filtres globaux
- [ ] Migration EF Core + script de seed
- [ ] `PermissionService`, `FeatureFlagService`, `TenantService`
- [ ] `PermissionGuard.razor` (composant réutilisable)
- [ ] `CustomAuthorizationHandler.cs`
- [ ] `IClaimsTransformation` pour injection des claims
- [ ] Page Blazor `/admin/roles`
- [ ] Tests unitaires xUnit pour `PermissionService`
- [ ] Résumé des modifications

---

## 7. Exemple de prompt de démarrage

```
Tu es un agent spécialisé en développement C# / Blazor.
Ta mission est d'implémenter un système RBAC multi-tenant dans ce projet existant,
en suivant la spécification fournie.

Commence par analyser la structure du projet, puis soumet un plan
avant de faire la moindre modification. Respecte les conventions
de nommage existantes et ne supprime rien sans confirmation.

Projet : [chemin ou fichiers joints]
```
