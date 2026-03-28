# T4C GestCom – Application Web

Application de gestion commerciale en Blazor Server (.NET 8) avec Entity Framework Core et SQL Server.

---

## Table des matières

1. [Stack technique](#stack-technique)
2. [Architecture](#architecture)
3. [Configuration](#configuration)
4. [Démarrage](#démarrage)
5. [Modules implémentés](#modules-implémentés)
   - [VENTES](#ventes)
   - [ACHATS](#achats)
   - [STOCK](#stock)
   - [PARAMÈTRES](#paramètres)
6. [Services](#services)
7. [Composants partagés](#composants-partagés)
8. [Modèles de données](#modèles-de-données)
9. [Numérotation des documents](#numérotation-des-documents)
10. [Gestion du stock](#gestion-du-stock)

---

## Stack technique

| Couche | Technologie |
|---|---|
| Framework | ASP.NET Core 8.0 – Blazor Interactive Server |
| ORM | Entity Framework Core 8.0 |
| Base de données | SQL Server (migrations automatiques au démarrage) |
| UI | Bootstrap 5 + Bootstrap Icons |
| Langage | C# 12 |
| Culture | Français (fr-FR) |

---

## Architecture

```
Components/Pages/          ← Pages Blazor (UI)
    ├── Clients/
    ├── Devis/
    ├── CommandesVente/
    ├── BonsLivraison/
    ├── FacturesClient/
    ├── Fournisseurs/
    ├── Produits/
    └── Stock/
Components/Shared/         ← Composants réutilisables
    ├── ConfirmDialog.razor
    ├── LoadingSpinner.razor
    └── Notification.razor
Services/                  ← Couche métier (interfaces + implémentations)
Data/
    ├── Models/            ← Entités EF Core
    └── AppDbContext.cs    ← Contexte EF Core
Migrations/                ← Migrations EF Core
```

**Flux de données :**

```
Page Blazor  →  @inject IService  →  Service  →  AppDbContext  →  SQL Server
```

---

## Configuration

Fichier `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=T4C_GestCom;Trusted_Connection=True;"
  },
  "AppConfig": {
    "TimbreFiscal": "0.6",
    "TauxRetenue":  "1.5",
    "DisplayRemise": "Yes",
    "DisplayTVA":    "Yes",
    "PathLogo":      "./logoApp.png"
  }
}
```

| Paramètre | Description | Défaut |
|---|---|---|
| `TimbreFiscal` | Montant du timbre fiscal ajouté aux factures | `0.6` |
| `TauxRetenue` | Taux de retenue à la source (%) | `1.5` |
| `DisplayRemise` | Afficher les colonnes remise | `Yes` |
| `DisplayTVA` | Afficher les colonnes TVA | `Yes` |
| `PathLogo` | Chemin vers le logo de l'entreprise | `./logoApp.png` |

---

## Démarrage

```bash
# Restaurer les dépendances
dotnet restore Web_T4C_GestCom.sln

# Lancer l'application (migrations appliquées automatiquement)
dotnet run --project Web_T4C_GestCom

# Build production
dotnet build --configuration Release
```

La base de données est créée et migrée automatiquement au premier démarrage.

---

## Modules implémentés

### VENTES

#### Clients `/clients`

Gestion complète des clients.

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /clients` | Tableau avec recherche (nom, code, téléphone) |
| Créer | `GET /clients/nouveau` | Formulaire de création |
| Modifier | `GET /clients/{code}` | Formulaire de modification |
| Supprimer | — | Bouton dans la liste avec confirmation |

**Champs gérés :** Code (auto CL#####), Nom, Matricule Fiscale, Type Personne (Physique/Morale), Devise, Étranger, Exonéré TVA, Adresse complète, Téléphone/Mobile/Fax/Email, Crédit Maximum, RIB, Responsable, État (Actif/Inactif), Note.

---

#### Devis `/devis`

Gestion des devis clients avec lignes de produits.

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /devis` | Tableau avec numéro, date, client, montants, état |
| Créer | `GET /devis/nouveau` | Formulaire avec lignes produits |
| Modifier | `GET /devis/{numero}` | Modification de l'en-tête et des lignes |
| Supprimer | — | Confirmation dans la liste |
| Cloner | — | Crée un nouveau devis identique (date = aujourd'hui) |

**En-tête :** N° auto (DV + AAAAMM + ###), Date, Client, Remise globale (%), État (Ouvert/Confirmé/Annulé), Note.
**Lignes :** Produit (sélection avec auto-remplissage prix/TVA), Quantité, Prix HT, Remise %, TVA %, Montant HT.
**Totaux :** Total HT, Remise, TVA, Total TTC (calculés en temps réel).
**Impact stock :** Aucun.

---

#### Commandes Vente `/commandes-vente`

Gestion des commandes clients.

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /commandes-vente` | Tableau avec état commande et état livraison |
| Créer | `GET /commandes-vente/nouveau` | Formulaire avec lignes |
| Modifier | `GET /commandes-vente/{numero}` | Modification |
| Supprimer | — | Avec confirmation |
| Cloner | — | Nouveau numéro, statut réinitialisé |

**En-tête :** N° auto (CV + AAAAMM + ###), Date, Client, Remise %, État (Ouvert/Confirmé/Annulé), État Livraison (Non Livré/Partiellement Livré/Livré), Note.
**Impact stock :** Aucun.

---

#### Bons de Livraison `/bons-livraison`

Gestion des bons de livraison clients. **Impacte le stock.**

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /bons-livraison` | Tableau avec commande liée, état et état facturation |
| Créer | `GET /bons-livraison/nouveau` | Formulaire avec sélection commande vente optionnelle |
| Modifier | `GET /bons-livraison/{numero}` | Modification (stock recalculé) |
| Supprimer | — | Restitution automatique du stock |
| Cloner | — | Nouveau bon + décrément stock |

**En-tête :** N° auto (BL + AAAAMM + ###), Date, Client, Commande Vente associée (optionnel, filtrée par client), Remise %, État, État Facturation (Non Facturé/Facturé), Note.
**Impact stock :** Décrément à la création, restitution à la suppression/modification.

---

#### Factures Client `/factures-client`

Gestion des factures clients avec règlements. **Impacte le stock.**

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /factures-client` | Tableau avec état facture et état règlement |
| Créer | `GET /factures-client/nouveau` | Formulaire complet |
| Modifier | `GET /factures-client/{numero}` | Modification + saisie des règlements |
| Supprimer | — | Restitution stock + suppression règlements |
| Cloner | — | Nouveau numéro, règlements non copiés |

**En-tête :** N° auto (FC + AAAAMM + ###), Date, Client, Remise %, État Facture, Timbre Fiscal (depuis config), Note.
**Lignes :** Produit, Quantité, Prix HT, Remise %, TVA %, FODEC % (auto-rempli depuis le produit), Montant HT.
**Totaux :** HT, Remise, FODEC, TVA, TTC, Timbre, **Net à Payer**.
**Règlements :** Ajout de règlements (Date, Mode de paiement, Montant, Référence) sur les factures existantes. État règlement calculé automatiquement (Non Réglé / Partiellement Réglé / Réglé).
**Impact stock :** Décrément à la création, restitution à la suppression/modification.

---

#### Avoirs `/avoirs`

Fonctionnement identique aux Factures Client avec le flag `IsAvoir = true`.

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /avoirs` | Liste des avoirs uniquement |
| Créer | `GET /avoirs/nouveau` | Formulaire avoir |
| Modifier | `GET /avoirs/{numero}` | Modification avoir |
| Supprimer | — | Avec restitution stock |
| Cloner | — | Clone en avoir |

---

### ACHATS

#### Fournisseurs `/fournisseurs`

Gestion complète des fournisseurs.

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /fournisseurs` | Tableau avec recherche |
| Créer | Bouton dans liste | Formulaire modal ou page |
| Modifier | — | Formulaire de modification |
| Supprimer | — | Avec confirmation |

> **Note :** Les modules Commandes Achat, Bons de Réception et Factures Fournisseur sont planifiés (modèles DB créés, routes dans la navigation). Voir `TODO.md`.

---

### STOCK

#### Produits `/produits`

Gestion du catalogue produits.

| Opération | Route | Description |
|---|---|---|
| Liste | `GET /produits` | Tableau avec alertes stock (rouge si sous le minimum) |
| Créer | `GET /produits/nouveau` | Formulaire complet |
| Modifier | `GET /produits/{code}` | Modification |
| Supprimer | — | Avec confirmation |

**Champs :** Code (auto PR#####), Désignation, Prix Unitaire, Devise, Catégorie, Fabricant, Unité, Prix Achat TTC, Taux Marge, Prix Vente HT, Prix Vente TTC, Remise maximale, TVA, FODEC, Stock actuel, Stock minimal, Fournisseur, Rayon, Étage.

**Alertes :** Ligne rouge si `Quantite <= StockMinimal`.

---

#### État du Stock `/stock`

Rapport analytique du stock.

| Indicateur | Description |
|---|---|
| Total références | Nombre total de produits |
| En alerte | Produits sous le seuil minimal |
| Valeur stock PA | Valeur totale au prix d'achat TTC |
| Valeur stock PV | Valeur totale au prix de vente HT |

Filtres disponibles : recherche texte, catégorie, afficher uniquement les alertes.

---

### PARAMÈTRES

> **Note :** Le module Entreprise (`/entreprise`) est planifié. Voir `TODO.md`.

---

## Services

| Service | Interface | Responsabilité |
|---|---|---|
| `ClientService` | `IClientService` | CRUD clients |
| `ProduitService` | `IProduitService` | CRUD produits + gestion stock |
| `FournisseurService` | `IFournisseurService` | CRUD fournisseurs |
| `DevisClientService` | `IDevisClientService` | CRUD + Clone devis |
| `CommandeVenteService` | `ICommandeVenteService` | CRUD + Clone commandes vente |
| `BonLivraisonService` | `IBonLivraisonService` | CRUD + Clone BL (avec stock) |
| `FactureClientService` | `IFactureClientService` | CRUD + Clone + Règlements (avec stock) |
| `DocumentNumberService` | *(sans interface)* | Génération numéros séquentiels |
| `AppConfigService` | *(singleton)* | Lecture configuration applicative |

---

## Composants partagés

| Composant | Usage |
|---|---|
| `<Notification>` | Affiche des alertes (succès, erreur, warning, info) – auto-dismiss |
| `<ConfirmDialog>` | Modal de confirmation avant suppression |
| `<LoadingSpinner>` | Indicateur de chargement pendant les opérations async |

---

## Modèles de données

### Données de référence (seeded automatiquement)

| Table | Données initiales |
|---|---|
| `Devise` | TND (1.0), EUR (3.3), USD (3.1) |
| `ModePayement` | Espèces, Chèque, Virement, Effet, À terme |
| `TvaProduit` | 19%, 13%, 7%, Exonéré (0%) |
| `UniteProduit` | Unité, Kg, Litre, Mètre, Boîte |
| `CategorieProduit` | Général |
| `FabriquantProduit` | Divers |

### Documents de vente

```
DevisClient (DV…)
  └── LigneDevisClient

CommandeVente (CV…)
  └── LigneCommandeVente
  └── BonLivraison (optionnel)

BonLivraison (BL…)
  └── LigneBonLivraison
  └─► CommandeVente (FK optionnel)

FactureClient (FC…)    [IsAvoir=false] ou Avoir [IsAvoir=true]
  └── LigneFactureClient
  └── ReglementFactureClient
```

### Documents d'achat (modèles créés, UI à implémenter)

```
CommandeAchat (CA…)
  └── LigneCommandeAchat

BonReception (BR…)
  └── LigneBonReception

FactureFournisseur (FF…)
  └── LigneFactureFournisseur
  └── ReglementFactureFournisseur
```

---

## Numérotation des documents

Format : `{Préfixe}{AAAAMM}{###}` — séquentiel par mois, remis à 001 chaque nouveau mois.

| Type | Préfixe | Exemple |
|---|---|---|
| Devis | `DV` | `DV202603001` |
| Commande Vente | `CV` | `CV202603001` |
| Bon de Livraison | `BL` | `BL202603001` |
| Facture Client | `FC` | `FC202603001` |
| Commande Achat | `CA` | `CA202603001` |
| Bon de Réception | `BR` | `BR202603001` |
| Facture Fournisseur | `FF` | `FF202603001` |

---

## Gestion du stock

La gestion du stock est automatique et transparente :

| Événement | Impact |
|---|---|
| Création Bon de Livraison | Décrément stock (quantité × chaque ligne) |
| Modification Bon de Livraison | Restitution anciennes lignes + décrément nouvelles lignes |
| Suppression Bon de Livraison | Restitution stock toutes lignes |
| Clone Bon de Livraison | Décrément stock (nouveau document) |
| Création Facture Client / Avoir | Décrément stock |
| Modification Facture Client / Avoir | Restitution + Décrément |
| Suppression Facture Client / Avoir | Restitution stock + suppression règlements |
| Clone Facture Client / Avoir | Décrément stock (nouveau document) |
| Devis / Commande Vente | **Aucun impact stock** |
