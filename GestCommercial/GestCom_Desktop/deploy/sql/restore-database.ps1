#Requires -Version 7.0
<#
.SYNOPSIS
    Restaure la base GestCom initiale (schéma + données de référence + admin par défaut) sur
    la machine du client, à partir du .bak produit par prepare-template-database.ps1.

.DESCRIPTION
    Exécuté chez le client (ou par Yamen à distance, RDP/AnyDesk) sur l'instance SQL Server locale,
    AVANT create-app-login.sql (qui a besoin que la base GestCom existe déjà pour y créer le
    login applicatif dessus).

    Étapes :
      1. RESTORE FILELISTONLY sur le .bak pour découvrir les noms logiques réels des fichiers data/
         log embarqués dedans (jamais codés en dur ici — ils peuvent varier selon la machine/instance
         qui a produit le .bak).
      2. Résout le dossier data par défaut de l'instance cible via
         SERVERPROPERTY('InstanceDefaultDataPath') / ('InstanceDefaultLogPath').
      3. RESTORE DATABASE ... WITH MOVE (un MOVE par fichier logique découvert), RECOVERY.

.PARAMETER BackupFile
    Chemin du .bak à restaurer. Par défaut : Database\GestCom_Template.bak à côté de ce script
    (c'est l'emplacement où build-client-package.ps1 le copie dans le package livré au client).

.PARAMETER TargetDatabaseName
    Nom de la base restaurée. Par défaut "GestCom" — c'est le nom attendu par
    GestCom_Desktop/appsettings.json (ConnectionStrings:DefaultConnection).

.PARAMETER Server
    Instance SQL Server locale sur laquelle restaurer. Par défaut "." (instance par défaut de la
    machine locale — c'est la machine du client, l'app et la base tournent dessus, voir DEPLOY.md).

.PARAMETER Force
    Nécessaire si une base nommée $TargetDatabaseName existe déjà — sans ce switch, le script refuse
    d'écraser une base existante (pourrait être une vraie base client avec des données réelles).

.EXAMPLE
    ./restore-database.ps1
    Restaure Database\GestCom_Template.bak vers la base "GestCom" sur l'instance locale.

.EXAMPLE
    ./restore-database.ps1 -BackupFile "C:\Temp\GestCom_Template.bak" -Server ".\SQLEXPRESS" -Force
