using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Models;
using System.Text.Json;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for managing application settings
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly HymnsContext _context;
    private AppSettings? _cachedSettings;

    public SettingsService(HymnsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AppSettings> GetAppSettingsAsync()
    {
        if (_cachedSettings != null)
        {
            return _cachedSettings;
        }

        // Get the singleton settings record (ID = 1)
        _cachedSettings = await _context.AppSettings.FindAsync(1);

        if (_cachedSettings == null)
        {
            // Create default settings if none exist
            _cachedSettings = new AppSettings
            {
                Id = 1,
                AudioLibraryPath = GetDefaultAudioLibraryPath(),
                AudioAutoPlayDelay = 5,
                GlobalVolume = 0.8f,
                AutoAdvanceEnabled = false,
                Language = "ro",
                Theme = "Dark",
                IsAspectRatio43 = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AppSettings.Add(_cachedSettings);
            await _context.SaveChangesAsync();
        }

        return _cachedSettings;
    }

    public async Task UpdateAppSettingsAsync(AppSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _context.AppSettings.Update(settings);
        await _context.SaveChangesAsync();

        // Update cache
        _cachedSettings = settings;
    }

    public async Task<string> GetAudioLibraryPathAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.AudioLibraryPath ?? GetDefaultAudioLibraryPath();
    }

    public async Task SetAudioLibraryPathAsync(string path)
    {
        var settings = await GetAppSettingsAsync();
        settings.AudioLibraryPath = path;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<string?> GetAudioOutputDeviceIdAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.AudioOutputDeviceId;
    }

    public async Task SetAudioOutputDeviceIdAsync(string? deviceId)
    {
        var settings = await GetAppSettingsAsync();
        settings.AudioOutputDeviceId = deviceId;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<int> GetAutoPlayDelayAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.AudioAutoPlayDelay;
    }

    public async Task SetAutoPlayDelayAsync(int seconds)
    {
        var settings = await GetAppSettingsAsync();
        settings.AudioAutoPlayDelay = seconds;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<float> GetGlobalVolumeAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.GlobalVolume;
    }

    public async Task SetGlobalVolumeAsync(float volume)
    {
        var settings = await GetAppSettingsAsync();
        settings.GlobalVolume = Math.Clamp(volume, 0.0f, 1.0f);
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<bool> GetAutoAdvanceEnabledAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.AutoAdvanceEnabled;
    }

    public async Task SetAutoAdvanceEnabledAsync(bool enabled)
    {
        var settings = await GetAppSettingsAsync();
        settings.AutoAdvanceEnabled = enabled;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<int?> GetActiveDisplayProfileIdAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.ActiveDisplayProfileId;
    }

    public async Task SetActiveDisplayProfileIdAsync(int? profileId)
    {
        var settings = await GetAppSettingsAsync();
        settings.ActiveDisplayProfileId = profileId;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<string> GetLanguageAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.Language;
    }

    public async Task SetLanguageAsync(string language)
    {
        var settings = await GetAppSettingsAsync();
        settings.Language = language;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<string> GetThemeAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.Theme;
    }

    public async Task SetThemeAsync(string theme)
    {
        var settings = await GetAppSettingsAsync();
        settings.Theme = theme;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<bool> GetIsAspectRatio43Async()
    {
        var settings = await GetAppSettingsAsync();
        return settings.IsAspectRatio43;
    }

    public async Task SetIsAspectRatio43Async(bool is43)
    {
        var settings = await GetAppSettingsAsync();
        settings.IsAspectRatio43 = is43;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<string?> GetLastWindowPositionAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.LastWindowPosition;
    }

    public async Task SetLastWindowPositionAsync(string? position)
    {
        var settings = await GetAppSettingsAsync();
        settings.LastWindowPosition = position;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<string?> GetLastWindowSizeAsync()
    {
        var settings = await GetAppSettingsAsync();
        return settings.LastWindowSize;
    }

    public async Task SetLastWindowSizeAsync(string? size)
    {
        var settings = await GetAppSettingsAsync();
        settings.LastWindowSize = size;
        await UpdateAppSettingsAsync(settings);
    }

    public async Task<RemoteWidgetSettings> LoadRemoteWidgetSettingsAsync()
    {
        var appSettings = await _context.AppSettings.FindAsync(1);

        if (appSettings?.RemoteWidgetSettingsJson == null)
        {
            // Return default settings
            return new RemoteWidgetSettings();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<RemoteWidgetSettings>(appSettings.RemoteWidgetSettingsJson);
            return settings ?? new RemoteWidgetSettings();
        }
        catch
        {
            // If deserialization fails, return default
            return new RemoteWidgetSettings();
        }
    }

    public async Task SaveRemoteWidgetSettingsAsync(RemoteWidgetSettings settings)
    {
        var appSettings = await _context.AppSettings.FindAsync(1);

        if (appSettings == null)
        {
            // Create default app settings if none exist
            appSettings = new AppSettings
            {
                Id = 1,
                AudioLibraryPath = GetDefaultAudioLibraryPath(),
                AudioAutoPlayDelay = 5,
                GlobalVolume = 0.8f,
                AutoAdvanceEnabled = false,
                Language = "ro",
                Theme = "Dark",
                IsAspectRatio43 = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AppSettings.Add(appSettings);
        }

        appSettings.RemoteWidgetSettingsJson = JsonSerializer.Serialize(settings);
        appSettings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Update cache if settings were cached
        _cachedSettings = appSettings;
    }

    private static string GetDefaultAudioLibraryPath()
    {
        // Platform-specific default paths
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "SDAHymns", "Audio");
    }
}
