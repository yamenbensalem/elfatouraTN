##############################################################################
# GestCom - Seed Data Script v3
# Inserts sample data via the API to test all frontend modules
# Usage: .\seed-data.ps1
# Prerequisites: Backend API running on http://localhost:5000
#                Reference data already seeded via SQL
##############################################################################

$ErrorActionPreference = "Continue"
$BaseUrl = "http://localhost:5000/api/v1"
$Token = $null
$script:totalCreated = 0
$script:totalSkipped = 0
$script:totalFailed  = 0

# ── Helpers ─────────────────────────────────────────────────────────────────
function Write-Step   ($msg) { Write-Host "  -> $msg" -ForegroundColor Cyan }
function Write-OK     ($msg) { Write-Host "  OK $msg" -ForegroundColor Green }
function Write-Warn   ($msg) { Write-Host "  !! $msg" -ForegroundColor Yellow }
function Write-Fail   ($msg) { Write-Host "  XX $msg" -ForegroundColor Red }
function Write-Section($msg) { Write-Host "`n== $msg ==" -ForegroundColor Magenta }

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null
    )
    $headers = @{ "Content-Type" = "application/json" }
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    $uri = "$BaseUrl$Path"
    $jsonBytes = $null
    if ($Body) {
        $json = $Body | ConvertTo-Json -Depth 10
        $jsonBytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    }

    try {
        $params = @{
            Uri             = $uri
            Method          = $Method
            Headers         = $headers
            UseBasicParsing = $true
        }
        if ($jsonBytes) { $params["Body"] = $jsonBytes }

        $response = Invoke-WebRequest @params
        $script:totalCreated++
        if ($response.Content) {
            return $response.Content | ConvertFrom-Json
        }
        return @{}
    }
    catch {
        $status = 0
        try { $status = [int]$_.Exception.Response.StatusCode } catch {}
        $detail = ""
        try { $detail = $_.ErrorDetails.Message } catch {}

        if ($status -ge 400 -and $status -lt 500) {
            Write-Warn "Skipped ($status): $Path"
            $script:totalSkipped++
        }
        else {
            Write-Fail "HTTP $status on $Path"
            if ($detail) { Write-Fail "  Detail: $detail" }
            $script:totalFailed++
        }
        return $null
    }
}

# ── 1. AUTH ─────────────────────────────────────────────────────────────────
Write-Section "1/10 - Authentification"
Write-Step "Login admin@gestcom.tn ..."
$loginResult = Invoke-Api -Method POST -Path "/auth/login" -Body @{
    email    = "admin@gestcom.tn"
    password = "Admin@123"
}
if (-not $loginResult -or -not $loginResult.token) {
    Write-Fail "Login failed - aborting"; exit 1
}
$Token = $loginResult.token
$script:totalCreated-- # don't count login as "created"
Write-OK "Logged in (token received)"

# ── Reference data IDs (from SQL seeding) ──
# Devise: TND=5, EUR=6, USD=7
# TVA: 19%=1, 13%=2, 7%=3, Exonere=4
# Unite: Unite=1, Kg=2, Litre=3, Metre=4, Carton=5
# Categorie: Electronique=1, Alimentaire=2, Textile=3, Fournitures=4, Cosmetique=5
# Magasin: Principal=6, Secondaire=7
# ModePayement: ESP, CHQ, VIR, TRT, CB

# ── 2. CLIENTS ──────────────────────────────────────────────────────────────
# CreateClientCommand: codeClient, nom, matriculeFiscale, typePersonne,
# adresse, ville, pays, tel, maxCredit, codeDevise (int FK)
Write-Section "2/10 - Clients"

