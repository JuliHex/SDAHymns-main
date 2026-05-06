namespace SDAHymns.Core.Services;

public interface IHotKeyManager
{
    void RegisterHotKey(string action, KeyGesture gesture);
    void UnregisterHotKey(string action);
    void RegisterAction(string actionName, Action callback);
    void UnregisterAction(string actionName);
    bool HandleKeyPress(string key, string modifiers);
    Dictionary<string, KeyGesture> GetAllHotKeys();
    List<ShortcutInfo> GetShortcutsByCategory(string category);
    List<string> GetAllCategories();
    void LoadCustomBindings(string? filePath = null);
    void SaveCustomBindings(string? filePath = null);
    void ResetToDefaults();
    bool HasConflict(KeyGesture gesture, out string? conflictingAction);
}

public record KeyGesture(string Key, string Modifiers)
{
    public override string ToString()
    {
        if (string.IsNullOrEmpty(Modifiers))
            return Key;

        return $"{Modifiers}+{Key}";
    }

    public static KeyGesture Parse(string gestureString)
    {
        var parts = gestureString.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var modifiersList = new List<string>();
        var key = string.Empty;

        foreach (var part in parts)
        {
            var normalized = part.ToLowerInvariant();
            if (normalized is "ctrl" or "control" or "alt" or "shift" or "win" or "meta" or "cmd")
            {
                // Normalize to standard form
                modifiersList.Add(normalized switch
                {
                    "control" => "Ctrl",
                    "ctrl" => "Ctrl",
                    "alt" => "Alt",
                    "shift" => "Shift",
                    "win" or "meta" or "cmd" => "Meta",
                    _ => normalized
                });
            }
            else
            {
                key = part;
            }
        }

        var modifiers = string.Join("+", modifiersList.OrderBy(m => m));
        return new KeyGesture(key, modifiers);
    }
}

public record ShortcutInfo(
    string Action,
    string DisplayName,
    string Category,
    KeyGesture Gesture,
    string? Description = null
);
