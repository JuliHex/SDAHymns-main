# Spec 014: Audio Download & Settings System

**Status:** ✅ Implemented (18/20 acceptance criteria complete - packaging scripts are external tooling)
**Created:** 2025-12-26
**Updated:** 2025-12-26
**Dependencies:** 011-audio-playback.md
**Related Issues:** [#18](https://github.com/ThorSPB/SDAHymns/issues/18)

## Overview

Implement a complete audio distribution and settings management system that allows users to download piano recordings from remote sources, manage their local audio library, and configure audio playback settings through a dedicated Settings UI.

**Goal:** Provide a seamless, user-friendly way to obtain and manage audio files without bloating the app download size or Git repository.

## Problem Statement

Audio files are large (typical hymn: 2-5MB MP3). Bundling all recordings with the app would:
- Bloat Git repository (hundreds of MB)
- Increase download size significantly
- Make updates slower
- Waste bandwidth for users who don't need all hymns

**Solution:** External audio library with on-demand downloads.

## Architecture

### 1. Audio Distribution Strategy

#### Option A: GitHub Releases (Primary - Recommended)
**Pros:**
- Free CDN-backed hosting
- Reliable, fast downloads worldwide
- Version controlled (v1.0.0, v1.1.0, etc.)
- Integrated with existing release workflow
- No maintenance required

**Cons:**
- 2GB file size limit (solvable with packs/splitting)
- Requires GitHub account (for uploading)

**Implementation:**
```
GitHub Release: audio-v1.0.0
├── manifest.json               # Package metadata
├── crestine-pack.zip          # 500MB - Hymns 1-200
├── companioni-pack.zip        # 300MB - All companioni hymns
├── exploratori-pack.zip       # 200MB - All exploratori hymns
├── licurici-pack.zip          # 150MB - All licurici hymns
└── tineret-pack.zip           # 250MB - All tineret hymns
```

**Manifest Format:**
```json
{
  "version": "1.0.0",
  "releaseDate": "2025-12-26",
  "categories": {
    "crestine": {
      "displayName": "Imnuri Crestine",
      "fileCount": 200,
      "totalSize": 524288000,
      "downloadUrl": "https://github.com/ThorSPB/SDAHymns/releases/download/audio-v1.0.0/crestine-pack.zip",
      "checksum": "sha256:abc123...",
      "required": false
    },
    "companioni": { ... },
    ...
  }
}
```

#### Option B: Raspberry Pi HTTP Server (Fallback/Mirror)
**Pros:**
- Full control over content
- No file size limits
- Can serve beta/test recordings
- Acts as backup mirror

**Cons:**
- Dependent on home internet/uptime
- Limited bandwidth (ISP upload speed)
- Requires DynamicDNS for stable URL

**Implementation:**
```bash
# Simple nginx setup on Raspberry Pi
sudo apt install nginx
# Files in: /var/www/html/sdahymns-audio/
# Access: http://your-pi.duckdns.org/sdahymns-audio/

# Directory structure:
/var/www/html/sdahymns-audio/
├── manifest.json
├── crestine-pack.zip
├── companioni-pack.zip
└── ...
```

**DynamicDNS Setup:**
- Use DuckDNS.org (free subdomain)
- Install DuckDNS updater on Pi
- URL example: `http://sdahymns.duckdns.org:8080/audio/`

#### Hybrid Approach (Recommended)
```csharp
public class AudioDownloadService
{
    private readonly string[] _downloadSources = new[]
    {
        "https://github.com/ThorSPB/SDAHymns/releases/download/audio-v{version}/",
        "http://sdahymns.duckdns.org:8080/audio/"  // Fallback
    };

    // Try primary source first, fallback to secondary if failed
}
```

### 2. Local Audio Library Structure

```
{UserDataPath}/SDAHymns/Audio/
├── crestine/
│   ├── 001.mp3
│   ├── 001.metadata.json  (optional - tempo, date, notes)
│   ├── 002.mp3
│   └── ...
├── companioni/
│   └── ...
├── exploratori/
├── licurici/
└── tineret/
```

**Default Paths:**
- Windows: `%APPDATA%\SDAHymns\Audio\`
- macOS: `~/Library/Application Support/SDAHymns/Audio/`
- Linux: `~/.local/share/SDAHymns/Audio/`

### 3. Settings Page Architecture

New window: `SettingsWindow.axaml` with tabs:

#### Tab 1: General Settings
- Language selection (future)
- Theme (Light/Dark - future)
- Startup options

#### Tab 2: Audio Settings
```
┌─────────────────────────────────────────────┐
│ Audio Library Settings                      │
├─────────────────────────────────────────────┤
│ Library Location:                           │
│ [C:\Users\...\SDAHymns\Audio]    [📁 Browse]│
│ [Migrate Existing Files]                    │
│                                             │
│ Installed Audio Packs:                      │
│ ┌─────────────────────────────────────────┐ │
│ │ ✅ Crestine (200 files, 500MB)         │ │
│ │    Downloaded: 2025-12-26               │ │
│ │    [Update] [Verify] [Delete]           │ │
│ │                                         │ │
│ │ ✅ Companioni (80 files, 300MB)        │ │
│ │    Downloaded: 2025-12-26               │ │
│ │    [Update] [Verify] [Delete]           │ │
│ │                                         │ │
│ │ ❌ Exploratori (Not installed)         │ │
│ │    Size: 200MB                          │ │
│ │    [Download]                           │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ [Download All Categories]                   │
│                                             │
│ Playback Settings:                          │
│ Output Device: [Default Audio Device ▼]     │
│ Auto-play Delay: [5] seconds                │
│ Default Volume: [████████░░] 80%            │
│                                             │
│ Total Space Used: 800MB / 2GB available     │
└─────────────────────────────────────────────┘
```

#### Tab 3: Display Settings
- Default display profile
- Aspect ratio preference
- Font size adjustments (future)

## Core Services

### 1. AudioDownloadService

**File:** `src/SDAHymns.Core/Services/AudioDownloadService.cs`

```csharp
public interface IAudioDownloadService
{
    // Manifest management
    Task<AudioPackageManifest> FetchManifestAsync();
    Task<List<CategoryPackage>> GetAvailablePackagesAsync();
    Task<List<CategoryPackage>> GetInstalledPackagesAsync();

    // Download operations
    Task DownloadCategoryAsync(string categorySlug, IProgress<DownloadProgress> progress);
    Task DownloadAllAsync(IProgress<DownloadProgress> progress);
    Task UpdateCategoryAsync(string categorySlug, IProgress<DownloadProgress> progress);

    // Library management
    Task<bool> VerifyCategoryAsync(string categorySlug);  // Check checksums
    Task DeleteCategoryAsync(string categorySlug);
    Task<long> GetTotalLibrarySizeAsync();
    Task MigrateLibraryAsync(string newPath, IProgress<int> progress);

    // Settings
    Task SetLibraryPathAsync(string path);
    Task<string> GetLibraryPathAsync();
}

public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete { get; set; }
    public string CurrentFile { get; set; }
    public DownloadState State { get; set; }  // Downloading, Extracting, Verifying, Complete
}

