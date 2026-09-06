# Déploiement GestCom Demo — aivorconsulting.com

**URL cible** : `https://aivorconsulting.com/demos/gestionscommercial`

Site de démonstration de GestCom Web hébergé en sous-chemin du site aivorconsulting.com.

---

## Architecture

```
Internet HTTPS
    ↓
nginx aivorconsulting.com:443  (SSL existant, géré par le site principal)
    └── location /demos/gestionscommercial  ── proxy_pass ──► gestcom-demo-app:8080 (Blazor Server)
                                                                       ↓
                                                           gestcom-demo-sqlserver:1433 (SQL Server 2019 Express)
```

### Différences avec le déploiement tunisiaauto.tn

| Aspect | tunisiaauto.tn | aivorconsulting (demo) |
|---|---|---|
| Routage | Virtual host (domaine) | Subpath `/demos/gestionscommercial` |
| SSL | nginx-proxy + Let's Encrypt dédié | SSL existant du site principal |
| Conteneurs app | Angular (frontend) + ASP.NET Core (API) | Blazor Server unique (frontend + backend) |
| nginx-proxy | Requis (container dédié) | Non requis (location block dans nginx existant) |
| Données | Production | Démo (reset quotidien à 2h UTC) |

---

## Prérequis

- Docker + Docker Compose v2 installés sur le VPS
- SSH passwordless configuré (voir `fix-ssh`)
- L'adresse IP/hostname `aivorconsulting.com` pointe vers le bon VPS
- nginx déjà en place et fonctionnel sur ce VPS pour `aivorconsulting.com`

---

## Modifications requises dans l'application

### 1. `Program.cs` — PathBase et ForwardedHeaders

Ajouter **avant** `app.UseRouting()` :

```csharp
// Support subpath /demos/gestionscommercial
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                     | ForwardedHeaders.XForwardedProto
                     | ForwardedHeaders.XForwardedHost
});

var basePath = builder.Configuration["APP_BASE_PATH"] ?? "";
if (!string.IsNullOrEmpty(basePath))
{
    app.UsePathBase(basePath);
}
```

### 2. `Components/App.razor` — base href dynamique

Remplacer `<base href="/" />` par :

```razor
<base href="@(Configuration["APP_BASE_PATH"]?.TrimEnd('/') + "/")" />
```

Ou, plus simple, définir la valeur fixe pour la démo :

```html
<base href="/demos/gestionscommercial/" />
```

### 3. `appsettings.Production.json` — variable d'environnement suffisante

La variable `APP_BASE_PATH=/demos/gestionscommercial` est injectée via docker-compose.
Aucune modification de `appsettings.json` requise.

---

## Déploiement pas à pas

Toutes les commandes s'exécutent depuis le dossier `prod/` avec PowerShell.

### Première installation

```powershell
# 1. Configurer la clé SSH (une seule fois)
.\deploy_to_vps.ps1 -Action fix-ssh
# → Copier manuellement la clé publique affichée dans ~/.ssh/authorized_keys du VPS

# 2. Déploiement complet (build image, push, DB, app)
.\deploy_to_vps.ps1 -Action deploy-all

# 3. Configurer nginx sur le VPS (voir instructions affichées)
.\deploy_to_vps.ps1 -Action nginx-config

# 4. Configurer le reset automatique quotidien des données
.\deploy_to_vps.ps1 -Action setup-cron
```

### Mise à jour de l'application

```powershell
# Rebuilder et redéployer uniquement l'app (DB inchangée)
.\deploy_to_vps.ps1 -Action build
.\deploy_to_vps.ps1 -Action push-image
.\deploy_to_vps.ps1 -Action deploy-app
```

### Opérations courantes

```powershell
.\deploy_to_vps.ps1 -Action status        # État des containers
.\deploy_to_vps.ps1 -Action logs          # Logs Blazor en temps réel
.\deploy_to_vps.ps1 -Action restart       # Redémarrer l'app
.\deploy_to_vps.ps1 -Action reset-demo    # Réinitialiser les données démo maintenant
.\deploy_to_vps.ps1 -Action cleanup       # Nettoyer Docker sur le VPS
.\deploy_to_vps.ps1 -Action stop          # Arrêter tous les containers
```

