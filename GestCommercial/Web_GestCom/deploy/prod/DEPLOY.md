# Déploiement VPS — gestioncom.tijaraflow.fr

Application **Blazor Server (.NET 8)** déployée via Docker sur un VPS OVH.

---

## Architecture de déploiement

```
Internet (HTTPS 443)
       │
  nginx-proxy  ←─ acme-companion (Let's Encrypt auto)
       │
  gestcom-app  (Blazor Server, port interne 8080)
       │
  gestcom-sqlserver  (SQL Server 2022 Express)
       │
  Docker network : ntw_gestcom_prod
```

### Conteneurs

| Conteneur | Image | Rôle |
|---|---|---|
| `gestcom-nginx-proxy` | `nginxproxy/nginx-proxy:1.6` | Reverse-proxy HTTP/HTTPS |
| `gestcom-letsencrypt` | `nginxproxy/acme-companion:2.4` | Certificats SSL automatiques |
| `gestcom-sqlserver` | `mssql/server:2022-latest` | Base de données |
| `gestcom-app` | `gestcom/app:prod` | Application Blazor Server |

### Volumes nommés

| Volume | Contenu |
|---|---|
| `gestcom_certs` | Certificats Let's Encrypt |
| `gestcom_vhost` | Overrides nginx-proxy (vhost.d) |
| `gestcom_html` | Fichiers HTML statiques ACME |
| `gestcom_acme` | État interne acme.sh |
| `gestcom_sqldata` | Données SQL Server (persistantes) |

---

## Structure des fichiers

```
deploy/prod/
├── DEPLOY.md                          ← ce fichier
├── .env.example                       ← template (copier en .env, ne pas committer)
├── .env                               ← secrets réels (gitignored)
├── deploy_to_vps.ps1                  ← script d'orchestration principal
├── docker-compose.infra.yml           ← nginx-proxy + acme-companion + réseau
├── docker-compose_sql_prod.yml        ← SQL Server 2022
├── docker-compose.app.yml             ← application Blazor
└── vhost.d/
    └── gestioncom.tijaraflow.fr       ← override nginx (WebSocket SignalR)

Web_GestCom/
├── Dockerfile                         ← build multi-stage .NET 8
└── .dockerignore
```

---

## Prérequis locaux

- **Docker Desktop** en cours d'exécution
- **OpenSSH** disponible (`ssh`, `scp`, `ssh-keygen` dans le PATH)
- **PowerShell 7+** (`pwsh`)
- Accès SSH au VPS OVH (`vps-bf0b3440.vps.ovh.net`)

---

## Premier déploiement

### 1. Configurer la clé SSH (une seule fois)

```powershell
cd deploy\prod
.\deploy_to_vps.ps1 -Action fix-ssh
```

Génère `~/.ssh/id_rsa` si absent et installe la clé publique sur le VPS.
Les connexions suivantes seront sans mot de passe.

### 2. Créer le fichier `.env`

```powershell
copy .env.example .env
```

Éditer `.env` avec les vraies valeurs :

```env
APP_DOMAIN=gestioncom.tijaraflow.fr
LETSENCRYPT_EMAIL=admin@tijaraflow.fr
DB_SA_PASSWORD=MonMotDePasse_Str0ng!   # min 8 chars, maj + chiffre + spécial
DB_NAME=GestCom
```

> ⚠️ Ne jamais committer `.env` — il est dans `.gitignore`.

### 3. Déploiement complet

```powershell
.\deploy_to_vps.ps1
```

Ce `deploy-all` exécute dans l'ordre :
1. Sync des fichiers de config vers le VPS
2. Vérification des certificats SSL existants
3. Démarrage nginx-proxy + acme-companion (réseau créé ici)
4. Build de l'image Docker `gestcom/app:prod` en local
5. Transfer de l'image vers le VPS (docker save → scp → docker load)
6. Démarrage SQL Server + attente du healthcheck
7. Démarrage de l'application Blazor
8. Vérification finale (containers, réseau, certificat, logs)

Durée estimée : **5–10 minutes** (selon débit upload et taille image).

---

## Mise à jour applicative (déploiements suivants)

### Mise à jour standard (rebuild + redéploiement)

```powershell
.\deploy_to_vps.ps1 -Action deploy-app
```

Rebuilde l'image, la transfère et redémarre le container.

### Mise à jour sans rebuild (image déjà présente sur le VPS)

```powershell
.\deploy_to_vps.ps1 -Action deploy-app -SkipImagePush
```

### Rebuild image seulement (sans déployer)

```powershell
.\deploy_to_vps.ps1 -Action build-image
```

---

## Actions disponibles

```powershell
.\deploy_to_vps.ps1 -Action <action> [options]
```