$clients = @(
    @{ codeClient="CLI001"; matriculeFiscale="1234567ABC001"; nom="Societe ABC SARL"; typePersonne="Personne Morale"; adresse="10 Rue de la Republique"; ville="Tunis"; pays="Tunisie"; tel="71000001"; maxCredit=50000; codeDevise=5 },
    @{ codeClient="CLI002"; matriculeFiscale="2345678XYZ002"; nom="Entreprise XYZ SA"; typePersonne="Personne Morale"; adresse="25 Avenue Habib Bourguiba"; ville="Sfax"; pays="Tunisie"; tel="74000002"; maxCredit=100000; codeDevise=5 },
    @{ codeClient="CLI003"; matriculeFiscale="3456789MBA003"; nom="Mohamed Ben Ali"; typePersonne="Personne Physique"; adresse="5 Rue Ibn Khaldoun"; ville="Sousse"; pays="Tunisie"; tel="73000003"; maxCredit=20000; codeDevise=5 },
    @{ codeClient="CLI004"; matriculeFiscale="4567890TSL004"; nom="Tech Solutions SARL"; typePersonne="Personne Morale"; adresse="12 Zone Industrielle"; ville="Monastir"; pays="Tunisie"; tel="73000004"; maxCredit=75000; codeDevise=5 },
    @{ codeClient="CLI005"; matriculeFiscale="5678901CMP005"; nom="Commerce Plus SA"; typePersonne="Personne Morale"; adresse="8 Rue de Marseille"; ville="Tunis"; pays="Tunisie"; tel="71000005"; maxCredit=150000; codeDevise=5 },
    @{ codeClient="CLI006"; matriculeFiscale="6789012FTR006"; nom="Fatma Trabelsi"; typePersonne="Personne Physique"; adresse="3 Rue de Paris"; ville="Bizerte"; pays="Tunisie"; tel="72000006"; maxCredit=30000; codeDevise=5 }
)

foreach ($c in $clients) {
    Write-Step "Client $($c.codeClient) ..."
    $result = Invoke-Api -Method POST -Path "/clients" -Body $c
    if ($result) { Write-OK "Created: $($c.nom)" }
}

# ── 3. FOURNISSEURS ────────────────────────────────────────────────────────
# CreateFournisseurCommand: codeFournisseur, raisonSociale, matriculeFiscale,
# telephone, adresse, ville, pays, email, delaiPaiement, tauxRemise
Write-Section "3/10 - Fournisseurs"

$fournisseurs = @(
    @{ codeFournisseur="FRN001"; raisonSociale="Fournisseur Alpha"; matriculeFiscale="1111111ALP001"; telephone="71100001"; adresse="Zone Ind. Ben Arous"; ville="Ben Arous"; pays="Tunisie"; email="contact@alpha.tn"; delaiPaiement=30; tauxRemise=5; codeDevise=5 },
    @{ codeFournisseur="FRN002"; raisonSociale="Import Export Beta"; matriculeFiscale="2222222BET002"; telephone="71200002"; adresse="15 Rue du Commerce"; ville="Tunis"; pays="Tunisie"; email="info@beta.tn"; delaiPaiement=45; tauxRemise=3; codeDevise=5 },
    @{ codeFournisseur="FRN003"; raisonSociale="Grossiste Gamma"; matriculeFiscale="3333333GAM003"; telephone="74300003"; adresse="Route de Gabes km5"; ville="Sfax"; pays="Tunisie"; email="ventes@gamma.tn"; delaiPaiement=60; tauxRemise=8; codeDevise=5 },
    @{ codeFournisseur="FRN004"; raisonSociale="Distribution Delta"; matriculeFiscale="4444444DEL004"; telephone="73400004"; adresse="Zone Commerciale Nord"; ville="Sousse"; pays="Tunisie"; email="cmd@delta.tn"; delaiPaiement=30; tauxRemise=2; codeDevise=5 }
)

foreach ($f in $fournisseurs) {
    Write-Step "Fournisseur $($f.codeFournisseur) ..."
    $result = Invoke-Api -Method POST -Path "/fournisseurs" -Body $f
    if ($result) { Write-OK "Created: $($f.raisonSociale)" }
}

# ── 4. PRODUITS ─────────────────────────────────────────────────────────────
# CreateProduitCommand: codeProduit, designation, prixAchatTTC, tauxMarge,
# prixVenteHT, prixVenteTTC, quantite, stockMinimal, tauxTVA, tauxFODEC,
# fodec (bool), codeFournisseur, codeUnite, codeCategorie, codeMagasin, codeTVA
Write-Section "4/10 - Produits"

