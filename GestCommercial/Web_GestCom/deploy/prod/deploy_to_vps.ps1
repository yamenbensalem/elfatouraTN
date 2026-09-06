# ================================================================
# VPS Deployment Orchestration Script - gestioncom.tijaraflow.fr
# ================================================================

param(
    [ValidateSet('fix-ssh','build-image','push-images','setup-network','deploy-db','deploy-app','deploy-all','status','logs','stop','restart','cleanup','reset-all','syncFile','fix-vhost')]
    [string]$Action = "deploy-all",

    [string]$RemoteHost = "vps-bf0b3440.vps.ovh.net",
    [string]$RemoteUser = "ubuntu",
    [string]$SshKey     = "C:\Users\$env:USERNAME\.ssh\id_rsa",
    [string]$RemotePath = "/home/ubuntu/docker/gestcom",

    [switch]$SkipImagePush
)

# ===================== CONFIGURATION =====================

$VpsHost         = $RemoteHost
$VpsUser         = $RemoteUser
$VpsSshKey       = $SshKey
$VpsPath         = $RemotePath
$LocalDeployPath = $PSScriptRoot                              # deploy/prod/
$ProjectRoot     = (Resolve-Path "$PSScriptRoot\..\..")      # Web_GestCom/

$AppDomain       = "gestioncom.tijaraflow.fr"
$NetworkName     = "ntw_gestcom_prod"

$Images = @(
    "gestcom/app:prod"
)

$Colors = @{ Success="Green"; Error="Red"; Warning="Yellow"; Info="Cyan" }

function Write-Status {
    param($Message, $Type = "Info")
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor $Colors[$Type]
}

# ===================== SSH HELPERS =====================

function Get-SshArgs {
    $a = @("-o","StrictHostKeyChecking=no","-o","BatchMode=yes","-o","ConnectTimeout=15")
    if ($VpsSshKey -and (Test-Path $VpsSshKey)) { $a += @("-i", $VpsSshKey) }
    return $a
}

function Invoke-RemoteCommand {
    param([string]$Command, [switch]$IgnoreError)
    $sshArgs = Get-SshArgs
    $result  = & ssh @sshArgs "${VpsUser}@${VpsHost}" $Command
    if ($LASTEXITCODE -ne 0 -and -not $IgnoreError) {
        Write-Status "Remote command failed (exit $LASTEXITCODE)" "Warning"
    }
    return $result
}

function Copy-ToRemote {
    param([string]$LocalPath, [string]$RemoteDest)
    Write-Status "Copying $(Split-Path $LocalPath -Leaf) to VPS:$RemoteDest" "Info"
    $sshArgs = Get-SshArgs
    & scp @sshArgs -r $LocalPath "${VpsUser}@${VpsHost}:${RemoteDest}"
    if ($LASTEXITCODE -eq 0) { Write-Status "Copy OK" "Success"; return $true }
    else                      { Write-Status "Copy FAILED: $LocalPath" "Error"; return $false }
}

# ===================== FIX SSH =====================

function Fix-SshKey {
    Write-Status "=== SSH KEY SETUP ===" "Info"

    if (-not (Test-Path $VpsSshKey)) {
        Write-Status "No key at $VpsSshKey - generating..." "Warning"
        $keyDir = Split-Path $VpsSshKey -Parent
        if (-not (Test-Path $keyDir)) { New-Item -ItemType Directory -Path $keyDir | Out-Null }
        & ssh-keygen -t rsa -b 4096 -f $VpsSshKey -N "" -C "deploy@gestcom"
        Write-Status "Key generated" "Success"
    }

    $pubKeyFile = "$VpsSshKey.pub"
    if (-not (Test-Path $pubKeyFile)) {
        Write-Status "Public key not found at $pubKeyFile" "Error"
        return
    }

    $pubKey = (Get-Content $pubKeyFile -Raw).Trim()

    Write-Status "Installing public key on VPS (password required once)..." "Info"

    $installArgs = @("-o","StrictHostKeyChecking=no","-o","ConnectTimeout=15")
    $remoteCmd   = "mkdir -p ~/.ssh && chmod 700 ~/.ssh && grep -qF '$pubKey' ~/.ssh/authorized_keys 2>/dev/null || echo '$pubKey' >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys && echo KEY_INSTALLED"
    $result      = & ssh @installArgs "${VpsUser}@${VpsHost}" $remoteCmd

    if ($result -match "KEY_INSTALLED") {
        Write-Status "SSH key installed - future connections will be passwordless" "Success"
    } else {
        Write-Status "Installation may have failed. Add manually on VPS:" "Warning"
        Write-Host "  mkdir -p ~/.ssh" -ForegroundColor Yellow
        Write-Host "  echo '$pubKey' >> ~/.ssh/authorized_keys" -ForegroundColor Yellow
        Write-Host "  chmod 600 ~/.ssh/authorized_keys" -ForegroundColor Yellow
    }
}

