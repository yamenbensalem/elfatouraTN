# Plan : Système de Connexion & Traçabilité pour T4C GestCom

## Ce qui existe déjà (ne pas recréer)

- `Entity/User.cs` — stub partiel (incomplet, pas de hachage de mot de passe)
- `Forms/Login.cs` — formulaire de login existant mais mal routé (va vers `AdministratorPanel` au lieu d'`Accueil`)
- `DAL/VariablesGlobales.cs` — déjà `ApplicationWithLogin = false` (il suffit de l'activer)
- `DAL/GlobalMessages.cs` — messages utilisateur déjà présents
- `Program.cs` — le flux de login est **commenté** (lignes 98-101), prêt à être décommenté

---

## Phase 1 — Base de données & Constantes DAL

### Nouvelles tables à créer dans le fichier `.mdb` (Access)

**Table `utilisateur`**
```sql
id_utilisateur        AUTOINCREMENT  PK
login_utilisateur     TEXT(50)   NOT NULL
password_utilisateur  TEXT(255)  NOT NULL  -- hash SHA-256
prenom_utilisateur    TEXT(50)   NOT NULL
nom_utilisateur       TEXT(50)   NOT NULL
email_utilisateur     TEXT(100)
role_utilisateur      TEXT(20)   NOT NULL  -- "Admin" / "Utilisateur"
actif_utilisateur     TEXT(3)    NOT NULL  -- "OUI" / "NON"
datecreation_utilisateur TEXT(20) NOT NULL
```

**Table `journalactivite`**
```sql
id_journal          AUTOINCREMENT  PK
login_journal       TEXT(50)   NOT NULL
action_journal      TEXT(100)  NOT NULL  -- "Ajout","Modification","Suppression","Connexion"
entite_journal      TEXT(50)   NOT NULL  -- "Client","Produit","FactureClient"...
code_entite_journal TEXT(50)             -- clé primaire de l'enregistrement affecté
date_journal        TEXT(20)   NOT NULL  -- dd/MM/yyyy
heure_journal       TEXT(10)   NOT NULL  -- HH:mm:ss
detail_journal      TEXT(255)
```

### Fichiers DAL à modifier

| Fichier | Changement |
|---|---|
| `DataBaseTableName.cs` | `TableUser = "utilisateur"` + ajouter `TableJournalActivite = "journalactivite"` |
| `DataBaseSQLQuery.cs` | Ajouter `requeteUtilisateurs`, `requeteUtilisateursActifs`, `requeteJournalActivite`, `requeteJournalActiviteParLogin(string)` |
| `GlobalMessages.cs` | Ajouter messages journal, inscription, admin |
| `VariablesGlobales.cs` | Ajouter `CurrentUser`, `RoleAdmin = "Admin"`, `RoleUtilisateur = "Utilisateur"` |

---

## Phase 2 — Couche Entity

### `Entity/Utilisateur.cs` (réécriture complète de `User.cs`)

Méthodes à implémenter selon le pattern existant :
- `ajouterUtilisateur()` — INSERT avec liste de colonnes explicite (AUTOINCREMENT)
- `modifierUtilisateur()` — UPDATE par `id_utilisateur`
- `desactiverUtilisateur(int _id)` / `activerUtilisateur(int _id)` — pas de DELETE physique
- `modifierMotDePasse(int _id, string _hashedPassword)` — UPDATE uniquement le mot de passe
- `authentifier(string _login, string _password)` — SELECT + comparaison SHA-256, retourne `Utilisateur` ou `null`
- `loginExists(string _login)` — COUNT pour validation avant inscription
- `getAllUtilisateurs()` — retourne `ArrayList`
- `hashPassword(string)` — private, SHA-256 via `System.Security.Cryptography.SHA256`

### `Entity/JournalActivite.cs` (nouvelle entité)

- `enregistrer()` — INSERT dans `journalactivite`
- `static enregistrerActivite(string action, string entite, string codeEntite, string detail)` — méthode de commodité statique qui lit `VariablesGlobales.CurrentUser`, injecte date/heure, et appelle `enregistrer()`. **Ne lève jamais d'exception** (le journal ne doit jamais crasher l'application)
- `getAllJournal()`, `getAllJournalByLogin(string)` — SELECT

---

## Phase 3 — Interface Connexion & Inscription

### `Forms/Login.cs` — Corrections

- Le handler `save_bn_Click` doit appeler `Utilisateur.authentifier()` (au lieu de l'ancienne `User.isExistUser()`)
- En cas de succès : stocker dans `VariablesGlobales.CurrentUser`, enregistrer l'activité `"Connexion"`, fermer avec `DialogResult.OK`
- Ajouter un bouton **"Créer un compte"** qui ouvre `Inscription` en `ShowDialog`

### `Forms/Inscription.cs` — Nouveau formulaire

Champs : prénom, nom, email, login, mot de passe, confirmer mot de passe

Validation :
1. Champs obligatoires non vides
2. Mot de passe >= 6 caractères
3. Confirmation identique
4. Login non déjà utilisé (`loginExists()`)
5. Rôle forcé à `"Utilisateur"` (seul un Admin peut créer des comptes Admin)

### `Program.cs` — Activer le flux de login

Décommenter et corriger les lignes 98-101 :
```csharp
if (VariablesGlobales.ApplicationWithLogin == true)
{
    Login loginForm = new Login();
    if (loginForm.ShowDialog() != DialogResult.OK)
        return;  // fermeture sans connexion → quitter l'app
}
Application.Run(getAccueilForm());
```

Ajouter un **seed du premier admin** si la table `utilisateur` est vide :
```
Login: admin / Mot de passe: admin123
```

### `Forms/Accueil.cs` — Modifications

- Afficher le nom de l'utilisateur connecté dans le titre de la fenêtre
- Ajouter un menu **"Administration"** avec :
  - Gestion Utilisateurs *(Admin uniquement)*
  - Journal d'Activité *(Admin uniquement)*
  - Déconnecter

---

## Phase 4 — Traçabilité des Activités

Ajouter `JournalActivite.enregistrerActivite()` dans les méthodes des entités **(non dans les formulaires)**, après chaque opération réussie :

```csharp
// Exemple dans Entity/Client.cs
bool result = DataBaseConnexion.addOrUpdateElementInDataBase(sql, errMsg);
if (result)
    JournalActivite.enregistrerActivite("Ajout", "Client", this.code_client, this.nom_client);
return result;
```

**Ordre de priorité d'instrumentation :**
1. `FactureClient` (Ajout / Modification / Suppression)
2. `BonLivraison`
3. `Client`, `Produit`, `Fournisseur`
4. `ReglementFacture`, `DevisClient`, `BonReception`
5. Toutes les autres entités (38 au total)

---

## Phase 5 — Ecran d'Administration

### `Forms/Utilisateurs.cs` + `Forms/AddOrUpdateUtilisateur.cs`

Ecran liste (pattern `Clients.cs`) avec DataGridView, boutons Ajouter / Modifier / Activer-Désactiver.
Pas de suppression physique — désactivation uniquement (intégrité du journal).
Accessible aux **Admins uniquement**.

### Extension de `Forms/AdministratorPanel.cs`

Ajouter deux onglets :
- **Gestion Utilisateurs** — héberge `Utilisateurs`
- **Journal d'Activité** — DataGridView avec filtres par utilisateur / entité / plage de dates

---

## Fichiers à créer (nouveaux)

| Fichier | Rôle |
|---|---|
| `Entity/Utilisateur.cs` | Entité utilisateur complète |
| `Entity/JournalActivite.cs` | Entité journal d'audit |
| `Forms/Inscription.cs` + `.Designer.cs` | Formulaire d'inscription |
| `Forms/Utilisateurs.cs` + `.Designer.cs` | Liste de gestion des utilisateurs |
| `Forms/AddOrUpdateUtilisateur.cs` + `.Designer.cs` | Dialogue création/édition utilisateur |

---

## Fichiers à modifier (existants)

| Fichier | Changement |
|---|---|
| `DAL/DataBaseTableName.cs` | `TableUser = "utilisateur"` + `TableJournalActivite` |
| `DAL/DataBaseSQLQuery.cs` | Requêtes utilisateurs et journal |
| `DAL/GlobalMessages.cs` | Messages journal, inscription, admin |
| `DAL/VariablesGlobales.cs` | `CurrentUser`, `RoleAdmin`, `RoleUtilisateur` |
| `DAL/Program.cs` | Activer flux login + seed premier admin |
| `Entity/User.cs` | Remplacer par `Utilisateur.cs` |
| `Forms/Login.cs` | Corriger routing + stocker `CurrentUser` + bouton Inscription |
| `Forms/AdministratorPanel.cs` | Ajouter onglets journal + utilisateurs |
| `Forms/Accueil.cs` | Menu Administration + titre avec utilisateur + déconnexion |
| `Entity/Client.cs` | Ajouter appels `JournalActivite.enregistrerActivite()` |
| `Entity/Produit.cs` | Idem |
| `Entity/FactureClient.cs` | Idem |
| `Entity/BonLivraison.cs` | Idem |
| `Entity/Fournisseur.cs` | Idem |
| `Entity/FactureFournisseur.cs` | Idem |
| `Entity/BonReception.cs` | Idem |
| `Entity/DevisClient.cs` | Idem |
| `Entity/ReglementFacture.cs` | Idem |

---

## Séquence d'implémentation recommandée

```
1.  Créer les tables dans le .mdb (Access)
2.  DAL : DataBaseTableName + DataBaseSQLQuery + GlobalMessages + VariablesGlobales
3.  Entity/Utilisateur.cs + Entity/JournalActivite.cs
4.  Tester les entités (queries de base)
5.  Corriger Forms/Login.cs + créer Forms/Inscription.cs
6.  Activer le flux dans Program.cs + seed admin
7.  Tester le login de bout en bout
8.  Instrumenter les entités prioritaires (traçabilité)
9.  Créer Forms/Utilisateurs.cs + AddOrUpdateUtilisateur.cs
10. Etendre AdministratorPanel.cs (journal + users)
11. Ajouter menu Administration dans Accueil.cs
```
