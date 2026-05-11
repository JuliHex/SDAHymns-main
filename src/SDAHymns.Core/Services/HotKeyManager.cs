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
        Register("FocusSearch", "Focalizează căutarea", "Global", "F", "Ctrl", "Focalizează caseta de căutare pentru a găsi imnuri");
        Register("ClearSearch", "Șterge căutarea / Închide dialogul", "Global", "Escape", "", "Șterge căutarea sau închide dialogurile deschise");
        Register("ToggleDisplay", "Comută fereastra de afișaj", "Global", "F5", "", "Arată sau ascunde fereastra de afișaj");
        Register("ToggleFullscreen", "Comută ecran complet", "Global", "F11", "", "Comută modul ecran complet pe fereastra de afișaj");
        Register("OpenSettings", "Deschide Setări", "Global", "OemComma", "Ctrl", "Deschide setările aplicației");
        Register("QuitApp", "Închide aplicația", "Global", "Q", "Ctrl", "Închide aplicația");

        // Hymn navigation
        Register("NextVerse", "Strofa următoare", "Navigare", "Space", "", "Trece la strofa următoare");
        Register("PreviousVerse", "Strofa anterioară", "Navigare", "Space", "Shift", "Trece la strofa anterioară");
        Register("NextVerseArrow", "Strofa următoare (Săgeată)", "Navigare", "Right", "", "Trece la strofa următoare");
        Register("PreviousVerseArrow", "Strofa anterioară (Săgeată)", "Navigare", "Left", "", "Trece la strofa anterioară");
        Register("NextVersePage", "Strofa următoare (Page Down)", "Navigare", "PageDown", "", "Trece la strofa următoare");
        Register("PreviousVersePage", "Strofa anterioară (Page Up)", "Navigare", "PageUp", "", "Trece la strofa anterioară");
        Register("FirstVerse", "Prima strofă", "Navigare", "Home", "", "Sari la prima strofă");
        Register("LastVerse", "Ultima strofă", "Navigare", "End", "", "Sari la ultima strofă");

        // Search & selection
        Register("SelectHymn", "Selectează Imnul", "Căutare", "Enter", "", "Încarcă imnul selectat din rezultatele căutării");
        Register("ToggleFavorite", "Comută Favorit", "Căutare", "D", "Ctrl", "Marchează imnul curent ca favorit");

        // Recent hymns (Ctrl+1 through Ctrl+5)
        Register("LoadRecent1", "Încarcă Imnul Recent 1", "Căutare", "D1", "Ctrl", "Încarcă primul imn recent");
        Register("LoadRecent2", "Încarcă Imnul Recent 2", "Căutare", "D2", "Ctrl", "Încarcă al doilea imn recent");
        Register("LoadRecent3", "Încarcă Imnul Recent 3", "Căutare", "D3", "Ctrl", "Încarcă al treilea imn recent");
        Register("LoadRecent4", "Încarcă Imnul Recent 4", "Căutare", "D4", "Ctrl", "Încarcă al patrulea imn recent");
        Register("LoadRecent5", "Încarcă Imnul Recent 5", "Căutare", "D5", "Ctrl", "Încarcă al cincilea imn recent");

        // Display control
        Register("BlankDisplay", "Ecran Negru", "Afișaj", "B", "", "Arată un ecran negru pe afișaj");
        Register("ShowHelpOverlay", "Arată Comenzile Rapide", "Global", "F1", "", "Arată suprapunerea cu comenzile rapide de la tastatură");
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
