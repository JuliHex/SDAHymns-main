# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SDAHymns is a modern, cross-platform desktop application for displaying hymns in church services, built for the Romanian Seventh Day Adventist Church. This is a **complete rewrite from scratch** of a legacy application, focusing on improved UX, streaming optimization, and automation capabilities.

## Spec-Driven Development

This project follows a **specification-driven approach** to manage complexity across multiple development sessions:

- **Spec files** are located in `docs/spec/`
- Each spec defines a feature, component, or system in detail
- Specs include implementation status, acceptance criteria, and technical details
- **`docs/spec/_context.md`** tracks all specs and their current status
- **Always check `_context.md` first** to understand what's planned, in-progress, or completed
- Update spec status as work progresses
- Reference related specs for cross-cutting concerns

### Spec Workflow

1. **Planning:** Create new spec file in `docs/spec/` with detailed requirements
2. **Implementation:** Follow spec, update status to "in-progress"
3. **Completion:** Mark as "implemented", add notes about deviations
4. **Testing:** Mark as "tested" once verified
5. **Update `_context.md`:** Keep context file synchronized with spec status

## Issue Tracking & Workflow

This project uses **GitHub Issues** to track all features, bugs, and tasks. Each spec should have a corresponding GitHub issue for visibility and tracking.

### Issue-Driven Development Flow

```
Spec File -> GitHub Issue -> Branch -> PR -> Merge (closes issue)
```

**Step-by-Step:**

1. **Create Issue** (via gh CLI or web):
   ```bash
   gh issue create --title "[Spec 010] Auto-Updates with Velopack" \
     --body "See docs/spec/010-auto-updates.md for details" \
     --label "enhancement"
   ```

2. **Create Branch** (named after issue):
   ```bash
   git checkout -b feature/auto-updates-#6
   # or
   git checkout -b fix/verse-display-#15
   ```

3. **Work on Implementation**:
   - Follow the spec
   - Commit regularly with clear messages
   - Reference issue in commits: `git commit -m "feat: add update check service (#6)"`

4. **Create Pull Request** (closes issue automatically):
   ```bash
   gh pr create --title "feat: implement auto-updates with Velopack" \
     --body "Closes #6"
   ```

5. **Merge PR** -> Issue automatically closes

### Closing Multiple Issues in One PR

Use `Closes #10, #11, #12` in the PR description body.

**Supported Keywords:**
- `Closes #123` or `Fixes #123` (case-insensitive)
- Can use on separate lines or comma-separated
- Works in PR description or commit messages

### Best Practices

1. **One Issue Per Spec** - Each spec should have exactly one tracking issue
2. **Reference Issues in Commits** - Use `#123` in commit messages for linkage
3. **Close Issues via PRs** - Always use "Closes #X" in PR descriptions
4. **Update Spec Status** - When closing issue, also update spec status in `_context.md`
5. **Keep Issues Focused** - Large features should be broken into multiple issues
6. **Use Draft PRs** - Mark PRs as draft while work is in progress

## Technology Stack

- **Language:** C# (.NET 10)
- **UI Framework:** Avalonia UI 11.3.x (cross-platform XAML-based)
- **MVVM:** CommunityToolkit.Mvvm 8.4.0 (`[ObservableProperty]`, `[RelayCommand]`, etc.)
- **Database:** SQLite with Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Audio:** NAudio 2.2.1 (piano recording playback)
- **PowerPoint Parsing:** DocumentFormat.OpenXml 3.3.0 (verse extraction from .pptx)
- **CLI:** CommandLineParser 2.9.1
- **Updates:** Velopack 0.0.1298 (auto-updates via GitHub Releases)
- **DI:** Microsoft.Extensions.DependencyInjection
- **Testing:** xUnit 2.5.3, FluentAssertions 8.8.0, Moq 4.20.72, EF Core InMemory provider
- **Code Quality:** .editorconfig, .NET Analyzers (Directory.Build.props), pre-commit hooks
- **CI/CD:** GitHub Actions (`ci.yml`, `release.yml`)
- **License:** AGPL-3.0