| Action | Description |
|---|---|
| `fix-ssh` | Installer la clé SSH sur le VPS (une seule fois) |
| `build-image` | Construire `gestcom/app:prod` localement |
| `push-images` | Transférer l'image vers le VPS |
| `setup-network` | Créer réseau Docker + nginx-proxy + Let's Encrypt |
| `deploy-db` | Démarrer SQL Server uniquement |
| `deploy-app` | Build + push + déploiement Blazor |
| `deploy-all` | **Déploiement complet** (défaut) |
| `status` | État des containers + espace disque |
| `logs` | Logs du container `gestcom-app` (100 dernières lignes) |
| `syncFile` | Synchroniser uniquement les fichiers de config |
| `restart` | Redémarrer le container applicatif |
| `stop` | Arrêter tous les services |
| `cleanup` | Supprimer images/containers/cache inutilisés |
| `reset-all` | ⚠️ Tout supprimer (DANGEREUX — voir note Let's Encrypt) |

### Options

| Option | Défaut | Description |
|---|---|---|
| `-RemoteHost` | `vps-bf0b3440.vps.ovh.net` | Hostname VPS |
| `-RemoteUser` | `ubuntu` | Utilisateur SSH |
| `-SshKey` | `~/.ssh/id_rsa` | Clé SSH privée |
| `-RemotePath` | `/home/ubuntu/docker/gestcom` | Répertoire de travail sur le VPS |
| `-SkipImagePush` | `false` | Ne pas rebuilder/pousser l'image |

---

## Spécificités Blazor Server

### WebSocket / SignalR

Le fichier `vhost.d/gestioncom.tijaraflow.fr` configure nginx-proxy pour les circuits Blazor :

```nginx
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection $connection_upgrade;
proxy_read_timeout  3600s;
proxy_send_timeout  3600s;
client_max_body_size 10m;
```

Sans cet override, les connexions SignalR tombent après ~60s (timeout nginx par défaut).

### Base de données

La chaîne de connexion est injectée via variable d'environnement dans `docker-compose.app.yml` — elle **surcharge** `appsettings.json` :

```
ConnectionStrings__DefaultConnection=Server=gestcom-sqlserver;Database=...
```

EF Core exécute `EnsureCreated()` au premier démarrage → la base est créée automatiquement avec les migrations.

---

## Certificats SSL

Les certificats sont générés automatiquement par `acme-companion` via Let's Encrypt.

> ⚠️ **Rate limit Let's Encrypt : 5 certificats par domaine par semaine.**
> Éviter `reset-all` en production — les volumes `gestcom_certs` et `gestcom_acme` doivent être préservés.

Vérifier l'état du certificat :

```powershell
.\deploy_to_vps.ps1 -Action status
```

---

## Opérations manuelles sur le VPS

Se connecter :

```bash
ssh ubuntu@vps-bf0b3440.vps.ovh.net
cd /home/ubuntu/docker/gestcom
```

Commandes utiles :

```bash
# État des containers
docker ps -a

# Logs applicatifs en temps réel
docker logs -f gestcom-app

# Logs SQL Server
docker logs gestcom-sqlserver

# Redémarrer l'app sans perte de données
docker compose -f docker-compose.app.yml restart

# Accéder au shell SQL Server
docker exec -it gestcom-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$DB_SA_PASSWORD" -No

# Vérifier le réseau
docker network inspect ntw_gestcom_prod
```

---

## Dépannage

### L'application ne répond pas

```bash
docker logs --tail 50 gestcom-app
docker ps -a  # vérifier que le container est "Up"
```

### Erreur de connexion SQL Server

```bash
docker logs gestcom-sqlserver
docker inspect --format='{{.State.Health.Status}}' gestcom-sqlserver
```

Le container attend le healthcheck avant d'accepter des connexions (~60–90s au démarrage).

### Certificat SSL manquant

```bash
docker logs gestcom-letsencrypt
# Attendre 2–3 minutes après le premier démarrage
```

### SSH refusé

```powershell
.\deploy_to_vps.ps1 -Action fix-ssh
```

### Image Docker manquante sur le VPS

```powershell
.\deploy_to_vps.ps1 -Action build-image
.\deploy_to_vps.ps1 -Action push-images
```

---

## Sauvegarde des données

Les données SQL Server sont dans le volume `gestcom_sqldata`. Pour sauvegarder :

```bash
# Sur le VPS — dump SQL
docker exec gestcom-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$DB_SA_PASSWORD" -No \
  -Q "BACKUP DATABASE [GestCom] TO DISK = '/var/opt/mssql/backup.bak'"

# Rapatrier en local
scp ubuntu@vps-bf0b3440.vps.ovh.net:/var/opt/mssql/backup.bak ./backup.bak
```
