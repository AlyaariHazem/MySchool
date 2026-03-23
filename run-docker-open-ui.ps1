param(
    [string]$SwaggerUrl = "http://localhost:8080/swagger",
    [string]$SwaggerJsonUrl = "http://localhost:8080/swagger/v1/swagger.json",
    [string]$AngularUrl = "http://localhost:4200"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting docker compose (build + up)..."
docker compose up -d --build | Out-Host

function Wait-ForHttp {
    param(
        [Parameter(Mandatory=$true)][string]$Url,
        [int]$TimeoutSeconds = 120
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 | Out-Null
            return $true
        } catch {
            Start-Sleep -Seconds 2
        }
    }
    return $false
}

Write-Host "Waiting for Swagger..."
if (Wait-ForHttp -Url $SwaggerJsonUrl -TimeoutSeconds 180) {
    Write-Host "Opening Swagger UI: $SwaggerUrl"
    Start-Process $SwaggerUrl | Out-Null
} else {
    Write-Host "Swagger not reachable in time. Check docker logs for backend."
}

Write-Host "Opening Angular URL: $AngularUrl"
Start-Process $AngularUrl | Out-Null

