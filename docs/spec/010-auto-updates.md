# Spec 010: Auto-Updates with Velopack

**Status:** ✓ Tested
**Created:** 2025-12-04
**Implemented:** 2025-12-26 (Commit 4e3ec7d)
**Dependencies:** 001-project-structure.md

## Overview

Implement automatic update functionality using Velopack to ensure users always have the latest version. The system will check GitHub Releases for new versions, notify users, and handle seamless installation with minimal disruption. This is prioritized early to enable testing during active development and to support easy distribution to early users.

## Goals

1. Integrate Velopack into the application for cross-platform updates
2. Check for updates on app startup (non-blocking)
3. Notify users when updates are available
4. Download and install updates with one click
5. Support delta updates to minimize download size
6. Work seamlessly with existing GitHub Releases CI/CD pipeline
7. Handle edge cases (no internet, update failures, etc.)

## Why Velopack?

**Advantages:**
- ✅ Modern, actively maintained (successor to Squirrel.Windows)
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ Built specifically for .NET applications
- ✅ Delta updates (only download changed files)
- ✅ Integrates perfectly with GitHub Releases
- ✅ Simple API, minimal boilerplate
- ✅ No external dependencies or installers needed

**vs. Alternatives:**
- **Squirrel.Windows**: Deprecated, Windows-only
- **ClickOnce**: Limited, outdated, poor UX
- **Custom solution**: More work, less reliable

## Architecture

### Update Flow

```
App Startup
    ↓
Check for Updates (Background)
    ↓
[No Update] → Continue normally
    ↓
[Update Available]
    ↓
Show Notification in UI
    ↓
User Clicks "Update Now"
    ↓
Download Update (Progress Bar)
    ↓
Install & Restart
    ↓
App Relaunches with New Version
```

### Components

1. **UpdateService** (`SDAHymns.Core/Services/UpdateService.cs`)
   - Check for updates via Velopack
   - Download updates
   - Apply updates and restart

2. **Update Notification UI** (In MainWindow or separate dialog)
   - Show update available message
   - Display changelog
   - "Update Now" and "Later" buttons
   - Progress indicator during download

3. **Velopack Integration**
   - Configure in Program.cs startup
   - Hook into application lifecycle

4. **CI/CD Changes**
   - Modify build workflow to create Velopack packages
   - Upload packages to GitHub Releases

## Implementation

### Step 1: Add Velopack NuGet Package

```xml
<!-- SDAHymns.Desktop/SDAHymns.Desktop.csproj -->
<PackageReference Include="Velopack" Version="0.0.*" />
```

### Step 2: Create UpdateService

```csharp
// src/SDAHymns.Core/Services/UpdateService.cs

using Velopack;
using Velopack.Sources;

namespace SDAHymns.Core.Services;

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task<bool> DownloadUpdatesAsync(UpdateInfo updateInfo, IProgress<int>? progress = null);
    void ApplyUpdatesAndRestart(UpdateInfo updateInfo);
    bool IsUpdateAvailable { get; }
    string? LatestVersion { get; }
}

public class UpdateService : IUpdateService
{
    private readonly UpdateManager _updateManager;
    private UpdateInfo? _pendingUpdate;

    public bool IsUpdateAvailable => _pendingUpdate != null;
    public string? LatestVersion => _pendingUpdate?.TargetFullRelease.Version.ToString();

    public UpdateService()
    {
        // Configure GitHub Releases source
        _updateManager = new UpdateManager(
            new GithubSource("https://github.com/YOUR-USERNAME/SDAHymns", null, false)
        );
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            _pendingUpdate = await _updateManager.CheckForUpdatesAsync();
            return _pendingUpdate;
        }
        catch (Exception)
        {
            // Log error (no internet, GitHub down, etc.)
            return null;
        }
    }

    public async Task<bool> DownloadUpdatesAsync(UpdateInfo updateInfo, IProgress<int>? progress = null)
    {
        try
        {
            await _updateManager.DownloadUpdatesAsync(updateInfo, progress);
            return true;
        }
        catch (Exception)
        {
            // Log download failure
            return false;
        }
    }

    public void ApplyUpdatesAndRestart(UpdateInfo updateInfo)
    {
        _updateManager.ApplyUpdatesAndRestart(updateInfo);
    }
}
```

### Step 3: Integrate with App Startup

