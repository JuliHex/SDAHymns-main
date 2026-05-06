namespace SDAHymns.Core.Data.Models;

public class AudioRecording
{
    public int Id { get; set; }

    // Hymn relationship
    public int HymnId { get; set; }
    public Hymn Hymn { get; set; } = null!;

    // File information
    public required string FilePath { get; set; }  // Relative path
    public string FileFormat { get; set; } = "mp3";  // mp3, opus, etc.
    public long FileSizeBytes { get; set; }

    // Recording metadata
    public DateTime? RecordingDate { get; set; }
    public string? Tempo { get; set; }  // "moderato", "allegro", etc.
    public int DurationSeconds { get; set; }
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }

    // Playback settings
    public double DefaultPlaybackSpeed { get; set; } = 1.0;
    public float VolumeOffset { get; set; } = 0.0f;  // Per-track volume normalization (-1.0 to 1.0)

    // Synchronization
    // JSON format: { "1": 12.5, "2": 45.2, "3": 88.0 }
    // Key: Verse Number, Value: Seconds from start
    public string? TimingMapJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
