using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Velopack;
using Velopack.Sources;

namespace SDAHymns.Core.Services;

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task<bool> DownloadUpdatesAsync(UpdateInfo updateInfo, IProgress<int>? progress = null);
    void ApplyUpdatesAndRestart(UpdateInfo updateInfo);
    bool IsUpdateAvailable { get; }
    string? LatestVersion { get; }
}

public class UpdateService : IUpdateService
{
    private readonly UpdateManager _updateManager;
    private readonly ILogger<UpdateService>? _logger;
    private UpdateInfo? _pendingUpdate;

    public bool IsUpdateAvailable => _pendingUpdate != null;
    public string? LatestVersion => _pendingUpdate?.TargetFullRelease.Version.ToString();

    /// <summary>
    /// Creates a new UpdateService
    /// </summary>
    /// <param name="options">Update configuration options</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    public UpdateService(IOptions<UpdateOptions> options, ILogger<UpdateService>? logger = null)
    {
        _logger = logger;

        // Configure GitHub Releases source from options
        var repoUrl = options.Value.GitHubRepoUrl;
        _updateManager = new UpdateManager(
            new GithubSource(repoUrl, null, false)
        );
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            _pendingUpdate = await _updateManager.CheckForUpdatesAsync();
            return _pendingUpdate;
        }
        catch (Exception ex)
        {
            // Log error (no internet, GitHub down, etc.)
            _logger?.LogWarning(ex, "Failed to check for updates. This is expected if there's no internet connection or GitHub is unreachable.");
            // Silently fail - app continues normally
            return null;
        }
    }

    public async Task<bool> DownloadUpdatesAsync(UpdateInfo updateInfo, IProgress<int>? progress = null)
    {
        // Guard clause - validate input
        if (updateInfo == null)
        {
            _logger?.LogWarning("DownloadUpdatesAsync called with null updateInfo");
            return false;
        }

        try
        {
            // Convert IProgress<int> to Action<int> for Velopack
            Action<int>? progressAction = progress != null
                ? (p => progress.Report(p))
                : null;

            await _updateManager.DownloadUpdatesAsync(updateInfo, progressAction);
            return true;
        }
        catch (Exception ex)
        {
            // Log download failure
            _logger?.LogError(ex, "Failed to download update package. Network issue or corrupted download.");
            return false;
        }
    }

    public void ApplyUpdatesAndRestart(UpdateInfo updateInfo)
    {
        _updateManager.ApplyUpdatesAndRestart(updateInfo);
    }
}
