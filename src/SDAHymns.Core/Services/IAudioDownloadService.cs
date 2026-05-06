using SDAHymns.Core.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for downloading and managing audio packages
/// </summary>
public interface IAudioDownloadService
{
    // Manifest management
    Task<AudioPackageManifest> FetchManifestAsync(CancellationToken cancellationToken = default);
    Task<List<CategoryPackage>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default);
    Task<List<InstalledPackage>> GetInstalledPackagesAsync();

    // Download operations
    Task DownloadCategoryAsync(string categorySlug, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default);
    Task DownloadAllAsync(IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(string categorySlug, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default);

    // Verification
    Task<bool> VerifyCategoryAsync(string categorySlug, IProgress<int>? progress = null);
}