$produits = @(
    @{ codeProduit="PRD001"; designation="Ordinateur Portable HP"; prixAchatTTC=2500; tauxMarge=25; prixVenteHT=2625; prixVenteTTC=3123.75; quantite=50; stockMinimal=5; tauxTVA=19; tauxFODEC=1; fodec=$true; codeFournisseur="FRN001"; codeUnite="1"; codeCategorie="1"; codeMagasin="6"; codeTVA="1" },
    @{ codeProduit="PRD002"; designation="Ecran Samsung 27 pouces"; prixAchatTTC=800; tauxMarge=30; prixVenteHT=874; prixVenteTTC=1040.06; quantite=30; stockMinimal=3; tauxTVA=19; tauxFODEC=1; fodec=$true; codeFournisseur="FRN001"; codeUnite="1"; codeCategorie="1"; codeMagasin="6"; codeTVA="1" },
    @{ codeProduit="PRD003"; designation="Clavier Mecanique Logitech"; prixAchatTTC=250; tauxMarge=40; prixVenteHT=294; prixVenteTTC=349.86; quantite=100; stockMinimal=10; tauxTVA=19; tauxFODEC=0; fodec=$false; codeFournisseur="FRN001"; codeUnite="1"; codeCategorie="1"; codeMagasin="6"; codeTVA="1" },
    @{ codeProduit="PRD004"; designation="Huile Olive Extra Vierge 1L"; prixAchatTTC=15; tauxMarge=20; prixVenteHT=15.12; prixVenteTTC=16.18; quantite=200; stockMinimal=20; tauxTVA=7; tauxFODEC=0; fodec=$false; codeFournisseur="FRN003"; codeUnite="3"; codeCategorie="2"; codeMagasin="7"; codeTVA="3" },
    @{ codeProduit="PRD005"; designation="Cafe Moulu Premium 250g"; prixAchatTTC=8; tauxMarge=35; prixVenteHT=9.07; prixVenteTTC=9.70; quantite=500; stockMinimal=50; tauxTVA=7; tauxFODEC=0; fodec=$false; codeFournisseur="FRN003"; codeUnite="1"; codeCategorie="2"; codeMagasin="7"; codeTVA="3" },
    @{ codeProduit="PRD006"; designation="T-Shirt Coton Bio"; prixAchatTTC=25; tauxMarge=50; prixVenteHT=31.5; prixVenteTTC=37.49; quantite=150; stockMinimal=15; tauxTVA=19; tauxFODEC=0; fodec=$false; codeFournisseur="FRN004"; codeUnite="1"; codeCategorie="3"; codeMagasin="6"; codeTVA="1" },
    @{ codeProduit="PRD007"; designation="Ramette Papier A4 500f"; prixAchatTTC=12; tauxMarge=25; prixVenteHT=12.61; prixVenteTTC=15; quantite=300; stockMinimal=30; tauxTVA=19; tauxFODEC=0; fodec=$false; codeFournisseur="FRN002"; codeUnite="1"; codeCategorie="4"; codeMagasin="6"; codeTVA="1" },
    @{ codeProduit="PRD008"; designation="Stylo Bic Cristal boite 50"; prixAchatTTC=18; tauxMarge=30; prixVenteHT=19.66; prixVenteTTC=23.40; quantite=400; stockMinimal=40; tauxTVA=19; tauxFODEC=0; fodec=$false; codeFournisseur="FRN002"; codeUnite="5"; codeCategorie="4"; codeMagasin="6"; codeTVA="1" },
    @{ codeProduit="PRD009"; designation="Creme Hydratante 100ml"; prixAchatTTC=20; tauxMarge=60; prixVenteHT=26.89; prixVenteTTC=32; quantite=80; stockMinimal=10; tauxTVA=19; tauxFODEC=0; fodec=$false; codeFournisseur="FRN004"; codeUnite="1"; codeCategorie="5"; codeMagasin="7"; codeTVA="1" },
    @{ codeProduit="PRD010"; designation="Cable HDMI 2m"; prixAchatTTC=10; tauxMarge=45; prixVenteHT=12.18; prixVenteTTC=14.50; quantite=200; stockMinimal=20; tauxTVA=19; tauxFODEC=0; fodec=$false; codeFournisseur="FRN001"; codeUnite="1"; codeCategorie="1"; codeMagasin="6"; codeTVA="1" }
)

foreach ($p in $produits) {
    Write-Step "Produit $($p.codeProduit) ..."
    $result = Invoke-Api -Method POST -Path "/produits" -Body $p
    if ($result) { Write-OK "Created: $($p.designation)" }
}

# ── 5. DEVIS CLIENTS ───────────────────────────────────────────────────────
# CreateDevisClientCommand: dateDevis, dateValidite, codeClient,
# tauxRemise, timbre, codeDevise (int), tauxChange, observations, lignes
# Ligne: codeProduit, quantite, prixUnitaireHT, tauxTVA, tauxRemise
Write-Section "5/10 - Devis Clients"

