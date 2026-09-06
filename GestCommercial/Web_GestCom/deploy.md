# Deploy — GestCom Web

## Prérequis

| Composant | Version |
|---|---|
| OS | Windows 10/11 · Windows Server 2019/2022 |
| Runtime | [ASP.NET Core **8.0** Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Base de données | SQL Server 2019/2022 (Express suffisant) |

> Le **Hosting Bundle** installe à la fois le Runtime .NET 8 et le module IIS ASP.NET Core.

---

## 1. Construire le package de livraison

```bash
dotnet publish Web_GestCom.csproj \
  -c Release \
  -r win-x64 \
  --self-contained false \
  -o ./publish
```

---

## 2. Configurer la connexion SQL

Modifier **`publish/appsettings.Production.json`** :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=MON_SERVEUR;Database=GestCom;User Id=SA;Password=***;TrustServerCertificate=True"
  },
  "AppConfig": {
    "TimbreFiscal": "0.6",
    "TauxRetenue": "1.5",
    "DisplayRemise": "Yes",
    "DisplayTVA": "Yes",
    "PathLogo": "./logoApp.png"
  }
}
```

Exemples de chaînes de connexion :

```
# SQL Server avec authentification SQL
Server=192.168.1.10;Database=GestCom;User Id=gestcom;Password=***;TrustServerCertificate=True

# Instance locale (authentification Windows)
Server=.\SQLEXPRESS;Database=GestCom;Trusted_Connection=True;TrustServerCertificate=True
```

> La migration de base de données et la création des tables s'effectuent **automatiquement** au premier démarrage.

---

## 3A. Démarrage direct (Kestrel)

```bat
cd publish
set ASPNETCORE_ENVIRONMENT=Production
Web_GestCom.exe --urls "http://0.0.0.0:5000"
```

Accès : **`http://ADRESSE_SERVEUR:5000`**

---

## 3B. Service Windows

```bat
sc create GestCom ^
  binPath= "C:\GestCom\publish\Web_GestCom.exe --urls http://0.0.0.0:5000" ^
  start= auto ^
  DisplayName= "GestCom Web"

sc start GestCom
```

Pour définir l'environnement Production sur le service :

```bat
reg add "HKLM\SYSTEM\CurrentControlSet\Services\GestCom\Parameters" ^
  /v "AppParameters" /t REG_SZ /d "--urls http://0.0.0.0:5000"
reg add "HKLM\SYSTEM\CurrentControlSet\Services\GestCom\Parameters\AppSettings" ^
  /v "ASPNETCORE_ENVIRONMENT" /t REG_SZ /d "Production"
```

---

## 3C. Déploiement IIS

1. Installer le **ASP.NET Core Hosting Bundle 8.0**
2. IIS Manager → **Add Website**
   - Physical path : `C:\inetpub\GestCom\publish\`
   - Port : `80`
3. Application Pool → **No Managed Code**
4. Ajouter la variable d'environnement dans IIS :
   - Configuration Editor → `system.webServer/aspNetCore` → `environmentVariables`
   - `ASPNETCORE_ENVIRONMENT` = `Production`

Le fichier `web.config` inclus dans le publish configure automatiquement le module ASP.NET Core.

---

## 4. Première connexion

| Login | Mot de passe |
|---|---|
| `admin` | `admin123` |

> ⚠️ Changer le mot de passe immédiatement : **Administration → Utilisateurs → admin**

Puis renseigner la fiche entreprise : **Paramètres → Entreprise**

---

## 5. Mise à jour

```bash
# 1. Construire le nouveau publish
dotnet publish Web_GestCom.csproj -c Release -r win-x64 --self-contained false -o ./publish

# 2. Arrêter le service
sc stop GestCom

# 3. Remplacer les fichiers (sauf appsettings.Production.json et logoApp.png)
robocopy publish\ C:\GestCom\publish\ /MIR /XF appsettings.Production.json logoApp.png

# 4. Redémarrer
sc start GestCom
```

La migration de base de données s'applique automatiquement au redémarrage.

---

## Variables d'environnement

| Variable | Valeur | Rôle |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Active `appsettings.Production.json`, désactive les pages d'erreur détaillées |
| `ASPNETCORE_URLS` | `http://0.0.0.0:5000` | Peut remplacer `--urls` |

---

## Dépannage rapide

| Symptôme | Cause probable | Solution |
|---|---|---|
| HTTP 500 au démarrage | Mauvaise chaîne de connexion | Vérifier `appsettings.Production.json` |
| HTTP 502.5 (IIS) | Runtime .NET 8 absent | Installer le Hosting Bundle 8.0 |
| Page de login en boucle | Cookie non persisté | Vérifier que le port n'est pas bloqué par un proxy |
| "Cannot open database" | SQL Server inaccessible | Vérifier le service SQL Server et le firewall (port 1433) |
| Migration échoue | Droits insuffisants | Donner `db_owner` au compte SQL sur `GestCom` |
