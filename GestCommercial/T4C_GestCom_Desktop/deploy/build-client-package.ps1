#Requires -Version 7.0
<#
.SYNOPSIS
    Assembles the complete, ready-to-hand-off package for one client: obfuscated app + license
    tool + database scripts.

.DESCRIPTION
    1. Runs publish-and-obfuscate.ps1 to produce the obfuscated app (see that script's own header
       for what it does and why — this script does not repeat that logic).
    2. Publishes T4C_GestCom_LicenseGenerator (Release, framework-dependent) into a LicenseTool/
       subfolder — the client runs `T4C_GestCom_LicenseGenerator.exe collect` from there, and Yamen
       runs the same exe locally with `issue` (see deploy/INSTALL.md).
    3. Copies deploy/sql/*.sql into a Database/ subfolder.
    4. Never touches or copies deploy/keys/ — the RSA private key must never leave Yamen's machine.
       This script asserts that explicitly at the end (see the guard near the bottom).

.PARAMETER ClientName
    Used to name the output folder under deploy/client-package/. Defaults to a timestamp if the
    client doesn't have a name yet (e.g. building a package before signing a deal).

.EXAMPLE
    ./build-client-package.ps1 -ClientName "Société-Exemple-SARL"
    Produces deploy/client-package/Société-Exemple-SARL/ ready to zip and send.
#>
param(
    [string]$ClientName = (Get-Date -Format "yyyyMMdd-HHmmss")
)

$ErrorActionPreference = "Stop"

$repoRoot          = Resolve-Path "$PSScriptRoot\..\.."
$licenseToolCsproj = Join-Path $repoRoot "T4C_GestCom_LicenseGenerator\T4C_GestCom_LicenseGenerator.csproj"
$sqlDir            = Join-Path $PSScriptRoot "sql"
$keysDir           = Join-Path $PSScriptRoot "keys"
$packageRoot       = Join-Path $PSScriptRoot "client-package\$ClientName"
$licenseToolPublishDir = Join-Path $PSScriptRoot "publish-license-tool"

if (Test-Path $packageRoot) { Remove-Item $packageRoot -Recurse -Force }
if (Test-Path $licenseToolPublishDir) { Remove-Item $licenseToolPublishDir -Recurse -Force }

Write-Host "==> [1/4] Build + obfuscation de l'application (publish-and-obfuscate.ps1)" -ForegroundColor Cyan
$appDistDir = Join-Path $packageRoot "App"
& "$PSScriptRoot\publish-and-obfuscate.ps1" -OutputDir $appDistDir
if ($LASTEXITCODE -ne 0) { throw "publish-and-obfuscate.ps1 a échoué" }

Write-Host "==> [2/4] Publication de l'outil de licence (LicenseTool/)" -ForegroundColor Cyan
dotnet publish $licenseToolCsproj -c Release -o $licenseToolPublishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish (LicenseGenerator) a échoué" }

$licenseToolDestDir = Join-Path $packageRoot "LicenseTool"
New-Item -ItemType Directory -Path $licenseToolDestDir -Force | Out-Null
Copy-Item "$licenseToolPublishDir\*" $licenseToolDestDir -Recurse -Force
Remove-Item $licenseToolPublishDir -Recurse -Force

Write-Host "==> [3/4] Copie des scripts SQL (Database/)" -ForegroundColor Cyan
$databaseDestDir = Join-Path $packageRoot "Database"
New-Item -ItemType Directory -Path $databaseDestDir -Force | Out-Null
Copy-Item "$sqlDir\*.sql" $databaseDestDir -Force

Write-Host "==> [4/4] Vérification : la clé privée n'a jamais été copiée" -ForegroundColor Cyan
# La clé privée (deploy/keys/) ne doit JAMAIS apparaître dans un package client. Ce script ne la
# référence nulle part ci-dessus, mais on le vérifie explicitement pour ne jamais livrer par erreur
# une régression future (ex: un `Copy-Item $PSScriptRoot\*` ajouté sans réfléchir).
$leakedKeyFiles = Get-ChildItem $packageRoot -Recurse -File | Where-Object {
    $_.Name -like "*.pem" -or $_.Name -like "*private*"
}
if ($leakedKeyFiles) {
    Remove-Item $packageRoot -Recurse -Force
    throw "ALERTE : des fichiers ressemblant à des clés ont été trouvés dans le package et ont été supprimés : $($leakedKeyFiles.FullName -join ', ')"
}
if ((Get-ChildItem $packageRoot -Recurse -Directory | Where-Object { $_.FullName -eq $keysDir }).Count -gt 0) {
    Remove-Item $packageRoot -Recurse -Force
    throw "ALERTE : le dossier deploy/keys/ a été copié dans le package — annulé."
}
Write-Host "    OK — aucune clé privée dans le package." -ForegroundColor Green

Write-Host ""
Write-Host "Package client prêt : $packageRoot" -ForegroundColor Green
Write-Host "  App/          — application obfusquée (voir DEPLOY.md pour le hardening SQL Server)" -ForegroundColor Green
Write-Host "  LicenseTool/  — T4C_GestCom_LicenseGenerator.exe (collect côté client, issue côté Yamen)" -ForegroundColor Green
Write-Host "  Database/     — scripts SQL de déploiement" -ForegroundColor Green
Write-Host ""
Write-Host "Prochaine étape : voir deploy/INSTALL.md pour le déroulé complet (collect -> issue -> installation)." -ForegroundColor Yellow
