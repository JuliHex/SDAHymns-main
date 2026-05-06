using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for managing application settings
/// </summary>
public interface ISettingsService
{
    // AppSettings CRUD
    Task<AppSettings> GetAppSettingsAsync();
    Task UpdateAppSettingsAsync(AppSettings settings);

    // Audio settings convenience methods
    Task<string> GetAudioLibraryPathAsync();
    Task SetAudioLibraryPathAsync(string path);
    Task<string?> GetAudioOutputDeviceIdAsync();
    Task SetAudioOutputDeviceIdAsync(string? deviceId);
    Task<int> GetAutoPlayDelayAsync();
    Task SetAutoPlayDelayAsync(int seconds);
    Task<float> GetGlobalVolumeAsync();
    Task SetGlobalVolumeAsync(float volume);
    Task<bool> GetAutoAdvanceEnabledAsync();
    Task SetAutoAdvanceEnabledAsync(bool enabled);

    // General settings
    Task<string> GetLanguageAsync();
    Task SetLanguageAsync(string language);
    Task<string> GetThemeAsync();
    Task SetThemeAsync(string theme);

    // Display settings
    Task<int?> GetActiveDisplayProfileIdAsync();
    Task SetActiveDisplayProfileIdAsync(int? profileId);
    Task<bool> GetIsAspectRatio43Async();
    Task SetIsAspectRatio43Async(bool is43);

    // Window state
    Task<string?> GetLastWindowPositionAsync();
    Task SetLastWindowPositionAsync(string? position);
    Task<string?> GetLastWindowSizeAsync();
    Task SetLastWindowSizeAsync(string? size);

    // Remote widget settings
    Task<RemoteWidgetSettings> LoadRemoteWidgetSettingsAsync();
    Task SaveRemoteWidgetSettingsAsync(RemoteWidgetSettings settings);
}