```csharp
// src/SDAHymns.Desktop/Program.cs or App.axaml.cs

public override void OnFrameworkInitializationCompleted()
{
    // Velopack startup hook (handle app updates)
    VelopackApp.Build().Run();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        // Register UpdateService in DI
        var services = new ServiceCollection();
        services.AddSingleton<IUpdateService, UpdateService>();
        // ... other services

        var mainWindow = new MainWindow();
        desktop.MainWindow = mainWindow;

        // Check for updates after window loads (non-blocking)
        Task.Run(async () =>
        {
            var updateService = serviceProvider.GetService<IUpdateService>();
            var updateInfo = await updateService.CheckForUpdatesAsync();

            if (updateInfo != null)
            {
                // Dispatch to UI thread
                Dispatcher.UIThread.Post(() =>
                {
                    mainWindow.ShowUpdateNotification(updateInfo);
                });
            }
        });
    }

    base.OnFrameworkInitializationCompleted();
}
```

### Step 4: Create Update Notification UI

**Option A: In-Window Notification Bar** (Recommended for non-intrusive UX)

```xml
<!-- MainWindow.axaml -->
<Border x:Name="UpdateNotificationBanner"
        IsVisible="{Binding IsUpdateAvailable}"
        Background="#2E7D32"
        Padding="12,8">
    <StackPanel Orientation="Horizontal" Spacing="12">
        <TextBlock Text="🎉 New version available!"
                   VerticalAlignment="Center"
                   Foreground="White" />
        <TextBlock Text="{Binding LatestVersion, StringFormat='v{0}'}"
                   VerticalAlignment="Center"
                   FontWeight="Bold"
                   Foreground="White" />
        <Button Content="Update Now"
                Command="{Binding UpdateNowCommand}"
                Classes="accent" />
        <Button Content="Later"
                Command="{Binding DismissUpdateCommand}"
                Classes="subtle" />
    </StackPanel>
</Border>
```

**Option B: Dialog Window** (More prominent)

Create a separate `UpdateAvailableWindow.axaml` with changelog display.

### Step 5: Add ViewModel Logic

```csharp
// MainWindowViewModel.cs additions

[ObservableProperty]
private bool isUpdateAvailable;

[ObservableProperty]
private string? latestVersion;

[ObservableProperty]
private bool isDownloadingUpdate;

[ObservableProperty]
private int downloadProgress;

private readonly IUpdateService _updateService;
private UpdateInfo? _pendingUpdate;

[RelayCommand]
private async Task UpdateNowAsync()
{
    if (_pendingUpdate == null) return;

    IsDownloadingUpdate = true;
    DownloadProgress = 0;

    var progress = new Progress<int>(p => DownloadProgress = p);
    var success = await _updateService.DownloadUpdatesAsync(_pendingUpdate, progress);

    if (success)
    {
        // Apply and restart
        _updateService.ApplyUpdatesAndRestart(_pendingUpdate);
    }
    else
    {
        // Show error message
        IsDownloadingUpdate = false;
        // TODO: Show error dialog
    }
}

[RelayCommand]
private void DismissUpdate()
{
    IsUpdateAvailable = false;
}

public void ShowUpdateNotification(UpdateInfo updateInfo)
{
    _pendingUpdate = updateInfo;
    LatestVersion = updateInfo.TargetFullRelease.Version.ToString();
    IsUpdateAvailable = true;
}
```

### Step 6: CI/CD Integration