$devis = @(
    @{
        dateDevis    = "2025-01-15"
        dateValidite = "2025-02-15"
        codeClient   = "CLI001"
        tauxRemise   = 0
        timbre       = 1
        codeDevise   = "TND"
        tauxChange   = 1
        observations = "Devis pour equipement informatique"
        lignes = @(
            @{ codeProduit="PRD001"; quantite=2; prixUnitaireHT=2625; tauxTVA=19; tauxRemise=0 },
            @{ codeProduit="PRD002"; quantite=2; prixUnitaireHT=874;  tauxTVA=19; tauxRemise=5 }
        )
    },
    @{
        dateDevis    = "2025-01-20"
        dateValidite = "2025-02-20"
        codeClient   = "CLI002"
        tauxRemise   = 3
        timbre       = 1
        codeDevise   = "TND"
        tauxChange   = 1
        observations = "Devis fournitures de bureau"
        lignes = @(
            @{ codeProduit="PRD007"; quantite=10; prixUnitaireHT=12.61; tauxTVA=19; tauxRemise=0 },
            @{ codeProduit="PRD008"; quantite=5;  prixUnitaireHT=19.66; tauxTVA=19; tauxRemise=0 }
        )
    },
    @{
        dateDevis    = "2025-02-01"
        dateValidite = "2025-03-01"
        codeClient   = "CLI003"
        tauxRemise   = 0
        timbre       = 1
        codeDevise   = "TND"
        tauxChange   = 1
        observations = "Devis produits alimentaires"
        lignes = @(
            @{ codeProduit="PRD004"; quantite=20; prixUnitaireHT=15.12; tauxTVA=7; tauxRemise=0 },
            @{ codeProduit="PRD005"; quantite=50; prixUnitaireHT=9.07;  tauxTVA=7; tauxRemise=10 }
        )
    }
)

foreach ($d in $devis) {
    Write-Step "Devis for $($d.codeClient) ..."
    $result = Invoke-Api -Method POST -Path "/devis" -Body $d
    if ($result) { Write-OK "Created: $($result.numeroDevis)" }
}

# ── 6. COMMANDES VENTE ──────────────────────────────────────────────────────
# CreateCommandeVenteCommand: dateCommande, dateLivraisonPrevue, codeClient,
# tauxRemise, codeDevise (string?), tauxChange, observations, lignes
# Ligne: codeProduit, quantite, prixUnitaireHT, tauxTVA, tauxRemise
Write-Section "6/10 - Commandes de Vente"

$commandesVente = @(
    @{
        dateCommande        = "2025-02-01"
        dateLivraisonPrevue = "2025-02-15"
        codeClient          = "CLI001"
        tauxRemise          = 0
        codeDevise          = "5"
        tauxChange          = 1
        observations        = "Commande urgente informatique"
        lignes = @(
            @{ codeProduit="PRD001"; quantite=3; prixUnitaireHT=2625; tauxTVA=19; tauxRemise=5 },
            @{ codeProduit="PRD003"; quantite=5; prixUnitaireHT=294;  tauxTVA=19; tauxRemise=0 }
        )
    },
    @{
        dateCommande        = "2025-02-10"
        dateLivraisonPrevue = "2025-02-28"
        codeClient          = "CLI004"
        tauxRemise          = 2
        codeDevise          = "5"
        tauxChange          = 1
        observations        = "Commande textile"
        lignes = @(
            @{ codeProduit="PRD006"; quantite=20; prixUnitaireHT=31.5; tauxTVA=19; tauxRemise=0 }
        )
    }
)

foreach ($cv in $commandesVente) {
    Write-Step "Commande Vente for $($cv.codeClient) ..."
    $result = Invoke-Api -Method POST -Path "/commandesvente" -Body $cv
    if ($result) { Write-OK "Created: $($result.numeroCommande)" }
}

# ── 7. COMMANDES ACHAT ─────────────────────────────────────────────────────
# CreateCommandeAchatCommand: dateCommande, dateLivraison, codeFournisseur,
# remise (decimal), notes (string?), lignes
# Ligne: codeProduit, quantite, prixUnitaire, tauxTVA, remise
Write-Section "7/10 - Commandes d'Achat"

