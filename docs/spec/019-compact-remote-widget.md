# Spec 019: Compact Remote Control Widget

**Status:** ✅ Implemented
**Created:** 2025-12-26
**Implemented:** 2025-12-26 (Session 8)
**Dependencies:** 005-basic-hymn-display.md
**Related:** 017-ui-ux-overhaul.md (will polish this implementation)

## Overview

Create a compact, widget-style remote control window that mimics a physical remote control interface. This becomes the **default GUI** for day-to-day operation, while the MainWindow serves as the "advanced/settings" interface. The remote control should be small, always-accessible, and contain only essential controls for quick hymn switching during services.

**Goal:** Provide a minimal, distraction-free interface that operators can keep visible at the bottom-right of their screen, similar to the legacy application's approach.

## Design Philosophy

**Compact Widget, Not Full Application:**
- Small footprint (~250-300px wide, ~400-500px tall)
- Lives at screen corner (default: bottom-right)
- No traditional window chrome (title bar, standard buttons)
- Custom minimize/close buttons
- Looks like a physical remote control or widget
- Always accessible during services

**Default vs Advanced Mode:**
- **Remote Widget** (Spec 019) = Default launch, quick operations
- **MainWindow** (existing) = Advanced mode for configuration, browsing, planning

## User Workflows

### Scenario A: Sunday Service (Typical Use)
1. Operator launches app → Remote widget appears at bottom-right
2. Worship leader says "Hymn 123" → Operator types `123` + Enter
3. Borderless fullscreen window appears on projector with hymn
4. Operator clicks `Next` button or presses arrow key to advance verses
5. Service ends → Operator clicks custom close button (widget disappears)

### Scenario B: Setup/Configuration
1. Operator right-clicks remote widget → "Open Settings" or "Advanced Mode"
2. MainWindow opens with full controls, browsing, favorites, etc.
3. Operator configures display profiles, audio settings, etc.
4. Closes MainWindow → Remote widget remains open

### Scenario C: Positioning & Locking
1. First run → Remote appears at default position (bottom-right)
2. Operator drags remote to preferred corner
3. Clicks "Lock Position" toggle → Remote becomes unmovable
4. Clicks "Always on Top" toggle → Remote stays above other windows
5. App remembers all settings for next launch

## Visual Design

### Remote Widget Layout

```
┌─────────────────────────────┐
│  SDAHymns Remote        [≡] │ ← Custom title bar (12px tall)
│                         [_][×]│ ← Minimize/Close buttons
├─────────────────────────────┤
│  ┌─────────────────────────┐│
│  │  Hymn 123               ││ ← Current hymn display
│  │  Vers 2/4               ││
│  └─────────────────────────┘│
├─────────────────────────────┤
│  Load Hymn:                 │
│  ┌─────────────────────────┐│
│  │ 123                   ⏎ ││ ← Input field
│  └─────────────────────────┘│
│                              │
│  ┌─┬─┬─┐  ┌─────────┐       │
│  │7│8│9│  │  SHOW   │       │ ← Number pad + Show button
│  ├─┼─┼─┤  └─────────┘       │
│  │4│5│6│  ┌─────────┐       │
│  ├─┼─┼─┤  │  BLANK  │       │ ← Blank screen button
│  │1│2│3│  └─────────┘       │
│  ├─┼─┼─┤                     │
│  │←│0│→│  ┌────┬────┐       │
│  └─┴─┴─┘  │ ▲  │ ▼  │       │ ← Verse navigation
│            └────┴────┘       │
│                              │
│  ┌──────────────┐            │
│  │ ▶ Play Audio │            │ ← Audio control
│  └──────────────┘            │
├─────────────────────────────┤
│ [📌] [🔒] [⚙]              │ ← Bottom toolbar
└─────────────────────────────┘

Legend:
[≡] = Menu (right-click options)
[_] = Minimize
[×] = Close
[📌] = Always on Top toggle
[🔒] = Lock Position toggle
[⚙] = Open Advanced Mode (MainWindow)
```

### Size Specifications

**Dimensions:**
- Width: 280px (fixed)
- Height: 520px (fixed, not resizable)
- Border Radius: 12px (rounded corners for widget feel)
- Shadow: Large drop shadow for depth

