# Rapport d'audit logiciel — GestCommercial (Web_GestCom)

Auditeur : agent d'audit logiciel agnostique (méthodologie `agent-audit-logiciel`)
Date : 12 septembre 2026
Application auditée : `D:\Entreprise\Projets\GitProjects\ElFatoura\ElFatourTN_TTN\elfatouraTN\GestCommercial\Web_GestCom`

---

## 1. Résumé exécutif

**Verdict global.** Web_GestCom est une application de gestion commerciale (Blazor Server, .NET 8, EF Core, SQL Server) au périmètre fonctionnel large — ventes, achats, stock, facturation, multi-tenant, RBAC — et à la qualité d'ingénierie nettement supérieure à ce qu'on attend d'un projet de cette taille. Les points forts (isolation multi-tenant à trois niveaux, RBAC avec garde applicative + garde base de données, protection anti-bruteforce, plus de 300 tests automatisés, journal des décisions techniques exceptionnellement rigoureux) sont documentés et vérifiables dans le code. Les faiblesses identifiées sont majoritairement déjà connues et tracées par l'équipe elle-même (fichier `TODO.md`), ce qui est un signe de maturité plutôt qu'un signal d'alarme — mais certaines n'ont pas encore de correctif et méritent d'être priorisées avant qu'elles ne causent un incident client.

**Niveau de risque global : Modéré.** Aucun défaut bloquant trouvé dans le code lu. Le risque principal est opérationnel (identifiants par défaut, absence de jeton de concurrence sur les documents financiers) plutôt qu'architectural.

