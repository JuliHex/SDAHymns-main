using SDAHymns.Core.Services;
using Xunit;

namespace SDAHymns.Tests.Services;

public class HotKeyManagerTests
{
    private readonly HotKeyManager _hotKeyManager;

    public HotKeyManagerTests()
    {
        _hotKeyManager = new HotKeyManager();
    }

    [Fact]
    public void Constructor_RegistersDefaultHotKeys()
    {
        // Act
        var hotKeys = _hotKeyManager.GetAllHotKeys();

        // Assert
        Assert.NotEmpty(hotKeys);
        Assert.Contains("FocusSearch", hotKeys.Keys);
        Assert.Contains("NextVerse", hotKeys.Keys);
        Assert.Contains("PreviousVerse", hotKeys.Keys);
    }

    [Fact]
    public void RegisterHotKey_AddsNewHotKey()
    {
        // Arrange
        var gesture = new KeyGesture("A", "Ctrl");

        // Act
        _hotKeyManager.RegisterHotKey("TestAction", gesture);
        var hotKeys = _hotKeyManager.GetAllHotKeys();

        // Assert
        Assert.Contains("TestAction", hotKeys.Keys);
        Assert.Equal("A", hotKeys["TestAction"].Key);
        Assert.Equal("Ctrl", hotKeys["TestAction"].Modifiers);
    }

    [Fact]
    public void UnregisterHotKey_RemovesHotKey()
    {
        // Arrange
        var gesture = new KeyGesture("B", "Alt");
        _hotKeyManager.RegisterHotKey("TempAction", gesture);

        // Act
        _hotKeyManager.UnregisterHotKey("TempAction");
        var hotKeys = _hotKeyManager.GetAllHotKeys();

        // Assert
        Assert.DoesNotContain("TempAction", hotKeys.Keys);
    }

    [Fact]
    public void HandleKeyPress_ExecutesRegisteredAction()
    {
        // Arrange
        var executed = false;
        _hotKeyManager.RegisterAction("TestAction", () => executed = true);
        _hotKeyManager.RegisterHotKey("TestAction", new KeyGesture("T", "Ctrl"));

        // Act
        var handled = _hotKeyManager.HandleKeyPress("T", "Ctrl");

        // Assert
        Assert.True(handled);
        Assert.True(executed);
    }

    [Fact]
    public void HandleKeyPress_ReturnsFalseForUnregisteredKey()
    {
        // Act
        var handled = _hotKeyManager.HandleKeyPress("Z", "Ctrl+Alt+Shift");

        // Assert
        Assert.False(handled);
    }

    [Fact]
    public void HandleKeyPress_IsCaseInsensitive()
    {
        // Arrange
        var executed = false;
        _hotKeyManager.RegisterAction("TestAction", () => executed = true);
        _hotKeyManager.RegisterHotKey("TestAction", new KeyGesture("X", "Ctrl"));

        // Act - different case
        var handled = _hotKeyManager.HandleKeyPress("x", "ctrl");

        // Assert
        Assert.True(handled);
        Assert.True(executed);
    }

    [Fact]
    public void RegisterAction_AllowsActionOverwrite()
    {
        // Arrange
        var counter = 0;
        _hotKeyManager.RegisterAction("TestAction", () => counter = 1);
        _hotKeyManager.RegisterAction("TestAction", () => counter = 2);
        _hotKeyManager.RegisterHotKey("TestAction", new KeyGesture("Y", ""));

        // Act
        _hotKeyManager.HandleKeyPress("Y", "");

        // Assert
        Assert.Equal(2, counter);
    }

    [Fact]
    public void GetShortcutsByCategory_ReturnsCorrectShortcuts()
    {
        // Act
        var globalShortcuts = _hotKeyManager.GetShortcutsByCategory("Global");
        var navigationShortcuts = _hotKeyManager.GetShortcutsByCategory("Navigation");

        // Assert
        Assert.NotEmpty(globalShortcuts);
        Assert.NotEmpty(navigationShortcuts);
        Assert.Contains(globalShortcuts, s => s.Action == "FocusSearch");
        Assert.Contains(navigationShortcuts, s => s.Action == "NextVerse");
    }

    [Fact]
    public void GetAllCategories_ReturnsAllCategories()
    {
        // Act
        var categories = _hotKeyManager.GetAllCategories();

        // Assert
        Assert.NotEmpty(categories);
        Assert.Contains("Global", categories);
        Assert.Contains("Navigation", categories);
        Assert.Contains("Search", categories);
    }