**Custom Window Chrome:**
- Title bar: 12px tall (very compact)
- Custom buttons: 16x16px each
- Draggable area: Entire title bar + logo area

**Position:**
- Default: Bottom-right corner (20px margin from screen edges)
- Saved position: Persisted in AppSettings
- Locked: Cannot be dragged when locked

### Color Scheme (Dark Theme)

```
Background:     #1E1E2E (Dark slate)
Surface:        #2A2A3C (Slightly lighter)
Border:         #3E3E52
Accent:         #6366F1 (Indigo - matches Phase 5 palette)
Text:           #E5E7EB
TextMuted:      #9CA3AF
Button:         #6366F1
ButtonHover:    #7C3AED
ButtonActive:   #5B21B6
```

### Typography

```
Title:          12px Bold
Hymn Display:   16px SemiBold (hymn number/title)
Verse Info:     12px Regular (verse indicator)
Input:          18px Regular (hymn input field)
Buttons:        13px SemiBold
Number Pad:     16px SemiBold
```

## Features & Controls

### 1. Custom Window Chrome

**Title Bar:**
- Height: 12px
- Background: Slightly darker than window background
- Draggable (unless locked)
- No standard Windows controls

**Custom Buttons:**
- **Menu [≡]:** Dropdown with options:
  - Open Advanced Mode
  - Settings
  - Display Profiles
  - Always on Top (toggle)
  - Lock Position (toggle)
  - About
  - Exit
- **Minimize [_]:** Minimize to taskbar
- **Close [×]:** Close entire application (with confirmation)

**Hover States:**
- Background highlight on hover
- Smooth 100ms transition

### 2. Current Hymn Display

**Info Shown:**
- Hymn number and title
- Current verse indicator (e.g., "Verse 2/4")
- Category badge (small, top-right)

**States:**
- **Idle:** Gray text "No hymn loaded"
- **Loaded:** White text with hymn info
- **Playing Audio:** Pulsing accent color border

### 3. Hymn Input Field

**Behavior:**
- Auto-focus on app launch
- Type hymn number directly
- Press Enter to load and display
- Clear on load (ready for next hymn)
- Shows category dropdown (optional, compact)

**Validation:**
- Red border if hymn not found
- Toast notification: "Hymn 999 not found"

### 4. Number Pad (Optional Feature)

**Layout:**
- 3x4 grid (0-9 + arrows)
- 30x30px buttons
- Tactile click feedback
- Shortcuts:
  - `←` = Backspace (delete last digit)
  - `→` = Enter (load hymn)
  - `0` = Zero digit

**Enable/Disable:**
- Toggle in settings (some users prefer keyboard only)
- Hidden when disabled (saves space)

### 5. Action Buttons

**SHOW Button:**
- Large, prominent (accent color)
- Loads typed hymn and displays on projector
- Keyboard: Enter key

**BLANK Button:**
- Secondary style
- Blanks the display window (black screen)
- Keyboard: B key or Esc

**Verse Navigation (▲/▼):**
- Previous/Next verse
- Auto-disable when at first/last verse
- Keyboard: Arrow Up/Down

**Play Audio:**
- Toggle button (Play ▶ / Pause ⏸)
- Only visible if audio available for hymn
- Keyboard: Spacebar

### 6. Bottom Toolbar

**Always on Top [📌]:**
- Toggle button
- Active state: Highlighted (accent color)
- Keeps widget above all windows
- Persisted in settings

**Lock Position [🔒]:**
- Toggle button
- Active state: Highlighted
- Prevents dragging when enabled
- Persisted in settings

**Advanced Mode [⚙]:**
- Opens MainWindow (full interface)
- Remote widget remains open
- Can have both windows open simultaneously

## Technical Implementation

### Window Configuration

**Avalonia Window Properties:**
```csharp
public class RemoteWidget : Window
{
    public RemoteWidget()
    {
        Width = 280;
        Height = 520;
        CanResize = false;

        // Remove standard window chrome
        SystemDecorations = SystemDecorations.None;

        // Rounded corners
        CornerRadius = new CornerRadius(12);

        // Shadow
        BoxShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 4,
            Blur = 20,
            Color = Color.FromArgb(60, 0, 0, 0)
        });

        // Transparent background for rounded corners
        TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;

        // Position
        Position = LoadSavedPosition();

        // Always on top
        Topmost = LoadAlwaysOnTopSetting();
    }
}
```