**Les 3 points les plus critiques :**
1. **Comptes par défaut à mot de passe connu** (`admin/admin123`, `superadmin/SuperAdmin123!`) créés automatiquement au premier démarrage — un oubli de changement de mot de passe en production est un risque d'accès non autorisé direct.
2. **Absence de jeton de concurrence (`RowVersion`)** sur les documents commerciaux : deux modifications simultanées du même document (facture, BL…) se résolvent en "dernier écrit gagne" silencieux, sans détection — risque réel sur des documents financiers en environnement multi-utilisateur.
3. **Incohérence de stock documentée et non corrigée** : une facture générée depuis un bon de livraison (`CreateFromBonLivraisonAsync`) ne décrémente pas le stock elle-même (le BL l'a déjà fait), mais sa suppression restitue quand même du stock à tort — écart de stock silencieux dans un scénario d'usage courant (facturer un BL existant, puis annuler cette facture).

---

## 2. Périmètre et méthode

### Ce qui a été observé
- Code source complet des projets `Web_GestCom` (application active) et `Web_GestCom.Core` (couche partagée : entités, `AppDbContext`, services métier) : modèles de données, couche services, authentification/RBAC, `Program.cs`, configuration, Dockerfile, documentation projet (`CLAUDE.md`, `README.md`, `TODO.md`, `RELEASE_NOTES.md`).
- Structure complète du projet de tests (`Web_GestCom.Tests`) : présence et organisation des fichiers de tests par couche (Services, Data, Components).
- Documentation de déploiement (`deploy/prod/DEPLOY.md`, `docker-compose*.yml`, `.env.example`).
- Historique auto-documenté des bugs de production et de leurs correctifs (`TODO.md`, section "DETTE TECHNIQUE" et journal chronologique par fonctionnalité).

### Ce qui n'a pas pu être vérifié avec l'accès actuel
- **Exécution de l'application** : aucun accès à une instance en fonctionnement (pas de test de bout en bout dans le navigateur, pas de vérification empirique des règles métier en conditions réelles). Les constats fonctionnels s'appuient sur la lecture du code et des journaux de tests manuels consignés dans `TODO.md`, pas sur une observation directe.
- **Exécution de la suite de tests** : aucun accès shell fonctionnel sur le poste (limitation d'outillage côté environnement, non liée au code audité) — les chiffres de tests verts ("322/322", etc.) proviennent des journaux internes du projet, non rejoués par l'auditeur.
- **État réel du dépôt Git** : aucun dossier `.git` trouvé à la racine du dossier `GestCommercial` fourni — impossible de confirmer si `deploy/prod/.env` (secrets réels) est effectivement exclu du contrôle de version, malgré l'intention documentée ("gitignored") dans `DEPLOY.md` et `.env.example`.
- **Dépendances / CVE** : versions des paquets NuGet relevées (voir §4.5) mais aucune vérification en ligne de vulnérabilités connues.
- **GestCom_Desktop** (application WinForms sœur, partage `Web_GestCom.Core`) et `GestCom_LicenseGenerator` : présence confirmée dans le dépôt mais non audités en détail — hors périmètre principal désigné par `CLAUDE.md` ("Almost all code changes happen inside Web_GestCom").
- **Pipeline CI/CD** : aucun fichier de workflow GitHub Actions trouvé (seul `.github/agents/` existe, un sous-agent Claude Code) — le déploiement semble piloté manuellement via un script PowerShell (`deploy_to_vps.ps1`) depuis un poste de développement.

### Domaine métier et stack technique identifiés

**Domaine** (déduit des entités `Client`, `Produit`, `Fournisseur`, `FactureClient`, `BonLivraison`, `DevisClient`, `CommandeVente/Achat`, du vocabulaire français des champs — TVA, FODEC, Timbre fiscal, Retenue à la source — et des taux de TVA seedés 19/13/7/0 %) : logiciel de **gestion commerciale pour PME tunisiennes** (marché du Maghreb francophone — FODEC et retenue à la source sont des mécanismes fiscaux tunisiens), avec support multi-devises (TND/EUR/USD) et architecture SaaS multi-tenant (plusieurs entreprises clientes sur la même instance).

**Stack** (déduite des fichiers `.csproj`, `Dockerfile`, `Program.cs`) : ASP.NET Core 8 / Blazor Server (rendu interactif serveur), Entity Framework Core 8.0 / SQL Server, authentification par cookie, BCrypt pour les mots de passe, xUnit + bUnit + Moq pour les tests, déploiement Docker derrière `nginx-proxy` + Let's Encrypt.

---

## 3. Évaluation fonctionnelle

| Critère | Statut | Preuve |
|---|---|---|
| Couverture des besoins (cycle ventes/achats/stock) | **Conforme** | Cycle de vente complet Devis→Commande→BL→Facture/Avoir et cycle d'achat complet Commande→Réception→Facture Fournisseur, tous implémentés avec services + pages (`Components/Pages/{Devis,CommandesVente,BonsLivraison,FacturesClient,CommandesAchat,BonsReception,FacturesFournisseur}`). |
| Fonctionnalités inachevées ou mortes | **Partiellement conforme** | `README.md` racine de `Web_GestCom` affirme que les modules Achats et la fiche Entreprise sont "planifiés" — faux : ils sont pleinement implémentés (confirmé par la présence des dossiers de pages et par `TODO.md` qui les marque `✅ TERMINÉ`). Documentation utilisateur en décalage avec le code réel. Par ailleurs, `Counter.razor` et `Weather.razor` (gabarits par défaut du template Blazor) sont toujours présents dans `Components/Pages/` — code mort sans usage métier. |
| Exactitude métier (calculs, statuts) | **Conforme, avec une limite connue non corrigée** | Calculs de totaux (`RecalculateTotals`) centralisés et cohérents entre `FactureClientService`/`BonLivraisonService`. Historique de bugs de calcul trouvés et corrigés (ex. totaux non recalculés à la modification d'une facture — corrigé, voir `FactureClientService.UpdateAsync`). Limite documentée et non corrigée : une facture générée depuis un BL (`CreateFromBonLivraisonAsync`) ne trace pas sa provenance ; sa suppression restitue du stock à tort (voir §1, point critique n°3). |
| Gestion des cas limites et erreurs | **Partiellement conforme** | Montants négatifs rejetés côté service (`LineCalculator.EnsureNoNegativeAmounts`, appliqué aux 7 services documents). Conflits EF Core traduits en message clair (`ConcurrencyConflictException`) et l'exception SQL réelle est exposée via `InnerException.Message` (pas le message générique EF inutilisable). Mais : pas de jeton de concurrence (`RowVersion`) — une modification concurrente silencieuse (pas une suppression) n'est pas détectée, uniquement "dernier écrit gagne". Race condition théorique documentée sur la génération de code séquentiel en cas d'insertions strictement simultanées (probabilité faible, assumée par l'équipe). |
| Cohérence et prévisibilité | **Conforme** | Terminologie et motifs de statuts homogènes entre documents similaires (`Ouvert/Confirmé/Annulé`, `Non Réglé/Partiellement Réglé/Réglé`, `Non Facturé/Facturé`) ; motif de clonage, de génération inter-documents (Commande→BL→Facture) et de permission (`{module}.{action}`) appliqué uniformément. |
| Expérience utilisateur | **Conforme (sur preuve indirecte)** | Composants partagés (`Notification`, `ConfirmDialog`, `LoadingSpinner`) présents et réutilisés systématiquement d'après la documentation et l'organisation des dossiers ; alertes visuelles de stock bas, badges d'état colorés. Non vérifié en conditions réelles (pas d'accès à l'UI en fonctionnement) — évaluation basée sur le code et les vérifications manuelles consignées dans `TODO.md`. |
| Adéquation aux utilisateurs réels (rôles, cas rares) | **Conforme** | RBAC granulaire par fonctionnalité × action (`view/create/update/delete`), rôles Admin/Manager/Employé avec permissions décroissantes cohérentes, rôle `SuperAdmin` strictement cantonné à la gestion de plateforme (aucun accès aux données métier — vérifié à trois niveaux indépendants, voir §4.3). Cas rares couverts : avoirs (crédit notes), suppression avec confirmation et restitution de stock, clonage de document, changement de mot de passe, désactivation (pas de suppression physique d'utilisateur). |

---

## 4. Évaluation technique

| Critère | Statut | Preuve |
|---|---|---|
| **4.1 Architecture** — séparation des responsabilités | **Conforme** | Chaîne stricte Composants Blazor → Services (interfaces + implémentations) → `AppDbContext` → Modèles, documentée et respectée dans le code lu (`FactureClientService`, `BonLivraisonService`, `ClientService`). Mutations de stock centralisées dans les services documents, jamais dans le `DbContext`. Mutualisation `Web_GestCom.Core` entre l'app web et l'app desktop (26 entités, `AppDbContext`, 22 services partagés en une seule fois) — évite la duplication entre les deux fronts. |
| **4.2 Qualité du code** | **Conforme** | Conventions homogènes (French domain naming, injection de dépendances par constructeur primaire C# 12, gestion transactionnelle systématique `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` dans tous les services documents observés). Duplication identifiée et corrigée activement (génération de code CL/PR/FO factorisée dans `AppDbContextSaveExtensions.GenerateNextCodeAsync` après un bug de duplication trouvé en trois endroits). Point d'attention : `Components/Pages/Home.razor` fait 71 Ko en un seul fichier — taille anormalement élevée pour une page d'accueil, signe possible de logique de tableau de bord non extraite en services/composants dédiés (non confirmé en détail, fichier non lu intégralement). |
| **4.3 Sécurité (OWASP générique)** | **Conforme pour la conception, à vérifier à l'exploitation** | Authentification par cookie avec re-validation à chaque requête (`OnValidatePrincipal` compare `SecurityStamp`/`PermissionsVersion`/tenant à chaque appel — révocation immédiate en cas de changement de rôle ou désactivation). Mots de passe en BCrypt avec migration automatique et transparente depuis d'anciens formats (SHA-256 non salé, bcrypt "nu") — bonne pratique de migration progressive sans forcer une réinitialisation de masse. Autorisation vérifiée côté serveur à trois niveaux indépendants et redondants : policy d'autorisation (`PermissionAuthorizationHandler`), garde applicative en tête de chaque méthode de service (`ServicePermissionGuard.EnsureAsync`), et filtre de requête EF Core au niveau `DbContext` (défense en profondeur explicitement commentée dans le code, avec une régression de sécurité réelle trouvée et corrigée en cours de développement — bypass `SuperAdmin` retiré, voir `TODO.md` "SuperAdmin — rôle dédié"). Protection anti-bruteforce à la connexion avec verrouillage progressif par login et par IP (`LoginProtectionService`). **Points d'attention réels** : comptes par défaut à mot de passe connu et documenté en clair dans le code/README (`admin/admin123`, `superadmin/SuperAdmin123!`) — acceptable en développement, dangereux si non changé en production ; aucune vérification possible ici que ce changement est effectivement imposé ou contrôlé au déploiement. Pas de secret en dur trouvé dans le code applicatif lu (les chaînes de connexion utilisent l'authentification Windows en dev ; les secrets prod passent par `.env`). |
| **4.4 Tests** | **Partiellement conforme** | Couverture de la couche Services très large et orientée comportement métier (pas de tests triviaux de plomberie) : ~30 fichiers de tests de services couvrant CRUD, impact stock, règlements, clonage, quotas, permissions, verrouillage de connexion, isolation tenant. Historique notable : plusieurs bugs de régression réels ont été détectés par les tests **avant** la mise en production (pattern "test qui échoue sans le fix, confirmé vert avec"). Point faible : la couche `Components/Pages` n'a que 5 fichiers de test (`Home`, `Counter`, `ClientsList`, et 4 composants partagés) sur une dizaine de modules de pages — la majorité des formulaires Blazor (Devis, Factures, BL, Achats…) n'ont pas de test d'intégration bUnit dédié, seule la logique de service sous-jacente est testée. |
| **4.5 Dépendances et supply chain** | **Non évaluable (partiellement)** | Dépendances peu nombreuses et ciblées : EF Core 8.0.0, BCrypt.Net-Next 4.0.3, xUnit 2.7.0, bUnit 1.35.3, Moq 4.20.72 — pas de sur-ingénierie apparente. Versions cohérentes avec .NET 8 (LTS). Aucune vérification de vulnérabilité connue (CVE) effectuée — hors périmètre de cet accès (pas d'exécution d'outil d'audit de dépendances). |
| **4.6 Performance et scalabilité** | **Conforme (sur le code lu)** | Pas de motif N+1 flagrant dans les requêtes examinées (`Include`/`ThenInclude` utilisés de façon ciblée). Point d'architecture à connaître, pas un défaut : Blazor Server maintient un `AppDbContext` scopé par circuit utilisateur (toute la session, pas juste une requête) — c'est justement la source documentée de plusieurs bugs de suivi d'entités (voir §4.8), et un facteur de consommation mémoire serveur proportionnel au nombre d'utilisateurs connectés simultanément (contrainte structurelle de Blazor Server, pas un choix arbitraire de ce projet). |
| **4.7 Observabilité et exploitabilité** | **Partiellement conforme** | Journal d'activité métier dédié (`JournalActivite`/`JournalActiviteService`) tracé sur les opérations sensibles (Ajout/Modification/Suppression/Clone), conçu pour ne jamais faire échouer l'opération métier en cas d'échec de journalisation. `SaveChangesGuardedAsync` expose le vrai message SQL en cas d'erreur (bonne pratique de diagnostic). En revanche, la configuration ne montre qu'une journalisation ASP.NET Core par défaut (`appsettings.json` : `Logging:LogLevel` standard) — aucune preuve de journalisation structurée, d'export vers un système de supervision, ou d'alerting opérationnel branché sur les seuils d'alerte de connexion (`AlertOnLoginFailures`/`AlertOnIpFailures` existent dans le modèle de configuration mais leur consommation effective — envoi d'un e-mail, d'un webhook — n'a pas été localisée dans le code lu). |
| **4.8 Maintenabilité et dette technique** | **Conforme, dette activement gérée** | Point fort majeur et rare : `TODO.md` (68 Ko) constitue un journal exceptionnellement rigoureux de chaque bug de production trouvé, sa cause racine, son correctif, et — souvent — sa limite assumée non corrigée avec justification explicite. C'est le meilleur indicateur qu'un développeur externe pourrait reprendre ce code : les pièges connus (suivi d'entités EF Core sur circuit Blazor Server, génération de code par MAX vs COUNT, script injecté dans un composant Razor) sont documentés both dans le code (commentaires) et dans `Web_GestCom/CLAUDE.md` ("Known Pitfalls"). Dette explicitement assumée et non résolue : généralisation incomplète d'`AsNoTracking()` aux services documents (testée puis délibérément non étendue, risque réel identifié) ; colonne `Utilisateur.Role` marquée `[Obsolete]` en transition vers le RBAC complet mais toujours présente ; pas de migrations EF Core formelles, uniquement des blocs SQL bruts idempotents dans `Program.cs` (convention documentée et suivie de façon cohérente, mais fragile à l'échelle si l'équipe grandit). |
| **4.9 Déploiement et cycle de vie** | **Conforme, avec une réserve** | Pipeline de déploiement structuré : build Docker multi-étapes, `nginx-proxy` + `acme-companion` pour HTTPS automatique, SQL Server en conteneur séparé, script d'orchestration PowerShell documenté (`DEPLOY.md`). Reproductibilité correcte via Docker Compose. Réserve : aucun pipeline CI/CD automatisé trouvé (pas de `.github/workflows`) — le déploiement semble déclenché manuellement depuis un poste de développeur, ce qui est un facteur de risque humain (oubli d'étape, environnement local différent) plutôt qu'un défaut technique en soi. |

---

## 5. Constats priorisés

| Constat | Sévérité | Effort estimé | Recommandation concrète |
|---|---|---|---|
| Comptes par défaut à mot de passe documenté en clair (`admin/admin123`, `superadmin/SuperAdmin123!`), créés automatiquement si aucun compte n'existe | **Majeur** | Faible | Ajouter une vérification de démarrage qui journalise un avertissement si ces identifiants par défaut sont encore actifs après N jours, ou forcer un changement de mot de passe à la première connexion de ces comptes. Documenter explicitement dans la procédure de mise en production le changement obligatoire de ces deux mots de passe. |
| Absence de jeton de concurrence (`RowVersion`) sur les documents commerciaux : modification concurrente silencieuse possible (dernier écrit gagne) | **Majeur** | Moyen | Ajouter une colonne `RowVersion`/`xmin`-équivalent (`[Timestamp]` EF Core) sur les entités documents (Facture, BL, Commande…) et migrer via le même motif SQL brut idempotent déjà utilisé dans `Program.cs`. |
| Facture générée depuis un Bon de Livraison (`CreateFromBonLivraisonAsync`) : la suppression de cette facture restitue du stock à tort (le BL, pas la facture, a décrémenté le stock) | **Majeur** | Moyen | Ajouter un champ de provenance (`NumeroBonLivraisonOrigine` ou équivalent) sur `FactureClient`, et faire vérifier ce champ par `DeleteAsync` avant de restituer le stock. Migration de schéma nécessaire (déjà anticipée dans `TODO.md`). |
| `README.md` du module décrit les modules Achats/Entreprise comme "planifiés" alors qu'ils sont pleinement implémentés | **Mineur** | Faible | Mettre à jour `README.md` pour refléter l'état réel (`TODO.md` est à jour, `README.md` ne l'est pas) — risque de confusion pour un nouveau développeur ou un client consultant la doc. |
| Couverture de test quasi nulle sur la couche `Components/Pages` (formulaires Blazor) hors Home/Counter/ClientsList | **Mineur** | Moyen | Étendre bUnit aux formulaires à plus fort enjeu financier (`FactureClientForm`, `BonLivraisonForm`) en priorité, sur le modèle des tests déjà écrits pour `ClientsList`. |
| Code mort issu du template Blazor par défaut (`Counter.razor`, `Weather.razor`) toujours présent | **Amélioration possible** | Faible | Supprimer ces deux fichiers, ou les déplacer hors de `Components/Pages` s'ils servent de bac à sable de développement. |
| Race condition théorique sur la génération de code séquentiel (deux insertions strictement simultanées) | **Mineur** (assumé et documenté par l'équipe) | Moyen | Envisager une séquence SQL dédiée ou une boucle de nouvelle tentative sur violation de contrainte, si le volume d'utilisateurs concurrents augmente significativement. |
| Aucun pipeline CI/CD automatisé identifié ; déploiement manuel via script PowerShell | **Amélioration possible** | Moyen | Ajouter un workflow CI a minima pour exécuter `dotnet test` à chaque push/PR, avant d'envisager l'automatisation du déploiement lui-même. |
| Alertes de protection de connexion (`AlertOnLoginFailures`/`AlertOnIpFailures`) définies dans la configuration mais consommation effective (notification réelle) non localisée | **Mineur** | Faible à moyen | Vérifier si ces alertes aboutissent réellement à une notification opérationnelle (e-mail, webhook, log dédié) ; sinon, les brancher pour qu'elles servent leur objectif de détection d'attaque. |

---

## 6. Hypothèses à valider

- **`.env` de production réellement exclu du contrôle de version** : l'intention est documentée (`DEPLOY.md`, `.env.example`) mais aucun dossier `.git` n'a été trouvé à la racine du dossier fourni pour vérifier l'historique réel des commits. À confirmer avec `git log --all -- "deploy/prod/.env"` sur le dépôt réel.
- **Volumétrie et charge cible** : aucune indication trouvée dans le code sur le nombre d'entreprises/utilisateurs simultanés visés — l'évaluation "performance/scalabilité" suppose une charge modérée (PME), cohérente avec le domaine observé, mais non confirmée explicitement par l'utilisateur.
- **Politique de changement des mots de passe par défaut en production** : à confirmer auprès de l'équipe — existe-t-il une procédure de mise en production qui impose ce changement avant l'ouverture au client ?
- **Périmètre réglementaire tunisien** (FODEC, retenue à la source, timbre fiscal) : les taux et mécanismes semblent correctement modélisés dans le code, mais leur conformité fiscale précise (barèmes à jour, obligations déclaratives) n'entre pas dans le périmètre de cet audit logiciel et devrait être validée par un expert-comptable ou fiscaliste tunisien.
- **`GestCom_Desktop`** : présence confirmée d'une application desktop sœur partageant `Web_GestCom.Core` ; son état d'avancement, ses écrans et son propre niveau de dette n'ont pas été audités — à traiter dans un audit dédié si cette application est également en production active.

---

## 7. Annexes

**Fichiers de preuve principaux consultés :**
- `Web_GestCom/CLAUDE.md` — architecture, pièges connus, convention de numérotation
- `Web_GestCom/README.md` — documentation fonctionnelle (partiellement obsolète, voir §5)
- `Web_GestCom/TODO.md` — journal détaillé des bugs, correctifs et dette technique assumée
- `Web_GestCom/RELEASE_NOTES.md` — historique des versions (ex. migration SHA-256 → BCrypt)
- `Web_GestCom.Core/Data/AppDbContext.cs` — filtres de tenant, règles d'appartenance, seed RBAC
- `Web_GestCom.Core/Services/{FactureClientService,BonLivraisonService,ClientService,UtilisateurService}.cs`
- `Web_GestCom.Core/Services/{ServicePermissionGuard,LoginProtectionService,AppDbContextSaveExtensions}.cs`
- `Web_GestCom/Auth/{PermissionAuthorizationHandler,PermissionClaimsTransformation,PermissionPolicyProvider}.cs`
- `Web_GestCom/Program.cs` — pipeline d'authentification, migrations SQL brutes, en-têtes de sécurité
- `Web_GestCom/deploy/prod/{DEPLOY.md,.env.example,docker-compose*.yml}`
- Structure complète de `Web_GestCom.Tests/` (recensement des ~40 fichiers de tests par couche)

**Méthodologie appliquée :** `01-system-prompt.md`, `02-methodologie.md`, `03-grille-evaluation-fonctionnelle.md`, `04-grille-evaluation-technique.md`, `05-template-rapport-audit.md` du projet `agent-audit-logiciel`.