# ===================== CHECK PREREQUISITES =====================

$script:UseSudo = $false

function Check-Prerequisites {
    Write-Status "=== CHECKING PREREQUISITES ===" "Info"

    if (-not (Test-Path $VpsSshKey)) {
        Write-Status "SSH key not found at $VpsSshKey" "Error"
        Write-Status "Run first: .\deploy_to_vps.ps1 -Action fix-ssh" "Warning"
        exit 1
    }

    $testArgs = @("-o","StrictHostKeyChecking=no","-o","ConnectTimeout=10","-o","PasswordAuthentication=no","-i",$VpsSshKey)
    $conn = & ssh @testArgs "${VpsUser}@${VpsHost}" "echo CONNECTED" 2>$null
    if ($conn -notmatch "CONNECTED") {
        Write-Status "SSH key not accepted by VPS!" "Error"
        Write-Status "The public key is not in ~/.ssh/authorized_keys on the VPS" "Warning"
        Write-Host ""
        Write-Host "  Fix option 1 - Run: .\deploy_to_vps.ps1 -Action fix-ssh" -ForegroundColor Yellow
        Write-Host "  Fix option 2 - Manually on VPS:" -ForegroundColor Yellow
        $pub = Get-Content "$VpsSshKey.pub" -ErrorAction SilentlyContinue
        if ($pub) {
            Write-Host "    echo '$pub' >> ~/.ssh/authorized_keys" -ForegroundColor Yellow
            Write-Host "    chmod 600 ~/.ssh/authorized_keys" -ForegroundColor Yellow
        }
        Write-Host ""
        exit 1
    }
    Write-Status "SSH OK (passwordless)" "Success"

    $dk = Invoke-RemoteCommand "docker ps > /dev/null 2>&1 && echo OK || echo FAIL" -IgnoreError
    if ($dk -match "FAIL") {
        Write-Status "Docker requires sudo - will use sudo automatically" "Warning"
        Write-Status "Permanent fix on VPS: sudo usermod -aG docker ubuntu" "Warning"
        $script:UseSudo = $true
    } else {
        Write-Status "Docker rights OK (no sudo needed)" "Success"
        $script:UseSudo = $false
    }
    Write-Status "" "Info"
}

function Invoke-Docker {
    param([string]$Command, [switch]$IgnoreError)
    if ($script:UseSudo) { Invoke-RemoteCommand "sudo $Command" -IgnoreError:$IgnoreError }
    else                  { Invoke-RemoteCommand $Command -IgnoreError:$IgnoreError }
}

# ===================== BUILD IMAGE =====================

