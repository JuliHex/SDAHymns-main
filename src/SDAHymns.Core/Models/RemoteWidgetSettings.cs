namespace SDAHymns.Core.Models;

/// <summary>
/// Settings for the compact remote control widget window.
/// Stores position, lock state, and display preferences.
/// </summary>
public class RemoteWidgetSettings
{
    /// <summary>
    /// X position of the widget window. NaN = use default position.
    /// </summary>
    public double PositionX { get; set; } = double.NaN;

    /// <summary>
    /// Y position of the widget window. NaN = use default position.
    /// </summary>
    public double PositionY { get; set; } = double.NaN;

    /// <summary>
    /// Whether the widget position is locked (cannot be dragged).
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Whether the widget should stay on top of all other windows.
    /// </summary>
    public bool AlwaysOnTop { get; set; } = true;

    /// <summary>
    /// Whether to show the number pad for hymn input.
    /// </summary>
    public bool ShowNumberPad { get; set; } = true;

    /// <summary>
    /// The last hymn number loaded in the widget.
    /// </summary>
    public int LastHymnNumber { get; set; } = 0;

    /// <summary>
    /// The last hymn category used.
    /// </summary>
    public string LastCategory { get; set; } = "crestine";

    /// <summary>
    /// Quick slots for hymn numbers.
    /// </summary>
    public List<int> QuickSlots { get; set; } = new List<int> { 0, 0, 0, 0 };

    /// <summary>
    /// Custom labels for quick slots (e.g., "Opening", "Choir").
    /// </summary>
    public List<string> QuickSlotLabels { get; set; } = new List<string> { "", "", "", "" };
}
