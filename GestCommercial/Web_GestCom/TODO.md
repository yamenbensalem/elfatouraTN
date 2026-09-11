# TODO – GestCom Web

Fonctionnalités restantes à implémenter, classées par priorité.

---

## PRIORITÉ HAUTE

### Module ACHATS — Services + Pages ✅ TERMINÉ

#### Commandes Achat ✅

- [x] Créer `Services/CommandeAchatService.cs` (interface `ICommandeAchatService`)
- [x] Créer `Components/Pages/CommandesAchat/CommandeAchatList.razor`
- [x] Créer `Components/Pages/CommandesAchat/CommandeAchatForm.razor`
- [x] Enregistrer `ICommandeAchatService` dans `Program.cs`

#### Bons de Réception ✅

- [x] Créer `Services/BonReceptionService.cs` (interface `IBonReceptionService`)
  - Incrémente le stock à la création, restitue à la suppression/modification
- [x] Créer `Components/Pages/BonsReception/BonReceptionList.razor`
- [x] Créer `Components/Pages/BonsReception/BonReceptionForm.razor`
  - Commande Achat liée filtrée par fournisseur
- [x] Enregistrer `IBonReceptionService` dans `Program.cs`

#### Factures Fournisseur ✅

- [x] Créer `Services/FactureFournisseurService.cs` (interface `IFactureFournisseurService`)
  - `AddReglementAsync`, `GetSoldeAsync`, `CloneAsync`
  - Incrémente le stock à la création
- [x] Créer `Components/Pages/FacturesFournisseur/FactureFournisseurList.razor` (`@page "/factures-fournisseur"`)
  - Colonnes : N°, Date, Fournisseur, Montant HT, TVA, TTC, Timbre, État, Règlement — déjà en place
- [x] Créer `Components/Pages/FacturesFournisseur/FactureFournisseurForm.razor`
  - Routes `/factures-fournisseur/nouveau` et `/factures-fournisseur/{Numero}`, section règlements,
    bouton Cloner, page d'impression (`Print/PrintFactureFournisseur.razor`) — déjà en place
- [x] Enregistrer `IFactureFournisseurService` — déjà fait via `AddGestComServices` (Core)
  - Cette checklist était périmée : le module était déjà entièrement implémenté (liste, formulaire,
    lignes avec recalcul auto, règlements avec calcul d'état, clonage, impression, permissions
    `factures-fournisseur.*` seedées, lien menu). Revérifié de bout en bout dans le navigateur :
    création, ajout de ligne (auto-remplissage prix/TVA depuis le produit), incrément de stock,
    ajout d'un règlement partiel (état → "Partiellement Réglé", solde correct), impression, clonage
    (nouveau numéro, stock re-incrémenté, règlement réinitialisé), suppression (stock restitué,
    règlements supprimés). Aucun bug trouvé, aucun changement de code nécessaire.

---

## PRIORITÉ MOYENNE

### Paramètres — Fiche Entreprise ✅

- [x] Créer `Components/Pages/Entreprise/EntrepriseForm.razor` (`@page "/entreprise"`)
  - Formulaire création/modification unique, directement via AppDbContext

### Fournisseurs — Formulaire dédié ✅

- [x] Créer `Components/Pages/Fournisseurs/FournisseurForm.razor`
  - Routes : `/fournisseurs/nouveau` et `/fournisseurs/{Code}`

### Amélioration Factures / Avoirs ✅

- [x] Ajouter la possibilité de **créer un Avoir à partir d'une Facture** existante (bouton "Générer Avoir" dans `FactureForm`)
      — bouton visible uniquement sur une facture existante non-avoir, appelle
      `FactureService.CloneAsync(Numero, isAvoir: true)` (déjà supporté par le service) et redirige
      vers `/avoirs/{nouveauNumero}`
- [x] Ajouter la **suppression de règlements** individuels depuis `FactureForm`
      — nouvelle méthode `IFactureClientService.DeleteReglementAsync(int reglementId)` (Core),
      recalcule `EtatReglement` après suppression ; bouton + `ConfirmDialog` dédié sur chaque ligne
      de règlement. 2 nouveaux tests service (`FactureClientServiceTests`)
- [x] Afficher **l'état règlement** (`EtatReglement`) sur la fiche facture (badge coloré)
      — badge à côté du titre, réutilise le même mapping couleur que `FacturesList`/`FactureFournisseurList`
      (Réglé=vert, Partiellement Réglé=orange, Non Réglé=rouge)
      - Bug trouvé et corrigé en passant : le total "Net à Payer" (et 2 lignes "Remise") de
        `FactureForm`, `BonLivraisonForm`, `CommandeVenteForm`, `DevisForm` et 3 pages `Print/*`
        affichaient le texte littéral `.ToString("0.###")` au lieu du montant formaté —
        `@(expr).ToString(...)` n'est pas équivalent à `@((expr).ToString(...))` en Razor (le
        `.ToString()` sortait de l'expression C# et était rendu comme texte HTML). Corrigé aux
        8 endroits concernés (grep `@(.*)\.ToString(` sans double parenthèse).
      Vérifié de bout en bout dans le navigateur : badge Non Réglé → Réglé après paiement complet,
      suppression de règlement ramène le badge à Non Réglé, Générer Avoir crée bien un avoir dans
      `/avoirs` avec les mêmes lignes. Suite complète : 239/239 tests verts, build 0 erreur.

### Amélioration Bons de Livraison ✅

- [x] Ajouter la possibilité de **facturer un BL** directement (bouton "Générer Facture" dans `BonLivraisonForm`)
- [x] Lier un BL à une facture existante (mettre à jour `EtatFacture` du BL)
      — `IFactureClientService.CreateFromBonLivraisonAsync(numeroBonLivraison, config)` (Core) copie
      les lignes du BL vers une nouvelle facture (FODEC repris du produit, absent sur les lignes de
      BL), sans re-décrémenter le stock (le BL l'a déjà fait à sa création — voir commentaire dans
      le code), et met `BonLivraison.EtatFacture = "Facturé"`. Rejette si déjà facturé. Bouton
      masqué une fois le BL facturé. **Limite connue documentée dans le code** : `DeleteAsync`
      d'une facture générée ainsi restituerait le stock à tort (elle ne l'a jamais décrémenté
      elle-même) — corriger ça proprement demanderait un champ de provenance sur `FactureClient` +
      migration de schéma, hors scope ici.

### Amélioration Commandes Vente ✅

- [x] Ajouter la possibilité de **créer un BL depuis une commande** (bouton "Générer BL" dans `CommandeVenteForm`)
      — `IBonLivraisonService.CreateFromCommandeVenteAsync(numeroCommandeVente)` (Core) copie les
      lignes de la commande vers un nouveau BL (numérotation/décrément de stock/journal via le
      `CreateAsync` existant, donc comportement de stock identique à un BL normal), et met
      `CommandeVente.EtatLivraison = "Livré"`. Bouton masqué une fois la commande livrée.
      6 nouveaux tests service (`BonLivraisonServiceTests`, `FactureClientServiceTests`). Vérifié
      de bout en bout dans le navigateur : Commande → Générer BL → BL lié affiché dans le
      formulaire (dropdown "Commande Vente") → Générer Facture → Facture créée sans double
      décrément de stock → BL passe à "Facturé" → bouton disparaît. Suite complète : 246/246 tests
      verts, build 0 erreur (Web + Desktop).

---

## PRIORITÉ BASSE

### Tableau de bord — Home.razor ✅