Update `.github/workflows/release.yml`:

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install Velopack
        run: dotnet tool install -g vpk

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet publish src/SDAHymns.Desktop -c Release -o publish/

      - name: Create Velopack Release (Windows)
        run: |
          vpk pack -u SDAHymns -v ${{ github.ref_name }} -p publish/ -e SDAHymns.Desktop.exe

      - name: Upload to GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          files: |
            Releases/*
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

**For macOS:**
```yaml
  build-macos:
    runs-on: macos-latest
    steps:
      # Similar steps but for osx-arm64 runtime
      - run: vpk pack -u SDAHymns -v ${{ github.ref_name }} -p publish/ -e SDAHymns.Desktop -o osx
```

### Step 7: First Release Setup

For the very first release, you need to create a bootstrap installer manually:

```bash
# Build the app
dotnet publish -c Release -r win-x64 --self-contained -o publish/

# Create initial installer
vpk pack -u SDAHymns -v 0.1.0 -p publish/ -e SDAHymns.Desktop.exe

# This creates:
# - Releases/SDAHymnsSetup.exe (first-time installer)
# - Releases/RELEASES (package index)
# - Releases/*.nupkg (update packages)
```

Upload all files to your first GitHub Release.

## User Experience

### First-Time Install
1. User downloads `SDAHymnsSetup.exe` from GitHub Releases
2. Runs installer → App installs to `%LocalAppData%\SDAHymns\`
3. Shortcut added to Start Menu
4. App launches automatically after install

### Subsequent Updates
1. User opens app
2. App checks for updates in background (1-2 seconds)
3. If update available: Green banner appears at top
4. User clicks "Update Now"
5. Progress bar shows download (delta updates = fast)
6. App restarts automatically
7. User is on latest version

### Edge Cases Handled
- **No Internet:** Silent failure, app continues normally
- **Download Fails:** Error message, user can retry
- **GitHub API Rate Limit:** Cached last check, retry after delay
- **Corrupted Update:** Velopack validates checksums, rejects bad packages
- **User Closes During Update:** Safe, can resume next launch

## Testing Strategy

### Manual Testing
1. **Initial Install:** Test installer on clean machine
2. **Update Flow:** Create v0.1.0, then release v0.1.1, verify update works
3. **Delta Updates:** Verify only changed files download
4. **Rollback:** Test reverting to previous version (Velopack supports this)
5. **Cross-Platform:** Test on Windows, macOS (if applicable)

### Automated Testing
```csharp
// tests/SDAHymns.Tests/Services/UpdateServiceTests.cs

[Fact]
public async Task CheckForUpdates_WhenNewerVersionAvailable_ReturnsUpdateInfo()
{
    // Mock GitHub API response
    var updateService = new UpdateService();
    var updateInfo = await updateService.CheckForUpdatesAsync();

    Assert.NotNull(updateInfo);
    Assert.True(updateInfo.TargetFullRelease.Version > currentVersion);
}

[Fact]
public async Task CheckForUpdates_WhenNoInternet_ReturnsNull()
{
    // Simulate network failure
    // ...
}
```

## Configuration

### AppSettings
```json
// Optional: appsettings.json for update behavior
{
  "Updates": {
    "CheckOnStartup": true,
    "CheckIntervalHours": 24,
    "AutoDownload": false,  // If true, download automatically
    "GitHubRepo": "YOUR-USERNAME/SDAHymns"
  }
}
```

### User Settings
Allow users to configure update behavior:
- "Check for updates automatically"
- "Download updates automatically"
- "Notify me about beta versions"

## Security Considerations

1. **HTTPS Only:** Velopack uses HTTPS for all downloads
2. **Signature Validation:** Velopack validates package signatures
3. **Checksum Verification:** Every file verified before installation
4. **No Elevation Required:** Installs to user directory (no admin needed)
5. **Rollback Support:** Can revert if update fails

## Documentation Updates

### README.md
Add installation instructions:
```markdown
## Installation

Download the latest `SDAHymnsSetup.exe` from [Releases](https://github.com/YOUR-USERNAME/SDAHymns/releases/latest) and run it. The app will automatically check for updates.
```

### User Guide (Future)
- How to check for updates manually
- How to disable auto-updates
- Troubleshooting update failures

## Acceptance Criteria

- [ ] Velopack NuGet package added to Desktop project
- [ ] `UpdateService` implemented with check, download, apply methods
- [ ] Update notification UI displays when update available
- [ ] "Update Now" button downloads and installs update
- [ ] App restarts automatically after update
- [ ] Non-blocking startup check (app usable immediately)
- [ ] Progress indicator during download
- [ ] Graceful handling of no internet/GitHub errors
- [ ] CI/CD workflow creates Velopack packages
- [ ] GitHub Release includes `.nupkg` and `RELEASES` files
- [ ] First-time installer (`Setup.exe`) works on clean machine
- [ ] Delta updates work (only changed files downloaded)
- [ ] Can manually trigger update check (optional menu item)
- [ ] Unit tests for UpdateService
- [ ] Tested on Windows (macOS if applicable)

## Future Enhancements

- **Beta Channel:** Allow users to opt into pre-release builds
- **Changelog Display:** Show release notes in update dialog
- **Background Updates:** Download in background, prompt when ready
- **Update History:** Show past update logs
- **Staged Rollout:** Release to 10% of users first, then 100%

## Notes

- Velopack uses NuGet packages internally, but they're just ZIP files
- First release must be done manually, subsequent releases automated
- Keep `RELEASES` file up-to-date (Velopack handles this automatically)
- Delta updates drastically reduce bandwidth (e.g., 2MB instead of 50MB)
- Users can always download full installer from GitHub Releases

## Related Specs

- **Previous:** 005-basic-hymn-display.md
- **Next:** TBD (CLI, remote control, or advanced features)

## Status Updates

- **2025-12-04:** Spec created, ready for implementation
- **2025-12-26:** Implemented and tested (Commit 4e3ec7d). Validated with Velopack integration, UpdateService, and unit tests.
