# Spec 008: Keyboard Shortcuts System

**Status:** ✅ Implemented
**Created:** 2025-12-04
**Completed:** 2025-12-26
**Dependencies:** 006-enhanced-control-window.md

## Overview

Implement a comprehensive keyboard shortcut system for productivity during live church services. Enable operators to control the entire application without touching the mouse, including hymn navigation, search, display control, and profile switching.

**Goal:** Make SDAHymns fully keyboard-driven for maximum efficiency during time-sensitive service operations.

## Goals

1. Global hotkeys for common actions (Next/Prev verse, Display toggle, etc.)
2. Context-aware shortcuts (search when typing, navigate when browsing)
3. Customizable keybindings
4. Shortcut hints/cheatsheet overlay
5. Support for multimedia keys (if available)
6. No conflicts with system shortcuts
7. Visual feedback when shortcuts are triggered

**Non-Goals:**
- MIDI controller support (Phase 4)
- Mobile/tablet gestures
- Voice control
- Macros/scripting

## Default Keyboard Shortcuts

### Global Shortcuts (Work Anywhere)

| Shortcut | Action | Context |
|----------|--------|---------|
| `Ctrl+F` | Focus search box | Global |
| `Esc` | Clear search / Close dialog | Global |
| `F5` | Toggle display window | Global |
| `F11` | Toggle fullscreen (display window) | Global |
| `Ctrl+,` | Open settings | Global |
| `Ctrl+Shift+P` | Open profile selector | Global |
| `Ctrl+Q` | Quit application | Global |

### Hymn Navigation

| Shortcut | Action | Context |
|----------|--------|---------|
| `Space` | Next verse | Display active |
| `Shift+Space` | Previous verse | Display active |
| `→` or `PageDown` | Next verse | Display active |
| `←` or `PageUp` | Previous verse | Display active |
| `Home` | First verse | Display active |
| `End` | Last verse | Display active |
| `Ctrl+→` | Next hymn (in recent/search) | Display active |
| `Ctrl+←` | Previous hymn (in recent/search) | Display active |

### Search & Selection

| Shortcut | Action | Context |
|----------|--------|---------|
| `↑` / `↓` | Navigate search results | Search focused |
| `Enter` | Select hymn from results | Search focused |
| `Ctrl+1` - `Ctrl+9` | Load recent hymn 1-9 | Global |
| `Ctrl+Shift+F` | Toggle favorites filter | Search focused |
| `Ctrl+D` | Toggle favorite (current hymn) | Global |

### Display Control

| Shortcut | Action | Context |
|----------|--------|---------|
| `B` | Blank/unblank display (black screen) | Display active |
| `L` | Logo screen (church logo) | Display active |
| `Ctrl+B` | Toggle background visibility | Display active |
| `Ctrl+T` | Toggle title visibility | Display active |
| `+` / `=` | Increase font size (temp) | Display active |
| `-` | Decrease font size (temp) | Display active |
| `Ctrl+0` | Reset font size | Display active |

### Profile Switching

| Shortcut | Action | Context |
|----------|--------|---------|
| `Ctrl+Shift+1` - `9` | Switch to profile 1-9 | Global |
| `Ctrl+P` | Profile quick switcher | Global |

### Service Planner (Phase 2, Spec 009)

| Shortcut | Action | Context |
|----------|--------|---------|
| `Ctrl+N` | New service plan | Global |
| `Ctrl+O` | Open service plan | Global |
| `Ctrl+S` | Save service plan | Plan open |
| `Ctrl+Shift+S` | Save as | Plan open |

### Utility

| Shortcut | Action | Context |
|----------|--------|---------|
| `F1` | Show keyboard shortcuts overlay | Global |
| `Ctrl+Shift+C` | Copy current verse text | Display active |
| `Ctrl+E` | Export current hymn (PDF) | Hymn loaded |
| `Ctrl+R` | Reload hymn database | Global |

## Architecture

### HotKeyManager Service

