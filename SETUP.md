# Setup Guide - Getting Started with SDAHymns

This guide will help you set up the project on a new computer.

## Prerequisites

Before you begin, make sure you have:

1. **.NET 10 SDK** installed
   - Download from: https://dotnet.microsoft.com/download
   - Verify with: `dotnet --version`

2. **Git** installed
   - Download from: https://git-scm.com/downloads
   - Verify with: `git --version`

3. **(Optional) Make** - For using Makefile commands
   - Windows: Install via `choco install make` or Git Bash
   - Linux: Usually pre-installed
   - macOS: Install Xcode Command Line Tools

## Quick Setup

### Option 1: Automated Setup (Recommended)

**Windows (PowerShell):**
```powershell
git clone https://github.com/your-org/SDAHymns.git
cd SDAHymns
./setup.ps1
```

**Linux/macOS:**
```bash
git clone https://github.com/your-org/SDAHymns.git
cd SDAHymns
chmod +x setup.sh
./setup.sh
```

**What the setup script does:**
- ✅ Checks .NET SDK is installed
- ✅ Restores NuGet packages
- ✅ Builds the solution
- ✅ Runs tests
- ✅ Installs git pre-commit hooks
- ✅ Checks database exists

### Option 2: Manual Setup

If you prefer to run commands manually:

```bash
# 1. Clone the repository
git clone https://github.com/your-org/SDAHymns.git
cd SDAHymns

# 2. Restore dependencies
dotnet restore

# 3. Build the solution
dotnet build

# 4. Run tests (verify everything works)
dotnet test

# 5. (Optional) Install pre-commit hooks
# Windows:
./scripts/install-hooks.ps1
# Linux/Mac:
./scripts/install-hooks.sh
```

### Option 3: Using Makefile

If you have `make` installed:

```bash
# Clone and setup
git clone https://github.com/your-org/SDAHymns.git
cd SDAHymns

# One command to do everything
make setup
```

## Verify Installation

After setup, verify everything works:

```bash
# Check build
dotnet build
# Should show: Build succeeded. X Warning(s) 0 Error(s)

# Check tests
dotnet test
# Should show: Passed! - Failed: 0, Passed: 7+

# Run desktop app
dotnet run --project src/SDAHymns.Desktop
# Should launch the application window
```

## Common Commands

### Using Makefile (Recommended)

```bash
make help          # Show all available commands
make build         # Build the solution
make test          # Run all tests
make run           # Run desktop app
make format        # Format code
make clean         # Clean build artifacts
```

### Using dotnet CLI Directly

```bash
# Development
dotnet restore                          # Restore packages
dotnet build                            # Build (includes type checking)
dotnet test                             # Run tests
dotnet run --project src/SDAHymns.Desktop  # Run desktop app
dotnet run --project src/SDAHymns.CLI      # Run CLI

# Code Quality
dotnet format                           # Auto-format code
dotnet format --verify-no-changes       # Check formatting
dotnet build                            # Includes code analysis

# Database
dotnet ef database update --project src/SDAHymns.Core           # Apply migrations
dotnet ef migrations add NAME --project src/SDAHymns.Core       # Create migration

# Clean
dotnet clean                            # Remove build artifacts
```

## Project Structure

```
SDAHymns/
├── src/
│   ├── SDAHymns.Desktop/      # Avalonia UI application
│   ├── SDAHymns.CLI/          # Command-line interface
│   └── SDAHymns.Core/         # Shared business logic & data
├── tests/
│   └── SDAHymns.Tests/        # Unit and integration tests
├── Resources/
│   └── hymns.db               # SQLite database (1,254 hymns)
├── scripts/
│   ├── install-hooks.sh       # Install git hooks (Linux/Mac)
│   └── install-hooks.ps1      # Install git hooks (Windows)
├── docs/
│   └── CI-CD.md               # CI/CD documentation
├── Makefile                   # Development commands
├── setup.sh                   # Setup script (Linux/Mac)
├── setup.ps1                  # Setup script (Windows)
└── .editorconfig              # Code style configuration
```

