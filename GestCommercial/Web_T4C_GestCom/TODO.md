# TODO – T4C GestCom Web

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
- [x] Enregistrer `IFactureFournisseurService` — déjà fait via `AddT4CGestComServices` (Core)
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

### Recherche et filtres avancés

- [ ] Ajouter filtre par **période** (date début / date fin) sur toutes les listes de documents
- [ ] Ajouter filtre par **état** sur les listes (Devis, Commandes, BL, Factures)
- [ ] Ajouter filtre par **client** sur les listes VENTES
- [ ] Ajouter filtre par **fournisseur** sur les listes ACHATS

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

### Retenues à la source

- [ ] Intégrer le calcul de la retenue à la source (`TauxRetenue` depuis `AppConfigService`)
  - Afficher le montant retenu sur les factures
  - Générer les déclarations de retenue

### Gestion multi-entreprises

- [ ] Support de plusieurs entreprises (sélection au démarrage)
  - Logique déjà présente dans le projet desktop (commentée)

### Améliorations UX

- [ ] Pagination sur les listes longues (Clients, Produits, Factures)
- [ ] Raccourcis clavier dans les formulaires (Ctrl+S pour sauvegarder)
- [ ] Mode sombre (Dark Mode)
- [ ] Notifications toast auto-disparaissant après quelques secondes
- [ ] Breadcrumb de navigation

### Données de référence — Pages de gestion

- [ ] Pages CRUD pour `TvaProduit` (taux de TVA)
- [ ] Pages CRUD pour `CategorieProduit`
- [ ] Pages CRUD pour `UniteProduit`
- [ ] Pages CRUD pour `ModePayement`
- [ ] Pages CRUD pour `Devise` (avec gestion des taux de change)
- [ ] Pages CRUD pour `FabriquantProduit`

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
- [x] Centraliser la logique de calcul des totaux dans une classe utilitaire partagée (éviter la duplication entre services) — `LineCalculator` déplacé dans `Web_T4C_GestCom.Core`, utilisé par les 7 pages document et par `T4C_GestCom_Desktop`
- [x] Unifier `Web_T4C_GestCom` et `Web_T4C_GestCom.Core` — `Web_T4C_GestCom.csproj` référence désormais `Web_T4C_GestCom.Core` (26 entités + `AppDbContext` + 22 services n'existent plus qu'une fois) ; `DeleteErrorMessageHelper`, `PartyDetailsHelper` et l'enregistrement DI (`AddT4CGestComServices`, appelée par `Program.cs` et `AppHost.cs`) sont eux aussi unifiés
- [x] Ajouter des tests unitaires sur les services (xUnit + InMemory EF Core) — 7 nouveaux fichiers
      de tests couvrant les services qui n'avaient encore aucune couverture : `BonLivraisonService`,
      `BonReceptionService`, `CommandeVenteService`, `CommandeAchatService`, `FactureFournisseurService`
      (création/màj/suppression avec impact stock, règlements, solde, clonage), `JournalActiviteService`
      (filtres login/entité/date, listes distinctes, et le fait qu'un échec de journalisation ne doit
      jamais remonter), `FeatureFlagService` (modèle opt-out par défaut, portée par entreprise, cache).
      `Helpers/InMemoryDbContextFactory.cs` a été extrait (dupliqué à l'identique dans
      `PermissionServiceTests`) pour être réutilisé par `FeatureFlagServiceTests`. Suite complète :
      222/222 tests verts (`Web_T4C_GestCom`) + build 0 erreur (`Web_T4C_GestCom` + `T4C_GestCom_Desktop`).
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
      1) `AppDbContext.OnModelCreating` (`Web_T4C_GestCom.Core/Data/AppDbContext.cs`) déclare
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
      `Program.cs` (voir `Web_T4C_GestCom/CLAUDE.md`). La convention "migration" ici = suivre ce
      pattern (`IF NOT EXISTS` / vérifier l'état actuel avant d'altérer) pour tout futur changement
      de schéma, comme démontré par le point DeleteBehavior ci-dessus. Rien à ajouter tant qu'aucun
      changement de schéma n'est en attente — ce n'était pas une tâche actionnable isolément.
