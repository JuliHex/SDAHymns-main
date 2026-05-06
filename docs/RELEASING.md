# Release Guide - Creating New Releases

This guide explains how to create new releases of SDAHymns with automated builds for all platforms.

## Overview

SDAHymns uses **automated GitHub Releases** for distribution. When you create a version tag, GitHub Actions automatically:

1. ✅ Builds for Windows (x64)
2. ✅ Builds for macOS (Apple Silicon + Intel)
3. ✅ Builds for Linux (x64)
4. ✅ Creates GitHub Release with all binaries
5. ✅ Generates release notes from commits
6. ✅ Attaches all platform packages to the release

## Release Workflow

### Option 1: Automatic Release (Tag-Based)

**This is the recommended approach:**

```bash
# 1. Update version in Directory.Build.props
# Edit Directory.Build.props and change <Version>0.1.0</Version>

# 2. Commit the version change
git add Directory.Build.props
git commit -m "chore: bump version to 1.0.0"

# 3. Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# 4. GitHub Actions automatically:
#    - Builds for all platforms
#    - Creates GitHub Release
#    - Uploads binaries
```

**Wait 5-10 minutes**, then check:
- GitHub Actions: `https://github.com/your-org/SDAHymns/actions`
- Releases: `https://github.com/your-org/SDAHymns/releases`

### Option 2: Manual Release (GitHub UI)

If you prefer to trigger manually:

1. Go to GitHub Actions: `https://github.com/your-org/SDAHymns/actions`
2. Click **"Release"** workflow
3. Click **"Run workflow"**
4. Enter version number (e.g., `1.0.0`)
5. Click **"Run workflow"**

GitHub Actions will create the release with all binaries.

## Version Numbering

We use **Semantic Versioning** (SemVer): `MAJOR.MINOR.PATCH`

### Format: `v1.2.3`

- **MAJOR** (1): Breaking changes, incompatible API changes
- **MINOR** (2): New features, backwards-compatible
- **PATCH** (3): Bug fixes, backwards-compatible

### Examples:

- `v0.1.0` - Initial beta release
- `v1.0.0` - First stable release
- `v1.1.0` - Added new feature
- `v1.1.1` - Fixed bug in v1.1.0
- `v2.0.0` - Breaking change (e.g., database schema change)

### When to Bump:

| Change Type | Version Change | Example |
|-------------|----------------|---------|
| Bug fix | `1.0.0` → `1.0.1` | Fixed crash when loading hymn |
| New feature (compatible) | `1.0.0` → `1.1.0` | Added search functionality |
| Breaking change | `1.0.0` → `2.0.0` | Changed database format |
| Internal refactor | `1.0.0` → `1.0.1` | No user-facing changes |

## Pre-Release Checklist

Before creating a release, ensure:

### 1. All Tests Pass

```bash
dotnet test
# Should show: Passed! - Failed: 0
```

### 2. Build Succeeds

```bash
dotnet build --configuration Release
# Should show: Build succeeded. 0 Error(s)
```

### 3. Manual Testing

- [ ] App launches without errors
- [ ] Can load hymns
- [ ] Verse navigation works
- [ ] Display window opens correctly
- [ ] Database is accessible
- [ ] No critical bugs

### 4. Update Version Number

**File: `Directory.Build.props`**

```xml
<PropertyGroup>
  <Version>1.0.0</Version>  <!-- Update this -->
  <!-- ... -->
</PropertyGroup>
```

### 5. Update CHANGELOG (Optional but Recommended)

**File: `CHANGELOG.md`**

```markdown
## [1.0.0] - 2025-12-04

### Added
- Full-text hymn search
- Display profiles with custom styling
- Keyboard shortcuts

### Fixed
- Crash when loading hymn #99

### Changed
- Improved UI layout
```

### 6. Commit Version Bump

```bash
git add Directory.Build.props CHANGELOG.md
git commit -m "chore: bump version to 1.0.0"
git push origin main
```

## Creating the Release

### Step-by-Step:

```bash
# 1. Ensure you're on the main branch
git checkout main
git pull origin main

# 2. Verify everything is clean
git status
# Should show: nothing to commit, working tree clean

# 3. Create version tag
git tag v1.0.0

# 4. Push tag to trigger release
git push origin v1.0.0
```

### What Happens Next:

1. **GitHub Actions starts** (~5-10 minutes)
   - Builds Windows binary
   - Builds macOS binaries (ARM + Intel)
   - Builds Linux binary
   - Packages each with database

