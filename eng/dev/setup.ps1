# PowerShell script to set up the DM Auth development environment.
# Checks SQL Server connectivity, updates appsettings.Development.json,
# applies the EF Core migration, and seeds the database.
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

# Step 3: Apply EF Core migration
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

# Step 4: Seed the database
Write-Host "`nSeeding database..." -ForegroundColor Yellow
sqlcmd -S $Server -d $Database -E -i $SeedScriptPath -b
if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Seeding failed" -ForegroundColor Red
    exit 1
}
Write-Host "[OK]   Database seeded" -ForegroundColor Green

Write-Host "`nSetup complete." -ForegroundColor Green
Write-Host "Start the API with: dotnet run --project src/DMAuth.Web" -ForegroundColor DarkGray
