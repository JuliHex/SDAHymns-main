# Install git hooks for SDAHymns (PowerShell version)

Write-Host "Installing git hooks..." -ForegroundColor Cyan

# Create .git/hooks directory if it doesn't exist
New-Item -ItemType Directory -Force -Path .git/hooks | Out-Null

# Copy pre-commit hook
Copy-Item scripts/pre-commit .git/hooks/pre-commit -Force

Write-Host "✅ Git hooks installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "The pre-commit hook will now run automatically before each commit."
Write-Host "It will:"
Write-Host "  - Build the solution (type checking)"
Write-Host "  - Run all tests"
Write-Host ""
Write-Host "To skip the hook temporarily, use: git commit --no-verify"
