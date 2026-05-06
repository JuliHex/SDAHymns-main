#!/bin/bash
# Setup script for SDAHymns (Linux/macOS)
# Run this script on a new machine to set up the development environment

set -e

SKIP_HOOKS=false
SKIP_TESTS=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-hooks)
            SKIP_HOOKS=true
            shift
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: ./setup.sh [--skip-hooks] [--skip-tests]"
            exit 1
            ;;
    esac
done

echo "🚀 SDAHymns Setup Script"
echo "========================"
echo ""

# Check .NET SDK
echo "Checking .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found!"
    echo "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK $DOTNET_VERSION found"
echo ""

# Check if in git repository
echo "Checking git repository..."
if [ ! -d ".git" ]; then
    echo "⚠️  Not a git repository. Run 'git init' first if needed."
else
    echo "✅ Git repository detected"
fi
echo ""

# Restore NuGet packages
echo "📦 Restoring NuGet packages..."
dotnet restore
echo "✅ Packages restored"
echo ""

# Build solution
echo "🔨 Building solution..."
dotnet build --configuration Debug --no-restore
echo "✅ Build succeeded"
echo ""

# Run tests (optional)
if [ "$SKIP_TESTS" = false ]; then
    echo "🧪 Running tests..."
    if dotnet test --configuration Debug --no-build --verbosity quiet; then
        echo "✅ All tests passed"
    else
        echo "⚠️  Some tests failed"
        echo "   You can continue, but please review test failures."
    fi
    echo ""
fi

# Install git hooks (optional)
if [ "$SKIP_HOOKS" = false ] && [ -d ".git" ]; then
    echo "🪝 Installing git pre-commit hooks..."
    if [ -f "scripts/install-hooks.sh" ]; then
        bash scripts/install-hooks.sh
        echo "✅ Pre-commit hooks installed"
    else
        echo "⚠️  Could not find scripts/install-hooks.sh (optional)"
    fi
    echo ""
fi

# Check database
echo "🗄️  Checking database..."
if [ -f "Resources/hymns.db" ]; then
    DB_SIZE=$(du -h "Resources/hymns.db" | cut -f1)
    echo "✅ Database found ($DB_SIZE)"
else
    echo "⚠️  Database not found in Resources/hymns.db"
    echo "   You may need to import hymns. See docs for instructions."
fi
echo ""

# Summary
echo "================================================"
echo "✅ Setup Complete!"
echo "================================================"
echo ""
echo "Next steps:"
echo "  • Run desktop app:  dotnet run --project src/SDAHymns.Desktop"
echo "  • Run CLI:          dotnet run --project src/SDAHymns.CLI"
echo "  • Run tests:        dotnet test"
echo "  • See all commands: make help"
echo ""
echo "Development workflow:"
echo "  1. Make changes to code"
echo "  2. Run: dotnet build"
echo "  3. Run: dotnet test"
echo "  4. Commit your changes"
echo ""
echo "For more info, see docs/CI-CD.md"
