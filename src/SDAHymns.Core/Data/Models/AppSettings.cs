namespace SDAHymns.Core.Data.Models;

public class AppSettings
{
    public int Id { get; set; }

    // Audio Configuration
    public string? AudioLibraryPath { get; set; }  // Path to local audio files folder
    public string? AudioOutputDeviceId { get; set; }  // Selected output device
    public int AudioAutoPlayDelay { get; set; } = 5;  // Seconds before auto-play starts
    public float GlobalVolume { get; set; } = 0.8f;  // 0.0 to 1.0
    public bool AutoAdvanceEnabled { get; set; } = false;  // Auto-advance slides with timings

    // General Configuration
    public string Language { get; set; } = "ro";
    public string Theme { get; set; } = "Dark";

    // Display Configuration
    public int? ActiveDisplayProfileId { get; set; }
    public DisplayProfile? ActiveDisplayProfile { get; set; }
    public bool IsAspectRatio43 { get; set; } = true;

    // Window State
    public string? LastWindowPosition { get; set; }  // JSON: { "x": 100, "y": 200 }
    public string? LastWindowSize { get; set; }  // JSON: { "width": 800, "height": 600 }
    public string? RemoteWidgetSettingsJson { get; set; }  // JSON: RemoteWidgetSettings

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
