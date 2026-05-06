namespace SDAHymns.Core.Data.Models;

/// <summary>
/// Represents a display profile with customizable fonts, colors, backgrounds, and layout settings.
/// Used to switch between different projection scenarios (projector, OBS, practice, etc.)
/// </summary>
public class DisplayProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsSystemProfile { get; set; } = false;  // Cannot be deleted
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Typography
    public string FontFamily { get; set; } = "Inter";
    public int TitleFontSize { get; set; } = 36;
    public int VerseFontSize { get; set; } = 48;
    public int LabelFontSize { get; set; } = 28;
    public string FontWeight { get; set; } = "Normal"; // Normal, SemiBold, Bold
    public double LineHeight { get; set; } = 1.4;
    public double LetterSpacing { get; set; } = 0;

    // Colors (stored as hex strings like "#FFFFFF")
    public string BackgroundColor { get; set; } = "#000000";
    public string TextColor { get; set; } = "#FFFFFF";
    public string TitleColor { get; set; } = "#FFFFFF";
    public string LabelColor { get; set; } = "#CCCCCC";
    public string AccentColor { get; set; } = "#0078D4";

    // Background
    public double BackgroundOpacity { get; set; } = 1.0;
    public string? BackgroundImagePath { get; set; }
    public string BackgroundImageMode { get; set; } = "Fill"; // Fill, Fit, Stretch, Tile
    public double BackgroundImageOpacity { get; set; } = 0.3;

    // Layout
    public string TextAlignment { get; set; } = "Left"; // Left, Center, Right
    public string VerticalAlignment { get; set; } = "Center"; // Top, Center, Bottom
    public int MarginLeft { get; set; } = 100;
    public int MarginRight { get; set; } = 100;
    public int MarginTop { get; set; } = 60;
    public int MarginBottom { get; set; } = 60;

    // Effects
    public bool EnableTextShadow { get; set; } = false;
    public string ShadowColor { get; set; } = "#000000";
    public int ShadowBlurRadius { get; set; } = 10;
    public int ShadowOffsetX { get; set; } = 2;
    public int ShadowOffsetY { get; set; } = 2;

    public bool EnableTextOutline { get; set; } = false;
    public string OutlineColor { get; set; } = "#000000";
    public int OutlineThickness { get; set; } = 2;

    // Advanced
    public bool TransparentBackground { get; set; } = false;
    public bool ShowVerseNumbers { get; set; } = true;
    public bool ShowHymnTitle { get; set; } = true;
}