**File:** `src/SDAHymns.Core/Services/HotKeyManager.cs`

```csharp
public interface IHotKeyManager
{
    void RegisterHotKey(string action, KeyGesture gesture);
    void UnregisterHotKey(string action);
    void ExecuteHotKey(string action);
    Dictionary<string, KeyGesture> GetAllHotKeys();
    void LoadCustomBindings();
    void SaveCustomBindings();
    void ResetToDefaults();
}

public class KeyGesture
{
    public Key Key { get; set; }
    public KeyModifiers Modifiers { get; set; }

    public override string ToString() =>
        $"{(Modifiers != KeyModifiers.None ? Modifiers + "+" : "")}{Key}";

    public static KeyGesture Parse(string gesture)
    {
        // Parse "Ctrl+Shift+F" into KeyGesture
    }
}

public class HotKeyManager : IHotKeyManager
{
    private Dictionary<string, KeyGesture> _hotKeys = new();
    private Dictionary<string, Action> _actions = new();

    public HotKeyManager()
    {
        RegisterDefaultHotKeys();
    }

    private void RegisterDefaultHotKeys()
    {
        RegisterHotKey("FocusSearch", new KeyGesture { Key = Key.F, Modifiers = KeyModifiers.Control });
        RegisterHotKey("ToggleDisplay", new KeyGesture { Key = Key.F5, Modifiers = KeyModifiers.None });
        RegisterHotKey("NextVerse", new KeyGesture { Key = Key.Space, Modifiers = KeyModifiers.None });
        RegisterHotKey("PreviousVerse", new KeyGesture { Key = Key.Space, Modifiers = KeyModifiers.Shift });
        // ... all default shortcuts
    }

    public void RegisterAction(string actionName, Action action)
    {
        _actions[actionName] = action;
    }

    public void ExecuteHotKey(string action)
    {
        if (_actions.TryGetValue(action, out var actionDelegate))
        {
            actionDelegate?.Invoke();
        }
    }

    public void HandleKeyDown(KeyEventArgs e)
    {
        var gesture = new KeyGesture
        {
            Key = e.Key,
            Modifiers = e.KeyModifiers
        };

        var action = _hotKeys.FirstOrDefault(kvp => kvp.Value.Equals(gesture)).Key;
        if (!string.IsNullOrEmpty(action))
        {
            ExecuteHotKey(action);
            e.Handled = true;
        }
    }
}
```

### Integration with MainWindow

**File:** `src/SDAHymns.Desktop/Views/MainWindow.axaml.cs`

```csharp
public partial class MainWindow : Window
{
    private readonly IHotKeyManager _hotKeyManager;

    public MainWindow()
    {
        InitializeComponent();

        _hotKeyManager = ServiceProvider.GetService<IHotKeyManager>();

        // Register actions
        _hotKeyManager.RegisterAction("FocusSearch", () =>
        {
            SearchTextBox.Focus();
        });

        _hotKeyManager.RegisterAction("ToggleDisplay", () =>
        {
            ToggleDisplayWindow();
        });

        _hotKeyManager.RegisterAction("NextVerse", () =>
        {
            if (DataContext is MainWindowViewModel vm)
                vm.NextVerseCommand.Execute(null);
        });

        // Global key handler
        this.KeyDown += (s, e) =>
        {
            _hotKeyManager.HandleKeyDown(e);
        };
    }
}
```

### Shortcuts Overlay

**File:** `src/SDAHymns.Desktop/Views/ShortcutsOverlay.axaml`

