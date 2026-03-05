# PowerShell script to set up the DM Auth development environment.
# Checks SQL Server connectivity, updates appsettings.Development.json,
# generates an RSA signing key in user secrets, applies the EF Core migration,
# and seeds the database.
#
# Usage:
#   .\eng\dev\setup.ps1
#   .\eng\dev\setup.ps1 -Server "localhost\sqlexpress"
#   .\eng\dev\setup.ps1 -Server "localhost\dev" -Database "DMAuth"

param(
    [string]$Server   = "localhost\dev",
    [string]$Database = "DMAuth"
)

$SolutionRoot     = Resolve-Path "$PSScriptRoot\..\.."
$AppSettingsPath  = Join-Path $SolutionRoot "src\DMAuth.Web\appsettings.Development.json"
$SeedScriptPath   = "$PSScriptRoot\seed-data.sql"
$ConnectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;"

Write-Host "DM Auth -- development environment setup" -ForegroundColor Cyan
Write-Host "  Server   : $Server"   -ForegroundColor DarkGray
Write-Host "  Database : $Database" -ForegroundColor DarkGray

# Step 1: Verify SQL Server connectivity
Write-Host "`nChecking SQL Server at $Server..." -ForegroundColor Yellow
sqlcmd -S $Server -E -Q "SELECT 1" -b | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Cannot reach SQL Server at $Server" -ForegroundColor Red
    Write-Host "       Ensure the instance is running and your account has access." -ForegroundColor DarkGray
    exit 1
}
Write-Host "[OK]   SQL Server accessible" -ForegroundColor Green

# Step 2: Sync appsettings.Development.json
Write-Host "`nUpdating appsettings.Development.json..." -ForegroundColor Yellow
$appSettings = Get-Content $AppSettingsPath -Raw | ConvertFrom-Json
$appSettings.ConnectionStrings.DmAuthConnection = $ConnectionString
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $AppSettingsPath -Encoding UTF8
Write-Host "[OK]   Connection string updated" -ForegroundColor Green

# Step 3: Generate RSA signing key and store in user secrets
Write-Host "`nConfiguring JWT signing key..." -ForegroundColor Yellow
$WebProjectPath = Join-Path $SolutionRoot "src\DMAuth.Web"

# Initialize user secrets for the Web project (idempotent)
dotnet user-secrets init --project $WebProjectPath 2>&1 | Out-Null

# Skip generation if a key already exists (re-running the script should not rotate the key)
$secretsList = dotnet user-secrets list --project $WebProjectPath 2>&1 | Out-String
if ($secretsList -match "Jwt:RsaPrivateKeyPem") {
    Write-Host "[OK]   RSA signing key already set (skipping generation)" -ForegroundColor Green
} else {
    # openssl ships with Git for Windows and is required to be on PATH
    $pem = (openssl genrsa 2048 2>$null) -join "`n"
    if (-not $pem -or $LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] openssl not found. Ensure Git for Windows (or standalone OpenSSL) is on your PATH." -ForegroundColor Red
        exit 1
    }
    dotnet user-secrets set "Jwt:RsaPrivateKeyPem" $pem --project $WebProjectPath | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] Failed to store RSA signing key in user secrets" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK]   RSA signing key generated and stored in user secrets" -ForegroundColor Green
}

# Step 4: Apply EF Core migration
Write-Host "`nApplying EF Core migration..." -ForegroundColor Yellow
Push-Location $SolutionRoot
try {
    dotnet ef database update `
        --project src/DMAuth.Infrastructure `
        --startup-project src/DMAuth.Web
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] Migration failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK]   Migration applied" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Step 5: Seed the database
Write-Host "`nSeeding database..." -ForegroundColor Yellow
sqlcmd -S $Server -d $Database -E -i $SeedScriptPath -b
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Seeding failed" -ForegroundColor Red
    exit 1
}
Write-Host "[OK]   Database seeded" -ForegroundColor Green

Write-Host "`nSetup complete." -ForegroundColor Green
Write-Host "Start the API with: dotnet run --project src/DMAuth.Web --launch-profile https" -ForegroundColor DarkGray