## Target Platforms

- Windows 10/11 (primary)
- Windows 7/8 (if feasible without extra effort)
- macOS ARM (M1/M2/M3)
- macOS Intel (support comes automatically with Avalonia)

## Architecture

### Solution Structure
```
SDAHymns/
├── SDAHymns.sln                  # Solution file (4 projects)
├── Directory.Build.props         # Shared build settings (version, analyzers, UTF-8)
├── .editorconfig                 # Code style rules
├── Makefile                      # Primary build/dev commands
├── setup.sh / setup.ps1          # First-time setup scripts
├── SETUP.md                      # Setup instructions
├── Resources/
│   └── hymns.db                  # SQLite database (1,254 hymns, 4,629 verses)
├── scripts/
│   ├── pre-commit                # Git pre-commit hook (format check + build)
│   ├── install-hooks.sh          # Hook installer (Linux/macOS)
│   └── install-hooks.ps1         # Hook installer (Windows)
├── .github/workflows/
│   ├── ci.yml                    # CI pipeline
│   └── release.yml               # Release pipeline
├── docs/
│   ├── RELEASING.md              # Release process documentation
│   ├── CI-CD.md                  # CI/CD pipeline documentation
│   ├── INSTALLATION.md           # Installation guide
│   └── spec/                     # Feature specifications
│       ├── _context.md           # Spec status tracker (check first!)
│       ├── 001-project-structure.md
│       ├── 002-data-layer.md
│       ├── ... (specs 003-019)
│       └── 019-compact-remote-widget.md
├── src/
│   ├── SDAHymns.Core/            # Shared business logic library
│   ├── SDAHymns.Desktop/         # Avalonia GUI application
│   └── SDAHymns.CLI/             # Command-line interface
└── tests/
    └── SDAHymns.Tests/           # Unit and integration tests (xUnit)
```

### SDAHymns.Core (Class Library)
Shared business logic, data layer, and services. No UI dependencies.

```
SDAHymns.Core/
├── Data/
│   ├── HymnsContext.cs                 # EF Core DbContext
│   ├── DesignTimeDbContextFactory.cs   # For EF migrations
│   └── Models/
│       ├── Hymn.cs                     # Hymn entity (title, number, category, favorites, access tracking)
│       ├── Verse.cs                    # Verse/slide entity (text, order, IsContinuation, IsInline)
│       ├── HymnCategory.cs            # Category entity (crestine, companioni, etc.)
│       ├── AudioRecording.cs           # Piano recording metadata
│       ├── DisplayProfile.cs           # Display configuration (30+ properties)
│       ├── ServicePlan.cs              # Service planning
│       ├── ServicePlanItem.cs          # Individual items in a service plan
│       ├── UsageStatistic.cs           # Hymn usage tracking
│       ├── AppSetting.cs               # Key-value app settings entity
│       └── AppSettings.cs              # Strongly-typed settings model
├── Models/
│   ├── AudioPackageManifest.cs         # Audio download manifest model
│   └── RemoteWidgetSettings.cs         # Remote widget position/state
├── Services/
│   ├── HymnDisplayService.cs           # Hymn data operations (load, navigate)
│   ├── SearchService.cs                # Smart search (diacritic/case-insensitive)
│   ├── DisplayProfileService.cs        # Profile CRUD, import/export JSON
│   ├── HotKeyManager.cs               # Keyboard shortcuts (string-based, UI-agnostic)
│   ├── SettingsService.cs              # App configuration persistence
│   ├── AudioPlayerService.cs           # NAudio playback management
│   ├── AudioLibraryService.cs          # Audio file library management
│   ├── AudioDownloadService.cs         # HTTP download with SHA256 verification
│   ├── HymnSynchronizer.cs            # Audio-verse sync engine
│   ├── TimingRecorder.cs              # Record verse timing maps
│   ├── AutoPlayCountdown.cs           # Countdown before auto-advancing verses
│   ├── UpdateService.cs               # Velopack auto-update logic
│   ├── LegacyXmlImportService.cs      # Parse legacy XML hymn indexes
│   ├── VerseImportService.cs          # Import verses from PowerPoint files
│   ├── PowerPointParserService.cs     # Parse .ppt/.pptx via OpenXml + LibreOffice
│   ├── ImportResult.cs                # Import operation result model
│   ├── I*.cs                          # Interfaces for all services above
│   └── UpdateOptions.cs               # Update configuration options
└── Migrations/                         # EF Core migration files
```

