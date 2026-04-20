# Guide de configuration Claude Code — C# / Blazor / Angular

## Le système de mémoire de Claude Code

Claude Code utilise une architecture mémoire à **4 niveaux** (du plus prioritaire au moins prioritaire) :

```
Enterprise Policy     ← Règles organisationnelles (priorité max)
Project Memory        ← CLAUDE.md à la racine du projet
Project Rules         ← .claude/rules/ (règles modulaires)
User Memory           ← ~/.claude/CLAUDE.md (préférences personnelles)
```

Les fichiers `CLAUDE.md` sont des fichiers Markdown lus automatiquement par Claude Code au début de chaque session. Traitez `CLAUDE.md` comme l'endroit où vous écrivez ce que vous devriez autrement ré-expliquer à chaque fois.

---

## Étape 1 — Créer la mémoire globale (toutes vos sessions)

Créez le fichier `~/.claude/CLAUDE.md` pour vos préférences personnelles transversales à tous vos projets :

```bash
mkdir -p ~/.claude
nano ~/.claude/CLAUDE.md
```

Contenu recommandé :

```markdown
## Mes préférences globales
- Langue des réponses : français
- Toujours expliquer les décisions architecturales
- Ne jamais modifier des fichiers sans me le confirmer au préalable
- Toujours utiliser async/await, jamais .Result ou .Wait()
- Toujours appliquer les nullable reference types (enable)
```

---

## Étape 2 — Créer le `CLAUDE.md` du projet

À la **racine de votre solution** (ex: `MySolution/CLAUDE.md`) :

```bash
touch CLAUDE.md
```

> Un `CLAUDE.md` contenant 15 règles impératives produit du code conforme dans la majorité des cas.
> Le même contenu rédigé en style **descriptif** tombe à 73% de conformité.
> Les règles négatives sont aussi puissantes que les règles positives.

### Template complet — C# / Blazor / Angular

```markdown
# [NomDuProjet] — Instructions Claude Code

## Stack technique
- Backend : ASP.NET Core Web API (.NET 8), C# 12
- Frontend : Blazor Server (OU Angular 17+ standalone)
- ORM : Entity Framework Core 8
- Tests : xUnit + FluentAssertions + Moq
- Auth : JWT + ASP.NET Identity

## Structure du projet
- `src/MyApp.API/`            → Controllers, Middleware, Program.cs
- `src/MyApp.Application/`   → Services, Commands, Queries, DTOs
- `src/MyApp.Domain/`        → Entités, Value Objects
- `src/MyApp.Infrastructure/`→ Repos EF Core, services externes
- `src/MyApp.Web/`           → Blazor/Angular frontend
- `tests/`                   → UnitTests, IntegrationTests

## Commandes essentielles
- Build   : dotnet build --configuration Release -warnaserror
- Tests   : dotnet test --collect:"XPlat Code Coverage"
- Run API : dotnet run --project src/MyApp.API

## Règles strictes — TOUJOURS appliquer
- TOUJOURS utiliser async/await, JAMAIS .Result ou .Wait()
- TOUJOURS passer des CancellationToken dans les méthodes async
- TOUJOURS utiliser les DTOs — ne jamais exposer les entités Domain
- TOUJOURS valider les inputs avec FluentValidation
- TOUJOURS appliquer <Nullable>enable</Nullable>
- JAMAIS mettre de logique métier dans les Controllers
- JAMAIS utiliser `new` pour instancier des services (DI obligatoire)
- JAMAIS mettre de secrets dans appsettings.json

## Règles de qualité
- Méthode > 20 lignes      → extraire
- Classe > 300 lignes      → découper par responsabilité
- Méthode > 3 paramètres   → créer un objet de requête
- Imbrication > 3 niveaux  → utiliser des guard clauses

## Workflow de développement
1. CODE    → Domain → Application → Infrastructure → API → Frontend
2. BUILD   → zéro warning (-warnaserror)
3. TEST    → Unit → Integration → E2E
4. REFACTOR→ seulement après tests verts
```

---

## Étape 3 — Créer les règles modulaires (`.claude/rules/`)

Les règles sont le **manuel des standards de l'équipe** — elles indiquent à Claude *comment* les choses doivent être faites. Séparer en petits fichiers par responsabilité garde les choses claires et facilite la maintenance.

```bash
mkdir -p .claude/rules
```

---

### `.claude/rules/csharp-coding-style.md`

```markdown
# Standards C# — Style de code

- Utiliser les record types pour les DTOs immuables
- Utiliser les switch expressions plutôt que if/else chains
- Appliquer le Result pattern pour les erreurs métier
- Nommage : PascalCase classes/méthodes, camelCase variables, _camelCase champs privés
- JAMAIS d'abréviations dans les noms (data, obj, temp → interdit)
- XML doc comments obligatoires sur toutes les méthodes publiques
- ConfigureAwait(false) obligatoire dans les méthodes de bibliothèques
- AsNoTracking() sur toutes les requêtes EF Core en lecture seule
```

---

### `.claude/rules/testing.md`