### Position Management

**Settings Model:**
```csharp
public class RemoteWidgetSettings
{
    public double PositionX { get; set; } = double.NaN; // NaN = use default
    public double PositionY { get; set; } = double.NaN;
    public bool IsLocked { get; set; } = false;
    public bool AlwaysOnTop { get; set; } = true;
    public bool ShowNumberPad { get; set; } = true;
    public int LastHymnNumber { get; set; } = 0;
}
```

**Default Position Calculation:**
```csharp
private PixelPoint GetDefaultPosition()
{
    var screen = Screens.Primary;
    var workingArea = screen.WorkingArea;

    // Bottom-right corner with 20px margin
    int x = workingArea.Width - (int)Width - 20;
    int y = workingArea.Height - (int)Height - 20;

    return new PixelPoint(x, y);
}
```

**Drag & Lock Logic:**
```csharp
private void OnTitleBarPointerPressed(object sender, PointerPressedEventArgs e)
{
    if (Settings.IsLocked)
    {
        // Show tooltip: "Position locked. Unlock to move."
        return;
    }

    BeginMoveDrag(e);
}

private void OnPositionChanged(object sender, EventArgs e)
{
    if (!Settings.IsLocked)
    {
        Settings.PositionX = Position.X;
        Settings.PositionY = Position.Y;
        SaveSettings();
    }
}
```

### Borderless DisplayWindow

**Updated DisplayWindow:**
```csharp
public class DisplayWindow : Window
{
    public DisplayWindow()
    {
        // Remove ALL chrome for true fullscreen
        SystemDecorations = SystemDecorations.None;

        // Fullscreen mode
        WindowState = WindowState.FullScreen;

        // Always on top of other windows on projector
        Topmost = true;

        // No taskbar icon
        ShowInTaskbar = false;
    }
}
```

**Multi-Monitor Support:**
```csharp
public void ShowOnProjector(int screenIndex)
{
    var screens = Screens.All;
    if (screenIndex >= 0 && screenIndex < screens.Count)
    {
        var targetScreen = screens[screenIndex];

        // Position window on target screen
        Position = new PixelPoint(
            targetScreen.WorkingArea.X,
            targetScreen.WorkingArea.Y
        );

        // Set to fullscreen on that monitor
        WindowState = WindowState.FullScreen;
        Show();
    }
}
```

**Projector Selection in Settings:**
```csharp
public class AppSettings
{
    // ... existing properties

    public int ProjectorScreenIndex { get; set; } = 1; // 0=primary, 1=secondary
    public bool ProjectorFullscreen { get; set; } = true;
}
```

### Application Startup Flow

**Program.cs / App.axaml.cs:**
```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        // Check launch mode
        var args = Environment.GetCommandLineArgs();
        bool advancedMode = args.Contains("--advanced");

        if (advancedMode)
        {
            // Launch full MainWindow
            desktop.MainWindow = new MainWindow();
        }
        else
        {
            // Launch compact remote widget (default)
            desktop.MainWindow = new RemoteWidget();
        }
    }

    base.OnFrameworkInitializationCompleted();
}
```

**Shared ViewModel:**
Both RemoteWidget and MainWindow share the same `HymnControlViewModel`:
```csharp
public class HymnControlViewModel : ViewModelBase
{
    // Shared state
    public Hymn? CurrentHymn { get; set; }
    public int CurrentVerseIndex { get; set; }
    public ObservableCollection<Verse> Verses { get; set; }

    // Shared commands
    public ICommand LoadHymnCommand { get; }
    public ICommand NextVerseCommand { get; }
    public ICommand PreviousVerseCommand { get; }
    public ICommand BlankDisplayCommand { get; }
    public ICommand PlayAudioCommand { get; }

    // Display window reference
    private DisplayWindow? _displayWindow;

    public void ShowDisplay()
    {
        if (_displayWindow == null)
        {
            _displayWindow = new DisplayWindow();
            _displayWindow.DataContext = this; // Share ViewModel
        }

        _displayWindow.ShowOnProjector(Settings.ProjectorScreenIndex);
    }
}
```