function Build-Image {
    Write-Status "=== BUILD DOCKER IMAGE ===" "Info"
    Write-Status "Project root: $ProjectRoot" "Info"

    $dockerfile = Join-Path $ProjectRoot "Dockerfile"
    if (-not (Test-Path $dockerfile)) {
        Write-Status "Dockerfile not found at $dockerfile" "Error"
        exit 1
    }

    # Web_GestCom.csproj references the sibling ../Web_GestCom.Core project, which sits
    # outside this build context — supplied as a separate named context instead of widening the
    # main one (which would otherwise also pull in GestCom_Desktop, __Delivery, etc.).
    $coreProjectRoot = Resolve-Path (Join-Path $ProjectRoot "..\Web_GestCom.Core")

    Write-Status "Building gestcom/app:prod ..." "Info"
    docker build --build-context "core=$coreProjectRoot" -t gestcom/app:prod $ProjectRoot

    if ($LASTEXITCODE -eq 0) {
        Write-Status "Image built successfully" "Success"
    } else {
        Write-Status "docker build FAILED" "Error"
        exit 1
    }
    Write-Status "" "Info"
}

# ===================== STEP 0: SYNC CONFIG FILES =====================

function Sync-ConfigFiles {
    Write-Status "=== STEP 0: SYNC CONFIG FILES ===" "Info"

    Invoke-RemoteCommand "mkdir -p $VpsPath/vhost.d $VpsPath/certs $VpsPath/html $VpsPath/images"

    if (Test-Path "$LocalDeployPath\.env") {
        Copy-ToRemote "$LocalDeployPath\.env" "$VpsPath/" | Out-Null
    } else {
        Write-Status ".env not found in $LocalDeployPath" "Error"
        Write-Status "Create it from .env.example and fill in real values!" "Warning"
        exit 1
    }

    # nginx-proxy vhost override (SignalR / WebSocket)
    $vhostFile = "$LocalDeployPath\vhost.d\$AppDomain"
    if (Test-Path $vhostFile) {
        Copy-ToRemote $vhostFile "$VpsPath/vhost.d/" | Out-Null
    } else {
        Write-Status "vhost.d/$AppDomain not found - WebSocket / SignalR will not work!" "Warning"
    }

    # Compose files
    foreach ($f in @("docker-compose.infra.yml","docker-compose_sql_prod.yml","docker-compose.app.yml")) {
        if (Test-Path "$LocalDeployPath\$f") {
            Copy-ToRemote "$LocalDeployPath\$f" "$VpsPath/" | Out-Null
        } else {
            Write-Status "$f not found - deployment may fail" "Warning"
        }
    }

    # Inject vhost override into the nginx-proxy vhost volume.
    # The volume name is resolved dynamically from the running nginx-proxy container
    # so this works whether the proxy is global (shared VPS) or dedicated.
    if (Test-Path $vhostFile) {
        Inject-VhostOverride $vhostFile
    }

    Write-Status "Config sync complete`n" "Success"
}

# ===================== STEP 1: PUSH DOCKER IMAGES =====================

function Push-ImagesToVPS {
    Write-Status "=== STEP 1: PUSH DOCKER IMAGES ===" "Info"

    $deliveryDir = "$LocalDeployPath\delivery_images"
    if (-not (Test-Path $deliveryDir)) { New-Item -ItemType Directory -Path $deliveryDir | Out-Null }

    foreach ($image in $Images) {
        Write-Status "Processing: $image" "Info"

        docker image inspect $image 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Status "Image not found locally: $image" "Warning"
            Write-Status "Run first: .\deploy_to_vps.ps1 -Action build-image" "Warning"
            continue
        }

        $sanitizedName = $image -replace "[^A-Za-z0-9_.-]", "-"
        $tarFile = "$deliveryDir\$sanitizedName.tar"

        Write-Status "Saving image to tar (may take a moment)..." "Info"
        docker save -o $tarFile $image

        if ($LASTEXITCODE -ne 0) {
            Write-Status "Failed to save $image" "Error"
            continue
        }

        $sizeMB = [Math]::Round((Get-Item $tarFile).Length / 1MB, 2)
        Write-Status "Saved ${sizeMB}MB - transferring to VPS..." "Info"

        $sshArgs = Get-SshArgs
        & scp @sshArgs $tarFile "${VpsUser}@${VpsHost}:${VpsPath}/"

        if ($LASTEXITCODE -ne 0) {
            Write-Status "Transfer failed for $image" "Error"
            Remove-Item $tarFile -Force -ErrorAction SilentlyContinue
            continue
        }

        Write-Status "Transferred - loading on VPS..." "Info"
        $fileName = Split-Path $tarFile -Leaf
        Invoke-Docker "docker load -i $VpsPath/$fileName"

        if ($LASTEXITCODE -eq 0) {
            Write-Status "Image loaded on VPS OK" "Success"
            Invoke-RemoteCommand "rm -f $VpsPath/$fileName"
        } else {
            Write-Status "docker load failed - check VPS manually" "Error"
        }

        Remove-Item $tarFile -Force -ErrorAction SilentlyContinue
    }

    Write-Status "Image push complete`n" "Success"
}