```markdown
# Standards de tests

- Framework : xUnit + FluentAssertions + Moq (ou NSubstitute)
- Pattern de nommage : `MethodName_WhenCondition_ExpectedResult`
- Structure : Arrange / Act / Assert (commentaires obligatoires)
- Couverture minimale :
  - Domain / Application : 90%
  - API Controllers      : 80%
  - Blazor Components    : 70%
  - Angular Components   : 70%
- TOUJOURS mocker les dépendances externes dans les tests unitaires
- JAMAIS d'appels réseau réels dans les tests unitaires
- WebApplicationFactory pour les tests d'intégration ASP.NET Core
```

---

### `.claude/rules/security.md`

```markdown
# Règles de sécurité
# globs: ["src/**/*.cs", "src/**/*.razor"]

- Validation obligatoire sur TOUS les endpoints
- Requêtes SQL via EF Core uniquement (pas de SQL brut sans paramètres)
- JWT : courte durée de vie + refresh token rotation
- Rate limiting sur tous les endpoints publics (AddRateLimiter)
- HTTPS obligatoire (UseHttpsRedirection + HSTS)
- CORS : jamais de wildcard en production
- [Authorize] au niveau Controller, [AllowAnonymous] sélectif
- Secrets dans les variables d'environnement (dotnet user-secrets en dev)
```

---

### `.claude/rules/blazor.md` *(si Blazor)*

```markdown
# Standards Blazor
# globs: ["**/*.razor", "**/*.razor.cs"]

- Un composant = une responsabilité
- @inject pour les services, jamais de state global statique
- EventCallback<T> pour la communication enfant→parent
- @key obligatoire dans les boucles @foreach
- IJSRuntime uniquement si accès DOM inévitable
- StateContainer ou Fluxor pour l'état partagé
- Lazy loading pour les modules WASM
```

---

### `.claude/rules/angular.md` *(si Angular)*

```markdown
# Standards Angular
# globs: ["**/*.ts", "**/*.html"]

- Standalone components uniquement (Angular 17+)
- OnPush change detection partout
- Signals pour l'état réactif (Angular 17+)
- inject() au lieu de constructor injection
- Routes lazy-loaded par feature
- Pattern smart/dumb components obligatoire
- Typed reactive forms avec FormBuilder
```

---

### `.claude/rules/api.md`

```markdown
# Standards API REST
# globs: ["src/**/Controllers/**/*.cs", "src/**/Endpoints/**/*.cs"]

- Versioning obligatoire : api/v1/resource
- ActionResult<T> sur tous les endpoints
- DTOs avec record types pour les requêtes et réponses
- ProblemDetails pour toutes les réponses d'erreur
- Health check endpoint : app.MapHealthChecks("/health")
- Swagger/OpenAPI configuré et accessible en dev
- Logging via ILogger<T> (Serilog recommandé)
```

---

## Étape 4 — Initialiser avec `/init`

La commande `/init` initialise automatiquement un fichier `CLAUDE.md` en analysant votre codebase :

```bash
# Dans Claude Code, depuis la racine du projet
/init
```

Puis vérifiez ce qui est chargé dans la session :

```bash
/memory
```

Pour ajouter une règle à la volée pendant une session :

```bash
# Préfixer avec # pour sauvegarder en mémoire
# Always use named imports for tree-shaking
```

---

## Étape 5 — Bonnes pratiques de maintenance

- Limitez chaque `CLAUDE.md` à **150 lignes maximum** (les fichiers trop longs gaspillent le contexte)
- Les instructions en **listes à puces** sont suivies 60% mieux que les paragraphes narratifs
- Rédigez des règles **impératives** ("TOUJOURS", "JAMAIS") plutôt que descriptives
- Placez les règles les plus importantes **en premier** (Claude lit séquentiellement)
- Programmez une **révision mensuelle** avec `/memory`

### Fichiers à versionner dans Git

```bash
# Versionner (partagé avec l'équipe)
CLAUDE.md
.claude/rules/

# Ne PAS versionner (préférences personnelles)
.claude/memory/
CLAUDE.local.md
```

Ajoutez à votre `.gitignore` :

```gitignore
# Claude Code — mémoire auto et préférences perso
.claude/memory/
CLAUDE.local.md
```

---

## Structure finale du projet

```
MySolution/
├── CLAUDE.md                          ← Mémoire projet (versionné Git)
├── CLAUDE.local.md                    ← Préférences perso (gitignored)
├── .claude/
│   └── rules/
│       ├── csharp-coding-style.md
│       ├── testing.md
│       ├── security.md
│       ├── blazor.md                  ← ou angular.md
│       └── api.md
├── src/
│   ├── MyApp.API/
│   ├── MyApp.Application/
│   ├── MyApp.Domain/
│   ├── MyApp.Infrastructure/
│   └── MyApp.Web/
└── tests/
    ├── MyApp.UnitTests/
    ├── MyApp.IntegrationTests/
    └── MyApp.E2ETests/
```

Et globalement sur votre machine :

```
~/.claude/
└── CLAUDE.md    ← Préférences personnelles (tous projets)
```

---

## Workflow recommandé dans Claude Code

```
1. Clarifier les exigences (framework, auth, DB)
2. Scaffolder la structure du projet
3. [CODE]    → Domain → Application → Infrastructure → API → Frontend
4. [BUILD]   → Compiler, vérifier les warnings, valider la config
5. [TEST]    → Unit → Integration → E2E
6. [REFACTOR]→ Nettoyer, documenter, optimiser
7. Vérifier la checklist sécurité
8. Résumer ce qui a été construit + suggérer les prochaines étapes
```
