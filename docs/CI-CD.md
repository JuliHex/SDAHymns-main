# CI/CD and Code Quality

This document explains the continuous integration and code quality setup for SDAHymns.

## Overview

Unlike JavaScript/TypeScript projects that need separate type-checking, linting, and testing steps, .NET consolidates most of this into the **build process**.

## What Happens During Build

When you run `dotnet build`, it automatically:

✅ **Type checks** - Verifies all types are correct
✅ **Syntax checks** - Ensures valid C# syntax
✅ **Null safety** - Checks nullable reference types
✅ **Code analysis** - Runs Roslyn analyzers for code quality
✅ **Style checks** - Enforces .editorconfig rules (if `EnforceCodeStyleInBuild` is enabled)

**This means: If `dotnet build` passes, your code is type-safe and follows basic quality rules.**

## Standard Development Workflow

### Before Committing

```bash
# 1. Build (includes type checking)
dotnet build

# 2. Run tests
dotnet test

# 3. (Optional) Check formatting
dotnet format --verify-no-changes

# 4. If all pass, commit
git add .
git commit -m "your message"
```

### Pre-Commit Hook (Optional)

To **automatically** run build + tests before every commit:

**Windows (PowerShell):**
```powershell
./scripts/install-hooks.ps1
```

**Linux/macOS:**
```bash
./scripts/install-hooks.sh
```

**What the hook does:**
- Runs `dotnet build` (type checking + compilation)
- Runs `dotnet test` (all unit tests)
- Blocks commit if either fails

**To skip the hook temporarily:**
```bash
git commit --no-verify
```

## GitHub Actions CI/CD

Every push and pull request automatically runs on GitHub Actions:

### What CI Checks

1. **Build on multiple platforms** (Windows, Linux, macOS)
2. **Run all tests** with detailed reporting
3. **Code analysis** with Roslyn analyzers
4. **Formatting check** (optional)

### CI Workflow Location

`.github/workflows/ci.yml`

### View CI Results

- ✅ Green checkmark = All checks passed
- ❌ Red X = Build or tests failed
- Click "Details" to see logs

## Code Quality Tools

### 1. EditorConfig (`.editorconfig`)

Defines code style rules:
- Indentation (4 spaces)
- Naming conventions (interfaces start with `I`, private fields with `_`)
- Bracing style
- Using directives organization

**Your IDE (VS Code, Rider, Visual Studio) will automatically apply these rules.**

### 2. Roslyn Analyzers

Built-in code quality checks enabled in `Directory.Build.props`:

```xml
<EnableNETAnalyzers>true</EnableNETAnalyzers>
<AnalysisMode>Recommended</AnalysisMode>
```

These catch:
- Potential bugs
- Performance issues
- Security vulnerabilities
- Best practice violations

### 3. Code Formatting

To auto-format your code:

```bash
# Format all files
dotnet format

# Check if formatting is correct (no changes)
dotnet format --verify-no-changes
```

## Comparison with JavaScript/TypeScript

| Task | JavaScript/TypeScript | .NET/C# |
|------|----------------------|---------|
| Type checking | `npm run type-check` | `dotnet build` |
| Linting | `npm run lint` (ESLint) | `dotnet build` (Roslyn) |
| Formatting | `npm run format` (Prettier) | `dotnet format` |
| Testing | `npm run test` | `dotnet test` |
| All together | 4 commands | 2 commands |

**In .NET, `dotnet build` does most of the heavy lifting!**

## When to Run What

### During Development
- **After making changes**: `dotnet build`
- **Before committing**: `dotnet build && dotnet test`

### When to Run Formatting
- **Manual check**: `dotnet format --verify-no-changes`
- **Auto-fix**: `dotnet format`
- **Usually not needed**: Your IDE formats on save

### CI/CD (Automatic)
- **Every push**: Full build + tests on all platforms
- **Pull requests**: Must pass before merge

## Troubleshooting

### Build Fails Locally

```bash
# See detailed errors
dotnet build --verbosity detailed

# Common issues:
# - Syntax errors (fix in IDE)
# - Type mismatches (check types)
# - Missing using directives (add at top of file)
```

### Tests Fail

```bash
# Run tests with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~YourTestName"
```

### Code Analysis Warnings

```bash
# Build shows warnings but succeeds
dotnet build

# To treat warnings as errors (strict mode):
dotnet build -p:TreatWarningsAsErrors=true
```

### Formatting Issues

```bash
# See what would change
dotnet format --verify-no-changes

# Apply formatting
dotnet format

# Format specific file
dotnet format --include src/YourFile.cs
```

## Best Practices

### ✅ Do's

- **Run `dotnet build` frequently** while developing
- **Run `dotnet test` before committing**
- **Let CI catch issues** - don't stress about local environment
- **Review CI logs** if build fails on GitHub
- **Keep tests passing** - don't commit broken tests

### ❌ Don'ts

- **Don't commit without building** first
- **Don't disable analyzers** without good reason
- **Don't use `--no-verify`** to skip pre-commit hooks (except emergencies)
- **Don't ignore CI failures** - fix them before merging

## Quick Reference

```bash
# Development
dotnet build              # Type check + compile
dotnet test               # Run tests
dotnet format             # Auto-format code

# CI/CD
git push                  # Triggers GitHub Actions
# → Builds on Windows, Linux, macOS
# → Runs all tests
# → Reports results on PR

# Pre-commit (if installed)
git commit                # Automatically runs build + tests
git commit --no-verify    # Skip pre-commit hook
```

## Questions?

- **Q: Is `dotnet build` enough?**
  A: For type safety, yes. But also run `dotnet test` before committing.

- **Q: Do I need to run code analysis separately?**
  A: No, it's included in `dotnet build` (if enabled in project).

- **Q: Should I install the pre-commit hook?**
  A: Optional, but recommended to catch issues early.

- **Q: What if CI fails but local build passes?**
  A: Check the GitHub Actions log - might be platform-specific or missing dependency.

- **Q: Can I disable specific analyzer rules?**
  A: Yes, in `.editorconfig` with `dotnet_diagnostic.CA1234.severity = none`
