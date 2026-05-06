# Spec Context

This file tracks all specifications and their implementation status.

**Always check this file first** when working on the project to understand current state.

## Status Definitions

- **📋 Planned** - Spec written, not started
- **🚧 In Progress** - Currently being implemented
- **✅ Implemented** - Code complete, not tested
- **✓ Tested** - Implemented and verified working
- **⏸️ Blocked** - Cannot proceed (dependencies or decisions needed)
- **❌ Deprecated** - No longer relevant

## Specifications

### Infrastructure

| ID | Spec | Status | Issue | Notes |
|----|------|--------|-------|-------|
| 001 | [Project Structure](001-project-structure.md) | ✅ Implemented | - | Solution with 4 projects, all dependencies configured |

### Core Features

| ID | Spec | Status | Issue | Notes |
|----|------|--------|-------|-------|
| 002 | [Data Layer & EF Core](002-data-layer.md) | ✅ Implemented | - | All 9 entities, DbContext, migration, 156KB database with seed data |
| 003 | [Legacy XML Import](003-legacy-xml-import.md) | ✅ Implemented | - | 1,254 hymns total (1,070 from XML + 184 from orphan PPT files) |
| 004 | [PowerPoint Verse Extraction](004-powerpoint-verse-extraction.md) | ✅ Implemented | - | 1,249/1,254 hymns (99.6%) - 4,629 verses imported successfully |
| 005 | [Basic Hymn Display](005-basic-hymn-display.md) | ✅ Implemented | - | Dual-window system (control + display), verse navigation, dark theme, auto-scaling |
| 006 | [Enhanced Control Window](006-enhanced-control-window.md) | ✅ Implemented | [#2](https://github.com/ThorSPB/SDAHymns/issues/2) | Full-text search, browse, recent/favorites - real-time search, recent hymns bar |
| 007 | [Display Profiles](007-display-profiles.md) | ✅ Implemented | [#3](https://github.com/ThorSPB/SDAHymns/issues/3) | 6 preset profiles, full editor UI, background images, 19 tests passing |
| 008 | [Keyboard Shortcuts](008-keyboard-shortcuts.md) | ✅ Implemented | [#4](https://github.com/ThorSPB/SDAHymns/issues/4) | Global hotkeys, F1 shortcuts overlay, tooltips, 24 tests passing |
| 009 | [Service Planner](009-service-planner.md) | 📋 Planned | [#5](https://github.com/ThorSPB/SDAHymns/issues/5) | Pre-plan services, live mode, templates, PDF export |
| 010 | [Auto-Updates with Velopack](010-auto-updates.md) | ✓ Tested | [#6](https://github.com/ThorSPB/SDAHymns/issues/6) | Seamless updates via GitHub Releases, delta updates, cross-platform |
| 011 | [Audio Playback](011-audio-playback.md) | ✅ Implemented | [#12](https://github.com/ThorSPB/SDAHymns/issues/12) | Complete: Player, sync, recorder mode UI, countdown overlay, device selector (19/19 criteria) |
| 012 | [Statistics Tracking](012-statistics-tracking.md) | 📋 Planned | [#13](https://github.com/ThorSPB/SDAHymns/issues/13) | Usage analytics, dashboard, reporting, "forgotten hymns" discovery |
| 013 | [Export Functionality](013-export-functionality.md) | 📋 Planned | [#14](https://github.com/ThorSPB/SDAHymns/issues/14) | PDF generation (QuestPDF), Image rendering (WYSIWYG), batch export |
| 014 | [Audio Download & Settings](014-audio-download-settings.md) | ✅ Implemented | [#18](https://github.com/ThorSPB/SDAHymns/issues/18) | Settings UI, download manager, library management (manifest generation pending) |
| ~~014~~ 015 | [CLI Interface](014-cli-interface.md) | 📋 Planned | [#15](https://github.com/ThorSPB/SDAHymns/issues/15) | Dual-mode (Direct/Control), IPC via Named Pipes, Headless display mode |
| 016 | [OBS Integration](016-obs-integration.md) | 📋 Planned | [#17](https://github.com/ThorSPB/SDAHymns/issues/17) | Window capture optimization, Browser Source, OBS WebSocket automation |

### UI/UX & Polish

| ID | Spec | Status | Issue | Notes |
|----|------|--------|-------|-------|
| 017 | [UI/UX Overhaul](017-ui-ux-overhaul.md) | 📋 Planned | [#20](https://github.com/ThorSPB/SDAHymns/issues/20) | Modern design system, animations, themes, icon integration, card layouts - INCLUDES polishing RemoteWidget & DisplayWindow |
| 018 | [Enhanced Slide Formatting](018-enhanced-slide-formatting.md) | 📋 Planned | [#21](https://github.com/ThorSPB/SDAHymns/issues/21) | Verse number styling, chorus differentiation, smart layout, transitions |
| 019 | [Compact Remote Widget](019-compact-remote-widget.md) | ✅ Implemented | [#22](https://github.com/ThorSPB/SDAHymns/issues/22) | Widget-style default GUI, custom chrome, lockable position, borderless display, PowerPoint-style keyboard controls |
| ~~015~~ 020 | [Remote Control API](015-remote-control-api.md) | 📋 Planned | [#16](https://github.com/ThorSPB/SDAHymns/issues/16) | Embedded Kestrel server, SignalR sync, mobile-first Vue.js web app |

## Implementation Order

### Phase 1: Foundation ✅ COMPLETED
1. ✅ Project structure setup
2. ✅ Data layer and models
3. ✅ Legacy XML import functionality (1,254 hymns)
   - ✅ XML parsing (1,070 hymns)
   - ✅ Orphan PPT import with title extraction (184 hymns)
4. ✅ PowerPoint verse extraction (4,629 verses from 1,249 hymns)
   - ✅ Verse extraction logic with automatic sequential numbering
   - ✅ Database import service with resumable imports
   - ✅ Full import complete: 99.6% success rate
5. ✅ Basic hymn display with dual-window system
   - ✅ Control window (search by number, navigate, preview)
   - ✅ Display window (clean projection, auto-scaling)
   - ✅ Dark theme, left-aligned text, aspect ratio toggle
   - ✅ Shared ViewModel, instant sync between windows

### Phase 2: Core Features (READY FOR IMPLEMENTATION)
1. **006: Enhanced Control Window** - Full-text search, browse, recent/favorites
2. **007: Display Profiles** - Customizable appearance (fonts, colors, backgrounds)
3. **008: Keyboard Shortcuts** - Global hotkeys, customizable keybindings
4. **009: Service Planner** - Pre-plan services, live mode, templates

### Phase 3: Enhanced Features
1. **011: Audio Playback** - ✅ COMPLETE (19/19 criteria - All features including recorder mode UI, countdown overlay, device selector)
2. **012: Statistics Tracking** - 📋 Planned (Usage analytics, dashboard, reporting)
3. **013: Export Functionality** - 📋 Planned (PDF/Image export, batch processing)
4. **014: Audio Download & Settings** - ✅ COMPLETE (18/20 app features - Settings UI, download manager, folder picker, library management)

### Phase 4: Advanced Features
1. **010: Auto-Updates** - ✅ COMPLETED (Early implementation for dogfooding)
2. **014: CLI Interface** - 📋 Planned (IPC control, headless mode, automation)
3. **016: OBS Integration** - 📋 Planned (Window capture, browser source, automation)

### Phase 5: UI/UX Refinement & Polish
1. **017: UI/UX Overhaul** - 📋 Planned (Modern design, animations, themes, icon system, polish RemoteWidget/DisplayWindow)
2. **018: Enhanced Slide Formatting** - 📋 Planned (Verse numbers, chorus styling, transitions, multi-column)
3. **019: Compact Remote Widget** - ✅ IMPLEMENTED (Widget-style default GUI, custom chrome, lockable, borderless display, keyboard controls)
4. **015: Remote Control API** - 📋 Planned (Web server, SignalR, mobile web app)

## Current Session Focus

**Date:** 2025-12-04
**Goal:** 🚧 PHASE 2 IN PROGRESS - Core Features (Spec 006 Complete)
**Session 1 (2025-12-03 - Completed):**
- ✅ Upgraded to .NET 10 LTS
- ✅ Spec 001: Project Structure
- ✅ Spec 002: Data Layer & EF Core
- ✅ Spec 003: Legacy XML Import (1,070 hymns from XML)
- ✅ Orphan PPT Import: LibreOffice integration + PowerPoint parser (184 hymns)
  - Built `PowerPointParserService` with PPT→PPTX conversion
  - Created `import-orphan-ppt` CLI command
  - All 1,254 hymns now in database with proper titles

**Session 2 (Completed - Spec 004):**
- ✅ Extended `PowerPointParserService` with enhanced verse extraction
  - `ExtractVersesAsync()` method with multi-segment slide parsing
  - Handles 4+ different verse/chorus patterns
  - Automatic sequential verse numbering for unnumbered verses
  - Y-position-based text extraction for correct reading order
  - 45-second timeout to prevent LibreOffice hangs
  - Enhanced chorus deduplication with content comparison
- ✅ Created `IVerseImportService` and `VerseImportService`
  - Per-hymn transaction isolation
  - Resumable imports with `--start-from` parameter
  - Automatic skip of already-imported hymns
- ✅ Created `import-verses` CLI command
  - Full support for category, hymn ID, limit, and start-from options
  - Progress reporting every 10 hymns
  - Statistics command
- ✅ Full import completed: **1,249/1,254 hymns (99.6%) - 4,629 verses**
  - Crestine: 919/919 (100%) - 3,468 verses
  - Exploratori: 120/120 (100%) - 390 verses
  - Licurici: 83/83 (100%) - 315 verses
  - Tineret: 65/69 (94.2%) - 250 verses
  - Companioni: 62/63 (98.4%) - 206 verses

**Session 3 (2025-12-04 - Completed - Spec 005):**
- ✅ Created `HymnDisplayService` for database queries
- ✅ Implemented `MainWindowViewModel` with MVVM pattern (CommunityToolkit.Mvvm)
- ✅ Built control window (MainWindow) with:
  - Hymn number input with Enter key support
  - Category dropdown selection
  - Previous/Next navigation with auto-enable/disable
  - 4:3/16:9 aspect ratio toggle
  - Verse preview with auto-scaling
  - Status bar and verse indicator
- ✅ Built display window (DisplayWindow) with:
  - Clean projection interface (no controls)
  - Black background, white text, left-aligned
  - Auto-scaling content (no scrollbars)
  - Instant sync with control window
- ✅ Fixed multiple issues:
  - Database path (copy to output directory)
  - ComboBox binding (x:String vs ComboBoxItem)
  - Command notifications (NotifyCanExecuteChangedFor)
  - Text alignment and scaling
- ✅ Added 7 integration tests (all passing)
- ✅ Dark theme throughout application
- ✅ Hymn title includes number prefix

**🎉 PHASE 1 FOUNDATION: COMPLETE**

All Phase 1 goals achieved:
- ✅ 1,254 hymns imported from legacy XML/PPT
- ✅ 4,629 verses extracted and stored
- ✅ Database layer functional (EF Core + SQLite)
- ✅ Basic display system validates data model end-to-end
- ✅ Dual-window system ready for projection
- ✅ Romanian text rendering correctly

**Session 4 (2025-12-04 - Completed - Spec 006):**

**Initial Implementation (PR #9, commit 260fd76):**
- ✅ Added LastAccessedAt, AccessCount, IsFavorite properties to Hymn model
- ✅ Created and applied AddHymnUsageTracking migration
- ✅ Implemented SearchService with smart search algorithm
  - Case-insensitive, diacritic-insensitive, punctuation-insensitive search
  - Real-time search by hymn number or title
  - Category filtering
  - Recent hymns tracking (top 5)
  - Favorite hymns toggle
- ✅ Enhanced MainWindowViewModel with search functionality
  - Auto-search on query change
  - Selected result auto-loads hymn
  - Recent hymns quick access
- ✅ Redesigned MainWindow.axaml with new layout
  - Search box with watermark
  - Recent hymns bar with quick-access buttons
  - Search results list (max height 200px)
  - Selected hymn info panel
  - Favorite star button
- ✅ Display window aspect ratio support (4:3/16:9)
- ✅ 16 comprehensive SearchService tests (all passing)
- ✅ Application builds and runs successfully

**Code Review Refactorings (commit fd56ce8):**
- ✅ Extracted `LoadAndDisplayHymnAsync()` helper method
  - Eliminated code duplication between load methods
  - Reduced 67 lines to 35 lines
- ✅ Changed `SearchResults` to `ObservableCollection<HymnSearchResult>`
- ✅ Made `HymnSearchResult` observable with `[ObservableProperty]`
- ✅ Optimized favorite toggle (no more full refresh)
- ✅ All 44 tests passing

**Technical Decisions:**
- ⏭️ Skipped database-side filtering optimization (not needed at 1,254 hymns)
- ⏭️ Skipped ViewModel tests (service layer well-tested)

**Session 5 (2025-12-26 - Completed - Spec 008):**
- ✅ Created `HotKeyManager` service with string-based key handling (UI-agnostic Core layer)
- ✅ Implemented `KeyGesture` record with parsing and formatting
- ✅ Registered 20+ default keyboard shortcuts across 4 categories (Global, Navigation, Search, Display)
- ✅ Integrated keyboard handlers into MainWindow with Avalonia→string conversion
- ✅ Created `ShortcutsWindow` with F1 shortcut to display all available shortcuts
- ✅ Added keyboard shortcut tooltips to all buttons for discoverability
- ✅ Added "Press F1 for keyboard shortcuts" hint in status bar
- ✅ Implemented JSON persistence for custom keyboard shortcuts (save/load)
- ✅ Added conflict detection for duplicate key bindings
- ✅ Wrote 24 comprehensive unit tests for HotKeyManager (all passing)
- ✅ Context-aware shortcuts (arrow keys navigate search vs verses based on focus)
- ✅ Support for Ctrl+1-5 to load recent hymns
- ✅ **Enhanced ShortcutsWindow with inline editing UI:**
  - Toggle between view/edit mode
  - Live key capture in TextBoxes (press keys to assign)
  - Real-time conflict detection with red warnings
  - Save/Cancel/Reset to Defaults buttons
  - All shortcuts editable without touching JSON
- ✅ All 68 tests passing (24 new + 44 existing)

**Technical Decisions:**
- Used string-based keys in Core to avoid Avalonia dependency (UI-agnostic design)
- Inline editing in F1 window instead of separate dialog (better UX)
- Shortcuts are case-insensitive for better UX
- Real-time validation prevents saving conflicting shortcuts

**Session 6 (2025-12-26 - Completed - Spec 007):**
- ✅ Created comprehensive `DisplayProfile` model with 30+ customization properties
  - Typography: Font family, title/verse/label sizes, weight, line height, letter spacing
  - Colors: Background, text, title, label, accent (all hex codes)
  - Background: Opacity, image path/mode/opacity support
  - Layout: Text/vertical alignment, individual margins (L/R/T/B)
  - Effects: Text shadow (color, blur, offset), text outline (color, thickness)
  - Advanced: Transparent background, show verse numbers/title
- ✅ Database migration with 6 preset profiles seeded:
  - Classic Dark (default) - Black bg, white text, left-aligned
  - High Contrast - Bold text with shadow for maximum visibility
  - OBS Stream - Transparent bg with outline for streaming
  - Bright Room - Navy bg with yellow text for bright environments
  - Minimalist - Clean and simple
  - Traditional - Classic church aesthetic (navy & gold)
- ✅ Implemented `DisplayProfileService` with full CRUD operations
  - GetAll, GetById, GetActive, Create, Update, Delete
  - SetActive profile with AppSettings persistence
  - Duplicate profile with new name
  - Export/Import profiles as JSON
- ✅ Created `ProfileEditorViewModel` with MVVM pattern
  - Profile list management (New/Duplicate/Delete)
  - All profile properties with data binding
  - Status messages for user feedback
- ✅ Built comprehensive `ProfileEditorWindow` UI in Avalonia
  - Left sidebar: Profile list with descriptions
  - Right panel: Scrollable editor with 7 sections
  - Bottom bar: Export/Import/Reset/Preview/Save buttons
  - Full data binding to all 30+ profile properties
- ✅ Integrated profile selector into MainWindow
  - Dropdown combobox in toolbar for instant switching
  - ⚙ Edit button to open full ProfileEditorWindow
  - Real-time profile application to DisplayWindow
- ✅ Enhanced DisplayWindow with dynamic profile application
  - Typography: Font family, sizes, weight, line height, spacing
  - Colors: Background, text, title, label with opacity
  - Background images: Load from path with stretch modes
  - Layout: Margins, text/vertical alignment
  - Effects: Shadow and outline rendering
  - **Fixed OBS transparent background bug** (was making everything black)
- ✅ Wrote 19 comprehensive unit tests for DisplayProfileService
  - CRUD operations, validation, error handling
  - Active profile switching, duplicate, import/export
  - All tests passing
- ✅ Updated MainWindowViewModel tests for new profile service dependency
- ✅ All 86 tests passing (68 existing + 18 new)

**Technical Decisions:**
- Used Avalonia-compatible syntax (TextBlock labels instead of Header property)
- Background images with opacity and stretch modes (Fill/Fit/Stretch/Tile)
- System profiles cannot be deleted (IsSystemProfile flag)
- Profile changes apply instantly without restart
- JSON export for easy profile sharing

**Session 7 (2025-12-26 - Completed - Spec 011 & 014 - FULLY COMPLETE):**
- ✅ **Spec 011: Audio Playback** - 100% COMPLETE (19/19 criteria)
  - Database: Added `TimingMapJson`, `VolumeOffset`, `AppSettings` entity
  - Services: `AudioPlayerService` (NAudio), `HymnSynchronizer`, `TimingRecorder`, `AutoPlayCountdown`
  - UI: Audio controls bar in MainWindow (Play/Pause, Stop, Seek, Volume, Auto-advance toggle, Record button)
  - **NEW:** Recorder Mode window - Full UI for recording verse timings with spacebar tap
  - **NEW:** Auto-play countdown overlay on DisplayWindow with cancel button
  - **NEW:** Audio output device selector in Settings window
  - Auto-advance synchronization working
  - Save timings to database via MainWindowViewModel
  - 37 new tests added (22 TimingRecorder + 15 AutoPlayCountdown)
- ✅ **Spec 014: Audio Download & Settings** - 100% APP FEATURES COMPLETE (18/20 criteria)
  - Models: `AudioPackageManifest` with 7 supporting classes
  - Services: `SettingsService`, `AudioLibraryService`, `AudioDownloadService`
  - Features: HTTP downloads, SHA256 verification, ZIP extraction, fallback sources, library migration
  - UI: Complete Settings window with tabbed interface (General/Audio/Display)
  - Audio tab: Library path, installed packages list, download manager, playback settings, device selector
  - **NEW:** Cross-platform folder picker (Avalonia StorageProvider)
  - **NEW:** Migrate Library button with folder selection dialog
  - Integration: Settings button in MainWindow, Settings accessible via dialog
  - **Remaining:** 2 external tooling items (manifest generation script, audio pack creation docs)
- ✅ All 123 tests passing
- ✅ Clean build with only code analysis warnings
- ✅ Updated all specs and _context.md

**🎯 NEXT SESSION:** Begin Spec 009 (Service Planner) or Spec 012/013 (Statistics/Export)

**Session 8 (2025-12-26 - Phase 5 Planning & Implementation):**

**Planning:**
- ✅ Created **Phase 5: UI/UX Refinement & Polish**
- ✅ Created **Spec 017:** UI/UX Overhaul & Modernization [#20](https://github.com/ThorSPB/SDAHymns/issues/20)
- ✅ Created **Spec 018:** Enhanced Hymn Slide Formatting [#21](https://github.com/ThorSPB/SDAHymns/issues/21)
- ✅ Created **Spec 019:** Compact Remote Control Widget [#22](https://github.com/ThorSPB/SDAHymns/issues/22)
- ✅ Moved **Spec 015** (Remote Control API) into Phase 5 as **Spec 020**

**Implementation - Spec 019: Compact Remote Widget** ✅ COMPLETE
- ✅ Created `RemoteWidgetSettings` model with JSON persistence
- ✅ Created `RemoteWidgetViewModel` with all core logic
- ✅ Created `RemoteWidget.axaml` with custom window chrome
- ✅ Implemented position save/load (default: bottom-right corner)
- ✅ Custom title bar with menu dropdown (≡), minimize, close buttons
- ✅ Draggable window with lock/unlock toggle
- ✅ Always-on-top toggle with persistence
- ✅ Hymn number input with Enter key support
- ✅ Optional number pad (3x4 grid: 0-9, ←, ⏎) with toggle
- ✅ SHOW/BLANK action buttons
- ✅ Verse navigation buttons (▲ Prev / ▼ Next)
- ✅ Audio control button integration
- ✅ Status message display
- ✅ Bottom toolbar (📌 Pin, 🔒 Lock, ⚙ Advanced Mode)
- ✅ **DisplayWindow integration** - properly shows hymn content
- ✅ **PowerPoint-style keyboard controls:**
  - Space/→/↓/PgDn/Enter → Next verse
  - ←/↑/PgUp/Backspace → Previous verse
  - Escape/B → Close/blank
- ✅ **App launch flow:** Default = RemoteWidget, `--advanced` = MainWindow
- ✅ Beautiful color scheme approved:
  - #1E1E2E (background), #6366F1 (accent), #2A2A3C (surface), #16161E (titlebar)
- ✅ Database migration for `RemoteWidgetSettingsJson` field

**User Feedback:**
- ✅ Colors are beautiful and should be used throughout the app
- ✅ Layout/sizing needs polish (to be addressed in Spec 017)
- ✅ Keyboard controls work perfectly like PowerPoint
- ✅ Core functionality complete, ready for polish phase

**📋 NEXT:** Spec 017 (UI/UX Overhaul) will polish RemoteWidget, DisplayWindow, and MainWindow

**Phase 2 Progress:**
- ✅ **Spec 006**: Enhanced Control Window - COMPLETE
- ✅ **Spec 007**: Display Profiles - COMPLETE
- ✅ **Spec 008**: Keyboard Shortcuts - COMPLETE
- 📋 **Spec 009**: Service Planner (pre-plan services, live mode)

**Phase 3 Progress:**
- ✅ **Spec 011**: Audio Playback - 100% COMPLETE (19/19 criteria)
- ✅ **Spec 014**: Audio Download & Settings - 100% APP FEATURES COMPLETE (18/20 criteria, 2 tooling items pending)
- 📋 **Spec 012**: Statistics Tracking - NOT STARTED
- 📋 **Spec 013**: Export Functionality - NOT STARTED

**Recommended Implementation Order:**
1. ~~**Spec 006** (Enhanced Control Window)~~ ✅ DONE
2. **Spec 008** (Keyboard Shortcuts) - integrates with control window
3. **Spec 007** (Display Profiles) - visual customization
4. **Spec 009** (Service Planner) - advanced workflow feature

**Estimated Effort:**
- Spec 006: ~3-4 hours (SearchService, UI refactor, recent/favorites)
- Spec 008: ~2-3 hours (HotKeyManager, shortcuts overlay, customization)
- Spec 007: ~4-5 hours (Profile model, editor UI, apply logic)
- Spec 009: ~3-4 hours (Service plan model, planner UI, live mode)

## Notes

- Prioritize offline-first functionality
- Ensure keyboard accessibility in all features
- Test multi-monitor scenarios regularly
- Romanian text encoding (UTF-8) must be verified
