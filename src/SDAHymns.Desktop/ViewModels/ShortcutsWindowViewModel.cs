using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Services;

namespace SDAHymns.Desktop.ViewModels;

public partial class ShortcutsWindowViewModel : ObservableObject
{
    private readonly IHotKeyManager _hotKeyManager;
    private readonly Dictionary<string, KeyGesture> _originalShortcuts = new();

    [ObservableProperty]
    private ObservableCollection<ShortcutGroup> _shortcutGroups = [];

    [ObservableProperty]
    private ShortcutDisplay? _listeningShortcut;

    [ObservableProperty]
    private bool _hasChanges = false;

    [ObservableProperty]
    private string? _swapMessage;

    public ShortcutsWindowViewModel(IHotKeyManager hotKeyManager)
    {
        _hotKeyManager = hotKeyManager;
        LoadShortcuts();
    }

    private void LoadShortcuts()
    {
        var categories = _hotKeyManager.GetAllCategories();
        var groups = new ObservableCollection<ShortcutGroup>();

        foreach (var category in categories)
        {
            var shortcuts = _hotKeyManager.GetShortcutsByCategory(category)
                .Select(s =>
                {
                    var display = new ShortcutDisplay(this, s.Action, s.Gesture.ToString(), s.DisplayName, s.Description ?? string.Empty);
                    _originalShortcuts[s.Action] = s.Gesture;
                    return display;
                })
                .ToList();

            groups.Add(new ShortcutGroup(category, new ObservableCollection<ShortcutDisplay>(shortcuts)));
        }

        ShortcutGroups = groups;
    }

    public void StartListening(ShortcutDisplay shortcut)
    {
        // Stop listening to previous
        if (ListeningShortcut != null)
        {
            ListeningShortcut.IsListening = false;
        }

        ListeningShortcut = shortcut;
        shortcut.IsListening = true;
    }

    public void CaptureKey(string key, string modifiers)
    {
        if (ListeningShortcut == null)
            return;

        var newShortcut = string.IsNullOrEmpty(modifiers) ? key : $"{modifiers}+{key}";
        ListeningShortcut.Shortcut = newShortcut;
        ListeningShortcut.IsListening = false;
        ListeningShortcut = null;

        HasChanges = true;
        ValidateAndBuildSwapMessage();
    }

    [RelayCommand]
    private void SaveChanges()
    {
        // Apply changes to manager
        var allShortcuts = ShortcutGroups.SelectMany(g => g.Shortcuts).ToList();
        foreach (var shortcut in allShortcuts)
        {
            var gesture = KeyGesture.Parse(shortcut.Shortcut);
            _hotKeyManager.RegisterHotKey(shortcut.Action, gesture);
        }

        // Save to disk
        _hotKeyManager.SaveCustomBindings();

        // Update originals
        _originalShortcuts.Clear();
        foreach (var shortcut in allShortcuts)
        {
            _originalShortcuts[shortcut.Action] = KeyGesture.Parse(shortcut.Shortcut);
        }

        HasChanges = false;
        SwapMessage = null;
    }

    [RelayCommand]
    private void CancelChanges()
    {
        // Restore originals
        foreach (var group in ShortcutGroups)
        {
            foreach (var shortcut in group.Shortcuts)
            {
                if (_originalShortcuts.TryGetValue(shortcut.Action, out var original))
                {
                    shortcut.Shortcut = original.ToString();
                }
            }
        }

        HasChanges = false;
        SwapMessage = null;
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        _hotKeyManager.ResetToDefaults();
        _originalShortcuts.Clear();
        ShortcutGroups.Clear();
        LoadShortcuts();
    }

    private void ValidateAndBuildSwapMessage()
    {
        var allShortcuts = ShortcutGroups.SelectMany(g => g.Shortcuts).ToList();
        var duplicates = allShortcuts
            .GroupBy(s => s.Shortcut)
            .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key))
            .ToList();

        if (!duplicates.Any())
        {
            SwapMessage = null;
            return;
        }

        // Build swap message
        var sb = new StringBuilder();
        sb.AppendLine("The following shortcuts will be swapped when you save:");
        sb.AppendLine();

        foreach (var group in duplicates)
        {
            var shortcuts = group.ToList();
            for (int i = 0; i < shortcuts.Count - 1; i++)
            {
                sb.AppendLine($"• '{shortcuts[i].ActionName}' ⇄ '{shortcuts[i + 1].ActionName}' ({group.Key})");
            }
        }

        SwapMessage = sb.ToString();
    }
}

public record ShortcutGroup(string Category, ObservableCollection<ShortcutDisplay> Shortcuts);

public partial class ShortcutDisplay : ObservableObject
{
    public string Action { get; }

    [ObservableProperty]
    private string _shortcut;

    public string ActionName { get; }
    public string Description { get; }

    [ObservableProperty]
    private bool _isListening;

    public ShortcutDisplay(ShortcutsWindowViewModel parent, string action, string shortcut, string actionName, string description)
    {
        Action = action;
        _shortcut = shortcut;
        ActionName = actionName;
        Description = description;
    }
}