## ViewModel: RemoteWidgetViewModel

```csharp
public partial class RemoteWidgetViewModel : ViewModelBase
{
    private readonly HymnDisplayService _hymnService;
    private readonly AudioPlayerService _audioService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _hymnNumberInput = "";

    [ObservableProperty]
    private string _currentHymnDisplay = "No hymn loaded";

    [ObservableProperty]
    private string _verseIndicator = "";

    [ObservableProperty]
    private bool _isAlwaysOnTop = true;

    [ObservableProperty]
    private bool _isPositionLocked = false;

    [ObservableProperty]
    private bool _showNumberPad = true;

    [ObservableProperty]
    private bool _canNavigateNext = false;

    [ObservableProperty]
    private bool _canNavigatePrevious = false;

    public RemoteWidgetSettings Settings { get; set; }

    [RelayCommand]
    private async Task LoadHymn()
    {
        if (!int.TryParse(HymnNumberInput, out int hymnNumber))
        {
            ShowError("Invalid hymn number");
            return;
        }

        var hymn = await _hymnService.GetHymnByNumberAsync(hymnNumber);
        if (hymn == null)
        {
            ShowError($"Hymn {hymnNumber} not found");
            return;
        }

        // Load hymn and show on display
        await _hymnService.LoadHymnAsync(hymn);
        _displayWindow?.Show();

        // Update display
        CurrentHymnDisplay = $"Hymn {hymn.Number}";
        VerseIndicator = $"Verse 1/{hymn.Verses.Count}";
        HymnNumberInput = ""; // Clear for next input

        UpdateNavigationButtons();
    }

    [RelayCommand]
    private void NumberPadPress(string digit)
    {
        HymnNumberInput += digit;
    }

    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        IsAlwaysOnTop = !IsAlwaysOnTop;
        Settings.AlwaysOnTop = IsAlwaysOnTop;
        SaveSettings();
    }

    [RelayCommand]
    private void ToggleLockPosition()
    {
        IsPositionLocked = !IsPositionLocked;
        Settings.IsLocked = IsPositionLocked;
        SaveSettings();
    }

    [RelayCommand]
    private void OpenAdvancedMode()
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
```

## Implementation Plan

### Phase 1: Window Infrastructure (2 hours)
1. Create `RemoteWidget.axaml` and `RemoteWidget.axaml.cs`
2. Configure window properties (no chrome, fixed size, shadow)
3. Create `RemoteWidgetViewModel`
4. Create `RemoteWidgetSettings` model
5. Implement position save/load logic
6. Update `App.axaml.cs` startup flow

### Phase 2: Custom Window Chrome (2 hours)
1. Design custom title bar in XAML
2. Implement custom minimize/close buttons
3. Add menu dropdown (≡ button)
4. Implement drag logic with lock support
5. Add hover/active states for buttons

### Phase 3: Main Content Area (3 hours)
1. Current hymn display panel
2. Hymn input field with validation
3. Number pad grid (3x4 buttons)
4. Action buttons (SHOW, BLANK)
5. Verse navigation buttons
6. Audio control button
7. Wire up all commands to ViewModel

### Phase 4: Bottom Toolbar (1 hour)
1. Always on Top toggle
2. Lock Position toggle
3. Advanced Mode button
4. Bind toggles to settings

### Phase 5: Borderless DisplayWindow (1 hour)
1. Update DisplayWindow to remove chrome
2. Implement true fullscreen mode
3. Add projector selection logic
4. Test multi-monitor scenarios

### Phase 6: Integration & Testing (2 hours)
1. Test RemoteWidget as default launch
2. Test position persistence
3. Test lock/unlock behavior
4. Test always-on-top functionality
5. Test launching both windows simultaneously
6. Test on multi-monitor setup
7. Verify keyboard shortcuts work in both modes

### Phase 7: Polish & Animations (1 hour)
1. Add button click animations
2. Add hover transitions
3. Add toast notifications for errors
4. Add loading state when loading hymn
5. Add "pulse" effect when audio playing