## Development Workflow

### Standard Workflow

```bash
# 1. Make changes to code

# 2. Build (includes type checking + code analysis)
dotnet build

# 3. Run tests
dotnet test

# 4. (Optional) Format code
dotnet format

# 5. Commit changes
git add .
git commit -m "your message"
# If pre-commit hooks installed, build + tests run automatically

# 6. Push to GitHub
git push
# GitHub Actions CI runs automatically
```

### Using Makefile

```bash
# 1. Make changes to code

# 2. Build and test
make build
make test

# 3. Commit (hooks run automatically if installed)
git commit -m "your message"
```

## IDE Setup

### Visual Studio Code

**Recommended Extensions:**
- C# Dev Kit (Microsoft)
- .NET Runtime Install Tool
- EditorConfig for VS Code

**Settings:**
- Format on save: Enabled
- Code analysis: Enabled (via .editorconfig)

### JetBrains Rider

**Settings:**
- EditorConfig: Automatically detected
- Code style: Use EditorConfig settings
- Code inspection: Enabled

### Visual Studio

- EditorConfig: Automatically detected
- Code analysis: Enabled via project settings

## Troubleshooting

### Build Fails with "SDK not found"

**Solution:** Install .NET 10 SDK
```bash
# Check SDK version
dotnet --version

# Should be 10.x.x
# If not, download from https://dotnet.microsoft.com/download
```

### Tests Fail on Fresh Clone

**Possible causes:**
1. Database missing
   - Check: `Resources/hymns.db` exists
   - Solution: Copy from another machine or regenerate

2. LibreOffice not installed (for PPT tests)
   - Solution: Install LibreOffice or skip those tests

### "Permission Denied" on setup.sh

**Solution:**
```bash
chmod +x setup.sh
chmod +x scripts/install-hooks.sh
```

### Pre-commit hook not running

**Solution:**
```bash
# Check hook is installed
ls .git/hooks/pre-commit

# Reinstall if needed
./scripts/install-hooks.ps1   # Windows
./scripts/install-hooks.sh    # Linux/Mac
```

### Code analysis warnings

**This is normal!** Warnings are suggestions, not errors.

To fix them (optional):
```bash
# See warnings
dotnet build

# Auto-fix formatting
dotnet format

# Suppress specific warnings in .editorconfig
```

## Database Setup

The project includes a pre-populated database with 1,254 hymns at `Resources/hymns.db`.

If you need to regenerate it:

```bash
# Import from legacy XML
dotnet run --project src/SDAHymns.CLI -- import-legacy --input "path/to/xml"

# Import verses from PowerPoint
dotnet run --project src/SDAHymns.CLI -- import-verses --category crestine
```

See CLI documentation for more details.

## Next Steps

After setup is complete:

1. **Read the documentation:**
   - `docs/CI-CD.md` - CI/CD workflow
   - `CLAUDE.md` - Project overview
   - `.claude/spec/` - Feature specifications

2. **Explore the code:**
   ```bash
   # Run the desktop app
   make run

   # Or directly:
   dotnet run --project src/SDAHymns.Desktop
   ```

3. **Run tests:**
   ```bash
   make test

   # Or directly:
   dotnet test --verbosity normal
   ```

4. **Make a change:**
   - Edit a file
   - Run `dotnet build`
   - Run `dotnet test`
   - Commit your changes

## Getting Help

- **Questions:** Open an issue on GitHub
- **Bugs:** Open an issue with reproduction steps
- **Docs:** Check `docs/` folder
- **Specs:** Check `.claude/spec/` folder

## Quick Reference

```bash
# First time setup
./setup.ps1              # Windows
./setup.sh               # Linux/Mac
make setup               # With make

# Daily development
make build               # Build
make test                # Test
make run                 # Run app
make help                # See all commands

# Without make
dotnet build             # Build
dotnet test              # Test
dotnet run --project src/SDAHymns.Desktop  # Run
```
