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
- [ ] Créer `Components/Pages/FacturesFournisseur/FactureFournisseurList.razor` (`@page "/factures-fournisseur"`)
  - Colonnes : N°, Date, Fournisseur, Montant HT, FODEC, TVA, TTC, Timbre, État Règlement
- [ ] Créer `Components/Pages/FacturesFournisseur/FactureFournisseurForm.razor`
  - Routes : `/factures-fournisseur/nouveau` et `/factures-fournisseur/{Numero}`
  - Section règlements fournisseur (identique à FactureClient)
  - Bouton Cloner
- [ ] Enregistrer `IFactureFournisseurService` dans `Program.cs`

---

## PRIORITÉ MOYENNE

### Paramètres — Fiche Entreprise ✅

- [x] Créer `Components/Pages/Entreprise/EntrepriseForm.razor` (`@page "/entreprise"`)
  - Formulaire création/modification unique, directement via AppDbContext

### Fournisseurs — Formulaire dédié ✅

- [x] Créer `Components/Pages/Fournisseurs/FournisseurForm.razor`
  - Routes : `/fournisseurs/nouveau` et `/fournisseurs/{Code}`

### Amélioration Factures / Avoirs

- [ ] Ajouter la possibilité de **créer un Avoir à partir d'une Facture** existante (bouton "Générer Avoir" dans `FactureForm`)
- [ ] Ajouter la **suppression de règlements** individuels depuis `FactureForm`
- [ ] Afficher **l'état règlement** (`EtatReglement`) sur la fiche facture (badge coloré)

### Amélioration Bons de Livraison

- [ ] Ajouter la possibilité de **facturer un BL** directement (bouton "Générer Facture" dans `BonLivraisonForm`)
- [ ] Lier un BL à une facture existante (mettre à jour `EtatFacture` du BL)

### Amélioration Commandes Vente

- [ ] Ajouter la possibilité de **créer un BL depuis une commande** (bouton "Générer BL" dans `CommandeVenteForm`)

---

## PRIORITÉ BASSE

### Tableau de bord — Home.razor

- [ ] Implémenter un vrai tableau de bord avec les métriques clés :
  - Chiffre d'affaires du mois (somme des factures TTC)
  - Factures non réglées (montant total)
  - Produits en alerte de stock (count)
  - Dernières factures (5 dernières)
  - Graphique CA mensuel (6 derniers mois)

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

### À compléter — Journal d'activité

- [ ] Ajouter la traçabilité sur les services restants :
  - `DevisClientService`
  - `CommandeVenteService`
  - `CommandeAchatService`
- [ ] Ajouter l'entrée "Connexion" dans le journal depuis la page login
- [ ] Permettre à l'administrateur de **purger** le journal (ancien de N mois)

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
- [ ] Ajouter des tests unitaires sur les services (xUnit + InMemory EF Core)
- [ ] Valider les montants négatifs dans les formulaires (quantité, prix)
- [ ] Ajouter une migration pour tout changement de schéma futur
- [ ] Revoir le `DeleteBehavior.Restrict` global — certaines suppressions pourraient nécessiter des règles plus fines