## Acceptance Criteria

**Window Behavior:**
- [ ] RemoteWidget launches as default (no command-line args)
- [ ] Window is 280x520px, fixed size (not resizable)
- [ ] Rounded corners (12px radius)
- [ ] Drop shadow for depth
- [ ] No standard window chrome (title bar, buttons)

**Custom Chrome:**
- [ ] Custom title bar (12px tall)
- [ ] Custom minimize/close buttons
- [ ] Menu dropdown with all options
- [ ] Title bar is draggable (when unlocked)
- [ ] Buttons have hover/active states

**Position & Layout:**
- [ ] Default position: bottom-right corner (20px margins)
- [ ] Position persisted across app restarts
- [ ] Lock toggle prevents dragging
- [ ] Always on Top toggle works correctly
- [ ] Widget stays in position on screen resolution changes

**Controls:**
- [ ] Hymn input field auto-focuses on launch
- [ ] Enter key loads and displays hymn
- [ ] Number pad inputs digits correctly
- [ ] SHOW button displays hymn on projector
- [ ] BLANK button blanks display
- [ ] Verse navigation buttons work (▲/▼)
- [ ] Audio button plays/pauses (if available)

**DisplayWindow:**
- [ ] DisplayWindow is truly borderless (no chrome)
- [ ] Fullscreen on selected projector
- [ ] No taskbar icon for DisplayWindow
- [ ] DisplayWindow stays on top of other windows

**Integration:**
- [ ] Advanced Mode button opens MainWindow
- [ ] Both windows can be open simultaneously
- [ ] Both windows share hymn state via ViewModel
- [ ] Changing hymn in one window updates the other
- [ ] Settings changes in MainWindow reflect in RemoteWidget

**Settings Persistence:**
- [ ] Position (X, Y) saved and restored
- [ ] Lock state saved and restored
- [ ] Always on Top state saved and restored
- [ ] Number pad visibility saved and restored
- [ ] Projector screen selection saved

**Visual Design:**
- [ ] Matches Phase 5 design system (colors, typography)
- [ ] Buttons have smooth hover transitions
- [ ] Error states show red border + toast
- [ ] Current hymn display updates correctly
- [ ] Verse indicator shows "Verse X/Y"

## Future Enhancements

- **Themes:** Multiple remote widget skins (classic, modern, minimal)
- **Opacity Control:** Adjustable widget transparency (50-100%)
- **Hotkey Activation:** Global hotkey to show/hide remote widget
- **Recent Hymns:** Quick access to last 5 hymns (small buttons)
- **Favorites:** Star button to mark current hymn as favorite
- **Service Queue:** Small queue indicator showing next hymn
- **Resizable:** Optional resizing (snap to preset sizes: small/medium/large)
- **Dock Modes:** Snap to screen edges (top/bottom/left/right)

## Related Specs

- **Spec 005:** Basic Hymn Display (foundation for display logic)
- **Spec 017:** UI/UX Overhaul (design system, colors, typography)
- **Spec 006:** Enhanced Control Window (MainWindow = advanced mode)
- **Spec 008:** Keyboard Shortcuts (shared shortcuts between windows)

---

## Implementation Notes (2025-12-26)

### ✅ Completed Features

**All Core Features Implemented:**
- ✅ RemoteWidget window (280x520px, fixed size, custom chrome)
- ✅ Position management (dragable, lockable, persistent via JSON)
- ✅ Always-on-top toggle with persistence
- ✅ Custom title bar with menu dropdown (≡), minimize, close buttons
- ✅ Hymn number input field with Enter key support
- ✅ Optional number pad (3x4 grid) with toggle in menu
- ✅ SHOW/BLANK action buttons
- ✅ Verse navigation buttons (▲ Prev / ▼ Next)
- ✅ Audio playback button (shows when audio available)
- ✅ Status message display
- ✅ Bottom toolbar (📌 Pin, 🔒 Lock, ⚙ Advanced Mode)