$commandesAchat = @(
    @{
        codeFournisseur = "FRN001"
        dateCommande    = "2025-01-20"
        dateLivraison   = "2025-02-05"
        remise          = 0
        notes           = "Reapprovisionnement informatique"
        lignes = @(
            @{ codeProduit="PRD001"; quantite=10; prixUnitaire=2100; tauxTVA=19; remise=0 },
            @{ codeProduit="PRD002"; quantite=5;  prixUnitaire=670;  tauxTVA=19; remise=2 },
            @{ codeProduit="PRD010"; quantite=50; prixUnitaire=6.89; tauxTVA=19; remise=0 }
        )
    },
    @{
        codeFournisseur = "FRN003"
        dateCommande    = "2025-02-01"
        dateLivraison   = "2025-02-20"
        remise          = 5
        notes           = "Commande alimentaire"
        lignes = @(
            @{ codeProduit="PRD004"; quantite=100; prixUnitaire=14;   tauxTVA=7; remise=0 },
            @{ codeProduit="PRD005"; quantite=200; prixUnitaire=5.96; tauxTVA=7; remise=0 }
        )
    }
)

foreach ($ca in $commandesAchat) {
    Write-Step "Commande Achat for $($ca.codeFournisseur) ..."
    $result = Invoke-Api -Method POST -Path "/commandes-achat" -Body $ca
    if ($result) { Write-OK "Created commande achat" }
}

# ── 8. FACTURES CLIENT ──────────────────────────────────────────────────────
# CreateFactureClientCommand: dateFacture, dateEcheance, codeClient,
# tauxRemise, timbre, tauxRAS, codeDevise (string?), tauxChange,
# codeModePaiement (string?), observations, lignes
# Ligne: codeProduit, quantite, prixUnitaireHT, tauxTVA, tauxRemise, tauxFODEC
Write-Section "8/10 - Factures Client"

$facturesClient = @(
    @{
        dateFacture      = "2025-02-15"
        dateEcheance     = "2025-03-15"
        codeClient       = "CLI001"
        tauxRemise       = 0
        timbre           = 1
        tauxRAS          = 0
        codeDevise       = "5"
        tauxChange       = 1
        codeModePaiement = "VIR"
        observations     = "Facture equipement informatique"
        lignes = @(
            @{ codeProduit="PRD001"; quantite=2; prixUnitaireHT=2625; tauxTVA=19; tauxRemise=0; tauxFODEC=1 },
            @{ codeProduit="PRD002"; quantite=2; prixUnitaireHT=874;  tauxTVA=19; tauxRemise=0; tauxFODEC=1 }
        )
    },
    @{
        dateFacture      = "2025-02-20"
        dateEcheance     = "2025-03-20"
        codeClient       = "CLI002"
        tauxRemise       = 2
        timbre           = 1
        tauxRAS          = 0
        codeDevise       = "5"
        tauxChange       = 1
        codeModePaiement = "CHQ"
        observations     = "Facture fournitures"
        lignes = @(
            @{ codeProduit="PRD007"; quantite=10; prixUnitaireHT=12.61; tauxTVA=19; tauxRemise=0; tauxFODEC=0 },
            @{ codeProduit="PRD008"; quantite=5;  prixUnitaireHT=19.66; tauxTVA=19; tauxRemise=0; tauxFODEC=0 }
        )
    },
    @{
        dateFacture      = "2025-03-01"
        dateEcheance     = "2025-04-01"
        codeClient       = "CLI005"
        tauxRemise       = 0
        timbre           = 1
        tauxRAS          = 0
        codeDevise       = "5"
        tauxChange       = 1
        codeModePaiement = "ESP"
        observations     = "Facture cosmetiques"
        lignes = @(
            @{ codeProduit="PRD009"; quantite=10; prixUnitaireHT=26.89; tauxTVA=19; tauxRemise=5; tauxFODEC=0 }
        )
    }
)

foreach ($fc in $facturesClient) {
    Write-Step "Facture Client for $($fc.codeClient) ..."
    $result = Invoke-Api -Method POST -Path "/factures/clients" -Body $fc
    if ($result) { Write-OK "Created: $($result.numeroFacture)" }
}

