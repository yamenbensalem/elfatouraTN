#Requires -Version 7.0
<#
.SYNOPSIS
    Produit deploy/db-template/<TemplateDatabaseName>.bak — l'image de référence (schéma + données
    de référence + admin par défaut) restaurée chez chaque nouveau client par restore-database.ps1.

.DESCRIPTION
    Web_T4C_GestCom/Program.cs initialise sa base à chaque démarrage via EnsureCreated() suivi
    d'une cinquantaine d'ExecuteSqlRaw (migrations incrémentales idempotentes, ALTER TABLE / CREATE
    TABLE IF NOT EXISTS, accumulées dans un ordre précis) puis, si aucun utilisateur n'existe, crée
    l'admin par défaut (admin/admin123). Cette logique est la seule source de vérité pour "à quoi
    ressemble une base T4C_GestCom correctement initialisée" — ce script ne la réimplémente PAS
    (ce serait fragile : un bloc oublié ou mal ordonné livrerait un schéma cassé à un client). Il se
    contente de :
      1. Créer une base vide et jetable ($TemplateDatabaseName, jamais T4C_GestCom — voir le garde-
         fou ci-dessous) sur l'instance SQL Server donnée par -Server.
      2. Lancer l'exécutable déjà buildé de Web_T4C_GestCom avec la variable d'environnement
         ConnectionStrings__DefaultConnection pointée sur cette base vide — les variables
         d'environnement priment sur appsettings.*.json dans la pile de configuration ASP.NET Core
         standard, donc AUCUN fichier sous Web_T4C_GestCom/ n'est lu pour la chaîne de connexion ni
         modifié. Vérifié empiriquement pendant le développement de ce script (override pointé vers
         un host bogus => l'appli a bien tenté de s'y connecter, pas vers YAMEN/T4C_GestCom).
      3. Attendre que l'appli ait fini son travail de démarrage synchrone (tout se passe AVANT
         app.Run() dans Program.cs, donc dès que "Now listening on" apparaît dans les logs, la base
         est prête) puis tuer le processus — il n'a pas besoin de servir une seule requête HTTP.
      4. Sauvegarder la base avec BACKUP DATABASE natif SQL Server (réplique exactement schéma +
         données, zéro risque de réimplémentation) vers deploy/db-template/.
      5. Supprimer la base temporaire (sauf -KeepTemplateDatabase).

    SEED_MOCK_DATA n'est jamais positionné (et explicitement nettoyé de l'environnement avant le
    lancement) : Program.cs ne seed le gros jeu de données de démo que si MockData:Enabled ou
    SEED_MOCK_DATA vaut true/1, ce qu'on ne veut surtout pas dans une image livrée à un client.

.PARAMETER Server
    Instance SQL Server cible. Par défaut celle utilisée par Web_T4C_GestCom en Development
    (appsettings.Development.json -> Server=YAMEN au moment de l'écriture de ce script) — mais reste
    un paramètre, ne jamais supposer que ça restera YAMEN sur une autre machine.

.PARAMETER TemplateDatabaseName
    Nom de la base temporaire et jetable créée puis supprimée par ce script. Ne DOIT jamais être
    "T4C_GestCom" (la vraie base de dev) — le script refuse de démarrer si c'est le cas, puisqu'il
    DROP/CREATE cette base à chaque exécution.

.PARAMETER TimeoutSeconds
    Délai maximum d'attente du démarrage complet de Web_T4C_GestCom (EnsureCreated + ~50 migrations
    + seed HasData + création admin). Tout ce travail est synchrone et se termine typiquement en
    quelques secondes sur une base vide ; 30s laisse de la marge.

.PARAMETER Configuration
    Configuration de build utilisée pour dotnet build avant de lancer l'exe (Debug par défaut —
    suffisant ici, ce build n'est jamais livré, seul le .bak produit l'est).

.PARAMETER KeepTemplateDatabase
    Ne supprime pas $TemplateDatabaseName après la sauvegarde — pratique pour inspecter la base
    obtenue dans SSMS avant de faire confiance au .bak.