### SDAHymns.Desktop (Avalonia WinExe)
The GUI application. References SDAHymns.Core.

```
SDAHymns.Desktop/
├── Program.cs                          # Entry point, Velopack init
├── App.axaml / App.axaml.cs            # App startup, DI configuration
├── ViewLocator.cs                      # Avalonia view resolution
├── Assets/
│   └── avalonia-logo.ico               # App icon
├── ViewModels/
│   ├── ViewModelBase.cs                # Base class (ReactiveObject)
│   ├── MainWindowViewModel.cs          # Main control window logic
│   ├── ProfileEditorViewModel.cs       # Display profile editor logic
│   ├── SettingsWindowViewModel.cs      # Settings dialog logic
│   ├── ShortcutsWindowViewModel.cs     # Keyboard shortcuts display/edit
│   ├── RecorderModeViewModel.cs        # Audio timing recorder
│   └── RemoteWidgetViewModel.cs        # Compact remote widget logic
└── Views/
    ├── MainWindow.axaml(.cs)           # Main control interface (advanced mode)
    ├── DisplayWindow.axaml(.cs)        # Full-screen projection display
    ├── ProfileEditorWindow.axaml(.cs)  # Display profile editor
    ├── SettingsWindow.axaml(.cs)       # Settings dialog (General/Audio/Display tabs)
    ├── ShortcutsWindow.axaml(.cs)      # F1 keyboard shortcuts overlay + editor
    ├── RecorderModeWindow.axaml(.cs)   # Verse timing recorder UI
    └── RemoteWidget.axaml(.cs)         # Compact remote widget (default GUI)
```

### SDAHymns.CLI (Console App)
Command-line interface for data import and testing. References SDAHymns.Core.

```
SDAHymns.CLI/
├── Program.cs                          # CLI entry point (CommandLineParser verbs)
└── Commands/
    ├── ImportCommand.cs                # Import hymns from legacy XML
    ├── ImportVersesCommand.cs          # Import verses from PowerPoint files
    ├── ImportOrphanPptCommand.cs       # Import orphan PPT files (no XML entry)
    ├── TestPptCommand.cs               # Test PowerPoint parsing
    └── TestVerseExtractionCommand.cs   # Test verse extraction logic
```

### SDAHymns.Tests (xUnit Test Project)
References both Core and Desktop projects.

```
SDAHymns.Tests/
├── UnitTest1.cs
├── ViewModels/
│   └── MainWindowViewModelTests.cs
└── Services/
    ├── SearchServiceTests.cs
    ├── DisplayProfileServiceTests.cs
    ├── HotKeyManagerTests.cs
    ├── HymnDisplayServiceTests.cs
    ├── AutoPlayCountdownTests.cs
    ├── TimingRecorderTests.cs
    └── UpdateServiceTests.cs
```

### Dual-Window System

**RemoteWidget (Default GUI):**
- Compact, widget-style window (launches by default)
- Custom title bar with menu, minimize, close
- Hymn number input with optional number pad
- SHOW/BLANK buttons, verse navigation
- Lockable position, always-on-top toggle
- PowerPoint-style keyboard controls (Space/arrows for navigation, Esc/B to blank)
- Color scheme: #1E1E2E background, #6366F1 accent, #2A2A3C surface