**DisplayWindow Integration:**
- ✅ Borderless fullscreen display on projector
- ✅ Properly loads and displays hymn content
- ✅ Verse navigation syncs between RemoteWidget and DisplayWindow
- ✅ **PowerPoint-style keyboard controls:**
  - Space/→/↓/PgDn/Enter → Next verse
  - ←/↑/PgUp/Backspace → Previous verse
  - Escape/B → Close/blank display

**App Launch Flow:**
- ✅ Default launch (`SDAHymns.exe`) → RemoteWidget
- ✅ Advanced mode (`SDAHymns.exe --advanced`) → MainWindow
- ✅ Both windows can run simultaneously
- ✅ Service provider integration for dependency injection

**Data & Persistence:**
- ✅ RemoteWidgetSettings model
- ✅ Database migration: `AddRemoteWidgetSettings`
- ✅ JSON serialization in AppSettings table
- ✅ Settings persist across app restarts

**Code Files Created:**
- `src/SDAHymns.Core/Models/RemoteWidgetSettings.cs`
- `src/SDAHymns.Core/Migrations/20251226120258_AddRemoteWidgetSettings.cs`
- `src/SDAHymns.Desktop/ViewModels/RemoteWidgetViewModel.cs`
- `src/SDAHymns.Desktop/Views/RemoteWidget.axaml`
- `src/SDAHymns.Desktop/Views/RemoteWidget.axaml.cs`
- Modified: `App.axaml.cs`, `ISettingsService.cs`, `SettingsService.cs`, `MainWindowViewModel.cs`, `DisplayWindow.axaml.cs`, `AppSettings.cs`

### 🎨 Color Scheme (User-Approved)

This implementation established the official color palette for the entire application:

```
Background:  #1E1E2E (Dark slate) - Main background
Accent:      #6366F1 (Indigo 500) - Primary accent color
Surface:     #2A2A3C - Cards, panels, surfaces
TitleBar:    #16161E (Extra dark) - Title bars, toolbars
Border:      #3E3E52 - Subtle borders
Text:        #E5E7EB (Off-white) - Primary text
TextMuted:   #9CA3AF (Gray 400) - Secondary text
```

**User feedback:** "I love the design, the colors are very beautiful. We should definitely use it for the main window as well."

This color scheme will be applied throughout the application in Spec 017 (UI/UX Overhaul).

### 📋 Known Limitations & Future Polish

**Layout & Sizing (to be addressed in Spec 017):**
- Button sizes and spacing need refinement
- Font sizes could be more balanced
- Some controls feel cramped
- Visual hierarchy could be improved

**Missing Optional Features (deferred):**
- Resizable widget (spec mentioned optional sizes)
- Theme switcher (dark/light)
- Dock modes (snap to screen edges)
- Opacity control
- Recent hymns quick access
- Favorites integration

**Audio Countdown Issue:**
- DisplayWindow shows "Starting audio in..." countdown overlay
- This is from the auto-play feature (IsCountdownActive)
- Needs setting to disable or hide by default
- Will be addressed in future update

### ✅ Acceptance Criteria Status

All critical acceptance criteria met:
- ✅ RemoteWidget launches as default
- ✅ Window is 280x520px, fixed size
- ✅ Custom title bar and buttons work
- ✅ Position persisted across restarts
- ✅ Lock/unlock toggle works
- ✅ Always on Top toggle works
- ✅ Hymn input loads and displays correctly
- ✅ SHOW button displays hymn on projector
- ✅ BLANK button closes display
- ✅ Verse navigation works
- ✅ DisplayWindow is borderless fullscreen
- ✅ Settings changes persist
- ✅ Keyboard shortcuts work (PowerPoint-style)

### 🎯 Overall Assessment

**Status:** ✅ **COMPLETE** - Core functionality fully implemented and working

The RemoteWidget is production-ready for basic use. All essential features work correctly:
- Hymn loading and display
- Verse navigation
- Position management
- DisplayWindow integration
- Keyboard controls

Visual polish and refinements will be handled in Spec 017 (UI/UX Overhaul), which will:
- Improve spacing, sizing, and layout
- Add animations and transitions
- Refine typography
- Add icon system
- Polish all windows (RemoteWidget, DisplayWindow, MainWindow)

**Estimated completion:** ~95% of planned features
**Remaining work:** Visual polish and optional enhancements
