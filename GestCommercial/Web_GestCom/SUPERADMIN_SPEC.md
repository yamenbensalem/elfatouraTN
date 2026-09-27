# Spécification — Rôle et dashboard SuperAdmin

**Périmètre :** `Web_GestCom` (Blazor Server, .NET 8) + `Web_GestCom.Core` (couche partagée avec `GestCom_Desktop`).
**Objectif de ce document :** donner à une session Claude Code qui n'a pas le contexte de ce projet une
vue complète et vérifiée du rôle SuperAdmin — ce qu'il est, ce qu'il fait, comment c'est appliqué en
code, et où sont les écarts connus. Toutes les affirmations ci-dessous sont sourcées par
fichier:ligne ; se référer au code cité en cas de doute plutôt qu'à ce document seul (il peut se
périmer).

---

## 1. Résumé exécutif

`SuperAdmin` est un **rôle de gestion de plateforme**, pas un niveau d'administration supérieur à
`Admin`. C'est la distinction la plus importante de toute cette spec :

- **`Admin`** gère les données métier (clients, factures, stock…) **de sa propre entreprise**.
- **`SuperAdmin`** gère la **plateforme elle-même** (entreprises clientes, comptes utilisateurs
  toutes entreprises confondues, rôles/permissions, journal d'audit, clés API, webhooks) et n'a
  **aucun accès aux données métier d'aucune entreprise** — ni en lecture, ni en écriture. Ce n'est
  pas une convention UI : c'est appliqué à trois niveaux indépendants du code (§5).

Il n'y a qu'un seul compte SuperAdmin par défaut, créé au premier démarrage
(`superadmin` / `SuperAdmin123!`, §6), et **aucune interface ne permet d'en créer un second** — le
formulaire d'ajout d'utilisateur n'offre pas ce rôle en option.

---

## 2. Ce qu'un SuperAdmin peut faire — vue d'ensemble

| Écran | Route | Ce qu'il permet |
|---|---|---|
| Tableau de bord (`Home.razor`) | `/` | KPI plateforme (nb entreprises, nb utilisateurs) + raccourcis. Aucune requête sur les données métier n'est faite pour ce rôle. |
| Entreprises | `/admin/entreprises` | Créer/modifier/supprimer les entreprises clientes (tenants), leur plan, leurs quotas de comptes. |
| Demandes d'Abonnement | `/admin/demandes-abonnement` | Traiter les demandes venues du site public (essai, activation, refus). Aucun paiement en ligne réel. |
| Tous les Utilisateurs | `/admin/utilisateurs-global` | Voir/créer les utilisateurs de **toutes** les entreprises. |
| Rôles & Permissions | `/admin/roles` | Éditer la matrice permissions par rôle (partagé avec Admin). |
| Journal d'Activité | `/admin/journal` | Voir/purger le journal d'audit — vue non filtrée par entreprise pour ce rôle (§10). |
| Clés d'API | `/admin/cles-api` | CRUD de clés API — stockage seulement, aucune API réelle branchée dessus. |
| Webhooks | `/admin/webhooks` | CRUD de webhooks — stockage seulement, aucune livraison réelle. |

Ce que SuperAdmin **ne peut pas faire** : consulter ou modifier un client, produit, devis,
commande, bon de livraison/réception ou facture d'une entreprise — même la sienne, puisqu'il n'en a
pas. Voir §5 pour le mécanisme exact.

---

## 3. Routage et pages

Toutes les pages SuperAdmin vivent dans `Components/Pages/Admin/` — il n'y a pas de dossier dédié,
la distinction se fait par l'attribut `[Authorize(Roles = ...)]` de chaque page.

### Pages exclusives SuperAdmin (`[Authorize(Roles = "SuperAdmin")]`)

- **`CompaniesList.razor`** (`/admin/entreprises`) — liste/CRUD des entreprises. La création
  d'une entreprise crée aussi, dans la même modale, son premier utilisateur Admin
  (login/mot de passe/email) — **sans transaction** : si la création de l'Admin échoue après
  que l'entreprise est enregistrée, l'entreprise reste créée orpheline et un message invite à
  créer l'Admin manuellement depuis "Tous les Utilisateurs" (`CompaniesList.razor:251-257`).
- **`AbonnementsList.razor`** (`/admin/demandes-abonnement`) — voir §8, workflow complet.
- **`UtilisateursGlobalList.razor`** (`/admin/utilisateurs-global`) — voir §9.
- **`ClesApiList.razor`** (`/admin/cles-api`) et **`WebhooksList.razor`** (`/admin/webhooks`) —
  CRUD simple, avec un bandeau à l'écran précisant qu'aucun mécanisme réel n'y est branché
  (`ClesApiList.razor:16-19`, `WebhooksList.razor:16-19`).

### Pages partagées Admin + SuperAdmin (`[Authorize(Roles = "Admin,SuperAdmin")]`)

- **`RolesGestion.razor`** (`/admin/roles`) — matrice de permissions par rôle.
- **`JournalActiviteList.razor`** (`/admin/journal`) — voir §10.
- **`UtilisateursList.razor`** (`/admin/utilisateurs`) — liste par entreprise, avec bouton
  Activer/Désactiver (absent de la version globale, §9).
- **`UtilisateurForm.razor`** (`/admin/utilisateurs/nouveau` et `/{Id}`) — formulaire partagé,
  se comporte différemment pour SuperAdmin (§9).
- `Entreprise/EntrepriseForm.razor` et les six écrans `Parametres/*.razor` (Devise, TVA, Unité,
  Catégorie, Fabricant, Mode de paiement) — ⚠️ voir l'écart signalé en §11 : ce sont des données
  globales (non liées à une entreprise), donc l'accès n'expose rien de sensible, mais leur
  présence dans ce groupe d'autorisation contredit la doctrine "zéro accès métier".

### Navigation

`NavMenu.razor:7-55` affiche pour SuperAdmin un groupe "Plateforme" entièrement séparé
(Entreprises, Demandes d'Abonnement, Utilisateurs, Rôles & Permissions, Journal d'Activité, Clés
d'API, Webhooks) — **aucune section métier n'est jamais rendue pour ce rôle** (commentaire
`NavMenu.razor:9-12`). `MainLayout.razor:37-47` affiche un badge de rôle distinct
(`"SuperAdmin" => "bg-dark"`).

---

## 4. Modèle de données

Toutes les entités sont dans `Web_GestCom.Core/Data/Models/`.

| Entité | Table | Portée | Notes |
|---|---|---|---|
| `Company` | `company` | C'est le tenant lui-même | `Id`, `Name`, `Slug`, `Plan` (Standard/Pro/Enterprise), `MaxAdmins`/`MaxManagers`/`MaxEmployes` (nullable = illimité). Pas de champ `Active` — pas de désactivation possible, seulement suppression. |
| `Utilisateur` | `utilisateurs` | `ITenantOwned` | `CompanyId` nullable ; **`IsSuperAdmin`** (bool) est le seul marqueur du rôle — pas une valeur de la colonne `Role` (obsolète). |
| `Abonnement` | `abonnement` | Globale (volontairement pas `ITenantOwned` — un SuperAdmin doit voir les demandes de toutes les entreprises, y compris avant qu'elles n'existent) | `Statut` : EnAttente / Contactee / Essai / Active / Refusee. |
| `AppRole` | `app_role` | `CompanyId` null = rôle système global | Rôles prédéfinis : Admin, Manager, Employé, SuperAdmin. |
| `Permission` / `RolePermission` / `UserRole` | — | Globales | Code de permission = `"{Feature}.{Action}"`. |
| `ApiKey` / `Webhook` | `api_key` / `webhook` | Volontairement pas `ITenantOwned` — SuperAdmin gère toutes les lignes indépendamment de l'entreprise taguée | `CompanyId` n'est qu'une étiquette informative. |
| `JournalActivite` | `journalactivite` | Pas `ITenantOwned`, mais porte un `CompanyId` nullable utilisé côté service (§10) | |

`ITenantOwned` (`Data/Models/ITenantOwned.cs`) n'est implémenté que par `Utilisateur` et les
entités documents métier (`Client`, `Fournisseur`, `Produit`, `DevisClient`, `CommandeVente`,
`BonLivraison`, `FactureClient`, `CommandeAchat`, `BonReception`, `FactureFournisseur`).

---

## 5. Sécurité — les trois couches d'application

C'est le cœur du modèle. Les commentaires suivants sont cités **verbatim** parce qu'ils
expliquent le *pourquoi*, pas seulement le *quoi*.

### (a) Vérification de permission — lecture (page) et écriture (service)

`Auth/PermissionAuthorizationHandler.cs:25-34` :

```csharp
// Admin always grants all business permissions within their own company. SuperAdmin does
// NOT get this bypass — it's a platform-management role (companies, users, access rights)
// with no business-data access by design, and only holds the superAdminModules
// permissions (tenants/users-global/roles-global/journal-global) seeded in RolePermission.
// Falling through to HasPermissionAsync below is what actually enforces that boundary.
if (context.User.IsInRole(RoleNameMapper.Admin))
{
    context.Succeed(requirement);
    return;
}
if (await _permissionService.HasPermissionAsync(userId.Value, requirement.Permission))
    context.Succeed(requirement);
else
    context.Fail();
```

`Web_GestCom.Core/Services/ServicePermissionGuard.cs:20-26` applique la même logique côté
service, en tête de chaque méthode de mutation (`Add`/`Update`/`Delete`/clone) des 10 services
documents (`ClientService`, `FournisseurService`, `ProduitService`, `DevisClientService`,
`CommandeVenteService`, `BonLivraisonService`, `FactureClientService`, `CommandeAchatService`,
`BonReceptionService`, `FactureFournisseurService`) :

```csharp
// Admin bypasses business-permission checks within their own company. SuperAdmin does
// NOT — it's a platform-management role with no business-data write access by design
// (see PermissionAuthorizationHandler for the matching read-side boundary). A SuperAdmin
// falls through to the HasPermissionAsync check below like any other unprivileged role,
// and fails it since SuperAdmin only holds the platform-scoped permissions.
if (currentUser.IsAdmin)
    return;
```

`CompanyService`, `AbonnementService`, `ApiKeyService`, `WebhookService` et les services de
données de référence **n'appellent jamais** `ServicePermissionGuard` — leur seule protection est
l'attribut `[Authorize(Roles=...)]` de la page qui les appelle (voir l'écart en §11).

### (b) Deux filtres de requête distincts dans `AppDbContext`

`Web_GestCom.Core/Data/AppDbContext.cs:26-47` :

```csharp
/// Tenant filters for business data engage whenever there is an active execution context —
/// including for SuperAdmin. SuperAdmin is a platform-management role with zero business-data
/// access by design ...; since SuperAdmin has no CompanyId, this filter naturally excludes
/// every business row for it rather than bypassing tenant isolation.
private bool ShouldApplyTenantFilter => _executionContext?.HasActiveContext == true;

/// Governs the Utilisateur query filter only. Unlike ShouldApplyTenantFilter above, SuperAdmin
/// DOES bypass this one — it needs cross-company visibility for the "Tous les utilisateurs"
/// global dashboard (platform-management scope, not business data). ...
private bool ShouldApplyTenantFilterToAuthenticatedUsers
    => (_executionContext?.HasActiveContext == true) && CurrentIsAuthenticated && !CurrentIsSuperAdmin;
```

- `ShouldApplyTenantFilter` filtre les 10 entités documents métier (`AppDbContext.cs:220-268`),
  forme `!ShouldApplyTenantFilter || (CurrentCompanyId.HasValue && e.CompanyId == CurrentCompanyId)`.
  Pour SuperAdmin, `CurrentCompanyId` est toujours `null` → **toutes les lignes sont exclues**,
  pas un contournement : une exclusion totale.
- `ShouldApplyTenantFilterToAuthenticatedUsers` ne gouverne que le filtre sur `Utilisateur`
  (`AppDbContext.cs:270-280`) — SuperAdmin **contourne** celui-ci, pour voir les utilisateurs de
  toutes les entreprises (c'est le seul cas de contournement volontaire, bien identifié comme
  "platform scope, not business data").

### (c) `ApplyTenantOwnershipRules` — côté écriture

`AppDbContext.cs:116-160` :

```csharp
// SuperAdmin is skipped too (unlike the read-side ShouldApplyTenantFilter above): its only
// legitimate write path is managing Utilisateur rows across companies (platform-management
// scope), which requires trusting an explicit, caller-supplied CompanyId rather than
// stamping the current tenant (SuperAdmin has none). It has no service-layer path to write
// genuine business entities at all — PermissionAuthorizationHandler/ServicePermissionGuard
// reject those before SaveChanges is ever reached — so skipping the stamp/check here for
// SuperAdmin does not reopen the business-data boundary those enforce.
if (_executionContext?.HasActiveContext != true || CurrentIsSuperAdmin || !CurrentIsAuthenticated)
    return;
```

Pour un utilisateur non-SuperAdmin authentifié, chaque entité `ITenantOwned` ajoutée se voit
imposer le `CompanyId` du tenant courant, et toute écriture cross-tenant est rejetée
(`UnauthorizedAccessException`). Pour SuperAdmin, ce bloc entier est sauté — sans risque
puisque la couche (a) l'empêche déjà d'atteindre `SaveChanges` sur une entité métier.

### Permissions réellement seedées pour SuperAdmin

Codes `tenants`, `users-global`, `roles-global`, `journal-global` (× view/create/update/delete),
seedés à trois endroits identiques : `AppDbContext.cs:340` (seed EF), `Program.cs:784-799` +
`900-914` (migration SQL brute au démarrage), et explicitement retirés du rôle Employé par
défense en profondeur (`Program.cs:889-898`).

**⚠️ Point important pour toute évolution future** : ces quatre codes de permission sont seedés
et attribués, **mais ne sont vérifiés nulle part dans le code** — aucune page n'a
`[Authorize(Policy = "perm:tenants.view")]`, aucun service n'appelle
`ServicePermissionGuard.EnsureAsync(..., "tenants.view")`. Le seul verrou réel sur chaque page
SuperAdmin est l'attribut `[Authorize(Roles = "SuperAdmin")]`. Ces lignes de permission existent
dans le modèle RBAC mais ne servent à rien opérationnellement aujourd'hui — probablement prévues
pour une future UI de permissions par module plateforme.

### Cohérence du cookie d'authentification

`Program.cs:83-99` (`OnValidatePrincipal`) recalcule `expectedRole` depuis la base à chaque
requête et considère **toute présence d'un `CompanyId` dans le claim d'un SuperAdmin** comme une
altération, forçant une déconnexion :
`user.IsSuperAdmin ? claimCompanyId.HasValue : (...)`.

---

## 6. Création et gestion du compte SuperAdmin

- **Compte par défaut** (`Program.cs:944-959`), créé si aucun `Utilisateur.IsSuperAdmin` n'existe
  encore : identifiants **`superadmin` / `SuperAdmin123!`**. Distinct du compte Admin métier par
  défaut (`admin` / `admin123`, `Program.cs:922-941`, rattaché à une entreprise "Entreprise par
  défaut").
- **Avertissement au démarrage** (`Program.cs:961-976`) si l'un de ces deux mots de passe par
  défaut fonctionne encore — journalisé en `Warning`, ne bloque jamais le démarrage.
- **Aucune interface ne permet de créer un second SuperAdmin.** `UtilisateurForm.razor:54-58`
  (le seul formulaire d'ajout/édition d'utilisateur, partagé Admin/SuperAdmin) n'offre que
  Employé/Manager/Admin dans la liste des rôles — pas d'option SuperAdmin, pas de case à cocher
  `IsSuperAdmin` nulle part dans l'UI. Le seul moyen d'en créer un second est une modification
  directe en base (`is_superadmin_utilisateur = 1`) ou un ajout de code de seed comme celui
  ci-dessus. `UtilisateursGlobalList.razor:117-119` a même un commentaire défensif anticipant ce
  cas (un SuperAdmin avec un `CompanyId` non nul suite à une modification manuelle en base).
- Mots de passe en bcrypt (`UtilisateurService.HashPassword`, préfixe `v2$bcrypt$`), avec
  migration automatique et transparente depuis d'anciens formats au prochain login réussi.

---

## 7. Workflow des demandes d'abonnement (cycle complet)

1. **Page publique** (`Home.razor:361-443`, section `#tarifs`) — 3 plans affichés (Standard
   390 DT/an, Pro 690 DT/an, Enterprise sur devis), chaque CTA renvoie vers
   `/demande-abonnement?plan={Plan}`. Le bandeau explique le process :
   > "1. Vous choisissez un plan · 2. Nous vous contactons sous 24-48h pour activer votre compte
   > · 3. Standard reste gratuit (14 jours pour prendre en main l'outil) ; Pro et Enterprise sont
   > facturés dès l'activation (virement bancaire pour l'instant)"

   Cette distinction "essai gratuit Standard / paiement immédiat Pro-Enterprise" est **du texte
   marketing et une consigne pour le SuperAdmin**, pas une branche de code appliquée
   automatiquement.
2. **Formulaire public** `Pages/Compte/DemandeAbonnement.cshtml.cs` (anonyme) → crée un
   `Abonnement` via `AbonnementService.CreateDemandeAsync`.
3. **`CreateDemandeAsync`** (`AbonnementService.cs:37-51`) force `Statut = "EnAttente"`, sauvegarde,
   puis envoie deux emails (`NotifyNewDemandeAsync:53-86`) via `IEmailTransport` : confirmation au
   demandeur, notification à `EmailOptions.AdminNotificationEmail`. Un échec d'envoi est
   journalisé et avalé (jamais remonté au demandeur).
4. **Traitement SuperAdmin** sur `/admin/demandes-abonnement` (`AbonnementsList.razor`) : la
   modale "Traiter" permet de changer `Statut` (EnAttente/Contactee/Essai/Active/Refusee), de
   rattacher un `CompanyId` existant (créer l'entreprise d'abord via "Entreprises" si besoin), de
   fixer `DateDebut`/`DateEcheance` (raccourci "14 jours" `ApplyTrialDates:215-219`), et d'ajouter
   des notes internes.
5. **`SaveAsync`** (`AbonnementsList.razor:202-213`) horodate `DateTraitement` et appelle
   `AbonnementService.UpdateAsync` — **aucun email n'est envoyé au changement de statut**, seule
   la création initiale déclenche un email.
6. **Aucune automatisation de fin d'essai, aucun paiement en ligne réel** — bandeau explicite sur
   la page (`AbonnementsList.razor:13-19`) : rien ne désactive automatiquement un essai arrivé à
   échéance, à surveiller manuellement.
7. **Email (Brevo)** : `IEmailTransport`/`BrevoEmailTransport` (POST vers l'API Brevo, clé jamais
   loguée) ; `NoOpEmailTransport` en repli si aucune clé n'est configurée (dev par défaut).

---

## 8. Gestion des entreprises (tenants)

- CRUD complet via `/admin/entreprises` (`CompaniesList.razor`).
- **Création** : crée aussi l'Admin initial de l'entreprise dans la même modale — **sans
  transaction** entre les deux écritures (voir §3, risque d'entreprise orpheline en cas d'échec
  partiel).
- **Plan** (Standard/Pro/Enterprise) → `CompanyService.ApplyDefaultQuotasIfUnset` dérive les
  quotas par défaut : Pro/Enterprise → 2 Admins / 3 Managers / 10 Employés ; Standard → 1/1/2.
  Modifiables ensuite dans la modale d'édition.
- **Quotas** (`MaxAdmins`/`MaxManagers`/`MaxEmployes`, `null` = illimité) : appliqués dans
  `UtilisateurService.EnsureRoleQuotaNotExceededAsync` — un Admin de l'entreprise ne peut pas
  s'auto-servir au-delà du quota ; **seul SuperAdmin peut créer des comptes au-delà**, et
  SuperAdmin n'est lui-même jamais soumis à un quota (`if (currentUser?.IsSuperAdmin == true)
  return;`, `UtilisateurService.cs:345-346`).
- **Pas de désactivation d'entreprise** : `Company` n'a pas de champ `Active`. Seule la
  suppression existe comme action de fin de vie.
- **Pas de fonction "se connecter en tant que"** — confirmé par recherche exhaustive dans le
  dépôt. SuperAdmin ne peut pas prendre la session d'un utilisateur d'une entreprise cliente.

---

## 9. Gestion globale des utilisateurs

`UtilisateursGlobalList.razor` (`/admin/utilisateurs-global`, SuperAdmin) vs
`UtilisateursList.razor` (`/admin/utilisateurs`, Admin+SuperAdmin) :

| | Global (SuperAdmin) | Par entreprise (Admin) |
|---|---|---|
| Colonne Entreprise | Oui — badge "Compte Système" pour les lignes `IsSuperAdmin`, sinon `Company.Name` | Non (implicite : toujours la sienne) |
| Filtre par entreprise | Oui, + case "Compte(s) Système" | Non |
| Activer/Désactiver | **Non** (absent de cet écran) | Oui, boutons inline |
| Nouvel Utilisateur | Lien vers le même `UtilisateurForm` | Idem |

`UtilisateurForm.razor:61-76` : pour SuperAdmin, le champ Entreprise devient un menu déroulant sur
toutes les `Company` (obligatoire, `SaveAsync:164-165`) ; pour Admin, un champ désactivé montrant
sa propre entreprise. Le sélecteur de rôle n'offre jamais SuperAdmin (§6).

**Point à noter pour une évolution future** : `UtilisateursList.razor` (la liste "par entreprise")
est routable par SuperAdmin aussi (`Roles="Admin,SuperAdmin"`), et
`UtilisateurService.GetAllAsync` ne filtre que si `CurrentCompanyId` est renseigné — donc un
SuperAdmin qui naviguerait directement vers `/admin/utilisateurs` verrait la liste globale sur
cette route aussi. La distinction Admin/SuperAdmin sur ce point n'est assurée que par le lien que
chaque `NavMenu` affiche, pas par la route ou la requête sous-jacente.

---

## 10. Journal d'activité

Un seul écran partagé (`JournalActiviteList.razor`, `Roles="Admin,SuperAdmin"`), pas de vue
SuperAdmin séparée. Le filtrage par entreprise se fait dans le service, pas dans la page :

`Web_GestCom.Core/Services/JournalActiviteService.cs` — `GetAllAsync` :

```csharp
if (tenantService?.CurrentCompanyId is int companyId)
    q = q.Where(j => j.CompanyId == companyId);
```

Pour un Admin, `CurrentCompanyId` est toujours renseigné → il ne voit que le journal de sa
propre entreprise. Pour SuperAdmin, `CurrentCompanyId` est `null` → **la condition ne s'applique
pas et le résultat est global, non filtré, toutes entreprises confondues** — comportement voulu,
cohérent avec le rôle de plateforme. `PurgeAsync` suit la même logique (purge globale pour
SuperAdmin, limitée à l'entreprise courante pour Admin).

Comme signalé en §5, le code de permission `journal-global` existe mais n'est jamais vérifié — le
seul verrou est `[Authorize(Roles = "Admin,SuperAdmin")]`.

---

## 11. Écarts connus entre la doctrine et le code (à garder en tête pour toute évolution)

Ces points sont réels mais n'ont pas d'impact de sécurité pratique aujourd'hui — les entités
concernées sont globales, pas des données métier. Ils sont documentés ici pour éviter qu'une
session future les prenne pour des bugs, ou au contraire les ignore si le périmètre du modèle
évoluait :

1. **Six écrans de données de référence** (`Entreprise/EntrepriseForm.razor`,
   `Parametres/CategorieProduitList.razor`, `DeviseList.razor`, `FabriquantProduitList.razor`,
   `ModePayementList.razor`, `TvaProduitList.razor`, `UniteProduitList.razor`) sont
   `Roles="Admin,SuperAdmin"` alors que la doctrine affirme "zéro accès métier" pour SuperAdmin.
   Sans conséquence réelle (données globales, non liées à une entreprise), mais incohérent avec
   le discours du reste du code.
2. **Les permissions `tenants`/`users-global`/`roles-global`/`journal-global` sont seedées mais
   jamais vérifiées.** Le contrôle d'accès réel de chaque page SuperAdmin repose entièrement sur
   l'attribut `[Authorize(Roles=...)]`, pas sur ces permissions fines.
3. **`CompanyService`, `AbonnementService`, `ApiKeyService`, `WebhookService` n'appellent jamais
   `ServicePermissionGuard`** — cohérent avec le point 2, mais à savoir si une évolution voulait un
   jour brancher un contrôle plus fin.
4. **Création d'entreprise non transactionnelle** (§8) — risque d'entreprise orpheline en cas
   d'échec partiel entre la création de la `Company` et celle de son premier Admin.
5. **Pas de désactivation de compte SuperAdmin depuis l'UI** — l'écran global n'a pas de bouton
   Activer/Désactiver (§9). Un compte SuperAdmin ne peut être désactivé qu'en base, ou en passant
   par l'écran par-entreprise si on y accède directement (voir le point de navigation en §9).
6. **Deux comptes plateforme distincts avec mots de passe par défaut connus** (`admin/admin123`,
   `superadmin/SuperAdmin123!`) — l'avertissement au démarrage n'est que journalisé, jamais
   bloquant. À changer avant toute mise en production réelle (déjà signalé dans l'audit externe
   du 12/09, voir `GestCommercial/audit_gestcommercial.md`).

---

## 12. Fichiers clés (annexe)

| Sujet | Fichier |
|---|---|
| Pages SuperAdmin/Admin | `Components/Pages/Admin/*.razor` |
| Sécurité — lecture | `Auth/PermissionAuthorizationHandler.cs`, `Auth/PermissionClaimsTransformation.cs`, `Auth/PermissionPolicyProvider.cs` |
| Sécurité — écriture | `Web_GestCom.Core/Services/ServicePermissionGuard.cs` |
| Filtres tenant / ownership | `Web_GestCom.Core/Data/AppDbContext.cs` |
| Comptes/quotas/rôles | `Web_GestCom.Core/Services/UtilisateurService.cs` |
| Entreprises | `Web_GestCom.Core/Services/CompanyService.cs` |
| Abonnements | `Web_GestCom.Core/Services/AbonnementService.cs` |
| Email | `Web_GestCom.Core/Services/{IEmailTransport,BrevoEmailTransport,NoOpEmailTransport}.cs` |
| Journal | `Web_GestCom.Core/Services/JournalActiviteService.cs` |
| Seed / migrations au démarrage | `Program.cs` (blocs `ExecuteSqlRaw`, seed `Utilisateur`/`AppRole`/`Permission`) |
| Tests de la frontière SuperAdmin | `Web_GestCom.Tests/Data/AppDbContextTenantIsolationTests.cs`, `Web_GestCom.Tests/Services/ServicePermissionGuardTests.cs`, `Web_GestCom.Tests/Services/UtilisateurServiceTests.cs`, `Web_GestCom.Tests/Components/Pages/HomeTests.cs` |

**Non vérifié dans cette spec** (à lire avant de s'appuyer dessus) : l'existence éventuelle d'un
CRUD pour `FeatureFlag` (l'entité existe dans `AppDbContext`, aucune page ne l'a été trouvée lors
de cette recherche — peut avoir été ajoutée depuis).