```xml
<Window Title="Keyboard Shortcuts" Width="700" Height="600">
    <ScrollViewer>
        <StackPanel Margin="20" Spacing="15">
            <TextBlock Text="Keyboard Shortcuts" FontSize="24" FontWeight="Bold"/>

            <TextBlock Text="Global" FontSize="18" FontWeight="SemiBold" Margin="0,20,0,10"/>
            <DataGrid ItemsSource="{Binding GlobalShortcuts}" AutoGenerateColumns="False">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Shortcut" Binding="{Binding Key}" Width="150"/>
                    <DataGridTextColumn Header="Action" Binding="{Binding Action}" Width="*"/>
                </DataGrid.Columns>
            </DataGrid>

            <TextBlock Text="Navigation" FontSize="18" FontWeight="SemiBold" Margin="0,20,0,10"/>
            <DataGrid ItemsSource="{Binding NavigationShortcuts}" AutoGenerateColumns="False">
                <!-- ... -->
            </DataGrid>

            <TextBlock Text="Display Control" FontSize="18" FontWeight="SemiBold" Margin="0,20,0,10"/>
            <DataGrid ItemsSource="{Binding DisplayShortcuts}" AutoGenerateColumns="False">
                <!-- ... -->
            </DataGrid>

            <StackPanel Orientation="Horizontal" Spacing="10" Margin="0,20,0,0">
                <Button Content="Customize Shortcuts" Command="{Binding OpenCustomizeCommand}"/>
                <Button Content="Reset to Defaults" Command="{Binding ResetDefaultsCommand}"/>
                <Button Content="Print Cheat Sheet" Command="{Binding PrintCommand}"/>
            </StackPanel>
        </StackPanel>
    </ScrollViewer>
</Window>
```

### Customize Shortcuts Dialog

**File:** `src/SDAHymns.Desktop/Views/CustomizeShortcutsWindow.axaml`

```xml
<Window Title="Customize Keyboard Shortcuts" Width="600" Height="500">
    <Grid RowDefinitions="*,Auto">
        <DataGrid Grid.Row="0" ItemsSource="{Binding AllActions}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Action" Binding="{Binding Name}" IsReadOnly="True"/>
                <DataGridTemplateColumn Header="Shortcut" Width="200">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <TextBox Text="{Binding Shortcut}"
                                    KeyDown="ShortcutInput_KeyDown"
                                    Watermark="Press keys..."/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
                <DataGridTemplateColumn Header="" Width="Auto">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Button Content="Clear" Command="{Binding ClearCommand}"/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>

        <Border Grid.Row="1" Background="#1A1A1A" Padding="15">
            <StackPanel Orientation="Horizontal" Spacing="10">
                <Button Content="Save" Command="{Binding SaveCommand}"/>
                <Button Content="Cancel" Command="{Binding CancelCommand}"/>
                <Button Content="Reset All" Command="{Binding ResetAllCommand}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

## Features

### 1. Context-Aware Shortcuts
- **Search mode**: Arrow keys navigate results
- **Display mode**: Arrow keys navigate verses
- **No conflict**: Same keys do different things in different contexts

### 2. Visual Feedback
- **Toast notification** when shortcut triggered (optional, can disable)
- **Highlight** active controls when focused via keyboard
- **Shortcut hints** on button tooltips

### 3. Conflict Detection
- **Warn user** if custom shortcut conflicts with existing
- **Cannot override** system shortcuts (Alt+Tab, Win+D, etc.)
- **Suggest alternatives** when conflict detected

### 4. Persisted Customization
- **Save to JSON** file in user's config folder
- **Per-user** customization (not global)
- **Import/export** shortcut schemes

### 5. Multimedia Keys Support
- **Play/Pause** → Next/Previous verse (if no media playing)
- **Next/Previous Track** → Next/Previous hymn
- **Stop** → Blank display

## Implementation Plan

### Step 1: Create HotKeyManager Service
- Implement key gesture parsing
- Register default shortcuts
- Load/save custom bindings from JSON

### Step 2: Integrate with Windows
- Add global KeyDown handler to MainWindow
- Add KeyDown handler to DisplayWindow
- Register all actions with HotKeyManager

### Step 3: Build Shortcuts Overlay
- Display all shortcuts grouped by category
- Add "Customize" button to open editor
- Show on F1 key press

### Step 4: Build Customization Dialog
- Allow editing shortcuts
- Detect conflicts
- Save changes to JSON

### Step 5: Add Visual Feedback
- Toast notifications for shortcuts
- Shortcut hints on buttons
- Focus indicators for keyboard navigation

## Testing Strategy

### Manual Testing
1. Press F1, verify overlay shows
2. Press Space, verify next verse works
3. Press Ctrl+F, verify search focuses
4. Customize shortcut, restart app, verify persists
5. Press conflicting shortcut, verify warning shows
6. Test multimedia keys

### Unit Tests
```csharp
[Fact]
public void ParseKeyGesture_ParsesCorrectly()
{
    var gesture = KeyGesture.Parse("Ctrl+Shift+F");
    Assert.Equal(Key.F, gesture.Key);
    Assert.Equal(KeyModifiers.Control | KeyModifiers.Shift, gesture.Modifiers);
}

