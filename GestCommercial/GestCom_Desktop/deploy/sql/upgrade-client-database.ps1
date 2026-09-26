#Requires -Version 7.0
<#
.SYNOPSIS
    Met à niveau, côté développeur, la sauvegarde (.bak) de la base d'un client Desktop déjà livré
    vers le schéma actuel de Web_GestCom.Core — sans jamais toucher à la base du client.

.DESCRIPTION
    Le Desktop n'exécute AUCUNE migration lui-même : seul Web_GestCom/Program.cs (EnsureCreated +
    ~65 blocs SQL idempotents + seed de référence) sait amener une base au schéma courant. Une base
    livrée avant un changement de schéma (ex. RowVersion, NumeroBonLivraisonOrigine) échoue donc dès
    que la nouvelle version de l'application lit la table concernée. Ce script réutilise
    Program.cs comme unique source de vérité (comme prepare-template-database.ps1) au lieu de
    recopier les migrations ici :
      1. Restaure le .bak du client dans une base temporaire et jetable ($WorkDatabaseName).
      2. Relève le nombre de lignes de chaque table (état AVANT).
      3. Lance Web_GestCom contre cette base (variable d'environnement ConnectionStrings__
         DefaultConnection, aucun fichier appsettings modifié) : Program.cs migre puis on tue le
         processus dès que "Now listening on" apparaît. Le mode Development de Web_GestCom n'est
         utilisé qu'ici, en local, jamais chez le client.
      4. Relève à nouveau les nombres de lignes (état APRÈS) et REFUSE de produire le .bak si une
         table a perdu des lignes (hors role_permission, voir ci-dessous) ou si les colonnes
         attendues du schéma courant sont absentes.
      5. Sauvegarde la base migrée dans $OutputBackupFile puis supprime la base temporaire.

    Le .bak d'origine n'est jamais modifié. Rien n'est écrit sur la machine du client : c'est un
    aller-retour (le client envoie son .bak, on lui renvoie le .bak migré, il le restaure).

    À SAVOIR AVANT D'ENVOYER LE RÉSULTAT AU CLIENT :
      - Program.cs réapplique à chaque démarrage la politique du rôle "Employé" (suppression de
        ses permissions update/delete/gouvernance dans role_permission). Toute personnalisation de
        ce rôle faite par le client via l'écran Rôles & Permissions du Desktop est donc remise à la
        politique par défaut. C'est pour cela que role_permission est exclue du contrôle "aucune
        ligne perdue" — et que le script l'affiche explicitement si elle diminue.
      - Le client doit cesser d'utiliser l'application entre l'envoi de son .bak et la restauration
        du .bak migré : toute saisie faite entre-temps serait perdue.
      - Le .bak migré ne se restaure que sur une instance SQL Server de version >= celle qui l'a
        produit. Le script affiche la version de l'instance de travail : si le client a une Express
        plus ancienne, utilise une instance de travail de version <= la sienne.

.PARAMETER ClientBackupFile
    Chemin du .bak envoyé par le client (obligatoire). Doit être lisible par le compte de service
    SQL Server de l'instance de travail (ex. le copier sous C:\Temp si besoin).

.PARAMETER OutputBackupFile
    Chemin du .bak migré à produire. Par défaut : <nom du .bak>.upgraded.bak à côté de l'original.

.PARAMETER Server
    Instance SQL Server de travail (celle du développeur).

.PARAMETER WorkDatabaseName
    Base temporaire et jetable, DROP/CREATE à chaque exécution. Ne DOIT jamais être "GestCom".

.PARAMETER TimeoutSeconds
    Délai maximum d'attente de la fin du démarrage de Web_GestCom (migrations + seed).

.PARAMETER Configuration
    Configuration de build de Web_GestCom (Debug par défaut, jamais livrée).

.PARAMETER KeepWorkDatabase
    Conserve la base temporaire après la sauvegarde, pour l'inspecter dans SSMS.