# ===================== CHECK CERTIFICATES =====================

function Check-Certificates {
    Write-Status "=== CHECKING SSL CERTIFICATES ===" "Info"

    $domains = @($AppDomain)

    foreach ($domain in $domains) {
        $check = Invoke-RemoteCommand "test -f $VpsPath/certs/$domain/fullchain.pem && echo EXISTS || echo MISSING" -IgnoreError

        if ($check -match "EXISTS") {
            $expiryRaw = Invoke-RemoteCommand "openssl x509 -enddate -noout -in $VpsPath/certs/$domain/fullchain.pem 2>/dev/null | cut -d= -f2" -IgnoreError
            if ($expiryRaw) {
                try {
                    $expiryDate = [datetime]::ParseExact($expiryRaw.Trim(), "MMM d HH:mm:ss yyyy 'GMT'", [System.Globalization.CultureInfo]::InvariantCulture)
                    $daysLeft = ($expiryDate - (Get-Date)).Days
                    if ($daysLeft -lt 30) {
                        Write-Status "  $domain cert OK but expiring in $daysLeft days (expires: $expiryDate)" "Warning"
                    } else {
                        Write-Status "  $domain cert OK (expires: $expiryDate, $daysLeft days left)" "Success"
                    }
                } catch {
                    Write-Status "  $domain cert exists (could not parse expiry: $expiryRaw)" "Warning"
                }
            } else {
                Write-Status "  $domain cert exists but could not read expiry date" "Warning"
            }
        } else {
            Write-Status "  $domain cert MISSING - Let's Encrypt will generate it on first start" "Warning"
        }
    }

    Write-Status "" "Info"
}

# ===================== VHOST INJECT HELPER =====================

function Get-NginxProxyContainer {
    # Returns the name of the running nginx-proxy container (any provider/name).
    $name = Invoke-Docker "docker ps --filter 'ancestor=nginxproxy/nginx-proxy' --format '{{.Names}}'" -IgnoreError
    if (-not $name) {
        $name = Invoke-Docker "docker ps --format '{{.Names}}' | grep -i proxy | head -1" -IgnoreError
    }
    return ($name -replace "`n","").Trim()
}

function Assert-VhostFileSafe {
    param([string]$LocalVhostFile)
    # Refuse d'injecter un fichier vhost contenant $connection_upgrade :
    # cette variable n'est définie que sur certaines versions de nginx-proxy
    # et provoque un crash nginx [emerg] à l'injection.
    $content = Get-Content $LocalVhostFile -Raw -ErrorAction SilentlyContinue
    if ($content -match '\$connection_upgrade') {
        Write-Status "ERREUR — vhost file uses `$connection_upgrade` which crashes older nginx-proxy!" "Error"
        Write-Status "  Replace: proxy_set_header Connection `$connection_upgrade;" "Warning"
        Write-Status "  With:    proxy_set_header Connection `$http_upgrade;"     "Warning"
        Write-Status "  File: $LocalVhostFile" "Warning"
        exit 1
    }
}