[Fact]
public void ExecuteHotKey_TriggersRegisteredAction()
{
    var called = false;
    _hotKeyManager.RegisterAction("Test", () => called = true);
    _hotKeyManager.ExecuteHotKey("Test");
    Assert.True(called);
}
```

## Acceptance Criteria

- [x] All default shortcuts work as documented
- [x] F1 shows shortcuts overlay
- [x] Can customize shortcuts in settings (click button, press keys)
- [x] Custom shortcuts persist after restart (JSON file)
- [x] Conflict detection warns user (orange banner with swap message)
- [x] Context-aware shortcuts work (search vs display)
- [x] Visual feedback on shortcut trigger (button states)
- [x] Cannot override critical system shortcuts (only captures non-modifier keys)
- [ ] Multimedia keys work (if available) - *Deferred to Phase 3*
- [x] Can reset shortcuts to defaults
- [x] Shortcut hints show on button tooltips
- [x] Keyboard-only workflow fully functional

## Future Enhancements (Phase 3)

- Shortcut recording mode (press keys to assign)
- Export/import shortcut schemes
- Multiple shortcut profiles (beginner, advanced, etc.)
- MIDI controller mapping
- Gamepad support
- Voice commands

## Implementation Notes (2025-12-26)

### Architecture Decisions

1. **String-based keys in Core layer**
   - Avoided Avalonia dependency in `SDAHymns.Core`
   - Used string representations (`"Ctrl+F"`) for UI-agnostic design
   - MainWindow converts Avalonia `Key` enums to strings before passing to manager

2. **Inline editing instead of separate dialog**
   - Click-to-edit buttons directly in F1 window
   - Better UX than modal dialog
   - Immediate visual feedback

3. **Auto-swap conflicts instead of blocking**
   - Shows orange warning banner describing swaps
   - Allows saving - automatically swaps conflicting shortcuts
   - User-friendly approach vs hard error

### Key Features Implemented

- **HotKeyManager Service** - 20+ default shortcuts, JSON persistence
- **ShortcutsWindow** - F1 overlay with inline editing
- **Smart conflict handling** - Auto-detects and swaps
- **Context-aware shortcuts** - Arrow keys work differently based on focus
- **Recent hymn shortcuts** - Ctrl+1-5 for quick access
- **Visual feedback** - Button hover/listening states, swap warning banner

### Test Coverage

- 24 unit tests for HotKeyManager
- 68 total tests (all passing)
- Coverage: gesture parsing, action registration, conflict detection, JSON persistence

### Deferred Features

- Multimedia key support (Phase 3)
- Import/export shortcut schemes (Phase 3)
- Multiple shortcut profiles (Phase 3)

## Related Specs

- **Previous:** 006-enhanced-control-window.md (provides UI to control)
- **Next:** 009-service-planner.md
- **Related:** 007-display-profiles.md (profile switching shortcuts)

## Notes

- **Accessibility first**: Keyboard shortcuts enable users with motor impairments
- **Speed matters**: Service operators need fast, reliable shortcuts
- **Conflicts are bad**: Better to have no shortcut than conflicting shortcut
- **Discoverability**: Users won't use shortcuts they don't know exist - show hints
- **Muscle memory**: Once users learn shortcuts, don't change defaults