.EXAMPLE
    ./prepare-template-database.ps1
    Produit deploy/db-template/T4C_GestCom_Template.bak sur l'instance YAMEN.

.EXAMPLE
    ./prepare-template-database.ps1 -Server "MONPC\SQLEXPRESS" -KeepTemplateDatabase
    Utilise une autre instance et conserve la base temporaire pour inspection manuelle.
#>
param(
    [string]$Server = "YAMEN",
    [string]$TemplateDatabaseName = "T4C_GestCom_Template",
    [int]$TimeoutSeconds = 30,
    [string]$Configuration = "Debug",
    [switch]$KeepTemplateDatabase
)

$ErrorActionPreference = "Stop"

if ($TemplateDatabaseName -eq "T4C_GestCom") {
    throw "TemplateDatabaseName ne peut pas être 'T4C_GestCom' (la vraie base de dev/test) — " +
          "ce script DROP puis CREATE cette base à chaque exécution, ce serait destructeur. " +
          "Utilise le nom par défaut (T4C_GestCom_Template) ou un autre nom clairement 'jetable'."
}

# ── Résolution des outils et chemins ─────────────────────────────────────────
$sqlcmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source
if (-not $sqlcmd) {
    $fallback = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"
    if (Test-Path $fallback) {
        $sqlcmd = $fallback
        Write-Host "sqlcmd introuvable sur le PATH — utilisation du chemin connu : $sqlcmd" -ForegroundColor Yellow
    } else {
        throw "sqlcmd introuvable (ni sur le PATH, ni au chemin connu $fallback). Installe les outils " +
              "en ligne de commande SQL Server (mssql-tools / Client SDK ODBC) ou ajoute sqlcmd au PATH."
    }
}

$repoRoot      = Resolve-Path "$PSScriptRoot\..\..\.."   # .../GestCommercial
$webProjectDir = Join-Path $repoRoot "Web_T4C_GestCom"
$webCsproj     = Join-Path $webProjectDir "Web_T4C_GestCom.csproj"
if (-not (Test-Path $webCsproj)) {
    throw "Web_T4C_GestCom.csproj introuvable à '$webCsproj' — vérifie que ce script est toujours " +
          "sous T4C_GestCom_Desktop/deploy/sql/ dans le repo."
}

$backupDir  = Join-Path $PSScriptRoot "..\db-template" | Resolve-Path -ErrorAction SilentlyContinue
if (-not $backupDir) {
    $backupDir = New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot "..\db-template") -Force
}
$backupFile = Join-Path $backupDir "$TemplateDatabaseName.bak"

Write-Host "T4C_GestCom_Template — préparation de l'image de base modèle" -ForegroundColor Cyan
Write-Host "  Serveur cible      : $Server" -ForegroundColor Gray
Write-Host "  Base temporaire    : $TemplateDatabaseName" -ForegroundColor Gray
Write-Host "  Fichier de sortie  : $backupFile" -ForegroundColor Gray
Write-Host ""

# ── [1/5] Base vide et jetable ───────────────────────────────────────────────
Write-Host "==> [1/5] Création de la base vide '$TemplateDatabaseName' sur $Server" -ForegroundColor Cyan
$existsQuery = "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$TemplateDatabaseName') IS NULL THEN 0 ELSE 1 END"
# Important : capturer la sortie dans une variable AVANT de la filtrer avec Select-Object -First —
# piper directement une commande native dans Select-Object -First coupe le pipeline en avance
# (StopUpstreamCommandsException) et $LASTEXITCODE finit vide au lieu du vrai code de sqlcmd.
$existsOutput = & $sqlcmd -S $Server -E -C -h -1 -W -Q $existsQuery
if ($LASTEXITCODE -ne 0) { throw "Impossible de contacter l'instance SQL Server '$Server' via sqlcmd." }
$exists = ($existsOutput | Select-Object -First 1).Trim()

