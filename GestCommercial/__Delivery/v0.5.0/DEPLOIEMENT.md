# Guide de Déploiement – T4C GestCom Web v0.5.0

## Prérequis Serveur

| Composant | Version minimale | Notes |
|---|---|---|
| Windows Server | 2019 / 2022 | ou Windows 10/11 Pro |
| .NET Runtime ASP.NET Core | **8.0.x** | Télécharger sur [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| SQL Server | 2019 / 2022 | Express édition suffisante |
| IIS (optionnel) | 10+ | Avec module **ASP.NET Core** (Hosting Bundle) |

> **Important** : Télécharger le **ASP.NET Core 8 Hosting Bundle** (inclut Runtime + IIS Module) :
> https://dotnet.microsoft.com/download/dotnet/8.0

---

## Contenu du Package

```
app/
├── Web_T4C_GestCom.exe          ← Exécutable principal (démarrage direct)
├── Web_T4C_GestCom.dll          ← Assembly .NET
├── appsettings.json             ← Configuration de base (ne pas modifier)
├── appsettings.Production.json  ← ⚠️ À CONFIGURER (chaîne de connexion SQL)
├── web.config                   ← Configuration IIS (si déploiement IIS)
├── wwwroot/                     ← Ressources statiques (CSS, JS, images)
└── ...                          ← Bibliothèques .NET
```

---

## Étape 1 — Configurer la Base de Données

1. Ouvrir **SQL Server Management Studio**
2. Créer une base de données nommée `T4C_GestCom` (optionnel — créée automatiquement au démarrage)
3. S'assurer que le compte SQL a les droits **db_owner** sur la base

---

## Étape 2 — Configurer la Chaîne de Connexion

Ouvrir `app\appsettings.Production.json` et modifier :

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=NOM_SERVEUR;Database=T4C_GestCom;User Id=SA;Password=VOTRE_MOT_DE_PASSE;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Exemples de chaînes de connexion :**

```
# Authentification SQL (recommandé en production)
Server=192.168.1.10;Database=T4C_GestCom;User Id=t4c_user;Password=MonMotDePasse;TrustServerCertificate=True

# Authentification Windows (même serveur)
Server=.\SQLEXPRESS;Database=T4C_GestCom;Trusted_Connection=True;TrustServerCertificate=True

# Instance nommée
Server=MONSRVEUR\SQLEXPRESS;Database=T4C_GestCom;User Id=SA;Password=***;TrustServerCertificate=True
```

---

## Étape 3 — Paramètres Fiscaux

Dans `appsettings.Production.json`, ajuster si nécessaire :

```json
"AppConfig": {
  "TimbreFiscal": "0.6",    ← Timbre fiscal par document (TND)
  "TauxRetenue": "1.5",     ← Taux de retenue à la source (%)
  "DisplayRemise": "Yes",   ← Afficher colonne Remise
  "DisplayTVA": "Yes",      ← Afficher colonne TVA
  "PathLogo": "./logoApp.png" ← Chemin vers le logo entreprise
}
```

Pour le logo : copier votre fichier logo dans `app\` en le nommant `logoApp.png`.

---

## Étape 4A — Démarrage Direct (sans IIS)

Lancer l'application en mode production :

```bat
cd app
set ASPNETCORE_ENVIRONMENT=Production
Web_T4C_GestCom.exe --urls "http://0.0.0.0:5000"
```

Accès : `http://ADRESSE_IP_SERVEUR:5000`

Pour démarrage automatique en tant que **Service Windows** :

```bat
sc create T4CGestCom binPath= "D:\GestCom\app\Web_T4C_GestCom.exe --urls http://0.0.0.0:5000" start= auto
sc start T4CGestCom
```

---

## Étape 4B — Déploiement sur IIS

### Créer le site IIS

1. Ouvrir **IIS Manager**
2. Clic droit sur **Sites** → **Add Website**
   - Site name : `T4C GestCom`
   - Physical path : `C:\inetpub\T4CGestCom\app\`
   - Port : `80` (ou le port souhaité)
3. **Application Pool** → Mettre **No Managed Code**
4. Copier le dossier `app\` vers `C:\inetpub\T4CGestCom\`

### web.config (déjà inclus)

Le fichier `web.config` fourni configure le module ASP.NET Core pour IIS.

### Définir la variable d'environnement Production

Dans IIS Manager → Site → **Configuration Editor** :
`system.webServer/aspNetCore` → `environmentVariables` → ajouter :
- `ASPNETCORE_ENVIRONMENT` = `Production`

---

## Étape 5 — Première Connexion

Au premier démarrage, l'application :
1. Crée automatiquement toutes les tables en base
2. Crée un utilisateur administrateur par défaut :

| Login | Mot de passe |
|---|---|
| `admin` | `admin123` |

> ⚠️ **Changer le mot de passe admin immédiatement** après la première connexion
> via **Administration → Utilisateurs → admin → Changer le mot de passe**

---

## Étape 6 — Configurer la Fiche Entreprise

Naviguer vers **Paramètres → Entreprise** et renseigner :
- Nom, Matricule Fiscale, Adresse, Téléphone, Email
- Ces informations apparaîtront sur les documents imprimés

---

## Dépannage

| Problème | Solution |
|---|---|
| Page blanche / erreur 500 | Vérifier la chaîne de connexion SQL dans `appsettings.Production.json` |
| "Cannot open database" | Vérifier que SQL Server est démarré et accessible |
| Port déjà utilisé | Changer le port dans la commande `--urls` |
| IIS 502.5 | Vérifier que le ASP.NET Core Hosting Bundle 8.0 est installé |
| Erreur de migration | Vérifier les droits du compte SQL sur la base de données |

---

## Mise à Jour

Pour mettre à jour vers une version ultérieure :
1. Arrêter le service / site IIS
2. Sauvegarder `appsettings.Production.json` et `logoApp.png`
3. Remplacer le contenu du dossier `app\` par la nouvelle version
4. Restaurer `appsettings.Production.json` et `logoApp.png`
5. Redémarrer le service / site IIS

La migration de base de données s'applique **automatiquement** au démarrage.

---

*T4C GestCom Web v0.5.0 — 2026-03-28*