**MainWindow (Advanced Mode):**
- Full control interface (launch with `--advanced` flag)
- Search and browse hymns (real-time, diacritic-insensitive)
- Recent hymns bar, favorites
- Audio controls (play/pause, stop, seek, volume, record, auto-advance)
- Display profile selector with editor
- Verse preview with auto-scaling

**DisplayWindow (Projection):**
- Full-screen on projector/second monitor
- Dynamic profile application (fonts, colors, backgrounds, effects)
- Auto-scaling content
- Countdown overlay for auto-advance
- Transparent background support for OBS

### Data Flow

1. **Import (first run):** Parse legacy XML files + PowerPoint files -> Populate SQLite database
2. **Search:** User searches hymn -> Query SQLite (SearchService) -> Display results
3. **Select:** User selects hymn -> Load verses from DB (HymnDisplayService) -> Send to display window
4. **Display:** Display window renders verses with active DisplayProfile styling
5. **Navigate:** Hotkeys/buttons -> Update current verse -> Refresh display
6. **Audio:** Play audio -> Load MP3 via AudioPlayerService (NAudio) -> Optional auto-advance with HymnSynchronizer

## Development Commands

### Using the Makefile (recommended)

```bash
make help            # Show all available commands

# Setup (first time)
make setup           # Restore + build + install git hooks

# Development
make restore         # Restore NuGet packages
make build           # Build solution (Debug)
make build-release   # Build solution (Release)
make test            # Run all tests
make test-verbose    # Run tests with detailed output
make run             # Run desktop app (dotnet run --project src/SDAHymns.Desktop)
make run-cli         # Run CLI app
make watch           # Watch mode (auto-rebuild on changes)

# Code Quality
make format          # Auto-format all code (dotnet format)
make format-check    # Check formatting without modifying
make lint            # Run code analysis (via build)

# Database
make db-update       # Apply EF Core migrations
make db-migration NAME=YourName  # Create new migration

# Publishing
make publish-win     # Publish self-contained Windows x64
make publish-mac     # Publish self-contained macOS ARM64
make publish-linux   # Publish self-contained Linux x64

# Release Management
make version-bump VERSION=1.0.0  # Bump version in Directory.Build.props and commit
make release VERSION=1.0.0       # Create git tag and push (triggers GitHub Actions)

# Cleanup
make clean           # Remove build artifacts
make clean-all       # Deep clean (removes bin, obj directories)
```

### Direct dotnet Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/SDAHymns.Desktop
dotnet run --project src/SDAHymns.CLI -- <verb> [options]
dotnet ef migrations add MigrationName --project src/SDAHymns.Core
dotnet ef database update --project src/SDAHymns.Core
dotnet publish src/SDAHymns.Desktop -c Release -r win-x64 --self-contained
```

### CLI Verbs (Data Import Tools)

```bash
# Import hymns from legacy XML index files
dotnet run --project src/SDAHymns.CLI -- import --path <xml-folder>

# Import verses from PowerPoint files
dotnet run --project src/SDAHymns.CLI -- import-verses --category <name> [--start-from <id>] [--limit <n>]

# Import orphan PPT files (not in XML index)
dotnet run --project src/SDAHymns.CLI -- import-orphan-ppt --path <folder>

# Test PowerPoint parsing
dotnet run --project src/SDAHymns.CLI -- test-ppt --path <file.pptx>

