# ─────────────────────────────────────────────────────────────────────────────
# deploy_to_vps.ps1 — Déploiement GestCom Demo sur aivorconsulting.com
#
# Usage :
#   .\deploy_to_vps.ps1 -Action <action>
#
# Actions disponibles :
#   build         — Build l'image Docker en local
#   push-image    — Transfère l'image vers le VPS
#   deploy-db     — Démarre SQL Server + init DB
#   deploy-app    — Démarre le container Blazor
#   deploy-all    — build + push-image + deploy-db + deploy-app
#   nginx-config  — Copie la config nginx et recharge nginx sur le VPS
#   status        — État des containers sur le VPS
#   logs          — Logs de l'app Blazor
#   restart       — Redémarre l'app Blazor
#   reset-demo    — Réinitialise les données démo (recrée la DB + redémarre)
#   cleanup       — Supprime images et volumes inutilisés sur le VPS
#   setup-cron    — Configure le reset automatique quotidien des données démo
#   stop          — Arrête tous les containers
# ─────────────────────────────────────────────────────────────────────────────

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("build","push-image","deploy-db","deploy-app","deploy-all",
                 "nginx-config","status","logs","restart","reset-demo",
                 "cleanup","setup-cron","stop","fix-ssh")]
    [string]$Action
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─── Configuration ─────────────────────────────────────────────────────────────
$VPS_HOST      = "aivorconsulting.com"          # ou IP du VPS
$VPS_USER      = "ubuntu"
$SSH_KEY_PATH  = "$env:USERPROFILE\.ssh\id_rsa_aivor"
$VPS_DEPLOY_DIR = "/home/$VPS_USER/docker/gestcom-demo"

$IMAGE_NAME    = "gestcom/demo"
$IMAGE_TAG     = "prod"
$IMAGE_FULL    = "${IMAGE_NAME}:${IMAGE_TAG}"
$IMAGE_TAR     = "gestcom_demo_prod.tar"

$SCRIPT_DIR    = $PSScriptRoot   # __Delivery/deploy/demos.aivorconsulting/prod/
$DOCKERFILE    = Join-Path (Split-Path $SCRIPT_DIR -Parent) "Dockerfile"
$REPO_ROOT     = (Get-Item $SCRIPT_DIR).Parent.Parent.Parent.Parent.FullName

# ─── Helpers ───────────────────────────────────────────────────────────────────
function Write-Step([string]$msg) {
    Write-Host "`n━━━ $msg ━━━" -ForegroundColor Cyan
}

function Write-Ok([string]$msg) {
    Write-Host "  ✓ $msg" -ForegroundColor Green
}

function Write-Err([string]$msg) {
    Write-Host "  ✗ $msg" -ForegroundColor Red
}

$SSH_OPTS = @(
    "-i", $SSH_KEY_PATH,
    "-o", "StrictHostKeyChecking=no",
    "-o", "BatchMode=yes",
    "-o", "ConnectTimeout=15"
)

function Invoke-SSH([string]$cmd) {
    $result = ssh @SSH_OPTS "${VPS_USER}@${VPS_HOST}" $cmd
    if ($LASTEXITCODE -ne 0) { throw "SSH command failed: $cmd" }
    return $result
}

function Invoke-SCP([string]$src, [string]$dest) {
    scp @SSH_OPTS -r $src "${VPS_USER}@${VPS_HOST}:$dest"
    if ($LASTEXITCODE -ne 0) { throw "SCP failed: $src -> $dest" }
}

function Test-SSHKey {
    if (-not (Test-Path $SSH_KEY_PATH)) {
        Write-Err "Clé SSH introuvable : $SSH_KEY_PATH"
        Write-Host "  Exécuter : .\deploy_to_vps.ps1 -Action fix-ssh"
        exit 1
    }
}

# ─── Actions ───────────────────────────────────────────────────────────────────

function Invoke-FixSSH {
    Write-Step "Génération clé SSH dédiée aivorconsulting"
    if (-not (Test-Path $SSH_KEY_PATH)) {
        ssh-keygen -t rsa -b 4096 -f $SSH_KEY_PATH -N '""' -C "gestcom-deploy@aivorconsulting"
        Write-Ok "Clé générée : $SSH_KEY_PATH"
    } else {
        Write-Ok "Clé déjà existante : $SSH_KEY_PATH"
    }
    $pubKey = Get-Content "$SSH_KEY_PATH.pub"
    Write-Host "`n  Ajouter cette clé publique sur le VPS :"
    Write-Host "  (via le panneau d'administration ou manuellement)"
    Write-Host "`n  $pubKey" -ForegroundColor Yellow
    Write-Host "`n  Commande manuelle sur le VPS :"
    Write-Host "  echo '$pubKey' >> ~/.ssh/authorized_keys"
}