# ── 9. FACTURES FOURNISSEUR ────────────────────────────────────────────────
# CreateFactureFournisseurCommand: dateFacture, dateEcheance, codeFournisseur,
# numeroFactureFournisseur, tauxRemiseGlobale, observation, lignes
# Ligne: codeProduit, quantite, prixUnitaireHT, tauxTVA, tauxRemise, tauxFodec
Write-Section "9/10 - Factures Fournisseur"

$facturesFournisseur = @(
    @{
        dateFacture              = "2025-02-10"
        dateEcheance             = "2025-03-10"
        codeFournisseur          = "FRN001"
        numeroFactureFournisseur = "FFEXT-2025-001"
        tauxRemiseGlobale        = 0
        observation              = "Facture equipement informatique"
        lignes = @(
            @{ codeProduit="PRD001"; quantite=10; prixUnitaireHT=2100; tauxTVA=19; tauxRemise=0; tauxFodec=1 },
            @{ codeProduit="PRD002"; quantite=5;  prixUnitaireHT=670;  tauxTVA=19; tauxRemise=2; tauxFodec=1 }
        )
    },
    @{
        dateFacture              = "2025-02-25"
        dateEcheance             = "2025-04-25"
        codeFournisseur          = "FRN003"
        numeroFactureFournisseur = "FFEXT-2025-002"
        tauxRemiseGlobale        = 3
        observation              = "Facture produits alimentaires"
        lignes = @(
            @{ codeProduit="PRD004"; quantite=100; prixUnitaireHT=14;   tauxTVA=7; tauxRemise=0; tauxFodec=0 },
            @{ codeProduit="PRD005"; quantite=200; prixUnitaireHT=5.96; tauxTVA=7; tauxRemise=0; tauxFodec=0 }
        )
    }
)

foreach ($ff in $facturesFournisseur) {
    Write-Step "Facture Fournisseur $($ff.numeroFactureFournisseur) ..."
    $result = Invoke-Api -Method POST -Path "/factures/fournisseurs" -Body $ff
    if ($result) { Write-OK "Created: $($result.numeroFacture)" }
}

# ── 10. SUMMARY ─────────────────────────────────────────────────────────────
Write-Section "VERIFICATION"
Write-Step "Checking data counts via SQL..."

$sqlcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE"
$sqlQuery = @"
SET NOCOUNT ON;
SELECT 'Client'              AS T, COUNT(*) AS C FROM Client              WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'Fournisseur',              COUNT(*) FROM Fournisseur              WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'Produit',                  COUNT(*) FROM Produit                  WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'DevisClient',              COUNT(*) FROM DevisClient              WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'CommandeVente',            COUNT(*) FROM CommandeVente            WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'CommandeAchat',            COUNT(*) FROM CommandeAchat            WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'FactureClient',            COUNT(*) FROM FactureClient            WHERE CodeEntreprise='DEFAULT'
UNION ALL
SELECT 'FactureFournisseur',       COUNT(*) FROM FactureFournisseur       WHERE CodeEntreprise='DEFAULT';
"@

$sqlResult = & $sqlcmd -S DESKTOP-7SKVRQI -d GestComDB_Dev -E -Q $sqlQuery -h -1 -W 2>$null

$expected = @{
    "Client"=6; "Fournisseur"=4; "Produit"=10; "DevisClient"=3;
    "CommandeVente"=2; "CommandeAchat"=2; "FactureClient"=3; "FactureFournisseur"=2
}

Write-Host ""
Write-Host "+----------------------------+-------+----------+" -ForegroundColor White
Write-Host "| Module                     | Count | Expected |" -ForegroundColor White
Write-Host "+----------------------------+-------+----------+" -ForegroundColor White

foreach ($line in $sqlResult) {
    $line = $line.Trim()
    if ($line -match '^(\S+)\s+(\d+)$') {
        $name = $Matches[1]
        $count = [int]$Matches[2]
        $exp = $expected[$name]
        $color = if ($count -ge $exp) { "Green" } else { "Red" }
        Write-Host ("| {0,-26} | {1,5} | {2,8} |" -f $name, $count, $exp) -ForegroundColor $color
    }
}
Write-Host "+----------------------------+-------+----------+" -ForegroundColor White

Write-Host ""
Write-Host "Created: $($script:totalCreated)  Skipped: $($script:totalSkipped)  Failed: $($script:totalFailed)" -ForegroundColor Cyan
Write-Host ""
Write-Host "=== SEED COMPLETE ===" -ForegroundColor Green
