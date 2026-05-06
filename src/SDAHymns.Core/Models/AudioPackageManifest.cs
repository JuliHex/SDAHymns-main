namespace SDAHymns.Core.Models;

/// <summary>
/// Represents the manifest file describing available audio packages
/// </summary>
public class AudioPackageManifest
{
    public string Version { get; set; } = "1.0.0";
    public DateTime ReleaseDate { get; set; }
    public Dictionary<string, CategoryPackage> Categories { get; set; } = new();
}

/// <summary>
/// Represents an audio package for a specific category
/// </summary>
public class CategoryPackage
{
    public string DisplayName { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public long TotalSize { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string? FallbackUrl { get; set; }
    public string Checksum { get; set; } = string.Empty;  // SHA256
    public bool Required { get; set; } = false;
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Represents the installation status of a category pack
/// </summary>
public class InstalledPackage
{
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public long TotalSize { get; set; }
    public DateTime InstalledDate { get; set; }
    public string Version { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = true;
}

/// <summary>
/// Download progress information
/// </summary>
public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete => TotalBytes > 0 ? (int)((BytesDownloaded * 100) / TotalBytes) : 0;
    public string CurrentFile { get; set; } = string.Empty;
    public DownloadState State { get; set; } = DownloadState.Idle;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Download state enumeration
/// </summary>
public enum DownloadState
{
    Idle,
    FetchingManifest,
    Downloading,
    Extracting,
    Verifying,
    Complete,
    Failed
}

/// <summary>
/// Library health check result
/// </summary>
public class LibraryHealth
{
    public int TotalFiles { get; set; }
    public int CorruptedFiles { get; set; }
    public int MissingMetadata { get; set; }
    public List<string> Issues { get; set; } = new();
    public bool IsHealthy => CorruptedFiles == 0 && Issues.Count == 0;
}

/// <summary>
/// Represents an installed audio file
/// </summary>
public class InstalledAudioFile
{
    public int HymnNumber { get; set; }
    public string Category { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public bool HasMetadata { get; set; }
}

/// <summary>
/// Audio file metadata
/// </summary>
public class AudioMetadata
{
    public int HymnNumber { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime? RecordingDate { get; set; }
    public string? Tempo { get; set; }
    public int DurationSeconds { get; set; }
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }
}