- [x] Implémenter un vrai tableau de bord avec les métriques clés :
  - Chiffre d'affaires du mois (somme des factures TTC) — nouvelle carte KPI "CA du Mois", net des
    avoirs du mois (une facture normale ajoute au CA, un avoir du même mois le réduit)
  - Factures non réglées (montant total) — nouvelle carte KPI "Montant Impayé" (TTC + timbre -
    règlements déjà reçus, sur les factures non réglées) ; la carte "Factures en attente" (count)
    existante est conservée telle quelle, les deux se complètent
  - Produits en alerte de stock (count) — déjà en place (tableau d'alerte existant)
  - Dernières factures (5 dernières) — déjà en place (bloc "Activité Récente" existant)
  - Graphique CA mensuel (6 derniers mois) — barres CSS (pas de nouvelle dépendance JS type
    Chart.js), hauteur proportionnelle au mois le plus élevé, libellés de mois en français
    (`CultureInfo("fr-FR")`, ex. "Sept. 26")
  Nécessitait un second appel `FactureService.GetAllAsync(avoirsOnly: true)` pour calculer le CA
  net des avoirs — a cassé 7 tests bUnit existants (`HomeTests`) qui ne stubaient que
  `GetAllAsync(false, null)` ; corrigé en ajoutant le stub par défaut manquant. 3 nouveaux tests
  bUnit pour les nouvelles cartes (montant impayé net des règlements, CA du mois qui exclut les
  autres mois, CA du mois qui soustrait bien les avoirs du même mois). Vérifié de bout en bout
  dans le navigateur avec une vraie facture. Suite complète : 259/259 tests verts, build 0 erreur
  (Web + Desktop).

### Recherche et filtres avancés ✅

- [x] Ajouter filtre par **période** (date début / date fin) sur toutes les listes de documents
- [x] Ajouter filtre par **état** sur les listes (Devis, Commandes, BL, Factures)
- [x] Ajouter filtre par **client** sur les listes VENTES
- [x] Ajouter filtre par **fournisseur** sur les listes ACHATS
      — appliqué aux 7 listes de documents (`DevisList`, `CommandeVenteList`, `BonLivraisonList`,
      `FacturesList` [factures + avoirs], `CommandeAchatList`, `BonReceptionList`,
      `FactureFournisseurList`). Filtrage **côté client** (LINQ en mémoire sur la liste déjà
      chargée par `GetAllAsync()`), pas de changement de service ni de requête — cohérent avec le
      volume de données d'une PME et évite de faire diverger 7 signatures de service. Barre de
      filtre uniforme (client/fournisseur déduit des lignes déjà chargées, état = mêmes libellés
      que le formulaire d'édition, période, bouton Réinitialiser) reprenant le style déjà utilisé
      dans `JournalActiviteList`. `FacturesList` réinitialise les filtres au changement de route
      (`/factures-client` ↔ `/avoirs`, deux listes distinctes sur la même page). Aucun test service
      nécessaire (aucune logique métier touchée) ; vérifié dans le navigateur (filtre état,
      période, réinitialisation) sur `DevisList` et rendu confirmé sur les 6 autres pages,
      y compris celles basées sur `Virtualize` (`BonLivraisonList`, `FacturesList`). Suite complète
      inchangée : 259/259 tests verts, build 0 erreur (Web + Desktop).

### Impression / Export

- [ ] Implémenter l'impression des documents (Devis, Facture, BL) au format PDF
  - Utiliser une bibliothèque comme QuestPDF ou DinkToPdf
  - En-tête avec logo + informations entreprise
  - Corps avec tableau de lignes
  - Pied de page avec totaux et signature
- [ ] Export Excel des listes (Clients, Produits, Factures)

### Authentification ✅ TERMINÉ (v0.5.0)

- [x] Page de connexion `/compte/connexion`
- [x] Gestion des rôles (Admin, Utilisateur)
- [x] Protection des routes (`AuthorizeRouteView`)
- [x] Journal d'activité automatique sur les 7 services prioritaires
- [x] Gestion des utilisateurs (Admin)

### À compléter — Journal d'activité ✅

- [x] Ajouter la traçabilité sur les services restants :
  - `DevisClientService`, `CommandeVenteService`, `CommandeAchatService` — `IJournalActiviteService`
    injecté, Ajout/Modification/Suppression/Clone journalisés (même convention que les autres
    services document). 3 nouveaux tests service qui vérifient qu'une vraie entrée est écrite
    (pas juste que le mock est appelé).
- [x] Ajouter l'entrée "Connexion" dans le journal depuis la page login
      — **bug trouvé et corrigé en le faisant** : `journalActiviteService.EnregistrerAsync(...)`
      appelé juste après `SignInAsync()` levait silencieusement `UnauthorizedAccessException:
      Aucun tenant actif dans le contexte de sécurité` (`AppDbContext.ApplyTenantOwnershipRules`),
      car `SignInAsync()` pose le cookie pour la *prochaine* requête mais ne met pas à jour
      `HttpContext.User` pour le reste de la requête courante — le contexte d'exécution voyait
      donc encore un utilisateur non authentifié. Corrigé en réaffectant explicitement
      `HttpContext.User = principal` juste après `SignInAsync()`, avant l'écriture du journal.
      **Ce même problème affecte aussi silencieusement `EmitSecurityAlertsAsync`** (les alertes
      "AlerteSecurite" sur tentatives de connexion échouées/bloquées, qui s'exécutent avant toute
      authentification et n'ont donc jamais de tenant résolu) — probablement cassé depuis
      l'introduction du guard multi-tenant, **pas corrigé ici** : la correction est plus délicate
      (pas de principal à assigner puisque l'auth a échoué ; il faudrait résoudre le tenant
      autrement, ex. via le login tenté plutôt que via les claims) et touche un guard de sécurité,
      donc hors scope de ce point précis. À reprendre séparément.
- [x] Permettre à l'administrateur de **purger** le journal (ancien de N mois)
      — `IJournalActiviteService.PurgeAsync(int olderThanMonths)` (Core), respecte l'isolation
      multi-tenant, écrit une entrée "Purge" après coup (hors de la sélection supprimée) pour
      garder une trace de qui a purgé et combien d'entrées. UI dans `JournalActiviteList.razor` :
      champ nombre de mois + bouton + `ConfirmDialog` (action irréversible). 4 nouveaux tests
      service. Vérifié de bout en bout dans le navigateur (bouton, dialogue de confirmation,
      rafraîchissement de la liste après purge — "Aucune entrée à purger" correctement renvoyé
      quand rien ne dépasse le seuil).
      Suite complète : 256/256 tests verts, build 0 erreur (Web + Desktop).

### Retenues à la source ✅

- [x] Intégrer le calcul de la retenue à la source (`TauxRetenue` depuis `AppConfigService`)
      — décision utilisateur : symétrique sur `FactureClient` ET `FactureFournisseur`, même
      traitement que le Timbre Fiscal. Nouvelle colonne `MontantRetenue` sur les deux entités
      (migration SQL idempotente dans `Program.cs`), calculée sur le HT (`MontantHT × TauxRetenue /
      100`), verrouillée à la création/modification comme les autres montants — jamais recalculée
      rétroactivement si le taux change en config après coup. `AppConfigService` injecté au
      constructeur des deux services (déjà singleton, injection sans risque de cycle de vie).
      **Non déduite automatiquement du solde/règlement** (reste informative) : appliquer une
      déduction automatique aurait supposé que TOUTES les factures sont soumises à la retenue, ce
      qui n'est pas garanti — décision conservatrice pour ne pas fausser le suivi des règlements
      existant.
      - Bug trouvé et corrigé en le faisant : `FactureClientService.UpdateAsync` copiait tels quels
        les `MontantHT/MontantTVA/MontantTTC` reçus en paramètre au lieu de les recalculer depuis
        les nouvelles lignes — et `FactureForm.razor` ne les réassigne jamais avant l'appel.
        Modifier les lignes d'une facture client existante persistait donc silencieusement les
        **anciens totaux**. Corrigé en appelant `RecalculateTotals` côté service (comme le fait
        déjà `FactureFournisseurService.UpdateAsync`), sans toucher au Timbre (verrouillé, extrait
        de `RecalculateTotals` vers les points d'appel qui doivent le fixer explicitement).
  - [x] Afficher le montant retenu sur les factures — carte totaux de `FactureForm.razor` et
        `FactureFournisseurForm.razor` (recalcul live à chaque ligne modifiée) + pages d'impression
        `PrintFactureClient.razor`/`PrintFactureFournisseur.razor` (valeur persistée), avec une
        ligne "net à recevoir/à verser" une fois la retenue déduite.
  - [x] Générer les déclarations de retenue — **décision utilisateur** : récapitulatif interne
        (`/rapports/retenues`, `RetenueRecap.razor`), PAS un document officiel — bannière
        d'avertissement explicite, car je ne peux pas garantir la conformité au formulaire exact
        exigé par l'administration fiscale tunisienne. Liste Ventes + Achats sur une période
        filtrable (dates, type), total, imprimable.
      8 nouveaux tests service (calcul, recalcul sur update, copie au clonage, + régression sur le
      bug de totaux). Vérifié de bout en bout dans le navigateur : calcul live, persistance après
      modification de lignes (totaux ET retenue recalculés correctement), impression, filtre du
      récapitulatif par type. Suite complète : 267/267 tests verts, build 0 erreur (Web + Desktop).

### Gestion multi-entreprises ✅

- [x] Support de plusieurs entreprises — **décision utilisateur** : gestion des entreprises
      côté SuperAdmin uniquement, PAS de bascule d'entreprise pour l'utilisateur normal
      (`Utilisateur.CompanyId` reste un FK fixe unique — pas d'appartenance multi-entreprises,
      ça aurait demandé un changement d'architecture plus profond). Réutilise le module de
      permissions "tenants" déjà seedé mais jusqu'ici inutilisé.
      - Nouveau `Web_GestCom.Core/Services/CompanyService.cs` (`ICompanyService`) : CRUD sur
        `Company`, `GetAllAsync` avec `Include(c => c.Utilisateurs)` pour afficher le nombre
        d'utilisateurs par entreprise dans la liste.
      - Nouvelle page `Components/Pages/Admin/CompaniesList.razor` (`/admin/entreprises`),
        `[Authorize(Roles = "SuperAdmin")]` (strictement SuperAdmin, pas Admin) : liste + modal
        Add/Edit (Nom, Slug, Plan) + `ConfirmDialog` de suppression, erreurs FK via
        `DeleteErrorMessageHelper`. Lien ajouté dans la section Administration du menu,
        visible uniquement si `CurrentUserService.IsSuperAdmin`.
      - Même bug de tracking EF Core que les pages de données de référence (voir section
        ci-dessous) : extrait dans une extension partagée `AppDbContextSaveExtensions.
        DetachStaleTrackedEntry<T>()`, réutilisée par `ReferenceDataService<T>` ET
        `CompanyService`.
      - **Bug critique trouvé et corrigé en le testant** : un utilisateur authentifié mais sans
        le rôle requis (ex. Admin visitant une page SuperAdmin, ou n'importe quel utilisateur
        visitant n'importe quelle page `[Authorize(Roles=...)]` sans le bon rôle) tombait dans
        une **boucle de redirection infinie** (`ERR_TOO_MANY_REDIRECTS`) — reproduit aussi sur
        `/admin/utilisateurs`, une page existante sans rapport avec ce chantier, donc bug
        préexistant jamais détecté faute d'avoir testé un scénario "authentifié mais rôle
        insuffisant" (tous les tests précédents utilisaient le compte `admin`, qui satisfait
        toujours "Admin,SuperAdmin"). Cause réelle : `Program.cs` configurait
        `options.AccessDeniedPath = "/compte/connexion"` (même page que `LoginPath`) — quand
        l'autorisation ASP.NET Core au niveau middleware refuse l'accès (Forbidden, utilisateur
        authentifié mais rôle insuffisant), le handler de cookie redirige vers
        `AccessDeniedPath` avec `?ReturnUrl=...` ; mais `Connexion.cshtml.cs.OnGet` renvoie
        immédiatement tout utilisateur déjà authentifié vers ce même `ReturnUrl` → boucle
        infinie entre les deux pages. Corrigé en créant une page dédiée
        `Pages/Compte/AccesRefuse.cshtml` (`/compte/acces-refuse`, message "Accès refusé" +
        lien retour tableau de bord, sans redirection) et en pointant `AccessDeniedPath` vers
        celle-ci au lieu de `LoginPath`. En passant, corrigé aussi `Components/Routes.razor` :
        le `<NotAuthorized>` de `AuthorizeRouteView` utilisait un `<AuthorizeView>` imbriqué
        pour distinguer authentifié/anonyme, mais celui-ci pouvait mal classer un utilisateur
        authentifié comme anonyme (root cause exacte non confirmée avec certitude — plausible :
        double enregistrement de l'état d'authentification en cascade entre l'ancien
        `<CascadingAuthenticationState>` de `App.razor` et le `services.
        AddCascadingAuthenticationState()` déjà enregistré dans `Program.cs`, retiré du wrapper
        de `App.razor` par précaution) ; utilise maintenant directement l'`AuthenticationState`
        déjà résolu passé en paramètre `Context` du `NotAuthorized`, plus fiable qu'une
        réévaluation via un `AuthorizeView` imbriqué.
      12 nouveaux tests service (`CompanyServiceTests` : CRUD, régression de tracking, métadonnées
      FK Restrict). Vérifié de bout en bout dans le navigateur : promotion temporaire d'un
      utilisateur en SuperAdmin (via SQL direct, restauré après coup) pour tester CRUD complet
      (créer/modifier/supprimer une entreprise) ; reproduction de la boucle infinie AVANT le fix
      (sur `/admin/entreprises` ET sur `/admin/utilisateurs` pour prouver que ce n'était pas
      spécifique à cette page) ; re-test après le fix confirmant l'affichage correct de la page
      "Accès refusé" sans boucle, pour un Admin normal visitant la page SuperAdmin. Suite
      complète : 284/284 tests verts, build 0 erreur.

### Multi-entreprises — assignation fiable + vue globale SuperAdmin ✅

- [x] Fiabiliser l'assignation d'entreprise à la création d'un utilisateur, et donner au
      SuperAdmin une vue transversale de tous les utilisateurs — **demande utilisateur** faisant
      suite au constat : "on n'a pas défini d'entreprise pour Manager/Employé, et le SuperAdmin
      n'a pas de vue globale sur tous les utilisateurs et leur entreprise". Diagnostic confirmé
      dans le code avant de coder quoi que ce soit (deux problèmes distincts, pas un) :
      1. `UtilisateurService.EnsureTenantDefaults` assignait déjà silencieusement l'entreprise de
         l'Admin courant à un nouvel utilisateur, MAIS se rabattait sur "la première entreprise de
         la table" si le tenant courant ne pouvait pas être résolu — un utilisateur pouvait donc
         être assigné à la mauvaise entreprise sans aucune alerte. `Utilisateur` n'avait en plus
         aucun filet de sécurité au niveau base de données (pas de `HasQueryFilter`, contrairement
         à Client/Produit/Facture), et `UtilisateurForm.razor` n'affichait jamais l'entreprise
         assignée.
      2. Aucune vue "tous les utilisateurs + leur entreprise" n'existait pour le SuperAdmin
         (`/admin/entreprises` n'affiche qu'un compteur, sans détail).
      - **Correctifs sécurité (partie 1)** :
        - `EnsureTenantDefaults` (renommé, n'est plus `async` — plus d'`await` restant après le
          retrait du repli) lève désormais `InvalidOperationException` au lieu de deviner une
          entreprise, si aucune n'est explicitement fournie et qu'aucun tenant actif n'est
          disponible.
        - `Utilisateur` implémente maintenant `ITenantOwned`, avec un `HasQueryFilter` dédié dans
          `AppDbContext` (même motif que Client/Produit/etc.) — défense en profondeur qui comble
          aussi une lacune que le filtrage manuel de `UtilisateurService` ne couvrait pas :
          `FindAsync` (utilisé par `ActiverAsync`/`DesactiverAsync`/`ChangePasswordAsync`)
          contourne les query filters par conception, mais `ApplyTenantOwnershipRules` rejette
          quand même la sauvegarde si l'entité chargée appartient à une autre entreprise.
        - **Piège trouvé et corrigé avant qu'il ne casse la connexion** : `Utilisateur` est la
          seule entité interrogée AVANT authentification (`AuthentifierAsync`, au login). Un
          `HasQueryFilter`/`ApplyTenantOwnershipRules` naïf aurait exclu toutes les lignes (aucun
          tenant résolu pour une requête anonyme) et cassé la connexion pour tout le monde — pire,
          `AuthentifierAsync` peut aussi déclencher une sauvegarde AVANT authentification (ré-hachage
          d'un ancien mot de passe), qui aurait levé "Aucun tenant actif" et cassé cette migration
          de hash silencieusement. Ajouté `IExecutionContext.IsAuthenticated` (implémenté dans
          `HttpExecutionContext`, `BackgroundExecutionContext`, et `DesktopExecutionContext` côté
          desktop) ; le filtre et la règle d'écriture pour `Utilisateur` exigent maintenant
          explicitement une requête authentifiée, pas seulement "une requête HTTP existe".
        - `UtilisateurForm.razor` affiche désormais l'entreprise : champ texte **lecture seule**
          (nom de sa propre entreprise) pour un Admin normal, **liste déroulante modifiable**
          (`ICompanyService.GetAllAsync`) pour le SuperAdmin, avec validation côté page si laissée
          vide. Nouveau `ICompanyService.GetByIdAsync`.
      - **Nouvelle vue globale (partie 2)** : `Components/Pages/Admin/UtilisateursGlobalList.razor`
        (`/admin/utilisateurs-global`), `[Authorize(Roles = "SuperAdmin")]` — liste tous les
        utilisateurs toutes entreprises confondues (Login, Nom, Email, **Entreprise**, Rôle, État,
        Créé le), filtrable par entreprise (y compris un filtre dédié "Compte(s) Système" pour les
        SuperAdmin). Lien ajouté dans le menu Administration, visible uniquement pour SuperAdmin.
        `UtilisateurService.GetAllAsync` inclut maintenant `.Include(u => u.Company)`.
      - **Bug de robustesse trouvé en testant** : le regroupement du filtre utilisait `CompanyId`
        brut, mais l'affichage du tableau utilise `IsSuperAdmin` pour décider "Compte Système" —
        les deux peuvent diverger si un SuperAdmin porte encore un `CompanyId` non nul (état
        atteignable seulement via une modification directe de la base, hors du flux applicatif
        normal, mais reproduit pendant les tests). Corrigé en regroupant/filtrant par
        `u.IsSuperAdmin ? null : u.CompanyId` partout, cohérent avec l'affichage.
      8 nouveaux tests service (`UtilisateurServiceTests` : auto-assignation, préservation d'un
      choix explicite, `IsSuperAdmin` force `CompanyId = null`, échec explicite sans tenant,
      inclusion de `Company` ; `CompanyServiceTests.GetByIdAsync`). Vérifié de bout en bout dans le
      navigateur : création d'un utilisateur par un Admin normal (entreprise auto-assignée et
      affichée en lecture seule, confirmée en base) ; création d'une 2ᵉ entreprise et d'un
      utilisateur assigné explicitement par un SuperAdmin (confirmée en base) ; vue globale
      affichant les 3 comptes avec la bonne entreprise pour chacun, filtre par entreprise et par
      "Compte(s) Système" tous deux vérifiés ; accès direct à `/admin/utilisateurs-global` et
      `/admin/entreprises` correctement refusé (page "Accès refusé", pas de boucle) pour un Admin
      normal. Suite complète : 291/291 tests verts (Web + Desktop : 56/56), build 0 erreur.

### SuperAdmin — rôle dédié à la gestion de plateforme (zéro accès métier) ✅

- [x] Corriger une faille de sécurité critique découverte en cours de route : `PermissionAuthorizationHandler`
      (lecture, policy `perm:X`) et `ServicePermissionGuard` (écriture, services) traitaient
      `IsInRole(SuperAdmin)` comme un bypass complet de toutes les permissions métier — comme
      `Admin`. Un SuperAdmin pouvait donc accéder à `/clients`, `/factures-client`, etc. par URL
      directe malgré les liens de nav masqués. Contradiction directe avec l'exigence explicite de
      l'utilisateur : "[SuperAdmin] ne doit avoir aucun accès aux fonctionnalités métier ni aux
      données fonctionnelles des entreprises". Retiré le bypass SuperAdmin des deux fichiers (bypass
      Admin conservé) : un SuperAdmin retombe désormais sur `HasPermissionAsync`, qui échoue
      puisqu'il ne détient que les permissions plateforme (`tenants`, `users-global`,
      `roles-global`, `journal-global`).
- [x] Faille de défense en profondeur trouvée en creusant : `AppDbContext.ShouldApplyTenantFilter`
      bypassait aussi les *query filters* EF Core pour SuperAdmin sur les 10 entités métier (Client,
      Fournisseur, Produit, DevisClient, CommandeVente, BonLivraison, FactureClient, CommandeAchat,
      BonReception, FactureFournisseur) — un SuperAdmin voyait donc TOUTES les lignes de TOUTES les
      entreprises dès qu'une requête EF passait, indépendamment des vérifications de permission
      ci-dessus. Séparé en deux flags : `ShouldApplyTenantFilter` (business data, plus de bypass
      SuperAdmin — `CurrentCompanyId` étant `null` pour un SuperAdmin, le filtre exclut alors
      naturellement toutes les lignes) et `ShouldApplyTenantFilterToAuthenticatedUsers` (Utilisateur
      uniquement, conserve le bypass SuperAdmin — nécessaire pour la vue globale
      `/admin/utilisateurs-global`). `ApplyTenantOwnershipRules` (écriture) garde aussi le skip
      SuperAdmin : son seul chemin d'écriture légitime (créer/éditer un `Utilisateur` pour
      n'importe quelle entreprise via `UtilisateurForm`) exige de faire confiance à un `CompanyId`
      explicite plutôt que de tamponner le tenant courant (qu'un SuperAdmin n'a pas) — les entités
      métier restent de toute façon inatteignables en écriture par un SuperAdmin (guard de
      permission ci-dessus, avant même `SaveChanges`).
- [x] Sidebar dédiée SuperAdmin (`NavMenu.razor`) : plus aucune section métier (Ventes/Achats/
      Stock/Rapports/Paramètres) n'est rendue — uniquement "Plateforme" (Entreprises, Utilisateurs
      → `/admin/utilisateurs-global`, Rôles & Permissions, Journal d'Activité, Clés d'API,
      Webhooks). Même correction apportée au menu déroulant du topbar (`MainLayout.razor`), qui
      dupliquait l'ancien lien admin "Utilisateurs" (`/admin/utilisateurs`, scope entreprise) au
      lieu de la vue globale.
- [x] `Home.razor` (`/`) rendu SuperAdmin-safe : branche dédiée qui ne charge et n'affiche plus
      aucune donnée métier (KPI clients/produits/factures) — remplacée par un résumé plateforme
      (nombre d'entreprises, nombre d'utilisateurs via `ICompanyService.GetAllAsync`) et des accès
      rapides vers les pages Plateforme.
- [x] Nouvelles entités `ApiKey`/`Webhook` (`Web_GestCom.Core/Data/Models/`) + services
      (`ApiKeyService`/`WebhookService`) + pages CRUD `admin/cles-api` et `admin/webhooks`
      (`[Authorize(Roles = "SuperAdmin")]`, même pattern que `CompaniesList.razor`) — **stockage/
      gestion uniquement**, aucune API ni mécanisme d'envoi de webhook réel (choix explicite de
      l'utilisateur : "Page de gestion seulement, pas de fonctionnalité"). `CompanyId` optionnel
      (tag informatif, pas d'appartenance tenant — ni l'une ni l'autre n'implémente `ITenantOwned`
      puisque le SuperAdmin doit voir toutes les lignes de toutes les entreprises). Tables créées
      via bloc SQL idempotent dans `Program.cs` (`api_key`, `webhook`), suivant la convention
      "pas de migrations EF".
- [x] Compte `superadmin` dédié seedé dans `Program.cs` (login `superadmin` / mot de passe
      `SuperAdmin123!`, `IsSuperAdmin = true`, `CompanyId` forcé `null` par
      `UtilisateurService.EnsureTenantDefaults`) — créé uniquement si aucun SuperAdmin n'existe
      encore. Un compte séparé plutôt que de promouvoir `admin` : demande explicite de
      l'utilisateur.
      Vérifié de bout en bout dans le navigateur avec le compte `superadmin` : sidebar et menu
      topbar affichant uniquement "Plateforme" ; tableau de bord dédié (1 entreprise, 1
      utilisateur, accès rapides) ; accès direct à `/clients` et `/factures-client` par URL
      correctement bloqué ("Accès refusé", pas de boucle) — la régression de sécurité corrigée
      ci-dessus est bien fermée ; pages Entreprises, Utilisateurs (globale, affiche `superadmin` en
      "Compte Système" et `admin` avec son entreprise), Rôles & Permissions, Journal d'Activité
      (entrées de connexion des deux comptes), Clés d'API et Webhooks toutes chargées sans erreur.
      13 nouveaux tests service (`ApiKeyServiceTests`, `WebhookServiceTests`,
      `AppDbContextTenantIsolationTests.QueryFilter_WhenSuperAdmin_ShouldSeeNoBusinessData` —
      remplace l'ancien test qui affirmait à tort le comportement de bypass). Suite complète :
      299/299 tests verts, build 0 erreur.
- [x] Bug trouvé et corrigé après coup : créer une nouvelle Entreprise (`CompaniesList.razor`) ne
      créait aucun utilisateur — l'entreprise était donc inaccessible (aucun Admin ne pouvait s'y
      connecter), et `UtilisateursGlobalList.razor` n'avait de toute façon aucun bouton "Nouvel
      Utilisateur" (le lien vers `/admin/utilisateurs` — qui en a un — a été retiré de la nav
      SuperAdmin dans le redesign ci-dessus, sans remplacement). Corrigé : le modal "Nouvelle
      Entreprise" collecte désormais aussi un compte Admin initial (prénom, nom, login, email,
      mot de passe) et crée l'entreprise puis l'admin en deux étapes séquentielles (même
      `AppDbContext` scopé, pas de vraie transaction — si la création de l'admin échoue après
      coup, ex. login déjà pris malgré la vérification préalable, l'entreprise reste créée et un
      message explicite invite à ajouter l'admin manuellement plutôt que de masquer l'échec).
      `UtilisateursGlobalList.razor` a aussi gagné un bouton "Nouvel Utilisateur" (route
      `/admin/utilisateurs/nouveau`, déjà autorisée pour SuperAdmin et déjà capable de choisir
      n'importe quelle entreprise) pour pouvoir ajouter un 2ᵉ admin/manager à une entreprise
      existante. `UtilisateurForm.razor` renvoyait toujours vers `/admin/utilisateurs` (page
      Admin, scope entreprise) après un ajout/annulation — changé pour renvoyer vers
      `/admin/utilisateurs-global` quand l'acteur est SuperAdmin.
      Vérifié de bout en bout dans le navigateur : création d'"Société Test Beta" avec un admin
      `karim.beta` depuis le compte `superadmin` → message "Entreprise et compte admin créés." →
      visible dans la vue globale avec le bon rôle/entreprise → déconnexion et connexion réussie
      en tant que `karim.beta` → atterrit bien sur le tableau de bord métier normal (Ventes/
      Achats/Stock complets, données vides comme attendu pour une entreprise neuve), pas sur le
      tableau de bord Plateforme. 299/299 tests verts, build 0 erreur.

### Quotas de comptes en libre-service, par entreprise ✅

- [x] Demande utilisateur : par défaut, l'Admin d'une entreprise ne peut créer/promouvoir en
      libre-service qu'1 Admin, 1 Manager et 2 Employés — au-delà, seul le SuperAdmin peut créer
      un nouveau compte de ce rôle pour cette entreprise. Choix affinés en discussion : quotas
      configurables **par entreprise** (pas seulement par Plan), seedés par défaut selon le Plan
      (Standard 1/1/2, Pro 2/3/10, Enterprise aligné sur Pro pour l'instant — l'utilisateur
      tranchera plus tard au cas par cas plutôt que de figer "Enterprise = illimité"), et le
      comptage inclut les comptes désactivés (désactiver quelqu'un ne libère pas de place).
- [x] `Company` (`Web_GestCom.Core/Data/Models/Company.cs`) gagne 3 colonnes nullables —
      `MaxAdmins`/`MaxManagers`/`MaxEmployes` — `null` = illimité. Colonnes ajoutées via bloc SQL
      idempotent dans `Program.cs` (`ALTER TABLE company ADD ...`), donc les entreprises déjà
      existantes en prod restent illimitées tant qu'un SuperAdmin ne configure pas leurs quotas
      explicitement (aucun changement rétroactif).
- [x] `CompanyService.AddAsync` seede ces 3 champs depuis le `Plan` de l'entreprise **uniquement**
      si aucun des trois n'a été positionné explicitement par l'appelant (sinon les valeurs
      fournies gagnent telles quelles, sans compléter les deux autres avec les défauts du Plan).
      Un changement de `Plan` après coup (via `UpdateAsync`) ne retouche jamais ces champs — une
      fois seedés ou configurés à la main, ils ne bougent plus tout seuls.
- [x] Application de la règle dans `UtilisateurService` (nouveau paramètre optionnel
      `ICurrentUserService? currentUser`, même pattern que `ClientService`) : une nouvelle méthode
      privée `EnsureRoleQuotaNotExceededAsync` s'exécute dans `AddAsync` (toujours) et `UpdateAsync`
      (seulement si `authStateChanged` — rôle/entreprise/IsSuperAdmin a changé ; resauvegarder un
      utilisateur sans toucher son rôle ne redéclenche pas la vérification). Compte tous les
      comptes du rôle visé dans l'entreprise (actifs + désactivés), exclut l'utilisateur en cours
      d'édition de son propre décompte (gère nativement le cas promotion sans branche spéciale).
      `currentUser?.IsSuperAdmin == true` court-circuite entièrement la vérification.
- [x] `CompaniesList.razor` : le modal "Modifier" (pas "Nouvelle Entreprise") expose les 3 champs
      numériques nullables (`InputNumber`, placeholder "Illimité") — c'est là que le SuperAdmin
      configure les quotas au cas par cas, entreprise par entreprise, comme demandé. `AskEdit`
      copie bien les 3 valeurs existantes pour ne pas les écraser à l'ouverture du modal.
      10 nouveaux tests service (`CompanyServiceTests` : seed par Plan pour Standard/Pro/
      Enterprise, non-écrasement si déjà positionné explicitement ; `UtilisateurServiceTests` :
      quota atteint bloque un acteur non-SuperAdmin, SuperAdmin le contourne, quota `null` reste
      illimité, comptes désactivés comptent quand même, promotion de rôle bloquée au quota,
      resauvegarde sans changement de rôle ne redéclenche pas la vérification).
      Vérifié de bout en bout dans le navigateur : SuperAdmin configure `MaxAdmins = 1` sur
      « Société Test Beta » (entreprise déjà existante, illimitée par défaut car créée avant cette
      fonctionnalité) → l'Admin de cette entreprise (`karim.beta`) tente de créer un 2ᵉ Admin →
      bloqué avec le message « Quota de comptes « Admin » atteint pour cette entreprise (max 1).
      Seul le SuperAdmin peut créer un nouveau compte de ce rôle au-delà de ce quota. » → le
      SuperAdmin crée ensuite ce même compte sans aucun blocage (bypass confirmé). 309/309 tests
      verts, build 0 erreur.

### Améliorations UX

- [ ] Pagination sur les listes longues (Clients, Produits, Factures)
- [ ] Raccourcis clavier dans les formulaires (Ctrl+S pour sauvegarder)
- [ ] Mode sombre (Dark Mode)
- [ ] Notifications toast auto-disparaissant après quelques secondes
- [ ] Breadcrumb de navigation

### Données de référence — Pages de gestion ✅

- [x] Pages CRUD pour `TvaProduit` (taux de TVA)
- [x] Pages CRUD pour `CategorieProduit`
- [x] Pages CRUD pour `UniteProduit`
- [x] Pages CRUD pour `ModePayement`
- [x] Pages CRUD pour `Devise` (avec gestion des taux de change)
- [x] Pages CRUD pour `FabriquantProduit`
      — un seul service générique `IReferenceDataService<T>`/`ReferenceDataService<T>`
      (`Web_GestCom.Core/Services/ReferenceDataService.cs`, enregistré en DI comme generic
      ouvert) remplace 6 services quasi identiques : les 6 entités sont de simples tables de lookup
      sans navigation properties ni logique métier. 6 pages Razor (`Components/Pages/Parametres/`),
      routes `/parametres/{tva,categories,unites,modes-payement,devises,fabricants}`, `[Authorize
      (Roles = "Admin,SuperAdmin")]` (même convention que `/admin/journal`, pas de nouvelle
      permission fine seedée) : liste + modal Add/Edit + `ConfirmDialog` de suppression, erreurs FK
      traduites via `DeleteErrorMessageHelper`. Liens ajoutés dans la section PARAMÈTRES du menu.
      - Bug trouvé et corrigé en le faisant : Blazor Server garde un `AppDbContext` scopé vivant
        pour tout le circuit (pas juste une requête) — une entité `Add`ée plus tôt dans la session
        restait trackée, et un `Update`/`Delete` suivant sur la même ligne (via une instance
        `AsNoTracking()` fraîchement chargée, ce que fait chaque page) levait `InvalidOperationException:
        The instance of entity type 'X' cannot be tracked because another instance with the same
        key value ... is already being tracked`. Reproduit dans le navigateur (créer un taux de TVA
        puis le supprimer dans la foulée). Corrigé en détachant, avant tout `Update`/`Delete`,
        toute entrée déjà trackée portant la même clé primaire (comparaison générique via les
        métadonnées EF, `ReferenceDataService.DetachStaleTrackedEntry`). 2 tests de régression qui
        reproduisent exactement le scénario Add-puis-Update/Delete sur le même contexte.
      - Second bug trouvé et corrigé en le faisant : `DeleteErrorMessageHelper` ne reconnaissait que
        le message d'erreur SQL Server en anglais ("REFERENCE constraint", "FOREIGN KEY", "DELETE
        statement conflicted"), pas la version française ("contrainte REFERENCE", "instruction
        DELETE est en conflit") — sur une base SQL Server en locale française, toute violation de
        contrainte FK (pas seulement sur ces nouvelles pages) affichait le message technique brut
        au lieu du message convivial. Corrigé en ajoutant les deux tournures françaises à la
        détection. 3 nouveaux tests (`DeleteErrorMessageHelperTests`, absent jusqu'ici).
      7 nouveaux tests service (`ReferenceDataServiceTests`, CRUD + les 2 régressions de tracking +
      un test de métadonnées EF confirmant que `TvaProduit`→`Produit` reste en `Restrict`) + 3 tests
      (`DeleteErrorMessageHelperTests`). Vérifié de bout en bout dans le navigateur : création,
      modification, suppression, suppression bloquée par FK avec message convivial (en français),
      page Devises (3 champs) testée séparément pour confirmer que le patron générique s'adapte aux
      entités à plusieurs colonnes. Suite complète : 277/277 tests verts, build 0 erreur.

---

## PAIEMENT / ABONNEMENT

### Page d'accueil publique — tarifs + demande d'abonnement (V1, sans passerelle de paiement) ✅

- [x] Refonte de la page d'accueil publique (`Home.razor`) pour se rapprocher d'une vraie landing
      page SaaS B2B (inspirée d'une maquette de référence fournie par l'utilisateur) : nav avec
      ancre « Tarifs », hero avec un mockup illustratif de tableau de bord (aucune capture réelle
      disponible dans `wwwroot` — schéma reconstitué à partir du vrai layout du dashboard
      authentifié, chiffres clairement illustratifs, pas une capture ni une donnée client réelle),
      mini-aperçus sous chaque carte de module, section « Comment ça marche » avec connecteurs
      flèche, et une nouvelle **section Tarifs** (`#tarifs`) avec 3 plans + un encart « Comment ça
      se passe ? ».
- [x] **Décision utilisateur (3 questions posées avant de coder, vu l'enjeu financier/architecture)** :
      1. **Approche paiement V1 = demande manuelle.** « Choisir ce plan » n'encaisse rien : ça
         envoie une demande d'abonnement, suivie et activée à la main par le SuperAdmin. Aucune
         intégration de passerelle de paiement à ce stade.
      2. **Aucun compte marchand actif** chez un prestataire tunisien (ClicToPay/SPS, Paymee,
         Flouci, e-DINAR Poste Tunisienne) — confirmé par l'utilisateur. Point ouvert pour une
         future itération : choisir un prestataire, ouvrir le compte, valider le KYC marchand.
      3. **Nouvelle entité `Abonnement`** créée dès maintenant (pas seulement `Company.Plan` comme
         avant) pour modéliser le cycle de vie complet : demande → contact → activation → échéance,
         avec historique (plusieurs lignes par entreprise dans le temps).
- [x] `Web_GestCom.Core/Data/Models/Abonnement.cs` — nouvelle entité, **volontairement PAS
      `ITenantOwned`** (même convention que `ApiKey`/`Webhook` : le SuperAdmin doit voir toutes les
      demandes de toutes les entreprises, y compris avant qu'une entreprise n'existe). Champs :
      identité du demandeur (nom entreprise/contact/email/téléphone), `Plan`, `ModePaiementSouhaite`
      (indicatif, aucun paiement réel traité), `Message`, `Statut` (EnAttente/Contactee/Active/
      Refusee), dates (demande/début/échéance/traitement), `NotesAdmin`, `CompanyId` optionnel
      (lien vers l'entreprise une fois créée/identifiée — nullable tant qu'elle n'existe pas encore).
      Table `abonnement` créée via le bloc SQL idempotent habituel dans `Program.cs` (pas de
      migration EF, convention du projet).
- [x] `Web_GestCom.Core/Services/AbonnementService.cs` (`IAbonnementService`) — CRUD simple,
      `CreateDemandeAsync` force `Statut = "EnAttente"` et `CompanyId = null` côté serveur (ignore
      toute valeur envoyée par l'appelant, la demande publique ne doit jamais pouvoir s'auto-activer
      ou se lier à une entreprise). Enregistré dans `AddGestComServices`. 5 tests service.
- [x] **Page publique `Pages/Compte/DemandeAbonnement.cshtml`** (route `/demande-abonnement`,
      `?plan=Standard|Pro|Enterprise` pré-sélectionne le plan) — Razor Page anonyme, même patron que
      `Connexion.cshtml`/`MotDePasseOublie.cshtml` (page HTML autonome, pas de layout partagé,
      `AssetVersion.Value` sur le CSS). Formulaire : entreprise, contact, email, téléphone, plan,
      mode de paiement souhaité (Virement bancaire / Carte bancaire (à venir) / À discuter),
      message libre. Écran de succès explicite sur les 3 étapes suivantes (recontact sous 24-48h,
      activation après règlement par virement bancaire pour l'instant, création du compte admin à
      l'activation) — aucune ambiguïté sur le fait qu'aucun paiement n'a eu lieu.
- [x] **Page admin `Components/Pages/Admin/AbonnementsList.razor`** (`/admin/demandes-abonnement`,
      `[Authorize(Roles = "SuperAdmin")]`, lien ajouté dans la section Plateforme du menu) — liste
      toutes les demandes (toutes entreprises), bannière rappelant explicitement qu'aucun paiement
      n'est traité automatiquement. Modal « Traiter » : changer le statut, lier une entreprise
      existante (dropdown `ICompanyService.GetAllAsync`), fixer date de début/échéance, notes
      internes. Ne crée PAS automatiquement l'entreprise — le SuperAdmin la crée séparément depuis
      « Entreprises » (flux déjà existant, avec compte admin initial) puis revient la lier ici ;
      choix délibéré pour ne pas dupliquer/complexifier le flux de création d'entreprise existant.
- [x] Ce qui reste **explicitement hors scope** de cette itération, à trancher plus tard :
      - Choisir un prestataire de paiement tunisien et ouvrir un compte marchand (carte bancaire,
        e-DINAR, Flouci) — bloqué tant que l'utilisateur n'a pas ce compte.
      - Webhooks de confirmation de paiement, réconciliation automatique.
      - Envoi d'email automatique à la soumission d'une demande — **aucune infrastructure email
        n'existe dans ce projet actuellement** (recherché : aucun `IEmailSender`/`SmtpClient`/
        MailKit ni équivalent) ; pour l'instant le SuperAdmin doit consulter `/admin/
        demandes-abonnement` manuellement pour voir les nouvelles demandes.
      - Génération de facture/reçu.
      - Renouvellement automatique à l'échéance (`DateEcheance` est stockée mais rien ne l'exploite
        encore — pas d'alerte, pas de désactivation automatique du compte à expiration).
      - **Montants tarifaires à confirmer** : 390 DT/an (Standard) et 690 DT/an (Pro) repris tels
        quels de la maquette de référence approuvée par l'utilisateur — à valider/ajuster, ce sont
        des montants métier que je ne peux pas fixer moi-même.
      Vérifié de bout en bout dans le navigateur : demande soumise depuis `/demande-abonnement?
      plan=Pro` → visible immédiatement dans `/admin/demandes-abonnement` (compte `superadmin`) →
      passage du statut à « Contactée » avec note interne → persistance confirmée après rechargement
      de la liste. Suite complète : 315/315 tests verts, build 0 erreur.

### Correction post-déploiement — fidélité à la maquette de référence + essai gratuit réel

- [x] **Retour utilisateur après le premier déploiement** : « t'as pas respecté l'image donné » — la
      première version avait dévié de la maquette de référence sur plusieurs points sans que ce soit
      assez mis en avant : le hero reprenait l'ancien texte générique au lieu du hero « offres » de
      la maquette, « Pourquoi choisir GestCom » était en grille de 4 cartes au lieu de 3 items reliés
      par des flèches, et le tableau comparatif des fonctionnalités sous les cartes de prix manquait
      entièrement. 3 décisions demandées avant de corriger (vu l'impact business du badge
      « essai gratuit » et du panneau paiement) :
      1. **Hero + comparatif** → corrigés pour coller à la maquette, mais aperçu montré avant mise en
         prod (pas de redéploiement direct).
      2. **« 14 jours d'essai gratuit »** → l'utilisateur confirme que c'est une vraie offre, pas
         juste un élément de maquette à ignorer.
      3. **Panneau « Modes de paiement / sécurisé par notre partenaire »** → reste sur la version
         honnête (« Comment ça se passe ? ») puisqu'il n'y a toujours pas de prestataire de paiement
         réel, conformément à la décision prise plus haut.
- [x] Hero réécrit pour reprendre le texte de la maquette : badge « 🎁 Des offres adaptées aux
      entreprises tunisiennes », titre « Des tarifs simples, une solution complète. », CTA
      « Commencer maintenant → » (ancre `#tarifs`) et « Voir les fonctionnalités » (ancre
      `#fonctionnalites`), ligne « ✓ 14 jours d'essai gratuit ».
- [x] Section « Pourquoi choisir GestCom » reconstruite en 3 items reliés par des flèches (au lieu de
      la grille de 4 cartes) : Pensé pour la Tunisie / Simple à prendre en main / Une vision claire de
      votre activité — mêmes libellés courts que la maquette.
- [x] Section Tarifs : titre déplacé vers le hero, la section elle-même reprend « Des plans adaptés à
      votre croissance » (titre de la maquette pour cette section précise). Le panneau « Comment ça
      se passe ? » est sorti de la grille des 3 cartes de prix (qui redevient 3 colonnes égales comme
      sur la maquette) et devient un bandeau horizontal compact juste en dessous, mentionnant
      explicitement l'essai de 14 jours dans l'étape 2.
- [x] **Nouveau tableau « Comparatif des fonctionnalités »** sous les cartes de prix, comme sur la
      maquette — mais avec un contenu **honnête** : Ventes/Achats/Stock/Rapports cochés identiquement
      sur les 3 plans (c'est la vérité : l'app ne bride aucune fonctionnalité par plan aujourd'hui),
      seules les lignes « Comptes utilisateurs inclus » (quotas réels) et « Accompagnement à la mise
      en route » (réservé Enterprise, promesse raisonnable vu que ce plan est négocié au cas par cas)
      différencient réellement les plans. Pas de fausses coches suggérant des fonctionnalités
      Pro/Enterprise qui n'existent pas.
- [x] **Essai gratuit rendu réellement actionnable**, pas juste un badge marketing : `Abonnement`
      accepte désormais le statut `"Essai"` en plus de EnAttente/Contactee/Active/Refusee (simple
      valeur de chaîne, aucun changement de schéma nécessaire). `AbonnementsList.razor` : nouvelle
      option « Essai (14 jours) » dans le select Statut, badge dédié, et un bouton « Remplir 14 jours
      à partir d'aujourd'hui » qui pré-remplit Date de début/Date d'échéance. La page publique
      `/demande-abonnement` explique maintenant le vrai parcours : contact sous 24-48h → démarrage de
      l'essai avec accès immédiat → poursuite payante après les 14 jours.
      - **Limite assumée, documentée dans la bannière admin** : rien ne désactive automatiquement un
        essai arrivé à échéance (`DateEcheance` reste une simple donnée informative) — le SuperAdmin
        doit surveiller `/admin/demandes-abonnement` manuellement. Automatiser cette désactivation
        demanderait une tâche planifiée/un middleware de vérification à l'ouverture de session, hors
        scope de cette correction ciblée.
      Vérifié en local (build + suite de tests) avant toute proposition d'aperçu — pas encore
      redéployé en production à ce stade, conformément à la demande explicite de montrer un aperçu
      d'abord.

---

## DETTE TECHNIQUE

- [~] Ajouter `AsNoTracking()` sur toutes les requêtes en lecture seule dans les services — fait sur
      les services de données de référence (`ClientService`, `ProduitService`, `FournisseurService`,
      `UtilisateurService`, `JournalActiviteService`). **Volontairement pas étendu** aux services
      documents (Devis/Commandes/Bons/Factures) : tenté, mais la suite de tests a révélé un vrai
      risque avec ce codebase — un `DbContext` scope Blazor Server suit tout le circuit (pas juste
      une requête), et une entité déjà trackée plus tôt dans ce circuit (ex. `AddReglementAsync`,
      ou un `UpdateAsync` juste avant un `CloneAsync`) entre en conflit d'identité avec un fetch
      `AsNoTracking()` du même enregistrement, ou pire, renvoie silencieusement une collection de
      lignes vide (`CloneAsync` après `UpdateAsync` dans le même scope). 5 tests xUnit ont détecté
      le problème avant merge — voir `DevisClientServiceTests`, `FactureClientServiceTests`. Étendre
      correctement demanderait de passer ces services sur `IDbContextFactory` (comme le font déjà
      `PermissionService`/`FeatureFlagService`) plutôt que d'injecter `AppDbContext` scopé.
- [x] **BUG DE RÉGRESSION CORRIGÉ** — `ClientService.UpdateAsync`, `FournisseurService.UpdateAsync`
      et `ProduitService.UpdateAsync` levaient systématiquement `InvalidOperationException: The
      instance of entity type 'X' cannot be tracked...` en production, cassant **l'édition de
      n'importe quel Client/Fournisseur/Produit**. Cause : le point `AsNoTracking()` ci-dessus a mis
      `GetByCodeAsync()` en `AsNoTracking().Include(...)`, mais `ClientForm`/`FournisseurForm`/
      `ProduitForm` chargent aussi une liste de référence trackée pour peupler un `<select>` (ex.
      `Db.Devises.ToListAsync()`, `Db.CategoriesProduit.ToListAsync()`) dans le même scope
      `DbContext` — `Update()` suit alors tout le graphe de navigation renvoyé par
      `GetByCodeAsync()` (Devise/Catégorie/Unité/TVA/Fabricant) et tente de le re-tracker,
      entrant en conflit d'identité avec l'instance déjà trackée par le chargement du `<select>`.
      Trouvé en testant manuellement le point "Générer BL/Facture" ci-dessous (l'édition de
      `Produit` a échoué en essayant de remettre le stock à zéro après le test). Corrigé en mettant
      à `null` les propriétés de navigation avant `Update()` dans les 3 services (seules les
      colonnes scalaires sont modifiées ; motif standard EF Core pour ce cas). 3 nouveaux tests de
      régression qui reproduisent exactement le scénario (charger la liste de référence trackée
      PUIS `GetByCodeAsync` PUIS `UpdateAsync`) — vérifiés comme échouant sans le fix avant d'être
      confirmés verts avec. Revérifié dans le navigateur : édition Client et Produit fonctionnent à
      nouveau. **À vérifier : si le commit qui a introduit `AsNoTracking()` sur ces 3 services a
      déjà été déployé chez le client, l'édition de Client/Fournisseur/Produit y était cassée
      jusqu'à ce fix — prévoir un déploiement correctif si c'est le cas.**
- [x] **BUG DE RÉGRESSION CORRIGÉ (suite du point ci-dessus)** — signalé par l'utilisateur en
      production : « lors de sauvegarde de produit : Erreur : An error occurred while saving the
      entity changes. See the inner exception for details. » Reproduit en local avec le scénario
      le plus simple qui soit : créer un produit puis cliquer « Modifier » sur ce même produit
      **dans la même session** (aucune raison pour un utilisateur de ne pas faire ça). Le message
      réel derrière ce texte générique était `The instance of entity type 'Produit' cannot be
      tracked because another instance with the same key value ... is already being tracked`.
      **Root cause distincte du point ci-dessus** : nuller les propriétés de navigation avant
      `Update()` protège contre un conflit sur le *graphe* (Devise/Catégorie/etc. déjà trackées
      par le chargement des listes de select), mais pas contre un conflit sur la *clé primaire de
      l'entité elle-même* quand elle a déjà été trackée plus tôt dans le même circuit Blazor Server
      (ici, par le `db.Produits.Add(produit)` de `AddAsync`, dont l'entité reste trackée après
      `SaveChangesAsync`). `DetachStaleTrackedEntry` existe déjà précisément pour ce cas et est
      déjà utilisé par `AbonnementService`/`ApiKeyService`/`CompanyService`/`WebhookService`/
      `ReferenceDataService` — mais **n'avait jamais été ajouté à `ClientService`,
      `FournisseurService`, `ProduitService` ni `UtilisateurService`**, les 4 services qui ont reçu
      `AsNoTracking()` sur leurs lectures dans un point antérieur de cette liste. Corrigé en
      ajoutant `db.DetachStaleTrackedEntry(entity)` juste avant `Update()` dans les 4. En passant,
      `AppDbContextSaveExtensions.SaveChangesGuardedAsync` traduit maintenant aussi tout
      `DbUpdateException` générique en y annexant `InnerException.Message` — le message par défaut
      d'EF Core ne montre jamais la vraie cause SQL, ce qui a rendu ce bug beaucoup plus long à
      diagnostiquer côté production que nécessaire.
      - **Pourquoi la suite de tests existante n'avait rien détecté** : le test de régression du
        point précédent (`UpdateAsync_AfterFormLoadsReferenceListsAndGetByCode_...`) appelle
        `db.ChangeTracker.Clear()` juste après `AddAsync` pour « simuler un circuit neuf » — ce qui
        élimine artificiellement exactement le scénario cassé, puisqu'un vrai circuit Blazor Server
        ne vide jamais son tracker entre deux actions. 4 nouveaux tests
        (`UpdateAsync_ImmediatelyAfterAddAsync_InSameCircuit_DoesNotThrowIdentityConflict` dans
        `ProduitServiceTests`/`ClientServiceTests`/`FournisseurServiceTests`/
        `UtilisateurServiceTests`) reproduisent le vrai enchaînement Add → GetByCode/GetById →
        Update **sans** ce `Clear()`.
      Revérifié dans le navigateur avec le scénario exact rapporté : créer un produit, cliquer
      « Modifier » immédiatement, enregistrer — fonctionne. Suite complète : 319/319 tests verts,
      build 0 erreur. **Même remarque que ci-dessus : si ce commit n'est pas encore déployé, l'édition
      d'un Client/Fournisseur/Produit/Utilisateur ajouté dans la même session reste cassée en prod
      jusqu'au déploiement.**
- [x] Ajouter la gestion des erreurs de concurrence EF Core (`DbUpdateConcurrencyException`) —
      `AppDbContextSaveExtensions.SaveChangesGuardedAsync()` (Core) remplace les 58 appels
      `db.SaveChangesAsync()` des services et traduit `DbUpdateConcurrencyException` (ex. un
      enregistrement supprimé/modifié par un autre utilisateur entre-temps) en
      `ConcurrencyConflictException` avec un message clair, sans toucher l'UI (Web et Desktop
      affichent déjà `ex.Message` dans leurs blocs `catch` génériques). Pas de token de concurrence
      (`RowVersion`) ajouté — ça détecte la ligne déjà supprimée, pas une modification concurrente
      silencieuse (dernier-écrit-gagne) ; ajouter un vrai token nécessiterait une migration de
      schéma, hors scope ici. Vérifié par un test qui simule un vrai conflit (deux `DbContext` sur
      la même base InMemory, l'un supprime la ligne pendant que l'autre tente de la sauvegarder).
- [x] Centraliser la logique de calcul des totaux dans une classe utilitaire partagée (éviter la duplication entre services) — `LineCalculator` déplacé dans `Web_GestCom.Core`, utilisé par les 7 pages document et par `GestCom_Desktop`
- [x] Unifier `Web_GestCom` et `Web_GestCom.Core` — `Web_GestCom.csproj` référence désormais `Web_GestCom.Core` (26 entités + `AppDbContext` + 22 services n'existent plus qu'une fois) ; `DeleteErrorMessageHelper`, `PartyDetailsHelper` et l'enregistrement DI (`AddGestComServices`, appelée par `Program.cs` et `AppHost.cs`) sont eux aussi unifiés
- [x] Ajouter des tests unitaires sur les services (xUnit + InMemory EF Core) — 7 nouveaux fichiers
      de tests couvrant les services qui n'avaient encore aucune couverture : `BonLivraisonService`,
      `BonReceptionService`, `CommandeVenteService`, `CommandeAchatService`, `FactureFournisseurService`
      (création/màj/suppression avec impact stock, règlements, solde, clonage), `JournalActiviteService`
      (filtres login/entité/date, listes distinctes, et le fait qu'un échec de journalisation ne doit
      jamais remonter), `FeatureFlagService` (modèle opt-out par défaut, portée par entreprise, cache).
      `Helpers/InMemoryDbContextFactory.cs` a été extrait (dupliqué à l'identique dans
      `PermissionServiceTests`) pour être réutilisé par `FeatureFlagServiceTests`. Suite complète :
      222/222 tests verts (`Web_GestCom`) + build 0 erreur (`Web_GestCom` + `GestCom_Desktop`).
      Restent sans test dédié (comportement simple, à faible risque) : `LineCalculator`,
      `DeleteErrorMessageHelper`, `PartyDetailsHelper`, `RoleNameMapper`, `AppConfigService` (déjà
      couvert), `TenantService`/`CurrentUserService` (dépendent de `HttpContext`/état Blazor, mieux
      couverts en intégration que via un mock).
- [x] Valider les montants négatifs dans les formulaires (quantité, prix) — `LineCalculator.EnsureNoNegativeAmounts()`
      (Core, partagé Web+Desktop) rejette toute ligne à quantité ou prix unitaire négatif, appelé en
      tête de `CreateAsync`/`UpdateAsync` dans les 7 services document (Devis, CommandeVente,
      CommandeAchat, BonLivraison, BonReception, FactureClient, FactureFournisseur) — c'est le point
      unique qui garantit qu'aucune valeur négative n'atteint `SaveChangesGuardedAsync`, quelle que
      soit l'UI (les deux affichent déjà `ex.Message` dans leurs blocs catch génériques, donc le
      message "La quantité et le prix unitaire d'une ligne ne peuvent pas être négatifs." remonte
      sans changement supplémentaire côté UI). En plus, `min="0"` ajouté sur les 14 `<InputNumber>`
      Quantité/Prix des 7 formulaires Web (petit garde-fou navigateur, pas suffisant seul puisque
      HTML `min` n'empêche pas la saisie manuelle — d'où le garde côté service). Pas de changement
      côté Desktop (`ProductLinesEditor`) : FluentValidation et `ObjectGraphDataAnnotationsValidator`
      ne sont pas utilisés dans ce projet et n'auraient de toute façon pas aidé ici (le
      `DataAnnotationsValidator` de Blazor ne valide que le modèle racine de l'`EditForm`, pas la
      liste `_lignes` séparée) ; dupliquer un garde-clause par formulaire aurait été redondant avec
      le message déjà clair renvoyé par le service. 12 nouveaux tests (`LineCalculatorTests` + un
      test de câblage par service confirmant qu'une ligne négative lève bien l'exception). Suite
      complète : 234/234 tests verts, build 0 erreur (Web + Desktop).
- [x] Revoir le `DeleteBehavior.Restrict` global — décision utilisateur : passer les 2 FK optionnelles
      de traçabilité (`BonLivraison.NumeroCommandeVente`, `BonReception.NumeroCommandeAchat`) en
      `SetNull`, pour pouvoir supprimer une Commande déjà livrée/reçue sans devoir d'abord supprimer
      son BL/BR. Toutes les autres FK restent `Restrict` (aucune ligne/document financier ne doit
      être orphelin ou supprimé en cascade) — vérifié par un test qui inspecte le modèle EF
      (`AppDbContextDeleteBehaviorTests`, 3 tests : les 2 FK concernées + un spot-check que
      `BonLivraison→Client` et `LigneFactureClient→FactureClient` restent `Restrict`). Deux volets :
      1) `AppDbContext.OnModelCreating` (`Web_GestCom.Core/Data/AppDbContext.cs`) déclare
         explicitement `.OnDelete(DeleteBehavior.SetNull)` sur ces 2 relations, après la boucle
         globale Restrict (sinon écrasé) — s'applique à toute nouvelle base créée via
         `EnsureCreated()`.
      2) `Program.cs` ajoute un bloc SQL brut idempotent (cherche la contrainte FK existante via
         `sys.foreign_keys`/`sys.foreign_key_columns`, ne la recrée que si elle n'est pas déjà
         `ON DELETE SET NULL`) pour mettre à jour les bases déjà déployées chez les clients, en
         suivant le même pattern `IF NOT EXISTS` que toutes les autres migrations de schéma de ce
         fichier.
- [x] Ajouter une migration pour tout changement de schéma futur — clarifié : ce projet n'utilise pas
      EF Core Migrations, seulement `EnsureCreated()` + des blocs SQL brut idempotents dans
      `Program.cs` (voir `Web_GestCom/CLAUDE.md`). La convention "migration" ici = suivre ce
      pattern (`IF NOT EXISTS` / vérifier l'état actuel avant d'altérer) pour tout futur changement
      de schéma, comme démontré par le point DeleteBehavior ci-dessus. Rien à ajouter tant qu'aucun
      changement de schéma n'est en attente — ce n'était pas une tâche actionnable isolément.
