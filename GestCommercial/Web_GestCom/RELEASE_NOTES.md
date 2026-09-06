# Release Notes – GestCom Web

---

## v0.5.0 — 2026-03-28 — Authentification + Journal d'Activité + Administration

### Nouvelles fonctionnalités

#### Authentification par Cookies (ASP.NET Core)
- **Nouveau** : Page de connexion `/compte/connexion` (RazorPage) — formulaire login/mot de passe avec CSRF
- **Nouveau** : Déconnexion via `/compte/deconnexion` (signe out le cookie + redirection)
- **Nouveau** : Toutes les routes Blazor protégées via `AuthorizeRouteView` — redirection auto vers la page de connexion
- **Nouveau** : Session valable 8 h avec `SlidingExpiration`
- **Sécurité** : Hachage des mots de passe via **SHA-256** (UTF-8)

#### Gestion des Utilisateurs
- **Nouveau** : Entité `Utilisateur` — Login, Prénom, Nom, Email, Rôle (Admin/Utilisateur), Actif, DateCréation
- **Nouveau** : Service `IUtilisateurService` / `UtilisateurService`
  - `AuthentifierAsync` — vérifie login + hash mot de passe + statut actif
  - `AddAsync`, `UpdateAsync`, `ChangePasswordAsync`
  - `ActiverAsync`, `DesactiverAsync` (pas de suppression physique)
  - `LoginExistsAsync` — vérifie unicité du login
- **Nouveau** : Page liste `/admin/utilisateurs` (Admin uniquement) — affiche rôle, état actif/inactif, boutons Activer/Désactiver
- **Nouveau** : Formulaire `/admin/utilisateurs/nouveau` et `/admin/utilisateurs/{Id}` — création/modification avec changement de mot de passe inline
- **Seed automatique** : à la première exécution, un utilisateur `admin / admin123` est créé s'il n'existe aucun utilisateur