public enum DownloadState
{
    Idle,
    FetchingManifest,
    Downloading,
    Extracting,
    Verifying,
    Complete,
    Failed
}
```

### 2. AudioLibraryService

**File:** `src/SDAHymns.Core/Services/AudioLibraryService.cs`

```csharp
public interface IAudioLibraryService
{
    // File discovery
    Task<bool> AudioFileExistsAsync(int hymnNumber, string category);
    Task<string?> GetAudioFilePathAsync(int hymnNumber, string category);
    Task<List<InstalledAudioFile>> GetInstalledFilesAsync(string category);

    // Metadata
    Task<AudioMetadata?> GetMetadataAsync(int hymnNumber, string category);
    Task SaveMetadataAsync(int hymnNumber, string category, AudioMetadata metadata);

    // Validation
    Task<LibraryHealth> ScanLibraryHealthAsync();
}

public class LibraryHealth
{
    public int TotalFiles { get; set; }
    public int CorruptedFiles { get; set; }
    public int MissingMetadata { get; set; }
    public List<string> Issues { get; set; }
}
```

### 3. SettingsService

**File:** `src/SDAHymns.Core/Services/SettingsService.cs`

```csharp
public interface ISettingsService
{
    // Get/Set settings from AppSettings entity
    Task<AppSettings> GetAppSettingsAsync();
    Task UpdateAppSettingsAsync(AppSettings settings);