---

## Configuration nginx (manuel)

Le script `nginx-config` copie les fichiers sur le VPS et affiche les instructions.
La modification nginx est **manuelle** car elle touche la config du site principal.

### Fichiers copiés sur le VPS

```
$VPS_DEPLOY_DIR/nginx/
├── gestcom-demo.location.conf   ← bloc location{} à inclure dans server{}
└── map-upgrade.conf             ← directive map{} pour WebSocket (dans http{})
```

### Ce qu'il faut ajouter dans la config nginx aivorconsulting.com

**Dans le bloc `http{}` global** (une seule fois, souvent `/etc/nginx/nginx.conf`) :
```nginx
include /home/ubuntu/docker/gestcom-demo/nginx/map-upgrade.conf;
```

**Dans le bloc `server{}` HTTPS** (`server_name aivorconsulting.com`) :
```nginx
include /home/ubuntu/docker/gestcom-demo/nginx/gestcom-demo.location.conf;
```

Puis recharger nginx :
```bash
sudo nginx -t && sudo systemctl reload nginx
# ou si nginx via Docker :
docker exec <nginx-container> nginx -s reload
```

---

## Comptes de démonstration

| Rôle | Email | Mot de passe |
|---|---|---|
| SuperAdmin | admin@demo.gestcom | Demo#Admin2024 |
| Manager | manager@demo.gestcom | Demo#Manager2024 |
| Employé | employe@demo.gestcom | Demo#Employe2024 |

> Les mots de passe sont dans `prod/.env`. Changer avant le premier déploiement.

---

## Réinitialisation automatique des données

Un cron s'exécute chaque nuit à **2h UTC** :
1. Supprime la base SQL Server
2. Relance `db-init` pour recréer la base
3. L'app au redémarrage re-seed les données démo (`SEED_MOCK_DATA=true`)

Log : `/home/ubuntu/docker/gestcom-demo/reset-demo.log`

---

## Fichiers de configuration

```
demos.aivorconsulting/
├── Dockerfile                        ← Build image Blazor (contexte = racine du repo)
├── README-deploy.md                  ← Ce fichier
└── prod/
    ├── .env                          ← Variables (NE PAS committer dans git)
    ├── docker-compose.app.yml        ← Container Blazor Server
    ├── docker-compose.sql.yml        ← SQL Server 2019 Express + init
    ├── deploy_to_vps.ps1             ← Script de déploiement
    ├── nginx/
    │   ├── gestcom-demo.location.conf  ← Bloc location nginx (subpath + SignalR)
    │   └── map-upgrade.conf            ← Map WebSocket upgrade
    └── sql-init/
        └── init-demo.sql               ← Init DB + login (données seedées par l'app)
```

---

## Dépannage

### L'app retourne 404 sur `/demos/gestionscommercial`
- Vérifier que `include gestcom-demo.location.conf` est bien dans le bon bloc `server{}` HTTPS
- Vérifier que le container `gestcom-demo-app` est `running` : `docker ps`
- Vérifier que `APP_PORT=5200` dans `.env` et que `proxy_pass http://127.0.0.1:5200` correspond

### Blazor se charge mais les navigations plantent (404)
- `UsePathBase` manquant ou incorrect dans `Program.cs`
- La `<base href>` dans `App.razor` ne correspond pas au subpath

### SignalR/WebSocket déconnecté immédiatement
- Le bloc `map $http_upgrade $connection_upgrade` est absent du `http{}` global
- Les timeouts nginx sont trop courts (vérifier `proxy_read_timeout 3600s`)

### La DB ne s'initialise pas
- `docker logs gestcom-demo-db-init` pour voir l'erreur
- Supprimer le volume et relancer : `docker volume rm gestcom_demo_db_init_status`

### Rebuild complet
```powershell
.\deploy_to_vps.ps1 -Action stop
# Sur le VPS manuellement :
# docker volume rm gestcom_demo_sqldata gestcom_demo_db_init_status
.\deploy_to_vps.ps1 -Action deploy-all
```
