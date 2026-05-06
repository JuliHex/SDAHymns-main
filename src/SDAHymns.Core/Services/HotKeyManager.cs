using System.Text.Json;

namespace SDAHymns.Core.Services;

public class HotKeyManager : IHotKeyManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly Dictionary<string, KeyGesture> _hotKeys = new();
    private readonly Dictionary<string, Action> _actions = new();
    private readonly Dictionary<string, ShortcutInfo> _shortcutInfo = new();
    private readonly string _defaultConfigPath;

    public HotKeyManager()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SDAHymns"
        );
        Directory.CreateDirectory(appDataPath);
        _defaultConfigPath = Path.Combine(appDataPath, "keyboard-shortcuts.json");

        RegisterDefaultHotKeys();
    }

    private void RegisterDefaultHotKeys()
    {
        // Global shortcuts
        Register("FocusSearch", "Focus Search Box", "Global", "F", "Ctrl", "Focus the search box to find hymns");
        Register("ClearSearch", "Clear Search / Close Dialog", "Global", "Escape", "", "Clear search or close open dialogs");
        Register("ToggleDisplay", "Toggle Display Window", "Global", "F5", "", "Show or hide the display window");
        Register("ToggleFullscreen", "Toggle Fullscreen", "Global", "F11", "", "Toggle fullscreen mode on display window");
        Register("OpenSettings", "Open Settings", "Global", "OemComma", "Ctrl", "Open application settings");
        Register("QuitApp", "Quit Application", "Global", "Q", "Ctrl", "Close the application");

        // Hymn navigation
        Register("NextVerse", "Next Verse", "Navigation", "Space", "", "Move to next verse");
        Register("PreviousVerse", "Previous Verse", "Navigation", "Space", "Shift", "Move to previous verse");
        Register("NextVerseArrow", "Next Verse (Arrow)", "Navigation", "Right", "", "Move to next verse");
        Register("PreviousVerseArrow", "Previous Verse (Arrow)", "Navigation", "Left", "", "Move to previous verse");
        Register("NextVersePage", "Next Verse (Page Down)", "Navigation", "PageDown", "", "Move to next verse");
        Register("PreviousVersePage", "Previous Verse (Page Up)", "Navigation", "PageUp", "", "Move to previous verse");
        Register("FirstVerse", "First Verse", "Navigation", "Home", "", "Jump to first verse");
        Register("LastVerse", "Last Verse", "Navigation", "End", "", "Jump to last verse");

        // Search & selection
        Register("SelectHymn", "Select Hymn", "Search", "Enter", "", "Load selected hymn from search results");
        Register("ToggleFavorite", "Toggle Favorite", "Search", "D", "Ctrl", "Mark current hymn as favorite");

        // Recent hymns (Ctrl+1 through Ctrl+5)
        Register("LoadRecent1", "Load Recent Hymn 1", "Search", "D1", "Ctrl", "Load first recent hymn");
        Register("LoadRecent2", "Load Recent Hymn 2", "Search", "D2", "Ctrl", "Load second recent hymn");
        Register("LoadRecent3", "Load Recent Hymn 3", "Search", "D3", "Ctrl", "Load third recent hymn");
        Register("LoadRecent4", "Load Recent Hymn 4", "Search", "D4", "Ctrl", "Load fourth recent hymn");
        Register("LoadRecent5", "Load Recent Hymn 5", "Search", "D5", "Ctrl", "Load fifth recent hymn");

        // Display control
        Register("BlankDisplay", "Blank Display", "Display", "B", "", "Show black screen on display");
        Register("ShowHelpOverlay", "Show Keyboard Shortcuts", "Global", "F1", "", "Show keyboard shortcuts overlay");
    }

    private void Register(string action, string displayName, string category, string key, string modifiers, string? description = null)
    {
        var gesture = new KeyGesture(key, modifiers);
        _hotKeys[action] = gesture;
        _shortcutInfo[action] = new ShortcutInfo(action, displayName, category, gesture, description);
    }

    public void RegisterHotKey(string action, KeyGesture gesture)
    {
        _hotKeys[action] = gesture;
    }

    public void UnregisterHotKey(string action)
    {
        _hotKeys.Remove(action);
    }

    public void RegisterAction(string actionName, Action callback)
    {
        _actions[actionName] = callback;
    }

    public void UnregisterAction(string actionName)
    {
        _actions.Remove(actionName);
    }

    public bool HandleKeyPress(string key, string modifiers)
    {
        // Find matching action
        var action = _hotKeys.FirstOrDefault(kvp =>
            kvp.Value.Key.Equals(key, StringComparison.OrdinalIgnoreCase) &&
            kvp.Value.Modifiers.Equals(modifiers, StringComparison.OrdinalIgnoreCase)
        ).Key;

        if (!string.IsNullOrEmpty(action) && _actions.TryGetValue(action, out var callback))
        {
            try
            {
                callback?.Invoke();
                return true; // Handled
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                Console.WriteLine($"Error executing hotkey action '{action}': {ex.Message}");
                return false;
            }
        }

        return false; // Not handled
    }

    public Dictionary<string, KeyGesture> GetAllHotKeys()
    {
        return new Dictionary<string, KeyGesture>(_hotKeys);
    }

    public List<ShortcutInfo> GetShortcutsByCategory(string category)
    {
        return _shortcutInfo.Values
            .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.DisplayName)
            .ToList();
    }

    public List<string> GetAllCategories()
    {
        return _shortcutInfo.Values
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public void LoadCustomBindings(string? filePath = null)
    {
        var path = filePath ?? _defaultConfigPath;

        if (!File.Exists(path))
            return;

        try
        {
            var json = File.ReadAllText(path);
            var customBindings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (customBindings == null)
                return;

            foreach (var (action, gestureString) in customBindings)
            {
                if (_hotKeys.ContainsKey(action))
                {
                    try
                    {
                        var gesture = KeyGesture.Parse(gestureString);
                        _hotKeys[action] = gesture;

                        // Update shortcut info if it exists
                        if (_shortcutInfo.TryGetValue(action, out var info))
                        {
                            _shortcutInfo[action] = info with { Gesture = gesture };
                        }
                    }
                    catch
                    {
                        // Invalid gesture, skip
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading custom shortcuts: {ex.Message}");
        }
    }

    public void SaveCustomBindings(string? filePath = null)
    {
        var path = filePath ?? _defaultConfigPath;

        try
        {
            var bindings = _hotKeys.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToString()
            );

            var json = JsonSerializer.Serialize(bindings, JsonOptions);

            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving custom shortcuts: {ex.Message}");
        }
    }

    public void ResetToDefaults()
    {
        _hotKeys.Clear();
        _shortcutInfo.Clear();
        RegisterDefaultHotKeys();
    }

    public bool HasConflict(KeyGesture gesture, out string? conflictingAction)
    {
        conflictingAction = _hotKeys.FirstOrDefault(kvp =>
            kvp.Value.Key.Equals(gesture.Key, StringComparison.OrdinalIgnoreCase) &&
            kvp.Value.Modifiers.Equals(gesture.Modifiers, StringComparison.OrdinalIgnoreCase)
        ).Key;

        return !string.IsNullOrEmpty(conflictingAction);
    }
}