if ($exists -eq "1") {
    Write-Host "    '$TemplateDatabaseName' existe déjà (reliquat d'une exécution précédente) — suppression avant recréation." -ForegroundColor Yellow
    & $sqlcmd -S $Server -E -C -Q "ALTER DATABASE [$TemplateDatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$TemplateDatabaseName];" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Échec de la suppression de la base existante '$TemplateDatabaseName'." }
}
& $sqlcmd -S $Server -E -C -Q "CREATE DATABASE [$TemplateDatabaseName];" | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Échec de la création de la base '$TemplateDatabaseName' sur '$Server'." }
Write-Host "    OK." -ForegroundColor Green

# ── [2/5] Build de Web_T4C_GestCom ───────────────────────────────────────────
Write-Host "==> [2/5] Build de Web_T4C_GestCom ($Configuration)" -ForegroundColor Cyan
dotnet build $webCsproj -c $Configuration --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw "dotnet build de Web_T4C_GestCom a échoué." }

$exeCandidates = Get-ChildItem -Path (Join-Path $webProjectDir "bin\$Configuration") -Filter "Web_T4C_GestCom.exe" -Recurse -ErrorAction SilentlyContinue
if (-not $exeCandidates) {
    throw "Web_T4C_GestCom.exe introuvable sous bin\$Configuration\ après le build. Le build a peut-être échoué silencieusement."
}
$exePath = $exeCandidates[0].FullName
$exeDir  = $exeCandidates[0].DirectoryName
Write-Host "    OK — $exePath" -ForegroundColor Green

# ── [3/5] Laisser Program.cs faire tout le travail (EnsureCreated + migrations + seed) ──
Write-Host "==> [3/5] Lancement de Web_T4C_GestCom contre '$TemplateDatabaseName' (init DB uniquement, pas de requête HTTP)" -ForegroundColor Cyan

$connString = "Server=$Server;Database=$TemplateDatabaseName;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$stamp   = Get-Date -Format "yyyyMMdd-HHmmss"
$logFile = Join-Path $env:TEMP "prepare-template-db-$stamp.out.log"
$errFile = Join-Path $env:TEMP "prepare-template-db-$stamp.err.log"

# Sauvegarde/restauration de l'environnement du processus courant — jamais de fichier appsettings*.json touché.
$envKeys = @("ASPNETCORE_ENVIRONMENT", "ConnectionStrings__DefaultConnection", "SEED_MOCK_DATA", "MockData__Enabled")
$envBackup = @{}
foreach ($key in $envKeys) { $envBackup[$key] = [Environment]::GetEnvironmentVariable($key) }

