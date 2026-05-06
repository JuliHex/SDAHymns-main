# Setup script for SDAHymns (Windows PowerShell)
# Run this script on a new machine to set up the development environment

param(
    [switch]$SkipHooks,
    [switch]$SkipTests
)

Write-Host "🚀 SDAHymns Setup Script" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET SDK $dotnetVersion found" -ForegroundColor Green
Write-Host ""

# Check if in git repository
Write-Host "Checking git repository..." -ForegroundColor Yellow
$isGitRepo = Test-Path ".git"
if (-not $isGitRepo) {
    Write-Host "⚠️  Not a git repository. Run 'git init' first if needed." -ForegroundColor Yellow
} else {
    Write-Host "✅ Git repository detected" -ForegroundColor Green
}
Write-Host ""

# Restore NuGet packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore packages" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Packages restored" -ForegroundColor Green
Write-Host ""

# Build solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build --configuration Debug --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build succeeded" -ForegroundColor Green
Write-Host ""

# Run tests (optional)
if (-not $SkipTests) {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    dotnet test --configuration Debug --no-build --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Some tests failed" -ForegroundColor Yellow
        Write-Host "   You can continue, but please review test failures." -ForegroundColor Yellow
    } else {
        Write-Host "✅ All tests passed" -ForegroundColor Green
    }
    Write-Host ""
}

# Install git hooks (optional)
if (-not $SkipHooks -and $isGitRepo) {
    Write-Host "🪝 Installing git pre-commit hooks..." -ForegroundColor Yellow
    $hookInstalled = $false
    if (Test-Path "scripts/install-hooks.ps1") {
        & scripts/install-hooks.ps1
        if ($LASTEXITCODE -eq 0) {
            $hookInstalled = $true
        }
    }

    if ($hookInstalled) {
        Write-Host "✅ Pre-commit hooks installed" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Could not install hooks (optional)" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Check database
Write-Host "🗄️  Checking database..." -ForegroundColor Yellow
if (Test-Path "Resources/hymns.db") {
    $dbSize = (Get-Item "Resources/hymns.db").Length
    Write-Host "✅ Database found ($([math]::Round($dbSize / 1KB, 2)) KB)" -ForegroundColor Green
} else {
    Write-Host "⚠️  Database not found in Resources/hymns.db" -ForegroundColor Yellow
    Write-Host "   You may need to import hymns. See docs for instructions." -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✅ Setup Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  • Run desktop app:  " -NoNewline; Write-Host "dotnet run --project src/SDAHymns.Desktop" -ForegroundColor White
Write-Host "  • Run CLI:          " -NoNewline; Write-Host "dotnet run --project src/SDAHymns.CLI" -ForegroundColor White
Write-Host "  • Run tests:        " -NoNewline; Write-Host "dotnet test" -ForegroundColor White
Write-Host "  • See all commands: " -NoNewline; Write-Host "make help" -ForegroundColor White
Write-Host ""
Write-Host "Development workflow:" -ForegroundColor Cyan
Write-Host "  1. Make changes to code" -ForegroundColor Gray
Write-Host "  2. Run: " -NoNewline -ForegroundColor Gray; Write-Host "dotnet build" -ForegroundColor White
Write-Host "  3. Run: " -NoNewline -ForegroundColor Gray; Write-Host "dotnet test" -ForegroundColor White
Write-Host "  4. Commit your changes" -ForegroundColor Gray
Write-Host ""
Write-Host "For more info, see docs/CI-CD.md" -ForegroundColor Gray