#>
param(
    [string]$BackupFile = (Join-Path $PSScriptRoot "GestCom_Template.bak"),
    [string]$TargetDatabaseName = "GestCom",
    [string]$Server = ".",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# ── Résolution des outils et chemins ─────────────────────────────────────────
$sqlcmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source
if (-not $sqlcmd) {
    $fallback = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"
    if (Test-Path $fallback) {
        $sqlcmd = $fallback
        Write-Host "sqlcmd introuvable sur le PATH — utilisation du chemin connu : $sqlcmd" -ForegroundColor Yellow
    } else {
        throw "sqlcmd introuvable (ni sur le PATH, ni au chemin connu $fallback). Installe SQL Server " +
              "(qui embarque sqlcmd) ou les outils en ligne de commande SQL Server séparément."
    }
}

if (-not (Test-Path $BackupFile)) {
    throw "Fichier de sauvegarde introuvable : $BackupFile`n" +
          "Si tu construis le package toi-même, lance d'abord deploy\sql\prepare-template-database.ps1 " +
          "puis deploy\build-client-package.ps1 pour qu'il soit copié ici."
}
$BackupFile = (Resolve-Path $BackupFile).Path

Write-Host "GestCom — restauration de la base initiale" -ForegroundColor Cyan
Write-Host "  Fichier source : $BackupFile" -ForegroundColor Gray
Write-Host "  Serveur cible  : $Server" -ForegroundColor Gray
Write-Host "  Base cible     : $TargetDatabaseName" -ForegroundColor Gray
Write-Host ""

# ── Garde-fou : ne jamais écraser une base existante sans -Force ────────────
Write-Host "==> [1/4] Vérification que '$TargetDatabaseName' n'existe pas déjà sur $Server" -ForegroundColor Cyan
$existsQuery = "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$TargetDatabaseName') IS NULL THEN 0 ELSE 1 END"
# Important : capturer la sortie AVANT de la filtrer avec Select-Object -First — piper directement
# une commande native dans Select-Object -First coupe le pipeline en avance (StopUpstreamCommands-
# Exception) et $LASTEXITCODE finit vide au lieu du vrai code de sqlcmd.
$existsOutput = & $sqlcmd -S $Server -E -C -h -1 -W -Q $existsQuery
if ($LASTEXITCODE -ne 0) {
    throw "Impossible de contacter l'instance SQL Server '$Server' via sqlcmd. Vérifie que SQL Server " +
          "est installé et démarré sur cette machine, et que -Server pointe sur la bonne instance " +
          "(ex: '.', '.\SQLEXPRESS', 'NOMMACHINE\SQLEXPRESS')."
}
$exists = ($existsOutput | Select-Object -First 1).Trim()

if ($exists -eq "1" -and -not $Force) {
    throw "La base '$TargetDatabaseName' existe déjà sur '$Server'. Ce script refuse de l'écraser sans " +
          "-Force — elle contient peut-être déjà des données réelles du client. Si tu es certain de " +
          "vouloir repartir de zéro (nouvelle installation, base de test), relance avec -Force."
}
if ($exists -eq "1" -and $Force) {
    Write-Host "    '$TargetDatabaseName' existe déjà — -Force précisé, la base sera écrasée." -ForegroundColor Yellow
} else {
    Write-Host "    OK — aucune base '$TargetDatabaseName' existante." -ForegroundColor Green
}

# ── Découverte des noms logiques réels dans le .bak ──────────────────────────
Write-Host "==> [2/4] Lecture des noms logiques des fichiers embarqués dans le .bak (RESTORE FILELISTONLY)" -ForegroundColor Cyan
$fileListQuery = "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'$BackupFile';"
$fileListRaw = & $sqlcmd -S $Server -E -C -s "|" -W -Q $fileListQuery
if ($LASTEXITCODE -ne 0) { throw "RESTORE FILELISTONLY a échoué — le fichier est-il un .bak SQL Server valide ?" }

# Colonnes utiles : LogicalName(0), Type(2, 'D'=data / 'L'=log)
$logicalFiles = @()
foreach ($line in $fileListRaw) {
    if ($line -match '^-+(\|-+)*$') { continue }              # ligne de séparateurs sqlcmd
    $cols = $line -split '\|'
    if ($cols.Count -lt 3) { continue }
    $logicalName = $cols[0].Trim()
    $fileType    = $cols[2].Trim()
    if ([string]::IsNullOrWhiteSpace($logicalName)) { continue }
    if ($logicalName -eq "LogicalName") { continue }           # ligne d'en-tête
    if ($fileType -notin @("D", "L")) { continue }
    $logicalFiles += [PSCustomObject]@{ LogicalName = $logicalName; Type = $fileType }
}
if ($logicalFiles.Count -eq 0) {
    throw "Impossible d'extraire les noms logiques depuis RESTORE FILELISTONLY. Sortie brute :`n$($fileListRaw -join "`n")"
}
Write-Host "    Fichiers trouvés : $(($logicalFiles | ForEach-Object { "$($_.LogicalName) ($($_.Type))" }) -join ', ')" -ForegroundColor Gray

# ── Résolution des dossiers data/log par défaut de l'instance cible ─────────
Write-Host "==> [3/4] Résolution des dossiers data/log par défaut de l'instance '$Server'" -ForegroundColor Cyan
$dataPathOutput = & $sqlcmd -S $Server -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(400))"
$logPathOutput  = & $sqlcmd -S $Server -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath')  AS NVARCHAR(400))"
$dataPath = ($dataPathOutput | Select-Object -First 1).Trim()
$logPath  = ($logPathOutput  | Select-Object -First 1).Trim()

if ([string]::IsNullOrWhiteSpace($dataPath) -or $dataPath -eq "NULL" -or [string]::IsNullOrWhiteSpace($logPath) -or $logPath -eq "NULL") {
    throw "SERVERPROPERTY('InstanceDefaultDataPath'/'InstanceDefaultLogPath') a renvoyé NULL — cette " +
          "propriété n'existe pas sur les versions de SQL Server antérieures à 2016. Sur cette machine, " +
          "détermine manuellement le dossier DATA de l'instance (SSMS -> clic droit sur le serveur -> " +
          "Propriétés -> Paramètres de la base de données, ou l'emplacement des .mdf d'une base " +
          "existante) et relance ce script avec un RESTORE manuel, ou adapte ce script avec le chemin " +
          "en dur pour cette machine."
}
Write-Host "    Dossier data : $dataPath" -ForegroundColor Gray
Write-Host "    Dossier log  : $logPath" -ForegroundColor Gray

# ── RESTORE DATABASE ──────────────────────────────────────────────────────────
Write-Host "==> [4/4] RESTORE DATABASE '$TargetDatabaseName'" -ForegroundColor Cyan
$moveClauses = foreach ($f in $logicalFiles) {
    $extension = if ($f.Type -eq "L") { "ldf" } else { "mdf" }
    $suffix    = if ($f.Type -eq "L") { "_log" } else { "" }
    $destPath  = Join-Path $(if ($f.Type -eq "L") { $logPath } else { $dataPath }) "$TargetDatabaseName$suffix.$extension"
    "MOVE N'$($f.LogicalName)' TO N'$destPath'"
}
$moveSql = $moveClauses -join ", `n    "

$restoreQuery = @"
RESTORE DATABASE [$TargetDatabaseName]
FROM DISK = N'$BackupFile'
WITH
    $moveSql,
    REPLACE,
    RECOVERY,
    STATS = 25;
"@

& $sqlcmd -S $Server -E -C -Q $restoreQuery
if ($LASTEXITCODE -ne 0) { throw "RESTORE DATABASE a échoué pour '$TargetDatabaseName'." }

# Vérification finale
$tableCountOutput = & $sqlcmd -S $Server -E -C -d $TargetDatabaseName -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables"
$tableCount = ($tableCountOutput | Select-Object -First 1).Trim()
Write-Host ""
Write-Host "OK — base '$TargetDatabaseName' restaurée sur '$Server' ($tableCount tables)." -ForegroundColor Green
Write-Host ""
Write-Host "Prochaine étape : Database\create-app-login.sql (voir DEPLOY.md section 8.2) pour créer" -ForegroundColor Yellow
Write-Host "le compte applicatif à droits minimaux (db_datareader/db_datawriter, pas db_owner)." -ForegroundColor Yellow
