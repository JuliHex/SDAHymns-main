# Makefile for SDAHymns
# Works on Windows (with make installed), Linux, and macOS

.PHONY: help setup restore build test run clean format lint install-hooks version-bump release

# Default target
help:
	@echo "SDAHymns Development Commands"
	@echo ""
	@echo "Setup (first time):"
	@echo "  make setup          - Complete first-time setup (restore + build + install hooks)"
	@echo "  make install-hooks  - Install git pre-commit hooks"
	@echo ""
	@echo "Development:"
	@echo "  make restore        - Restore NuGet packages"
	@echo "  make build          - Build the solution (includes type checking)"
	@echo "  make test           - Run all tests"
	@echo "  make run            - Run the desktop application"
	@echo "  make run-cli        - Run the CLI application"
	@echo ""
	@echo "Code Quality:"
	@echo "  make format         - Auto-format all code"
	@echo "  make format-check   - Check if code is formatted correctly"
	@echo "  make lint           - Run code analysis (same as build)"
	@echo ""
	@echo "Database:"
	@echo "  make db-update      - Apply database migrations"
	@echo "  make db-migration   - Create new migration (use NAME=YourMigrationName)"
	@echo ""
	@echo "Release Management:"
	@echo "  make version-bump VERSION=1.0.0  - Bump version and commit"
	@echo "  make release VERSION=1.0.0       - Create release tag (triggers GitHub Actions)"
	@echo ""
	@echo "Publishing:"
	@echo "  make publish-win    - Build release for Windows"
	@echo "  make publish-mac    - Build release for macOS ARM64"
	@echo "  make publish-linux  - Build release for Linux"
	@echo ""
	@echo "Cleanup:"
	@echo "  make clean          - Remove build artifacts"
	@echo "  make clean-all      - Deep clean (removes bin, obj, packages)"

# First-time setup
setup: restore build install-hooks
	@echo ""
	@echo "✅ Setup complete! You're ready to start developing."
	@echo ""
	@echo "Next steps:"
	@echo "  - Run the app: make run"
	@echo "  - Run tests: make test"
	@echo "  - See all commands: make help"

# Restore dependencies
restore:
	@echo "Restoring NuGet packages..."
	dotnet restore

# Build solution
build:
	@echo "Building solution..."
	dotnet build --configuration Debug

# Build for release
build-release:
	@echo "Building release..."
	dotnet build --configuration Release

# Run tests
test:
	@echo "Running tests..."
	dotnet test --verbosity normal

# Run tests with detailed output
test-verbose:
	@echo "Running tests (verbose)..."
	dotnet test --verbosity detailed

# Run desktop application
run:
	@echo "Starting desktop application..."
	dotnet run --project src/SDAHymns.Desktop

# Run CLI application
run-cli:
	@echo "Starting CLI application..."
	dotnet run --project src/SDAHymns.CLI

# Format code
format:
	@echo "Formatting code..."
	dotnet format

# Check formatting
format-check:
	@echo "Checking code formatting..."
	dotnet format --verify-no-changes

# Run code analysis (lint)
lint: build
	@echo "Code analysis completed during build."

# Install git hooks
install-hooks:
	@echo "Installing git pre-commit hooks..."
ifeq ($(OS),Windows_NT)
	powershell -ExecutionPolicy Bypass -File scripts/install-hooks.ps1
else
	bash scripts/install-hooks.sh
endif

# Database: Apply migrations
db-update:
	@echo "Applying database migrations..."
	dotnet ef database update --project src/SDAHymns.Core

# Database: Create new migration
db-migration:
ifndef NAME
	@echo "Error: Please specify migration name with NAME=YourMigrationName"
	@exit 1
endif
	@echo "Creating migration: $(NAME)"
	dotnet ef migrations add $(NAME) --project src/SDAHymns.Core

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean

# Deep clean (removes all bin, obj, packages)
clean-all:
	@echo "Deep cleaning..."
	dotnet clean
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	@echo "Run 'make restore' to restore packages."

# Watch mode (auto-rebuild on file changes)
watch:
	@echo "Starting watch mode..."
	dotnet watch --project src/SDAHymns.Desktop run

# Publish for Windows
publish-win:
	@echo "Publishing for Windows..."
	dotnet publish src/SDAHymns.Desktop -c Release -r win-x64 --self-contained

# Publish for macOS
publish-mac:
	@echo "Publishing for macOS..."
	dotnet publish src/SDAHymns.Desktop -c Release -r osx-arm64 --self-contained

# Publish for Linux
publish-linux:
	@echo "Publishing for Linux..."
	dotnet publish src/SDAHymns.Desktop -c Release -r linux-x64 --self-contained

# Release Management
.PHONY: version-bump release

# Bump version and create git tag
version-bump:
ifndef VERSION
	@echo "Error: Please specify version with VERSION=1.0.0"
	@exit 1
endif
	@echo "Bumping version to $(VERSION)..."
	@sed -i 's/<Version>.*<\/Version>/<Version>$(VERSION)<\/Version>/' Directory.Build.props || \
	  sed -i '' 's/<Version>.*<\/Version>/<Version>$(VERSION)<\/Version>/' Directory.Build.props
	git add Directory.Build.props
	git commit -m "chore: bump version to $(VERSION)"
	@echo "Version bumped to $(VERSION)"
	@echo "Next: Run 'make release VERSION=$(VERSION)' to create tag and trigger release"

# Create release tag (triggers GitHub Actions)
release:
ifndef VERSION
	@echo "Error: Please specify version with VERSION=1.0.0"
	@exit 1
endif
	@echo "Creating release tag v$(VERSION)..."
	git tag v$(VERSION)
	git push origin main
	git push origin v$(VERSION)
	@echo ""
	@echo "✅ Release v$(VERSION) triggered!"
	@echo "Check GitHub Actions: https://github.com/your-org/SDAHymns/actions"
	@echo "Release will be available at: https://github.com/your-org/SDAHymns/releases"