# Test verse extraction
dotnet run --project src/SDAHymns.CLI -- test-verse-extraction --path <file.pptx>
```

## Core Features (Implementation Status)

### Implemented
- **Hymn Display & Projection:** Dual-window system with RemoteWidget (default) and MainWindow (advanced)
- **Enhanced Search:** Real-time, diacritic/case/punctuation-insensitive, category filtering, recent hymns, favorites
- **Display Profiles:** 6 preset profiles, full editor UI, 30+ properties (fonts, colors, backgrounds, effects, margins), JSON import/export
- **Keyboard Shortcuts:** 20+ default shortcuts, F1 overlay, inline editing, conflict detection, JSON persistence
- **Audio Playback:** NAudio player, verse synchronization, timing recorder, auto-advance countdown, device selector
- **Audio Download & Settings:** Settings UI with tabs, download manager with SHA256 verification, library migration
- **Auto-Updates:** Velopack integration with GitHub Releases, delta updates
- **Compact Remote Widget:** Widget-style default GUI, custom chrome, lockable, always-on-top, number pad, PowerPoint keyboard controls

### Planned (Not Yet Implemented)
- **Service Planner:** Pre-plan hymn order for services (Spec 009)
- **Statistics Tracking:** Usage analytics and dashboard (Spec 012)
- **Export Functionality:** PDF/image generation (Spec 013)
- **CLI Interface (IPC):** Control running GUI via CLI, headless mode (Spec 014/015)
- **OBS Integration:** Window capture optimization, browser source (Spec 016)
- **UI/UX Overhaul:** Modern design system, animations, themes (Spec 017)
- **Enhanced Slide Formatting:** Verse numbers, chorus styling, transitions (Spec 018)
- **Remote Control API:** Embedded web server for mobile control (Spec 020)

## Key Design Decisions

### Why Parse PowerPoint Files?
- **Extract text** from .PPT files into SQLite verses (via DocumentFormat.OpenXml + LibreOffice PPT->PPTX conversion)
- **Eliminates Microsoft Office dependency** at runtime
- **Full styling control** for OBS/streaming optimization
- **Faster performance** (no PowerPoint overhead)

### Display Profile System
Each profile stores 30+ properties: typography (font family, sizes, weight, line height, letter spacing), colors (background, text, title, label, accent as hex), background images (path, mode, opacity), layout (text/vertical alignment, margins), and effects (text shadow, text outline). Profiles can be exported/imported as JSON.

### Offline-First Design
- **All data bundled:** Hymn database shipped with app
- **No external dependencies at runtime:** No API calls, no internet required
- **Updates only:** Only feature that needs internet is auto-update checking

### App Launch Modes
- **Default:** RemoteWidget (compact, widget-style) - best for church operators
- **`--advanced` flag:** MainWindow (full control interface) - best for setup/configuration

### Keyboard Shortcuts (Productivity Focus)
All shortcuts stored in Core layer as strings (UI-agnostic). Default shortcuts include:
- `Ctrl+F`: Focus search
- `Up/Down`: Navigate hymn list
- `Enter`: Display selected hymn
- `Left/Right`: Previous/next verse
- `Space`: Play/pause audio
- `F1`: Show shortcuts overlay
- `F11`: Toggle fullscreen display
- `Esc`: Close display / blank
- `Ctrl+1-5`: Load recent hymns

RemoteWidget uses PowerPoint-style controls: Space/Right/Down/PgDn/Enter for next, Left/Up/PgUp/Backspace for previous, Esc/B to blank.

## Romanian Hymn Categories

1. **Imnuri crestine** - Main Christian hymnal (919 hymns, largest collection)
2. **Imnuri companioni** - Pathfinder/Companion hymns (63 hymns)
3. **Imnuri exploratori** - Explorer hymns (120 hymns)
4. **Imnuri licurici** - Firefly hymns/children's songs (83 hymns)
5. **Imnuri tineret** - Youth hymns (69 hymns)

**Total:** 1,254 hymns, 4,629 verses imported (99.6% success rate)

## Important Notes

- **Romanian text:** Ensure UTF-8 encoding for proper diacritics (a, a, i, s, t)
- **Multi-monitor:** Test display window on various screen configurations
- **Performance:** Keep display window rendering smooth (60 FPS target)
- **Accessibility:** Maintain keyboard navigation for all features
- **Testing:** Test with real projectors and OBS before releases
- **Pre-commit hooks:** Format check + build must pass before committing (install with `make install-hooks`)
- **Version:** Currently 0.1.0 (set in Directory.Build.props)