function Inject-VhostOverride {
    param([string]$LocalVhostFile)

    Write-Status "Injecting nginx vhost override (SignalR/WebSocket config)..." "Info"

    # Safety check — prevent injecting a config that will crash nginx
    Assert-VhostFileSafe $LocalVhostFile

    $proxyContainer = Get-NginxProxyContainer
    if (-not $proxyContainer) {
        Write-Status "No running nginx-proxy found - vhost override skipped" "Warning"
        Write-Status "  WARNING: SignalR timeouts will cause Blazor circuit drops after ~60s!" "Warning"
        return
    }
    Write-Status "  nginx-proxy container: $proxyContainer" "Info"

    # docker cp works regardless of whether vhost.d is a named volume or a bind mount
    $remoteTmp = "/tmp/gestcom-vhost-override"
    Copy-ToRemote $LocalVhostFile $remoteTmp | Out-Null
    Invoke-Docker "docker cp $remoteTmp ${proxyContainer}:/etc/nginx/vhost.d/$AppDomain" -IgnoreError
    Invoke-RemoteCommand "rm -f $remoteTmp"

    # Reload nginx to pick up the new config without dropping connections
    Invoke-Docker "docker kill --signal=HUP $proxyContainer" -IgnoreError
    Write-Status "vhost override injected — nginx reloaded" "Success"
}

function Fix-Vhost {
    Write-Status "=== FIX VHOST (re-inject + nginx reload) ===" "Info"
    $vhostFile = "$LocalDeployPath\vhost.d\$AppDomain"
    if (-not (Test-Path $vhostFile)) {
        Write-Status "vhost file not found: $vhostFile" "Error"; exit 1
    }
    Inject-VhostOverride $vhostFile
    Write-Status "Done — test: curl -I http://$AppDomain" "Success"
}

# ===================== STEP 2: SETUP NETWORK =====================

function Setup-Network {
    Write-Status "=== STEP 2: SETUP DOCKER NETWORK ===" "Info"

    # 1. Create ntw_gestcom_prod if it doesn't exist (compose file is network-only)
    $netCheck = Invoke-Docker "docker network ls --filter name=$NetworkName --format '{{.Name}}'" -IgnoreError
    if ($netCheck -match $NetworkName) {
        Write-Status "Network $NetworkName already exists" "Success"
    } else {
        Invoke-Docker "docker compose -f $VpsPath/docker-compose.infra.yml up -d" -IgnoreError
        $netCheck2 = Invoke-Docker "docker network ls --filter name=$NetworkName --format '{{.Name}}'" -IgnoreError
        if ($netCheck2 -match $NetworkName) {
            Write-Status "Network $NetworkName created" "Success"
        } else {
            Write-Status "Failed to create network $NetworkName" "Error"
            exit 1
        }
    }

    # 2. Connect the global nginx-proxy to the gestcom network
    $proxyContainer = Get-NginxProxyContainer
    if ($proxyContainer) {
        Write-Status "Connecting $proxyContainer to $NetworkName..." "Info"
        $already = Invoke-Docker "docker network inspect $NetworkName --format '{{range .Containers}}{{.Name}} {{end}}'" -IgnoreError
        if ($already -match $proxyContainer) {
            Write-Status "  $proxyContainer already connected to $NetworkName" "Success"
        } else {
            Invoke-Docker "docker network connect $NetworkName $proxyContainer" -IgnoreError
            Write-Status "  Connected $proxyContainer to $NetworkName" "Success"
        }

        # 3. Connect acme-companion too (so it can issue certs for gestcom).
        # The tunisiaauto companion is named 'nginx-letsencrypt' — search by image AND by name.
        $acme = Invoke-Docker "docker ps --filter 'ancestor=nginxproxy/acme-companion' --format '{{.Names}}'" -IgnoreError
        $acme = ($acme -replace "`n","").Trim()
        if (-not $acme) {
            # Fallback: search by common names used for acme-companion containers
            $acme = Invoke-Docker "docker ps --format '{{.Names}}' | grep -iE 'letsencrypt|acme' | head -1" -IgnoreError
            $acme = ($acme -replace "`n","").Trim()
        }
        if ($acme) {
            $alreadyAcme = Invoke-Docker "docker network inspect $NetworkName --format '{{range .Containers}}{{.Name}} {{end}}'" -IgnoreError
            if ($alreadyAcme -match $acme) {
                Write-Status "  $acme already connected to $NetworkName" "Success"
            } else {
                Invoke-Docker "docker network connect $NetworkName $acme" -IgnoreError
                Write-Status "  Connected $acme ($NetworkName) — SSL cert will be issued for $AppDomain" "Success"
            }
        } else {
            Write-Status "No acme-companion found - SSL cert will NOT be auto-issued for $AppDomain" "Warning"
        }
    } else {
        Write-Status "No nginx-proxy found on VPS - deploy your own proxy first" "Warning"
    }

    Write-Status "" "Info"
}