#### Journal d'Activité
- **Nouveau** : Entité `JournalActivite` — Login, Action, Entité, Code, DateHeure, Détail
- **Nouveau** : Service `IJournalActiviteService` / `JournalActiviteService`
  - `EnregistrerAsync` — journal silencieux (try/catch, ne bloque jamais l'app)
  - `GetAllAsync` avec filtres : login, entité, plage de dates
- **Nouveau** : Page `/admin/journal` (Admin uniquement) — tableau filtrable (utilisateur, entité, dates) + badges colorés par action
- **Traçabilité activée** sur 7 services prioritaires :

| Service | Actions tracées |
|---|---|
| `ClientService` | Ajout, Modification, Suppression |
| `ProduitService` | Ajout, Modification, Suppression |
| `FournisseurService` | Ajout, Modification, Suppression |
| `FactureClientService` | Ajout, Modification, Suppression, Clone |
| `BonLivraisonService` | Ajout, Modification, Suppression, Clone |
| `BonReceptionService` | Ajout, Modification, Suppression, Clone |
| `FactureFournisseurService` | Ajout, Modification, Suppression, Clone |

#### Navigation — Section Administration (Admin uniquement)
- Menu latéral enrichi : section **ADMINISTRATION** visible uniquement pour les admins
  - Liens vers Utilisateurs et Journal d'Activité
- Bande utilisateur en bas de la sidebar : login + badge de rôle + lien Déconnecter

### Modifications techniques

#### Nouveaux fichiers
| Fichier | Type |
|---|---|
| `Data/Models/Utilisateur.cs` | Modèle EF Core |
| `Data/Models/JournalActivite.cs` | Modèle EF Core |
| `Services/ICurrentUserService.cs` | Service scopé (état utilisateur du circuit) |
| `Services/UtilisateurService.cs` | Service CRUD + auth |
| `Services/JournalActiviteService.cs` | Service journal |
| `Pages/_ViewStart.cshtml` | Support RazorPages |
| `Pages/_ViewImports.cshtml` | Support RazorPages |
| `Pages/Compte/Connexion.cshtml` + `.cshtml.cs` | Page login |
| `Pages/Compte/Deconnexion.cshtml` + `.cshtml.cs` | Page logout |
| `Components/Pages/Admin/UtilisateursList.razor` | Page admin |
| `Components/Pages/Admin/UtilisateurForm.razor` | Page admin |
| `Components/Pages/Admin/JournalActiviteList.razor` | Page admin |

#### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `Data/AppDbContext.cs` | + `DbSet<Utilisateur>` + `DbSet<JournalActivite>` |
| `Program.cs` | Cookie auth + RazorPages + `ICurrentUserService`/journal/auth services + création tables SQL + seed admin |
| `Components/App.razor` | Ajout `<CascadingAuthenticationState>` |
| `Components/Routes.razor` | `RouteView` → `AuthorizeRouteView` |
| `Components/Layout/MainLayout.razor` | Initialise `ICurrentUserService` depuis l'état d'authentification |
| `Components/Layout/NavMenu.razor` | Section Administration + bande utilisateur + déconnexion |
| `Components/_Imports.razor` | + `@using Microsoft.AspNetCore.Authorization` + `@using ...Authorization` |
| `Services/ClientService.cs` | + journal Ajout/Modification/Suppression |
| `Services/ProduitService.cs` | + journal Ajout/Modification/Suppression |
| `Services/FournisseurService.cs` | + journal Ajout/Modification/Suppression |
| `Services/FactureClientService.cs` | + journal Ajout/Modification/Suppression/Clone |
| `Services/BonLivraisonService.cs` | + journal Ajout/Modification/Suppression/Clone |
| `Services/BonReceptionService.cs` | + journal Ajout/Modification/Suppression/Clone |
| `Services/FactureFournisseurService.cs` | + journal Ajout/Modification/Suppression/Clone |
| `wwwroot/app.css` | + style `.nav-user-info` |

---

## v0.4.0 — 2026-03-28 — Module ACHATS complet + Fournisseurs + Entreprise

### Nouvelles fonctionnalités

#### Fournisseurs
- **Nouveau** : Formulaire `/fournisseurs/nouveau` et `/fournisseurs/{Code}` — création et modification complète
- **Champs** : Code (auto FO#####), Nom, Matricule Fiscale, Devise, État, Adresse, Téléphone, Mobile, Fax, Email, RIB, Note

#### Commandes Achat
- **Nouveau** : Page liste `/commandes-achat` — tableau avec N°, Fournisseur, montants, État, État Réception
- **Nouveau** : Formulaire `/commandes-achat/nouveau` et `/commandes-achat/{Numero}`
  - Lignes : Produit, Quantité, Prix Achat HT, TVA %, Montant HT
  - Totaux : HT, TVA, TTC
  - Bouton Cloner
- **Nouveau** : Service `ICommandeAchatService` / `CommandeAchatService`
  - **Aucun impact stock**

#### Bons de Réception
- **Nouveau** : Page liste `/bons-reception` — affiche Commande Achat liée, État, État Facturation
- **Nouveau** : Formulaire `/bons-reception/nouveau` et `/bons-reception/{Numero}`
  - Sélection Commande Achat associée (filtrée par fournisseur)
  - Bouton Cloner
- **Nouveau** : Service `IBonReceptionService` / `BonReceptionService`
  - **Incrémente le stock** à la création et au clonage
  - **Restitue** le stock à la suppression / modification

#### Factures Fournisseur
- **Nouveau** : Page liste `/factures-fournisseur` — tableau avec états et badge règlement
- **Nouveau** : Formulaire `/factures-fournisseur/nouveau` et `/factures-fournisseur/{Numero}`
  - Timbre Fiscal (lecture depuis AppConfig)
  - Section règlements (Date, Mode paiement, Montant, Référence)
  - Solde restant calculé automatiquement
  - Bouton Cloner
- **Nouveau** : Service `IFactureFournisseurService` / `FactureFournisseurService`
  - **Incrémente le stock** à la création et au clonage
  - **Restitue** le stock à la suppression / modification
  - `AddReglementAsync`, `GetSoldeAsync`, mise à jour automatique `EtatReglement`

#### Paramètres — Entreprise
- **Nouveau** : Page `/entreprise` — fiche unique de l'entreprise
  - Champs : Nom, Matricule Fiscale, Adresse complète, Téléphone, Fax, Email, Site, RIB, Logo, Note
  - Crée ou met à jour la fiche (upsert)

### Modifications techniques

#### Program.cs
- Enregistrement des 3 nouveaux services Scoped :
  - `ICommandeAchatService` → `CommandeAchatService`
  - `IBonReceptionService` → `BonReceptionService`
  - `IFactureFournisseurService` → `FactureFournisseurService`

#### Nouveaux fichiers
| Fichier | Type |
|---|---|
| `Services/CommandeAchatService.cs` | Service |
| `Services/BonReceptionService.cs` | Service |
| `Services/FactureFournisseurService.cs` | Service |
| `Components/Pages/Fournisseurs/FournisseurForm.razor` | Page |
| `Components/Pages/CommandesAchat/CommandeAchatList.razor` | Page |
| `Components/Pages/CommandesAchat/CommandeAchatForm.razor` | Page |
| `Components/Pages/BonsReception/BonReceptionList.razor` | Page |
| `Components/Pages/BonsReception/BonReceptionForm.razor` | Page |
| `Components/Pages/FacturesFournisseur/FactureFournisseurList.razor` | Page |
| `Components/Pages/FacturesFournisseur/FactureFournisseurForm.razor` | Page |
| `Components/Pages/Entreprise/EntrepriseForm.razor` | Page |

#### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `Program.cs` | + 3 enregistrements DI |
| `TODO.md` | Mise à jour des tâches complétées |

---

## v0.3.0 — 2026-03-28 — Module VENTES complet

### Nouvelles fonctionnalités

#### Devis
- **Nouveau** : Page liste `/devis` — tableau avec N°, date, client, montants HT/TVA/TTC, état
- **Nouveau** : Formulaire `/devis/nouveau` — création avec lignes produits, calculs en temps réel
- **Nouveau** : Formulaire `/devis/{numero}` — modification de l'en-tête et des lignes
- **Nouveau** : Suppression avec dialogue de confirmation
- **Nouveau** : **Clonage** — crée un nouveau devis identique (nouveau numéro, date du jour, état = Ouvert)
- **Nouveau** : Service `IDevisClientService` / `DevisClientService` (GetAll, GetByNumero, Create, Update, Delete, Clone)
- **Nouveau** : Auto-remplissage des lignes depuis le produit (Prix HT, TVA)

#### Commandes Vente
- **Nouveau** : Page liste `/commandes-vente` — affiche état commande + état livraison
- **Nouveau** : Formulaire `/commandes-vente/nouveau` et `/commandes-vente/{numero}`
- **Nouveau** : Suppression avec confirmation
- **Nouveau** : **Clonage** — nouveau numéro, EtatCommandeVente = Ouvert, EtatLivraison = Non Livré
- **Nouveau** : Service `ICommandeVenteService` / `CommandeVenteService`
- **Nouveau** : Badge état livraison coloré (vert/orange/rouge)

#### Bons de Livraison
- **Nouveau** : Page liste `/bons-livraison` — affiche commande vente liée, état BL, état facturation
- **Nouveau** : Formulaire `/bons-livraison/nouveau` et `/bons-livraison/{numero}`
- **Nouveau** : Sélection de la commande vente associée (filtrée par client sélectionné)
- **Nouveau** : Suppression avec restitution automatique du stock
- **Nouveau** : **Clonage** — nouveau bon + décrément stock
- **Nouveau** : Service `IBonLivraisonService` / `BonLivraisonService` avec gestion complète du stock

#### Factures Client
- **Nouveau** : Formulaire `/factures-client/nouveau` — création complète avec FODEC, Timbre Fiscal
- **Nouveau** : Formulaire `/factures-client/{numero}` — modification + section règlements
- **Nouveau** : Section règlements sur les factures existantes (Date, Mode paiement, Montant, Référence)
- **Nouveau** : Affichage solde restant après règlements
- **Nouveau** : Calcul Net à Payer (TTC + Timbre)
- **Amélioré** : Bouton **Modifier** dans la liste (remplace l'icône œil)
- **Nouveau** : Bouton **Cloner** dans la liste et dans le formulaire

#### Avoirs
- **Nouveau** : Formulaire `/avoirs/nouveau` — création d'avoir (IsAvoir = true)
- **Nouveau** : Formulaire `/avoirs/{numero}` — modification + règlements
- **Nouveau** : Bouton **Cloner** dans la liste et dans le formulaire
- **Note** : Partage `FactureForm.razor` avec les factures (route détermine le type)

### Modifications techniques

#### Services mis à jour
- **`FactureClientService`** : ajout de la méthode `CloneAsync(string numero, bool isAvoir = false)`
  - Génère un nouveau numéro via `DocumentNumberService`
  - Copie client, lignes, remise, timbre
  - Réinitialise EtatReglement = "Non Réglé", EtatFacture = "Facture Ouverte"
  - Ne copie pas les règlements existants
  - Décrémente le stock pour les lignes clonées

#### Program.cs
- Enregistrement des 3 nouveaux services Scoped :
  - `IDevisClientService` → `DevisClientService`
  - `ICommandeVenteService` → `CommandeVenteService`
  - `IBonLivraisonService` → `BonLivraisonService`

#### Nouveaux fichiers
| Fichier | Type |
|---|---|
| `Services/DevisClientService.cs` | Service |
| `Services/CommandeVenteService.cs` | Service |
| `Services/BonLivraisonService.cs` | Service |
| `Components/Pages/Devis/DevisList.razor` | Page |
| `Components/Pages/Devis/DevisForm.razor` | Page |
| `Components/Pages/CommandesVente/CommandeVenteList.razor` | Page |
| `Components/Pages/CommandesVente/CommandeVenteForm.razor` | Page |
| `Components/Pages/BonsLivraison/BonLivraisonList.razor` | Page |
| `Components/Pages/BonsLivraison/BonLivraisonForm.razor` | Page |
| `Components/Pages/FacturesClient/FactureForm.razor` | Page |

#### Fichiers modifiés
| Fichier | Modification |
|---|---|
| `Services/FactureClientService.cs` | + interface `CloneAsync` + implémentation |
| `Program.cs` | + 3 enregistrements DI |
| `Components/Pages/FacturesClient/FacturesList.razor` | Bouton Modifier + bouton Cloner + méthode `CloneFacture` |

---

## v0.2.0 — 2026-03-26 — Base du module VENTES

### Nouvelles fonctionnalités

- **Clients** : liste, création, modification, suppression avec recherche
- **Produits** : liste, création, modification, suppression avec alertes stock visuelles
- **Fournisseurs** : liste, création, modification, suppression
- **Factures Client** (partiel) : liste avec suppression, sans formulaire de création/modification
- **État du Stock** : rapport analytique avec métriques (total références, alertes, valeur stock)
- **Avoirs** (partiel) : liste uniquement (filtre `IsAvoir=true`)

### Fondations techniques

- Mise en place du projet Blazor Server (.NET 8)
- Entity Framework Core avec SQL Server
- Migrations automatiques au démarrage (`db.Database.Migrate()`)
- Seed des données de référence (devises, TVA, modes paiement, unités, catégories)
- `DocumentNumberService` — numérotation séquentielle `{Préfixe}{AAAAMM}{###}`
- `AppConfigService` — lecture de `appsettings.json` (TimbreFiscal, TauxRetenue, etc.)
- Composants partagés : `Notification`, `ConfirmDialog`, `LoadingSpinner`
- Culture française (fr-FR) configurée globalement
- Navigation latérale avec sections VENTES / ACHATS / STOCK / PARAMÈTRES

---

## v0.1.0 — 2026-03-26 — Initialisation

- Création du projet `Web_GestCom`
- Définition de tous les modèles EF Core (28 entités)
- Migration initiale `InitialCreate`
- Structure des dossiers et configuration de base