    [Fact]
    public void HasConflict_DetectsConflictingGesture()
    {
        // Arrange
        var existingGesture = new KeyGesture("F", "Ctrl"); // FocusSearch uses this

        // Act
        var hasConflict = _hotKeyManager.HasConflict(existingGesture, out var conflictingAction);

        // Assert
        Assert.True(hasConflict);
        Assert.Equal("FocusSearch", conflictingAction);
    }

    [Fact]
    public void HasConflict_ReturnsFalseForUniqueGesture()
    {
        // Arrange
        var uniqueGesture = new KeyGesture("Z", "Ctrl+Alt+Shift");

        // Act
        var hasConflict = _hotKeyManager.HasConflict(uniqueGesture, out var conflictingAction);

        // Assert
        Assert.False(hasConflict);
        Assert.Null(conflictingAction);
    }

    [Fact]
    public void ResetToDefaults_RestoresDefaultHotKeys()
    {
        // Arrange
        _hotKeyManager.RegisterHotKey("FocusSearch", new KeyGesture("Z", "Ctrl"));
        var modifiedHotKeys = _hotKeyManager.GetAllHotKeys();
        Assert.Equal("Z", modifiedHotKeys["FocusSearch"].Key);

        // Act
        _hotKeyManager.ResetToDefaults();
        var resetHotKeys = _hotKeyManager.GetAllHotKeys();

        // Assert
        Assert.Equal("F", resetHotKeys["FocusSearch"].Key);
        Assert.Equal("Ctrl", resetHotKeys["FocusSearch"].Modifiers);
    }

    [Theory]
    [InlineData("Ctrl+F", "F", "Ctrl")]
    [InlineData("Alt+Enter", "Enter", "Alt")]
    [InlineData("Shift+Space", "Space", "Shift")]
    [InlineData("Space", "Space", "")]
    [InlineData("F1", "F1", "")]
    public void KeyGesture_Parse_ParsesCorrectly(string input, string expectedKey, string expectedModifiers)
    {
        // Act
        var gesture = KeyGesture.Parse(input);

        // Assert
        Assert.Equal(expectedKey, gesture.Key);
        Assert.Equal(expectedModifiers, gesture.Modifiers);
    }

    [Theory]
    [InlineData("F", "Ctrl", "Ctrl+F")]
    [InlineData("Enter", "Alt", "Alt+Enter")]
    [InlineData("Space", "", "Space")]
    [InlineData("F11", "", "F11")]
    public void KeyGesture_ToString_FormatsCorrectly(string key, string modifiers, string expected)
    {
        // Arrange
        var gesture = new KeyGesture(key, modifiers);

        // Act
        var result = gesture.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SaveAndLoadCustomBindings_PersistsChanges()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-shortcuts-{Guid.NewGuid()}.json");
        _hotKeyManager.RegisterHotKey("FocusSearch", new KeyGesture("G", "Ctrl"));

        // Act - Save
        _hotKeyManager.SaveCustomBindings(tempFile);

        // Create new manager and load
        var newManager = new HotKeyManager();
        newManager.LoadCustomBindings(tempFile);
        var loadedHotKeys = newManager.GetAllHotKeys();

        // Assert
        Assert.Equal("G", loadedHotKeys["FocusSearch"].Key);

        // Cleanup
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }

    [Fact]
    public void LoadCustomBindings_SkipsInvalidFile()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.json");

        // Act & Assert - Should not throw
        _hotKeyManager.LoadCustomBindings(nonExistentFile);
        var hotKeys = _hotKeyManager.GetAllHotKeys();

        // Should still have defaults
        Assert.NotEmpty(hotKeys);
    }

    [Fact]
    public void UnregisterAction_RemovesAction()
    {
        // Arrange
        var executed = false;
        _hotKeyManager.RegisterAction("TestAction", () => executed = true);
        _hotKeyManager.RegisterHotKey("TestAction", new KeyGesture("K", ""));

        // Act
        _hotKeyManager.UnregisterAction("TestAction");
        var handled = _hotKeyManager.HandleKeyPress("K", "");

        // Assert
        Assert.False(handled); // Not handled because action was removed
        Assert.False(executed);
    }
}