# ===================== STEP 3: DEPLOY DATABASE =====================

function Deploy-Database {
    Write-Status "=== STEP 3: DEPLOY SQL SERVER ===" "Info"

    Invoke-Docker "docker compose -f $VpsPath/docker-compose_sql_prod.yml --env-file $VpsPath/.env up -d"
    if ($LASTEXITCODE -ne 0) {
        Write-Status "Failed to start SQL Server" "Error"
        exit 1
    }

    Write-Status "Waiting for SQL Server healthcheck (up to 120s)..." "Info"
    $maxWait = 120
    $elapsed = 0
    do {
        Start-Sleep -Seconds 15
        $elapsed += 15
        $health = Invoke-Docker "docker inspect --format='{{.State.Health.Status}}' gestcom-sqlserver" -IgnoreError
        Write-Status "  SQL Server health: $health ($elapsed s)" "Info"
    } while ($health -notmatch "healthy" -and $elapsed -lt $maxWait)

    if ($health -match "healthy") {
        Write-Status "SQL Server is healthy" "Success"
    } else {
        Write-Status "SQL Server did not become healthy in ${maxWait}s - check: docker logs gestcom-sqlserver" "Warning"
    }

    Write-Status "Database deployment complete`n" "Success"
}

# ===================== STEP 4: DEPLOY APPLICATION =====================

function Deploy-Application {
    Write-Status "=== STEP 4: DEPLOY BLAZOR APP ===" "Info"

    Write-Status "Verifying images on VPS..." "Info"
    foreach ($image in $Images) {
        $check = Invoke-Docker "docker image inspect $image > /dev/null 2>&1 && echo OK || echo MISSING" -IgnoreError
        if ($check -match "MISSING") {
            Write-Status "Image MISSING on VPS: $image" "Error"
            Write-Status "Run: .\deploy_to_vps.ps1 -Action build-image then push-images" "Warning"
            return
        }
        Write-Status "  $image OK" "Success"
    }

    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml down" -IgnoreError
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml --env-file $VpsPath/.env up -d"

    if ($LASTEXITCODE -eq 0) {
        Write-Status "Application started" "Success"
    } else {
        Write-Status "Failed to start application" "Error"
        return
    }

    Write-Status "Waiting for app to initialize (15s)..." "Info"
    Start-Sleep -Seconds 15

    Write-Status "Application status:" "Info"
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml ps"

    Write-Status "Application deployment complete`n" "Success"
}

# ===================== VERIFY =====================

function Verify-Deployment {
    Write-Status "=== VERIFICATION ===" "Info"

    Write-Status "Containers:" "Info"
    Invoke-Docker "docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'"

    Write-Status "Network members:" "Info"
    Invoke-Docker "docker network inspect $NetworkName --format '{{range .Containers}}{{.Name}} {{end}}'" -IgnoreError

    Write-Status "App logs (last 15):" "Info"
    Invoke-Docker "docker logs --tail 15 gestcom-app" -IgnoreError

    Write-Status "SSL certificate:" "Info"
    $ok  = Invoke-RemoteCommand "test -f $VpsPath/certs/$AppDomain/fullchain.pem && echo OK || echo MISSING" -IgnoreError
    $exp = Invoke-RemoteCommand "openssl x509 -enddate -noout -in $VpsPath/certs/$AppDomain/fullchain.pem 2>/dev/null | cut -d= -f2" -IgnoreError
    if ($ok -match "OK") { Write-Status "  $AppDomain - OK (expires: $exp)" "Success" }
    else                  { Write-Status "  $AppDomain - MISSING (will be auto-generated by acme-companion)" "Warning" }

    Write-Status "Verification complete`n" "Success"
}