.EXAMPLE
    ./upgrade-client-database.ps1 -ClientBackupFile C:\Temp\Hadil-GestCom.bak
    Produit C:\Temp\Hadil-GestCom.upgraded.bak sur l'instance YAMEN.
#>
param(
    [Parameter(Mandatory)][string]$ClientBackupFile,
    [string]$OutputBackupFile,
    [string]$Server = "YAMEN",
    [string]$WorkDatabaseName = "GestCom_Upgrade",
    [int]$TimeoutSeconds = 120,
    [string]$Configuration = "Debug",
    [switch]$KeepWorkDatabase
)

$ErrorActionPreference = "Stop"

if ($WorkDatabaseName -eq "GestCom") {
    throw "WorkDatabaseName ne peut pas être 'GestCom' (la vraie base de dev) — ce script DROP puis CREATE cette base à chaque exécution."
}
if (-not (Test-Path $ClientBackupFile)) { throw "Fichier introuvable : $ClientBackupFile" }
$ClientBackupFile = (Resolve-Path $ClientBackupFile).Path
if (-not $OutputBackupFile) {
    $OutputBackupFile = Join-Path (Split-Path $ClientBackupFile -Parent) ((Split-Path $ClientBackupFile -LeafBase) + ".upgraded.bak")
}
if ((Resolve-Path $OutputBackupFile -ErrorAction SilentlyContinue).Path -eq $ClientBackupFile) {
    throw "OutputBackupFile ne peut pas être le .bak d'origine du client."
}

# ── Outils et chemins ────────────────────────────────────────────────────────
$sqlcmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source
if (-not $sqlcmd) {
    $fallback = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"
    if (Test-Path $fallback) { $sqlcmd = $fallback } else { throw "sqlcmd introuvable (PATH ni $fallback)." }
}
$repoRoot      = Resolve-Path "$PSScriptRoot\..\..\.."
$webProjectDir = Join-Path $repoRoot "Web_GestCom"
$webCsproj     = Join-Path $webProjectDir "Web_GestCom.csproj"
if (-not (Test-Path $webCsproj)) { throw "Web_GestCom.csproj introuvable à '$webCsproj'." }

function Invoke-Sql([string]$Query, [string]$Database = "master") {
    $out = & $sqlcmd -S $Server -E -C -b -d $Database -h -1 -W -s "|" -Q "SET NOCOUNT ON; $Query"
    if ($LASTEXITCODE -ne 0) { throw "Échec SQL ($Database) : $Query`n$out" }
    return $out
}

function Get-RowCounts {
    $q = "SELECT t.name, SUM(p.rows) FROM sys.tables t JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1) GROUP BY t.name"
    $counts = @{}
    foreach ($line in (Invoke-Sql $q $WorkDatabaseName)) {
        $parts = "$line".Split("|")
        if ($parts.Count -eq 2 -and $parts[0].Trim()) { $counts[$parts[0].Trim()] = [long]$parts[1].Trim() }
    }
    return $counts
}

Write-Host "Mise à niveau de la base d'un client Desktop" -ForegroundColor Cyan
Write-Host "  .bak du client   : $ClientBackupFile" -ForegroundColor Gray
Write-Host "  .bak migré       : $OutputBackupFile" -ForegroundColor Gray
Write-Host "  Instance travail : $Server ($((Invoke-Sql 'SELECT CAST(SERVERPROPERTY(''ProductVersion'') AS nvarchar(50))') | Select-Object -First 1))" -ForegroundColor Gray
Write-Host ""

$envKeys   = @("ASPNETCORE_ENVIRONMENT", "ConnectionStrings__DefaultConnection", "SEED_MOCK_DATA", "MockData__Enabled")
$envBackup = @{}
foreach ($key in $envKeys) { $envBackup[$key] = [Environment]::GetEnvironmentVariable($key) }
$workDbCreated = $false
$proc = $null