function Invoke-Build {
    Write-Step "Build image Docker $IMAGE_FULL"

    if (-not (Test-Path $DOCKERFILE)) {
        throw "Dockerfile introuvable : $DOCKERFILE"
    }

    # Build depuis la racine du repo (contexte = solution .NET)
    docker build `
        -f $DOCKERFILE `
        -t $IMAGE_FULL `
        --label "build.date=$(Get-Date -Format 'yyyy-MM-ddTHH:mm:ss')" `
        --label "build.app=gestcom-demo" `
        $REPO_ROOT

    if ($LASTEXITCODE -ne 0) { throw "docker build a échoué" }
    Write-Ok "Image $IMAGE_FULL construite"
}

function Invoke-PushImage {
    Write-Step "Transfert image $IMAGE_FULL vers VPS"
    Test-SSHKey

    # Vérifier que l'image existe localement
    $exists = docker image inspect $IMAGE_FULL 2>$null
    if (-not $exists) {
        throw "Image $IMAGE_FULL introuvable en local. Exécuter d'abord : -Action build"
    }

    Write-Host "  Sauvegarde en tar..."
    $tarPath = Join-Path $env:TEMP $IMAGE_TAR
    docker save -o $tarPath $IMAGE_FULL
    Write-Ok "Tar créé : $tarPath ($('{0:N0}' -f ((Get-Item $tarPath).Length / 1MB)) MB)"

    Write-Host "  Transfert SCP vers $VPS_DEPLOY_DIR..."
    Invoke-SSH "mkdir -p $VPS_DEPLOY_DIR/delivery"
    scp @SSH_OPTS $tarPath "${VPS_USER}@${VPS_HOST}:$VPS_DEPLOY_DIR/delivery/$IMAGE_TAR"
    if ($LASTEXITCODE -ne 0) { throw "SCP échoué" }

    Write-Host "  Chargement de l'image sur le VPS..."
    Invoke-SSH "docker load -i $VPS_DEPLOY_DIR/delivery/$IMAGE_TAR"
    Invoke-SSH "rm -f $VPS_DEPLOY_DIR/delivery/$IMAGE_TAR"

    Remove-Item $tarPath -Force
    Write-Ok "Image $IMAGE_FULL chargée sur le VPS"
}

function Invoke-SyncConfig {
    Write-Step "Synchronisation des fichiers de config vers le VPS"
    Test-SSHKey

    Invoke-SSH "mkdir -p $VPS_DEPLOY_DIR/nginx $VPS_DEPLOY_DIR/sql-init"

    # .env (obligatoire)
    $envFile = Join-Path $SCRIPT_DIR ".env"
    if (-not (Test-Path $envFile)) { throw ".env introuvable : $envFile" }
    Invoke-SCP $envFile "$VPS_DEPLOY_DIR/.env"

    # Docker composes
    Invoke-SCP (Join-Path $SCRIPT_DIR "docker-compose.app.yml") "$VPS_DEPLOY_DIR/"
    Invoke-SCP (Join-Path $SCRIPT_DIR "docker-compose.sql.yml") "$VPS_DEPLOY_DIR/"

    # SQL init
    Invoke-SCP (Join-Path $SCRIPT_DIR "sql-init\init-demo.sql") "$VPS_DEPLOY_DIR/sql-init/"

    Write-Ok "Fichiers synchronisés vers $VPS_DEPLOY_DIR"
}

function Invoke-DeployDB {
    Write-Step "Déploiement SQL Server + init DB"
    Test-SSHKey
    Invoke-SyncConfig

    Invoke-SSH "cd $VPS_DEPLOY_DIR && docker compose -f docker-compose.sql.yml --env-file .env up -d"

    # Attendre que db-init se termine
    Write-Host "  Attente initialisation DB (max 3 min)..."
    $maxWait = 180
    $waited  = 0
    do {
        Start-Sleep 10
        $waited += 10
        $status = Invoke-SSH "docker inspect -f '{{.State.Status}}' gestcom-demo-db-init 2>/dev/null || echo 'missing'"
        Write-Host "  [$waited s] db-init status: $status"
        if ($status -eq "exited") {
            $exit = Invoke-SSH "docker inspect -f '{{.State.ExitCode}}' gestcom-demo-db-init"
            if ($exit -eq "0") {
                Write-Ok "DB initialisée avec succès"
                return
            } else {
                throw "db-init a échoué (exit code $exit). Voir : docker logs gestcom-demo-db-init"
            }
        }
    } while ($waited -lt $maxWait)

    throw "Timeout : db-init n'a pas terminé en $maxWait secondes"
}

function Invoke-DeployApp {
    Write-Step "Déploiement container Blazor"
    Test-SSHKey
    Invoke-SyncConfig

    # Vérifier que l'image est présente sur le VPS
    $imgCheck = Invoke-SSH "docker image inspect $IMAGE_FULL >/dev/null 2>&1 && echo 'ok' || echo 'missing'"
    if ($imgCheck -eq "missing") {
        throw "Image $IMAGE_FULL absente du VPS. Exécuter d'abord : -Action push-image"
    }

    Invoke-SSH "cd $VPS_DEPLOY_DIR && docker compose -f docker-compose.app.yml --env-file .env up -d --force-recreate"

    Write-Host "  Attente démarrage Blazor (30 s)..."
    Start-Sleep 30

    $appStatus = Invoke-SSH "docker inspect -f '{{.State.Status}}' gestcom-demo-app 2>/dev/null || echo 'missing'"
    if ($appStatus -ne "running") {
        throw "Container gestcom-demo-app n'est pas running (status: $appStatus)"
    }
    Write-Ok "Container gestcom-demo-app running"

    # Vérifier le health check
    $health = Invoke-SSH "docker inspect -f '{{.State.Health.Status}}' gestcom-demo-app 2>/dev/null || echo 'none'"
    Write-Host "  Health: $health"
}

function Invoke-DeployAll {
    Invoke-Build
    Invoke-PushImage
    Invoke-DeployDB
    Invoke-DeployApp
    Write-Step "Déploiement complet terminé"
    Write-Ok "URL : https://aivorconsulting.com/demos/gestionscommercial"
    Invoke-NginxConfig
}

function Invoke-NginxConfig {
    Write-Step "Configuration nginx (location block)"
    Test-SSHKey

    $nginxDir = Join-Path $SCRIPT_DIR "nginx"
    $locationConf  = Join-Path $nginxDir "gestcom-demo.location.conf"
    $mapConf       = Join-Path $nginxDir "map-upgrade.conf"

    # Copier les fichiers de config nginx
    Invoke-SSH "mkdir -p $VPS_DEPLOY_DIR/nginx"
    Invoke-SCP $locationConf "$VPS_DEPLOY_DIR/nginx/gestcom-demo.location.conf"
    Invoke-SCP $mapConf      "$VPS_DEPLOY_DIR/nginx/map-upgrade.conf"

    Write-Host ""
    Write-Host "  ─── ACTION MANUELLE REQUISE ───────────────────────────────────" -ForegroundColor Yellow
    Write-Host "  Les fichiers de config nginx ont été copiés sur le VPS dans :"   -ForegroundColor Yellow
    Write-Host "    $VPS_DEPLOY_DIR/nginx/"                                         -ForegroundColor White
    Write-Host ""
    Write-Host "  1. Éditer la config nginx d'aivorconsulting.com :"               -ForegroundColor Yellow
    Write-Host "     sudo nano /etc/nginx/sites-available/aivorconsulting.com"     -ForegroundColor White
    Write-Host "     OU si nginx via Docker :"                                      -ForegroundColor Yellow
    Write-Host "     nano /path/to/nginx/conf.d/aivorconsulting.conf"              -ForegroundColor White
    Write-Host ""
    Write-Host "  2. Dans le bloc server{} HTTPS, ajouter include :"               -ForegroundColor Yellow
    Write-Host "     include $VPS_DEPLOY_DIR/nginx/gestcom-demo.location.conf;"    -ForegroundColor White
    Write-Host ""
    Write-Host "  3. Dans le bloc http{} global, ajouter (une seule fois) :"       -ForegroundColor Yellow
    Write-Host "     include $VPS_DEPLOY_DIR/nginx/map-upgrade.conf;"              -ForegroundColor White
    Write-Host ""
    Write-Host "  4. Recharger nginx :"                                             -ForegroundColor Yellow
    Write-Host "     sudo nginx -t && sudo systemctl reload nginx"                 -ForegroundColor White
    Write-Host "     OU Docker : docker exec <nginx-container> nginx -s reload"    -ForegroundColor White
    Write-Host "  ──────────────────────────────────────────────────────────────" -ForegroundColor Yellow
}

function Invoke-Status {
    Write-Step "État des containers GestCom Demo"
    Test-SSHKey
    Invoke-SSH "docker ps -a --filter 'name=gestcom-demo' --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'"
    Write-Host ""
    Invoke-SSH "docker stats --no-stream --filter 'name=gestcom-demo' --format 'table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}' 2>/dev/null || true"
}

function Invoke-Logs {
    Write-Step "Logs gestcom-demo-app (100 dernières lignes)"
    Test-SSHKey
    Invoke-SSH "docker logs --tail 100 -f gestcom-demo-app"
}

function Invoke-Restart {
    Write-Step "Redémarrage gestcom-demo-app"
    Test-SSHKey
    Invoke-SSH "docker restart gestcom-demo-app"
    Write-Ok "Container redémarré"
}

function Invoke-ResetDemo {
    Write-Step "Réinitialisation données démo"
    Test-SSHKey

    Write-Host "  Suppression du marqueur d'init DB..."
    Invoke-SSH "docker volume rm gestcom_demo_db_init_status 2>/dev/null || true"

    Write-Host "  Suppression de la base de données..."
    Invoke-SSH @"
docker exec gestcom-demo-sqlserver /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa \
    -P `$(docker exec gestcom-demo-sqlserver bash -c 'echo \$MSSQL_SA_PASSWORD') \
    -Q "DROP DATABASE IF EXISTS [GestComDemoDB]" -b 2>/dev/null || true
"@

    Write-Host "  Relance db-init..."
    Invoke-SSH "cd $VPS_DEPLOY_DIR && docker compose -f docker-compose.sql.yml --env-file .env run --rm gestcom-db-init"

    Write-Host "  Redémarrage app Blazor..."
    Invoke-SSH "docker restart gestcom-demo-app"

    Write-Ok "Données démo réinitialisées"
}