# ===================== STATUS / LOGS / STOP / RESTART =====================

function Show-Status {
    Write-Status "=== STATUS ===" "Info"
    Invoke-Docker "docker ps -a --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'"
    Write-Status "=== DISK ===" "Info"
    Invoke-Docker "docker system df"
}

function Show-Logs {
    param([string]$Service = "gestcom-app")
    Write-Status "=== LOGS: $Service ===" "Info"
    Invoke-Docker "docker logs --tail 100 $Service"
}

function Stop-Services {
    Write-Status "Stopping all services..." "Warning"
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml down" -IgnoreError
    Invoke-Docker "docker compose -f $VpsPath/docker-compose_sql_prod.yml down" -IgnoreError
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.infra.yml down" -IgnoreError
    Write-Status "All services stopped" "Success"
}

function Restart-Services {
    Write-Status "Restarting application..." "Info"
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml restart"
    Write-Status "Restarted" "Success"
}

# ===================== CLEANUP =====================

function Invoke-Cleanup {
    Write-Status "=== CLEANUP ===" "Warning"

    Write-Status "Disk usage BEFORE:" "Info"
    Invoke-Docker "docker system df"

    Write-Status "Stopping stacks..." "Info"
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml down" -IgnoreError
    Invoke-Docker "docker compose -f $VpsPath/docker-compose_sql_prod.yml down --remove-orphans" -IgnoreError

    Write-Status "Removing dangling images..." "Info"
    Invoke-Docker "docker image prune -f"
    Invoke-Docker "docker image prune -a -f"
    Invoke-Docker "docker container prune -f"
    Invoke-Docker "docker network prune -f"
    Invoke-Docker "docker builder prune -f"
    Invoke-RemoteCommand "rm -f $VpsPath/*.tar && echo Tar files removed"

    Write-Status "Disk usage AFTER:" "Success"
    Invoke-Docker "docker system df"

    Write-Status "Cleanup complete`n" "Success"
}

# ===================== RESET ALL =====================

function Invoke-ResetAll {
    Write-Status "=== RESET ALL ===" "Warning"
    Write-Status "WARNING: This will destroy ALL containers, images and volumes on the VPS!" "Warning"
    Write-Status "NOTE: Let's Encrypt rate limit = 5 certs per domain per week!" "Warning"
    Write-Host ""
    Write-Host "  Are you sure? Type YES to confirm: " -ForegroundColor Red -NoNewline
    $confirm = Read-Host
    if ($confirm -ne "YES") {
        Write-Status "Reset cancelled." "Info"
        return
    }

    Write-Status "Stopping compose stacks..." "Info"
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.app.yml down -v" -IgnoreError
    Invoke-Docker "docker compose -f $VpsPath/docker-compose_sql_prod.yml down -v" -IgnoreError
    Invoke-Docker "docker compose -f $VpsPath/docker-compose.infra.yml down -v" -IgnoreError

    Write-Status "Stopping all containers..." "Info"
    Invoke-Docker 'sh -c ''ids=$(docker ps -aq); [ -n "$ids" ] && docker stop $ids; exit 0''' -IgnoreError

    Write-Status "Removing all containers..." "Info"
    Invoke-Docker 'sh -c ''ids=$(docker ps -aq); [ -n "$ids" ] && docker rm -f $ids; exit 0''' -IgnoreError

    Write-Status "Removing all images..." "Info"
    Invoke-Docker 'sh -c ''ids=$(docker images -aq); [ -n "$ids" ] && docker rmi -f $ids; exit 0''' -IgnoreError

    Write-Status "Removing volumes..." "Info"
    Invoke-Docker "docker volume prune -f"

    Write-Status "Removing networks..." "Info"
    Invoke-Docker "docker network prune -f"

    Write-Status "Removing build cache..." "Info"
    Invoke-Docker "docker builder prune -af"

    Write-Status "Disk usage after reset:" "Info"
    Invoke-Docker "docker system df"

    Write-Status "Reset complete - VPS is clean" "Success"
    Write-Status "Run deploy-all to redeploy from scratch" "Info"
}