try {
    # ── [1/5] Restauration dans la base jetable ──────────────────────────────
    Write-Host "==> [1/5] Restauration du .bak client dans '$WorkDatabaseName'" -ForegroundColor Cyan
    if ((Invoke-Sql "SELECT CASE WHEN DB_ID(N'$WorkDatabaseName') IS NULL THEN 0 ELSE 1 END" | Select-Object -First 1).Trim() -eq "1") {
        Write-Host "    '$WorkDatabaseName' existe déjà (reliquat) — suppression." -ForegroundColor Yellow
        Invoke-Sql "ALTER DATABASE [$WorkDatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$WorkDatabaseName];" | Out-Null
    }
    $dataDir = (Invoke-Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(260))" | Select-Object -First 1).Trim()
    $logDir  = (Invoke-Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(260))" | Select-Object -First 1).Trim()
    $fileList = Invoke-Sql "RESTORE FILELISTONLY FROM DISK = N'$ClientBackupFile'"
    $moves = @(); $i = 0
    foreach ($line in $fileList) {
        $f = "$line".Split("|")
        if ($f.Count -lt 3 -or -not $f[0].Trim()) { continue }
        $i++
        $target = if ($f[2].Trim() -eq "L") { Join-Path $logDir "$WorkDatabaseName`_$i.ldf" } else { Join-Path $dataDir "$WorkDatabaseName`_$i.mdf" }
        $moves += "MOVE N'$($f[0].Trim())' TO N'$target'"
    }
    if ($moves.Count -eq 0) { throw "Aucun fichier logique lu dans le .bak (RESTORE FILELISTONLY vide) — ce fichier est-il bien une sauvegarde SQL Server ?" }
    Invoke-Sql "RESTORE DATABASE [$WorkDatabaseName] FROM DISK = N'$ClientBackupFile' WITH $($moves -join ', '), REPLACE" | Out-Null
    $workDbCreated = $true
    Write-Host "    OK." -ForegroundColor Green

    # ── [2/5] État avant ──────────────────────────────────────────────────────
    Write-Host "==> [2/5] Relevé du nombre de lignes par table (avant)" -ForegroundColor Cyan
    $before = Get-RowCounts
    Write-Host "    $($before.Count) tables, $(($before.Values | Measure-Object -Sum).Sum) lignes au total." -ForegroundColor Green

    # ── [3/5] Migration par Program.cs ────────────────────────────────────────
    Write-Host "==> [3/5] Build de Web_GestCom puis migration ($Configuration)" -ForegroundColor Cyan
    dotnet build $webCsproj -c $Configuration --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet build de Web_GestCom a échoué." }
    $exe = Get-ChildItem -Path (Join-Path $webProjectDir "bin\$Configuration") -Filter "Web_GestCom.exe" -Recurse | Select-Object -First 1
    if (-not $exe) { throw "Web_GestCom.exe introuvable sous bin\$Configuration\." }

    $stamp   = Get-Date -Format "yyyyMMdd-HHmmss"
    $logFile = Join-Path $env:TEMP "upgrade-client-db-$stamp.out.log"
    $errFile = Join-Path $env:TEMP "upgrade-client-db-$stamp.err.log"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ConnectionStrings__DefaultConnection = "Server=$Server;Database=$WorkDatabaseName;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
    Remove-Item Env:\SEED_MOCK_DATA -ErrorAction SilentlyContinue
    Remove-Item Env:\MockData__Enabled -ErrorAction SilentlyContinue

    $proc = Start-Process -FilePath $exe.FullName -WorkingDirectory $exe.DirectoryName `
        -RedirectStandardOutput $logFile -RedirectStandardError $errFile -PassThru -NoNewWindow
    $ready = $false
    for ($elapsed = 0; $elapsed -lt $TimeoutSeconds; $elapsed++) {
        Start-Sleep -Seconds 1
        if ($proc.HasExited) {
            throw "Web_GestCom s'est arrêté prématurément (code $($proc.ExitCode)) pendant la migration. Erreur :`n$(Get-Content $errFile -Raw -ErrorAction SilentlyContinue)`nJournal : $logFile"
        }
        $content = if (Test-Path $logFile) { Get-Content $logFile -Raw -ErrorAction SilentlyContinue } else { "" }
        if ($content -match "Now listening on" -or $content -match "Application started") { $ready = $true; break }
    }
    if (-not $ready) { throw "Timeout (${TimeoutSeconds}s) sans fin de migration détectée. Journaux : $logFile / $errFile" }
    Write-Host "    OK — Program.cs a terminé ses migrations et son seed." -ForegroundColor Green
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    $proc = $null

    # ── [4/5] Contrôles ───────────────────────────────────────────────────────
    Write-Host "==> [4/5] Contrôles (aucune perte de données, schéma courant présent)" -ForegroundColor Cyan
    $after = Get-RowCounts
    $problems = @()
    foreach ($table in $before.Keys) {
        if (-not $after.ContainsKey($table)) { $problems += "table '$table' a disparu"; continue }
        if ($after[$table] -lt $before[$table]) {
            if ($table -eq "role_permission") {
                Write-Host "    ATTENTION : role_permission $($before[$table]) -> $($after[$table]) lignes (politique du rôle Employé réappliquée — voir l'en-tête du script)." -ForegroundColor Yellow
            } else {
                $problems += "table '$table' : $($before[$table]) -> $($after[$table]) lignes"
            }
        }
    }
    $rowVersionCols = [int](Invoke-Sql "SELECT COUNT(*) FROM sys.columns WHERE LOWER(name) LIKE '%rowversion%'" $WorkDatabaseName | Select-Object -First 1).Trim()
    $origineCols    = [int](Invoke-Sql "SELECT COUNT(*) FROM sys.columns WHERE LOWER(name) LIKE '%origine%'" $WorkDatabaseName | Select-Object -First 1).Trim()
    if ($rowVersionCols -lt 7) { $problems += "colonnes RowVersion attendues : 7, trouvées : $rowVersionCols" }
    if ($origineCols -lt 1)    { $problems += "colonne de provenance BL->facture absente" }
    if ($problems.Count -gt 0) {
        throw "Contrôles échoués — AUCUN .bak migré produit :`n  - $($problems -join "`n  - ")"
    }
    $added = $after.Keys | Where-Object { -not $before.ContainsKey($_) } | Sort-Object
    if ($added) { Write-Host "    Tables ajoutées : $($added -join ', ')" -ForegroundColor Gray }
    Write-Host "    OK — RowVersion : $rowVersionCols colonnes, provenance BL : $origineCols colonne(s)." -ForegroundColor Green

    # ── [5/5] Sauvegarde du résultat ──────────────────────────────────────────
    Write-Host "==> [5/5] BACKUP de la base migrée -> $OutputBackupFile" -ForegroundColor Cyan
    if (Test-Path $OutputBackupFile) { Remove-Item $OutputBackupFile -Force }
    Invoke-Sql "BACKUP DATABASE [$WorkDatabaseName] TO DISK = N'$OutputBackupFile' WITH INIT" | Out-Null
    if (-not (Test-Path $OutputBackupFile)) { throw "BACKUP terminé sans erreur mais fichier absent : $OutputBackupFile" }
    Remove-Item $logFile, $errFile -ErrorAction SilentlyContinue
    Write-Host ""
    Write-Host "Fichier produit : $OutputBackupFile ($([Math]::Round((Get-Item $OutputBackupFile).Length / 1MB, 2)) Mo)" -ForegroundColor Green
    Write-Host "Le client doit cesser d'utiliser l'application, puis restaurer ce fichier (restore-database.ps1 -BackupFile ... -Force)." -ForegroundColor Yellow
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    foreach ($key in $envKeys) {
        if ($null -eq $envBackup[$key]) { Remove-Item "Env:\$key" -ErrorAction SilentlyContinue } else { Set-Item "Env:\$key" $envBackup[$key] }
    }
    if ($workDbCreated -and -not $KeepWorkDatabase) {
        & $sqlcmd -S $Server -E -C -Q "ALTER DATABASE [$WorkDatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$WorkDatabaseName];" | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Host "ATTENTION : échec de la suppression de '$WorkDatabaseName' sur $Server — à nettoyer manuellement." -ForegroundColor Red }
    }
}