function Invoke-SetupCron {
    Write-Step "Configuration cron reset quotidien (2h UTC)"
    Test-SSHKey

    $cronScript = "$VPS_DEPLOY_DIR/reset-demo.sh"
    $cronLine   = "0 2 * * * $cronScript >> $VPS_DEPLOY_DIR/reset-demo.log 2>&1"

    Invoke-SSH @"
cat > $cronScript << 'SCRIPT'
#!/bin/bash
# Reset automatique des données démo GestCom
set -e
cd $VPS_DEPLOY_DIR

echo "[\$(date)] Début reset démo..."

# Supprimer marqueur init
docker volume rm gestcom_demo_db_init_status 2>/dev/null || true

# Supprimer la base
docker exec gestcom-demo-sqlserver /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa \
    -P "\$(docker exec gestcom-demo-sqlserver bash -c 'echo \$MSSQL_SA_PASSWORD')" \
    -Q "DROP DATABASE IF EXISTS [GestComDemoDB]" -b

# Relancer init
docker compose -f docker-compose.sql.yml --env-file .env run --rm gestcom-db-init

# Redémarrer l'app
docker restart gestcom-demo-app

echo "[\$(date)] Reset démo terminé."
SCRIPT
chmod +x $cronScript
"@

    # Ajouter au crontab sans doublon
    Invoke-SSH "(crontab -l 2>/dev/null | grep -v 'gestcom-demo\|reset-demo' ; echo '$cronLine') | crontab -"
    Write-Ok "Cron configuré : reset quotidien à 2h00 UTC"
    Write-Host "  Script : $cronScript"
    Write-Host "  Log    : $VPS_DEPLOY_DIR/reset-demo.log"
}