# ===================== MAIN =====================

Write-Status "=======================================" "Info"
Write-Status " VPS Deployment - $AppDomain" "Info"
Write-Status " Action : $Action" "Info"
Write-Status " Target : ${VpsUser}@${VpsHost}:${VpsPath}" "Info"
Write-Status "=======================================" "Info"
Write-Status "" "Info"

if ($Action -eq "fix-ssh")     { Fix-SshKey; exit 0 }
if ($Action -eq "build-image") { Build-Image; exit 0 }

Check-Prerequisites

switch ($Action) {
    "push-images"   { Push-ImagesToVPS }
    "setup-network" { Check-Certificates; Sync-ConfigFiles; Setup-Network }
    "deploy-db"     { Sync-ConfigFiles; Deploy-Database }
    "deploy-app"    {
        Sync-ConfigFiles
        if (-not $SkipImagePush) { Build-Image; Push-ImagesToVPS }
        Deploy-Application; Verify-Deployment
    }
    "deploy-all"    {
        Sync-ConfigFiles; Check-Certificates; Setup-Network
        if (-not $SkipImagePush) { Build-Image; Push-ImagesToVPS }
        Deploy-Database; Deploy-Application; Verify-Deployment
    }
    "fix-vhost" { Fix-Vhost }
    "status"    { Show-Status }
    "syncFile"  { Sync-ConfigFiles }
    "logs"      { Show-Logs "gestcom-app" }
    "stop"      { Stop-Services }
    "restart"   { Restart-Services }
    "cleanup"   { Invoke-Cleanup }
    "reset-all" { Invoke-ResetAll }
    default {
        Write-Host @"

deploy_to_vps.ps1 -Action <action> [options]

ACTIONS:
  fix-ssh        Installer la cle SSH sur le VPS (une seule fois)
  build-image    Construire l'image Docker gestcom/app:prod localement
  push-images    Transferer l'image vers le VPS (docker save/load)
  setup-network  Creer le reseau Docker + nginx-proxy + Let's Encrypt
  deploy-db      Demarrer SQL Server uniquement
  deploy-app     Deployer l'application Blazor uniquement
  deploy-all     Deploiement complet (defaut) - build + push + infra + db + app
  fix-vhost      Re-injecter le vhost override SignalR dans nginx-proxy + reload
  status         Etat containers + espace disque
  logs           Logs du container gestcom-app
  syncFile       Synchroniser les fichiers de config vers le VPS
  restart        Redemarrer le container applicatif
  stop           Arreter tous les services
  cleanup        Nettoyer images/containers/cache inutilises
  reset-all      Tout supprimer sur le VPS (DANGEREUX - rate limit Let's Encrypt!)

OPTIONS:
  -RemoteHost     Hostname VPS  (defaut: vps-bf0b3440.vps.ovh.net)
  -RemoteUser     User SSH      (defaut: ubuntu)
  -SshKey         Cle SSH       (defaut: ~/.ssh/id_rsa)
  -RemotePath     Path VPS      (defaut: /home/ubuntu/docker/gestcom)
  -SkipImagePush  Ne pas rebuilder/pousser l'image (utiliser celle deja sur le VPS)

PREMIER DEPLOIEMENT:
  1. .\deploy_to_vps.ps1 -Action fix-ssh
  2. Copier deploy\prod\.env.example en deploy\prod\.env et remplir les valeurs
  3. .\deploy_to_vps.ps1
  (deploy-all : build image + push + infra + db + app)

MISE A JOUR APPLICATIVE:
  .\deploy_to_vps.ps1 -Action deploy-app

MISE A JOUR CODE SANS REBUILD:
  .\deploy_to_vps.ps1 -Action deploy-app -SkipImagePush
"@
    }
}

Write-Status "" "Info"
Write-Status "=== DONE ===" "Success"
