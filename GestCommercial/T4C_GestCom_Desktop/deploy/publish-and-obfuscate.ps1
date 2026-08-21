#Requires -Version 7.0
<#
.SYNOPSIS
    Builds, obfuscates and packages T4C_GestCom_Desktop for handoff to a client.

.DESCRIPTION
    1. dotnet publish (Release, framework-dependent, no PDBs — see the csproj Release DebugType=none).
    2. Runs Obfuscar (deploy/Obfuscar.xml) against T4C_GestCom_Desktop.dll only — see that file's
       header comment for exactly why Web_T4C_GestCom.Core.dll is deliberately left unobfuscated.
    3. Overlays the obfuscated DLL back into the publish folder; everything else (Core.dll, the
       apphost .exe, third-party dependencies, appsettings.json) ships as published.
    4. Moves Obfuscar's Mapping.txt (original-name -> obfuscated-name) OUT of the ship folder into
       a private archive next to this script — never hand this file to the client, it's the key
       that undoes the obfuscation.

.PARAMETER OutputDir
    Where the final, ready-to-zip client package ends up. Defaults to deploy/dist next to this script.

.EXAMPLE
    ./publish-and-obfuscate.ps1
    Then manually smoke-test deploy/dist/T4C_GestCom_Desktop.exe against a real database before
    handing it off — see the mandatory manual check called out in Obfuscar.xml's header comment.
#>
param(
    [string]$OutputDir = "$PSScriptRoot\dist"
)

$ErrorActionPreference = "Stop"

$repoRoot   = Resolve-Path "$PSScriptRoot\..\.."
$desktopCsproj = Join-Path $repoRoot "T4C_GestCom_Desktop\T4C_GestCom_Desktop.csproj"
$publishDir = Join-Path $PSScriptRoot "publish"
$obfDir     = Join-Path $PSScriptRoot "obfuscated"
$mappingArchive = Join-Path $PSScriptRoot "mapping-archive"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (Test-Path $obfDir)     { Remove-Item $obfDir -Recurse -Force }
if (Test-Path $OutputDir)  { Remove-Item $OutputDir -Recurse -Force }

Write-Host "==> dotnet publish (Release)" -ForegroundColor Cyan
dotnet publish $desktopCsproj -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

if (-not (Get-Command obfuscar.console -ErrorAction SilentlyContinue)) {
    throw "obfuscar.console not found. Install it once with: dotnet tool install --global obfuscar.globaltool"
}

Write-Host "==> Obfuscating T4C_GestCom_Desktop.dll" -ForegroundColor Cyan
$obfuscarXml = Join-Path $PSScriptRoot "Obfuscar.generated.xml"
(Get-Content (Join-Path $PSScriptRoot "Obfuscar.xml") -Raw) `
    -replace '\$\(InPath\)', $publishDir `
    -replace '\$\(OutPath\)', $obfDir `
    | Set-Content $obfuscarXml

obfuscar.console -v:n $obfuscarXml
if ($LASTEXITCODE -ne 0) { throw "Obfuscar failed" }
Remove-Item $obfuscarXml

Write-Host "==> Assembling client package -> $OutputDir" -ForegroundColor Cyan
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
Copy-Item "$publishDir\*" $OutputDir -Recurse -Force
Copy-Item (Join-Path $obfDir "T4C_GestCom_Desktop.dll") $OutputDir -Force

Write-Host "==> Archiving Obfuscar mapping privately (NEVER ship this file)" -ForegroundColor Yellow
New-Item -ItemType Directory -Path $mappingArchive -Force | Out-Null
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
Copy-Item (Join-Path $obfDir "Mapping.txt") (Join-Path $mappingArchive "Mapping.$stamp.txt") -Force

Remove-Item $publishDir -Recurse -Force
Remove-Item $obfDir -Recurse -Force

Write-Host ""
Write-Host "Package ready: $OutputDir" -ForegroundColor Green
Write-Host "Mapping archived: $mappingArchive\Mapping.$stamp.txt (keep private, needed only if you ever must debug a client crash from an obfuscated build)" -ForegroundColor Yellow
Write-Host ""
Write-Host "MANDATORY before handoff: run $OutputDir\T4C_GestCom_Desktop.exe against a real database," -ForegroundColor Yellow
Write-Host "log in, open a Facture/Devis line's Produit combo, save a document. See Obfuscar.xml header." -ForegroundColor Yellow
