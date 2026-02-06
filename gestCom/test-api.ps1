# Test script for GestCom API
Write-Host "Testing GestCom API Login..." -ForegroundColor Cyan

$body = @{
    email = "admin@gestcom.tn"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -ContentType "application/json" -Body $body
    Write-Host "SUCCESS! Login response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorContent = $reader.ReadToEnd()
        Write-Host "Response content: $errorContent" -ForegroundColor Yellow
    }
}
