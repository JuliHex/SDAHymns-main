using System.Text.Json;
using SDAHymns.Core.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for managing local audio library
/// </summary>
public class AudioLibraryService : IAudioLibraryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly ISettingsService _settingsService;

    public AudioLibraryService(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task<bool> AudioFileExistsAsync(int hymnNumber, string category)
    {
        var filePath = await GetAudioFilePathAsync(hymnNumber, category);
        return filePath != null && File.Exists(filePath);
    }

    public async Task<string?> GetAudioFilePathAsync(int hymnNumber, string category)
    {
        var libraryPath = await _settingsService.GetAudioLibraryPathAsync();
        var fileName = $"{hymnNumber:D3}.mp3";  // e.g., 001.mp3
        var fullPath = Path.Combine(libraryPath, category, fileName);

        return File.Exists(fullPath) ? fullPath : null;
    }

    public async Task<List<InstalledAudioFile>> GetInstalledFilesAsync(string category)
    {
        var libraryPath = await _settingsService.GetAudioLibraryPathAsync();
        var categoryPath = Path.Combine(libraryPath, category);

        if (!Directory.Exists(categoryPath))
        {
            return new List<InstalledAudioFile>();
        }

        var files = new List<InstalledAudioFile>();
        var mp3Files = Directory.GetFiles(categoryPath, "*.mp3");

        foreach (var filePath in mp3Files)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (int.TryParse(fileName, out var hymnNumber))
            {
                var fileInfo = new FileInfo(filePath);
                var metadataPath = Path.Combine(categoryPath, $"{fileName}.metadata.json");

                files.Add(new InstalledAudioFile
                {
                    HymnNumber = hymnNumber,
                    Category = category,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    HasMetadata = File.Exists(metadataPath)
                });
            }
        }

        return files.OrderBy(f => f.HymnNumber).ToList();
    }

    public async Task<List<string>> GetInstalledCategoriesAsync()
    {
        var libraryPath = await _settingsService.GetAudioLibraryPathAsync();

        if (!Directory.Exists(libraryPath))
        {
            return new List<string>();
        }

        var categories = new List<string>();
        var subdirs = Directory.GetDirectories(libraryPath);

        foreach (var dir in subdirs)
        {
            var categoryName = Path.GetFileName(dir);
            var mp3Count = Directory.GetFiles(dir, "*.mp3").Length;

            if (mp3Count > 0)
            {
                categories.Add(categoryName);
            }
        }

        return categories;
    }

    public async Task<AudioMetadata?> GetMetadataAsync(int hymnNumber, string category)
    {
        var libraryPath = await _settingsService.GetAudioLibraryPathAsync();
        var metadataPath = Path.Combine(libraryPath, category, $"{hymnNumber:D3}.metadata.json");

        if (!File.Exists(metadataPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(metadataPath);
            return JsonSerializer.Deserialize<AudioMetadata>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveMetadataAsync(AudioMetadata metadata)
    {
        var libraryPath = await _settingsService.GetAudioLibraryPathAsync();
        var categoryPath = Path.Combine(libraryPath, metadata.Category);

        if (!Directory.Exists(categoryPath))
        {
            Directory.CreateDirectory(categoryPath);
        }

        var metadataPath = Path.Combine(categoryPath, $"{metadata.HymnNumber:D3}.metadata.json");
        var json = JsonSerializer.Serialize(metadata, JsonOptions);
        await File.WriteAllTextAsync(metadataPath, json);
    }

    public async Task<LibraryHealth> ScanLibraryHealthAsync()
    {
        var health = new LibraryHealth();
        var categories = await GetInstalledCategoriesAsync();

        foreach (var category in categories)
        {
            var categoryHealth = await ScanCategoryHealthAsync(category);
            health.TotalFiles += categoryHealth.TotalFiles;
            health.CorruptedFiles += categoryHealth.CorruptedFiles;
            health.MissingMetadata += categoryHealth.MissingMetadata;
            health.Issues.AddRange(categoryHealth.Issues);
        }

        return health;
    }

    public async Task<LibraryHealth> ScanCategoryHealthAsync(string category)
    {
        var health = new LibraryHealth();
        var files = await GetInstalledFilesAsync(category);

        health.TotalFiles = files.Count;

        foreach (var file in files)
        {
            // Check if file is readable (basic corruption check)
            try
            {
                using var stream = File.OpenRead(file.FilePath);
                if (stream.Length == 0)
                {
                    health.CorruptedFiles++;
                    health.Issues.Add($"{category}/{file.HymnNumber:D3}.mp3: Empty file");
                }
            }
            catch (Exception ex)
            {
                health.CorruptedFiles++;
                health.Issues.Add($"{category}/{file.HymnNumber:D3}.mp3: {ex.Message}");
            }

            // Check for missing metadata
            if (!file.HasMetadata)
            {
                health.MissingMetadata++;
            }
        }

        return health;
    }

    public async Task<long> GetTotalLibrarySizeAsync()
    {
        var categories = await GetInstalledCategoriesAsync();
        long totalSize = 0;

        foreach (var category in categories)
        {
            totalSize += await GetCategorySizeAsync(category);
        }

        return totalSize;
    }

    public async Task<long> GetCategorySizeAsync(string category)
    {
        var files = await GetInstalledFilesAsync(category);
        return files.Sum(f => f.FileSize);
    }

    public async Task<bool> DeleteCategoryAsync(string category)
    {
        var libraryPath = await _settingsService.GetAudioLibraryPathAsync();
        var categoryPath = Path.Combine(libraryPath, category);

        if (!Directory.Exists(categoryPath))
        {
            return false;
        }

        try
        {
            Directory.Delete(categoryPath, recursive: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MigrateLibraryAsync(string newPath, IProgress<int>? progress = null)
    {
        var oldPath = await _settingsService.GetAudioLibraryPathAsync();

        if (!Directory.Exists(oldPath))
        {
            // Nothing to migrate
            await _settingsService.SetAudioLibraryPathAsync(newPath);
            return true;
        }

        if (oldPath == newPath)
        {
            // Same path, nothing to do
            return true;
        }

        try
        {
            // Create new directory
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            // Get all files to migrate
            var allFiles = Directory.GetFiles(oldPath, "*.*", SearchOption.AllDirectories);
            var totalFiles = allFiles.Length;
            var filesCopied = 0;

            foreach (var sourceFile in allFiles)
            {
                var relativePath = Path.GetRelativePath(oldPath, sourceFile);
                var destFile = Path.Combine(newPath, relativePath);
                var destDir = Path.GetDirectoryName(destFile);

                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(sourceFile, destFile, overwrite: true);
                filesCopied++;

                // Report progress
                progress?.Report((filesCopied * 100) / totalFiles);
            }

            // Delete old directory first
            Directory.Delete(oldPath, recursive: true);

            // Only update settings after successful deletion (atomic operation)
            await _settingsService.SetAudioLibraryPathAsync(newPath);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
