using SDAHymns.Core.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for managing local audio library
/// </summary>
public interface IAudioLibraryService
{
    // File discovery
    Task<bool> AudioFileExistsAsync(int hymnNumber, string category);
    Task<string?> GetAudioFilePathAsync(int hymnNumber, string category);
    Task<List<InstalledAudioFile>> GetInstalledFilesAsync(string category);
    Task<List<string>> GetInstalledCategoriesAsync();

    // Metadata
    Task<AudioMetadata?> GetMetadataAsync(int hymnNumber, string category);
    Task SaveMetadataAsync(AudioMetadata metadata);

    // Validation
    Task<LibraryHealth> ScanLibraryHealthAsync();
    Task<LibraryHealth> ScanCategoryHealthAsync(string category);
    Task<long> GetTotalLibrarySizeAsync();
    Task<long> GetCategorySizeAsync(string category);

    // Management
    Task<bool> DeleteCategoryAsync(string category);
    Task<bool> MigrateLibraryAsync(string newPath, IProgress<int>? progress = null);
}