function Invoke-Cleanup {
    Write-Step "Nettoyage Docker sur le VPS"
    Test-SSHKey
    Invoke-SSH "docker image prune -f"
    Invoke-SSH "docker container prune -f"
    Invoke-SSH "docker volume prune -f --filter 'label!=keep'"
    Invoke-SSH "df -h /var/lib/docker 2>/dev/null || df -h /"
    Write-Ok "Nettoyage terminé"
}

function Invoke-Stop {
    Write-Step "Arrêt de tous les containers GestCom Demo"
    Test-SSHKey
    Invoke-SSH "cd $VPS_DEPLOY_DIR && docker compose -f docker-compose.app.yml --env-file .env down 2>/dev/null || true"
    Invoke-SSH "cd $VPS_DEPLOY_DIR && docker compose -f docker-compose.sql.yml --env-file .env down 2>/dev/null || true"
    Write-Ok "Containers arrêtés"
}

# ─── Dispatch ──────────────────────────────────────────────────────────────────
switch ($Action) {
    "fix-ssh"      { Invoke-FixSSH }
    "build"        { Invoke-Build }
    "push-image"   { Invoke-PushImage }
    "deploy-db"    { Invoke-DeployDB }
    "deploy-app"   { Invoke-DeployApp }
    "deploy-all"   { Invoke-DeployAll }
    "nginx-config" { Invoke-NginxConfig }
    "status"       { Invoke-Status }
    "logs"         { Invoke-Logs }
    "restart"      { Invoke-Restart }
    "reset-demo"   { Invoke-ResetDemo }
    "setup-cron"   { Invoke-SetupCron }
    "cleanup"      { Invoke-Cleanup }
    "stop"         { Invoke-Stop }
}