2. **GitHub Release created**
   - Tag: `v1.0.0`
   - Release name: `SDAHymns v1.0.0`
   - Attached files:
     - `SDAHymns-v1.0.0-windows-x64.zip`
     - `SDAHymns-v1.0.0-macos-arm64.zip`
     - `SDAHymns-v1.0.0-macos-x64.zip`
     - `SDAHymns-v1.0.0-linux-x64.tar.gz`

3. **Users can download**
   - Go to Releases page
   - Download for their platform
   - Extract and run

## Release Artifacts

Each release includes:

### Windows (`SDAHymns-v1.0.0-windows-x64.zip`)
- `SDAHymns.Desktop.exe` (self-contained, no .NET install needed)
- `Resources/hymns.db` (database with all hymns)
- All required dependencies

**Size:** ~80-100 MB

### macOS ARM64 (`SDAHymns-v1.0.0-macos-arm64.zip`)
- `SDAHymns.Desktop` (executable for Apple Silicon M1/M2/M3)
- `Resources/hymns.db`
- All required dependencies

**Size:** ~70-90 MB

### macOS x64 (`SDAHymns-v1.0.0-macos-x64.zip`)
- `SDAHymns.Desktop` (executable for Intel Macs)
- `Resources/hymns.db`
- All required dependencies

**Size:** ~70-90 MB

### Linux (`SDAHymns-v1.0.0-linux-x64.tar.gz`)
- `SDAHymns.Desktop` (executable for Linux x64)
- `Resources/hymns.db`
- All required dependencies

**Size:** ~70-90 MB

## Troubleshooting

### Release Build Failed

1. Check GitHub Actions logs
2. Common issues:
   - Build errors → Fix and re-tag
   - Test failures → Fix tests first
   - Missing files → Check .csproj includes

### Tag Already Exists

If you need to re-release:

```bash
# Delete local tag
git tag -d v1.0.0

# Delete remote tag
git push origin :refs/tags/v1.0.0

# Delete GitHub Release (in UI)
# Go to Releases → Click release → Delete

# Recreate tag
git tag v1.0.0
git push origin v1.0.0
```

### Wrong Version Number

If you tagged with wrong version:

```bash
# Delete tag (see above)
# Fix version in Directory.Build.props
# Commit and push
# Create correct tag
```

## Post-Release

After release is published:

### 1. Verify Downloads

Download each platform binary and test:

```bash
# Windows
.\SDAHymns.Desktop.exe

# macOS/Linux
chmod +x SDAHymns.Desktop
./SDAHymns.Desktop
```

### 2. Update Documentation

Update README.md with:
- Latest version number
- Download links
- Installation instructions

### 3. Announce Release

- Create GitHub Discussions post
- Update project website
- Notify users

### 4. Monitor Issues

Watch for:
- Installation problems
- Platform-specific bugs
- User feedback

## Beta/Pre-Release

For beta releases, use pre-release versions:

```bash
# Beta version
git tag v1.0.0-beta.1
git push origin v1.0.0-beta.1

# Release candidate
git tag v1.0.0-rc.1
git push origin v1.0.0-rc.1
```

Mark as pre-release in GitHub UI (edit release).

## Hotfix Releases

For urgent bug fixes:

```bash
# 1. Create hotfix branch
git checkout -b hotfix/1.0.1

# 2. Fix the bug
# ... make changes ...

# 3. Update version to 1.0.1
# Edit Directory.Build.props

# 4. Commit
git add .
git commit -m "fix: critical bug in hymn loader"

# 5. Merge to main
git checkout main
git merge hotfix/1.0.1
git push origin main

# 6. Tag and release
git tag v1.0.1
git push origin v1.0.1
```

## Version History

Keep track of releases:

| Version | Date | Notes |
|---------|------|-------|
| v0.1.0  | 2025-12-04 | Initial beta |
| v1.0.0  | TBD | First stable release |

## Quick Commands

```bash
# Create release (full process)
git add Directory.Build.props
git commit -m "chore: bump version to X.Y.Z"
git push origin main
git tag vX.Y.Z
git push origin vX.Y.Z

# Check release status
# Go to: https://github.com/your-org/SDAHymns/actions

# Delete tag (if mistake)
git tag -d vX.Y.Z
git push origin :refs/tags/vX.Y.Z
```

## Future: Auto-Update

In a future release, we'll add **automatic updates**:
- App checks for new version on startup
- One-click update button
- No manual downloads needed

See: `docs/AUTO-UPDATE.md` (coming soon)

## Questions?

- Check GitHub Actions logs for build errors
- Open an issue for release process problems
- See `docs/CI-CD.md` for general CI/CD info
