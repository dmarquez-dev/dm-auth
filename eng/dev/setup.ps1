# PowerShell script to set up the DM Auth development environment.
# Verifies Azure CLI authentication, retrieves secrets from Key Vault,
# applies the EF Core migration, and seeds the database.
#
# Usage:
#   .\eng\dev\setup.ps1 -VaultName "my-dev-vault"

param(
    [Parameter(Mandatory)]
    [string]$VaultName
)

$SolutionRoot   = Resolve-Path "$PSScriptRoot\..\.."
$SeedScriptPath = "$PSScriptRoot\seed-data.sql"

Write-Host "DM Auth -- development environment setup" -ForegroundColor Cyan
Write-Host "  Key Vault: $VaultName" -ForegroundColor DarkGray

# Step 1: Verify Azure CLI authentication
Write-Host "`nVerifying Azure CLI authentication..." -ForegroundColor Yellow
az account show 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Not logged in to Azure CLI." -ForegroundColor Red
    Write-Host "       Run: az login" -ForegroundColor DarkGray
    exit 1
}
Write-Host "[OK]   Azure CLI authenticated" -ForegroundColor Green

# Step 2: Retrieve DB connection string from Key Vault
Write-Host "`nRetrieving connection string from Key Vault..." -ForegroundColor Yellow
$ConnectionString = az keyvault secret show `
    --vault-name $VaultName `
    --name "ConnectionStrings--DmAuth" `
    --query "value" `
    --output tsv 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Could not retrieve ConnectionStrings--DmAuth from vault '$VaultName'." -ForegroundColor Red
    Write-Host "       Ensure the secret exists and your account has the 'Key Vault Secrets User' role." -ForegroundColor DarkGray
    exit 1
}
Write-Host "[OK]   Connection string retrieved" -ForegroundColor Green

# Step 3: Apply EF Core migration
Write-Host "`nApplying EF Core migration..." -ForegroundColor Yellow
Push-Location $SolutionRoot
try {
    dotnet ef database update `
        --project src/DMAuth.Infrastructure `
        --startup-project src/DMAuth.Web `
        --connection $ConnectionString
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[FAIL] Migration failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK]   Migration applied" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Step 4: Seed the database
Write-Host "`nSeeding database..." -ForegroundColor Yellow
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Host "Installing SqlServer PowerShell module (one-time)..." -ForegroundColor DarkGray
    Install-Module SqlServer -Scope CurrentUser -AllowClobber -Force
}

try {
    Invoke-Sqlcmd -ConnectionString $ConnectionString -InputFile $SeedScriptPath -ErrorAction Stop
    Write-Host "[OK]   Database seeded" -ForegroundColor Green
}
catch {
    Write-Host "[FAIL] Seeding failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nSetup complete." -ForegroundColor Green
Write-Host "Start the API with: dotnet run --project src/DMAuth.Web --launch-profile https" -ForegroundColor DarkGray