    // Convenience methods for common settings
    Task<string> GetAudioLibraryPathAsync();
    Task SetAudioLibraryPathAsync(string path);
    Task<int> GetAutoPlayDelayAsync();
    Task SetAutoPlayDelayAsync(int seconds);
    // ... etc
}
```

## Implementation Plan

### Step 1: Core Services (Backend)

1. **Create `AudioDownloadService`**
   - HTTP client for downloading from GitHub/Pi
   - Manifest parsing and caching
   - ZIP extraction
   - Checksum verification (SHA256)
   - Progress reporting

2. **Create `AudioLibraryService`**
   - File discovery and scanning
   - Metadata management
   - Health checks

3. **Create `SettingsService`**
   - AppSettings CRUD operations
   - Path validation and migration

### Step 2: Settings Window UI

1. **Create `SettingsWindow.axaml`**
   - Tabbed interface (General, Audio, Display)
   - Audio library location picker
   - Download manager UI
   - Progress bars and status messages

2. **Create `SettingsWindowViewModel`**
   - Observable properties for settings
   - Commands for download/migrate/verify
   - Progress tracking

### Step 3: Integration

1. **Add Settings Menu Item to MainWindow**
   - Button or menu item to open SettingsWindow
   - Keyboard shortcut (Ctrl+,)

2. **Update MainWindowViewModel**
   - Use `SettingsService` to get audio library path
   - Remove hardcoded path placeholder

3. **Update AudioPlayerService**
   - Get library path from settings dynamically

### Step 4: Manifest & Packaging

1. **Create manifest.json template**
   - Define structure for audio packages
   - Include checksums, file counts, sizes

2. **Create packaging script** (PowerShell/Bash)
   - Scan audio directory
   - Generate manifest
   - Create category ZIP files
   - Compute checksums

3. **Document release process**
   - How to create audio release
   - How to update manifest
   - How to upload to GitHub Releases

## User Workflows

### Scenario A: First-Time Setup
1. User installs app
2. User loads a hymn → "No audio file found"
3. User clicks Settings → Audio tab
4. User clicks "Download All Categories" or selects specific category
5. Progress bar shows download → extraction → verification
6. Audio is now available for playback

### Scenario B: Changing Library Location
1. User has audio in `D:\Music\SDAHymns`
2. User wants to move to `E:\Media\SDAHymns`
3. User opens Settings → Audio
4. User clicks "Browse" → selects new path
5. User clicks "Migrate Existing Files"
6. Files are moved with progress indicator
7. AppSettings updated with new path

### Scenario C: Updating Audio Pack
1. New version of Crestine pack released (v1.1.0)
2. App detects update in manifest
3. User sees "Update Available" badge in Settings
4. User clicks "Update" → downloads delta or full pack
5. Old files replaced with new versions

### Scenario D: Verifying Integrity
1. User suspects corrupted files
2. User opens Settings → Audio
3. User clicks "Verify" on a category
4. App computes checksums and compares
5. Report shows: "3 files corrupted, 197 OK"
6. User can re-download or delete corrupted category

## Technical Considerations

### Download Strategy
- Use `HttpClient` with progress reporting
- Support resume (Range headers) for large files
- Timeout and retry logic
- Cancel token support

### Security
- HTTPS for GitHub downloads
- Checksum verification (SHA256)
- Signature verification (future - GPG)

### Performance
- Extract ZIP files on background thread
- Don't block UI during downloads
- Show real-time progress

### Error Handling
- Network failures → retry 3 times
- Disk space check before download
- Corrupted download → delete and retry
- User-friendly error messages

### Platform Considerations
- Path separators (cross-platform)
- Permissions (write access to AppData)
- File locking (Windows)

## Acceptance Criteria

- [x] **Manifest:** Can fetch and parse manifest.json from GitHub/Pi
- [x] **Download:** Can download category ZIP files with progress
- [x] **Extract:** Can extract ZIP to library folder
- [x] **Verify:** Can verify checksums of downloaded files
- [x] **Settings UI:** Settings window with tabbed interface (General/Audio/Display)
- [x] **Library Path:** Can configure and change library path
- [x] **Migration:** Full migration service with progress reporting and folder picker
- [x] **Discovery:** AudioPlayerService finds files in configured path
- [x] **Health Check:** Can scan library and report issues
- [x] **Delete:** Can delete installed category packs
- [x] **Update:** Can update existing packs when new version available
- [x] **Fallback:** Falls back to Pi server if GitHub fails
- [x] **Progress:** Real-time progress reporting for downloads
- [x] **Integration:** Settings accessible from MainWindow via ⚙ Settings button
- [x] **UI:** Folder picker implementation (Avalonia StorageProvider - cross-platform)
- [x] **UI:** Migrate Library button with folder picker dialog
- [ ] **Packaging:** Manifest generation script (tooling - not app feature)
- [ ] **Packaging:** Audio pack creation workflow (documentation - not app feature)

## Future Enhancements

- **Auto-Updates:** Check for new audio packs on startup
- **Selective Download:** Download individual hymns instead of full packs
- **Peer-to-Peer:** BitTorrent for large downloads
- **Cloud Sync:** Sync library across devices (user's OneDrive/Dropbox)
- **Compression:** Better compression (Opus instead of MP3)
- **Streaming:** Stream audio without downloading (for previews)

## Testing Strategy

### Unit Tests
- AudioDownloadService: Mock HTTP client, test download logic
- AudioLibraryService: Test file discovery and scanning
- SettingsService: Test CRUD operations
- Checksum verification
- Path validation and migration

### Integration Tests
- Download from real GitHub URL (or test server)
- Extract and verify real ZIP file
- Migrate real files

### Manual Testing
- Test on Windows, macOS (Pi hosting)
- Test with slow network (throttling)
- Test with interrupted downloads
- Test disk space scenarios