$proc = $null
try {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ConnectionStrings__DefaultConnection = $connString
    # Ne jamais seeder le jeu de données de démo dans l'image livrée à un client.
    Remove-Item Env:\SEED_MOCK_DATA -ErrorAction SilentlyContinue
    Remove-Item Env:\MockData__Enabled -ErrorAction SilentlyContinue

    $proc = Start-Process -FilePath $exePath -WorkingDirectory $exeDir `
        -RedirectStandardOutput $logFile -RedirectStandardError $errFile `
        -PassThru -NoNewWindow

    Write-Host "    Processus démarré (PID $($proc.Id)), attente du démarrage complet (max ${TimeoutSeconds}s)..." -ForegroundColor Gray

    $ready = $false
    $elapsed = 0
    while ($elapsed -lt $TimeoutSeconds) {
        Start-Sleep -Seconds 1
        $elapsed++

        if ($proc.HasExited) {
            $stderrTail = if (Test-Path $errFile) { (Get-Content $errFile -Raw -ErrorAction SilentlyContinue) } else { "" }
            throw "Web_T4C_GestCom s'est arrêté prématurément (code $($proc.ExitCode)) pendant l'initialisation de '$TemplateDatabaseName'. Erreur :`n$stderrTail"
        }

        $content = if (Test-Path $logFile) { Get-Content $logFile -Raw -ErrorAction SilentlyContinue } else { "" }
        if ($content -match "Now listening on" -or $content -match "Application started") {
            $ready = $true
            break
        }
    }

    if (-not $ready) {
        throw "Timeout (${TimeoutSeconds}s) atteint sans détecter la fin de l'initialisation (ligne 'Now listening on' absente). " +
              "L'init DB (EnsureCreated + ~50 migrations SQL + seed) est peut-être plus lente que prévu — relance avec " +
              "-TimeoutSeconds plus grand, ou consulte $logFile / $errFile."
    }

    Write-Host "    OK — schéma + données de référence (devises, TVA, rôles, permissions) + admin par défaut créés." -ForegroundColor Green
}
finally {
    if ($proc -and -not $proc.HasExited) {
        Write-Host "    Arrêt du processus Web_T4C_GestCom (PID $($proc.Id))..." -ForegroundColor Gray
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    foreach ($key in $envKeys) {
        if ($null -eq $envBackup[$key]) {
            Remove-Item "Env:\$key" -ErrorAction SilentlyContinue
        } else {
            Set-Item "Env:\$key" $envBackup[$key]
        }
    }
}

# ── [4/5] Sauvegarde native SQL Server ───────────────────────────────────────
Write-Host "==> [4/5] BACKUP DATABASE '$TemplateDatabaseName' -> $backupFile" -ForegroundColor Cyan
if (Test-Path $backupFile) { Remove-Item $backupFile -Force }
$backupQuery = "BACKUP DATABASE [$TemplateDatabaseName] TO DISK = N'$backupFile' WITH INIT, STATS = 25;"
& $sqlcmd -S $Server -E -C -Q $backupQuery
if ($LASTEXITCODE -ne 0) { throw "BACKUP DATABASE a échoué pour '$TemplateDatabaseName'." }
if (-not (Test-Path $backupFile)) { throw "BACKUP DATABASE s'est terminé sans erreur mais le fichier attendu est absent : $backupFile" }
Write-Host "    OK." -ForegroundColor Green

# ── [5/5] Nettoyage ───────────────────────────────────────────────────────────
Write-Host "==> [5/5] Nettoyage" -ForegroundColor Cyan
if ($KeepTemplateDatabase) {
    Write-Host "    -KeepTemplateDatabase précisé — '$TemplateDatabaseName' est conservée sur $Server pour inspection." -ForegroundColor Yellow
} else {
    & $sqlcmd -S $Server -E -C -Q "ALTER DATABASE [$TemplateDatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$TemplateDatabaseName];" | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    ATTENTION : échec de la suppression de '$TemplateDatabaseName' sur $Server — à nettoyer manuellement." -ForegroundColor Red
    } else {
        Write-Host "    Base temporaire '$TemplateDatabaseName' supprimée (le .bak est l'artefact durable)." -ForegroundColor Green
    }
}
Remove-Item $logFile, $errFile -ErrorAction SilentlyContinue

# ── Résumé ────────────────────────────────────────────────────────────────────
$sizeMb = [Math]::Round((Get-Item $backupFile).Length / 1MB, 2)
Write-Host ""
Write-Host "Fichier produit : $backupFile ($sizeMb Mo)" -ForegroundColor Green
Write-Host ""
Write-Host "Rappel : relance ce script à chaque changement du schéma de Web_T4C_GestCom" -ForegroundColor Yellow
Write-Host "(Program.cs, Data/AppDbContext.cs, Data/Models/) — sinon ce .bak dérive silencieusement de la" -ForegroundColor Yellow
Write-Host "vraie base et un nouveau client recevrait un schéma obsolète. build-client-package.ps1 copie" -ForegroundColor Yellow
Write-Host "ce fichier tel quel dans chaque package client — régénère-le avant de construire un package" -ForegroundColor Yellow
Write-Host "si le schéma a bougé depuis la dernière fois." -ForegroundColor Yellow
