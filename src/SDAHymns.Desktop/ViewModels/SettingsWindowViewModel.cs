using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Models;
using SDAHymns.Core.Services;

namespace SDAHymns.Desktop.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IAudioLibraryService _libraryService;
    private readonly IAudioDownloadService _downloadService;
    private readonly IAudioPlayerService _audioPlayer;
    private readonly IDisplayProfileService _profileService;

    // General Settings
    [ObservableProperty]
    private string _selectedLanguage = "ro";

    [ObservableProperty]
    private string _selectedTheme = "Dark";

    [ObservableProperty]
    private ObservableCollection<string> _availableLanguages = new() { "ro", "en" };

    [ObservableProperty]
    private ObservableCollection<string> _availableThemes = new() { "Light", "Dark", "System" };

    // Display Settings
    [ObservableProperty]
    private ObservableCollection<DisplayProfile> _availableDisplayProfiles = new();

    [ObservableProperty]
    private DisplayProfile? _activeDisplayProfile;

    [ObservableProperty]
    private bool _isAspectRatio43 = true;

    [ObservableProperty]
    private string _audioLibraryPath = string.Empty;

    [ObservableProperty]
    private int _autoPlayDelay = 5;

    [ObservableProperty]
    private double _globalVolume = 80;

    [ObservableProperty]
    private ObservableCollection<InstalledPackage> _installedPackages = new();

    [ObservableProperty]
    private ObservableCollection<CategoryPackage> _availablePackages = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isDownloading = false;

    [ObservableProperty]
    private int _downloadProgress = 0;

    [ObservableProperty]
    private string _downloadStatusText = string.Empty;

    [ObservableProperty]
    private long _totalLibrarySize = 0;

    [ObservableProperty]
    private ObservableCollection<AudioDeviceInfo> _availableAudioDevices = new();

    [ObservableProperty]
    private AudioDeviceInfo? _selectedAudioDevice;

    public SettingsWindowViewModel(
        ISettingsService settingsService, 
        IAudioLibraryService libraryService, 
        IAudioDownloadService downloadService, 
        IAudioPlayerService audioPlayer,
        IDisplayProfileService profileService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
        _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Load current settings
            AudioLibraryPath = await _settingsService.GetAudioLibraryPathAsync();
            AutoPlayDelay = await _settingsService.GetAutoPlayDelayAsync();
            var volume = await _settingsService.GetGlobalVolumeAsync();
            GlobalVolume = volume * 100; // Convert 0-1 to 0-100

            // General Settings
            SelectedLanguage = await _settingsService.GetLanguageAsync();
            SelectedTheme = await _settingsService.GetThemeAsync();

            // Display Settings
            IsAspectRatio43 = await _settingsService.GetIsAspectRatio43Async();
            
            var profiles = await _profileService.GetAllProfilesAsync();
            AvailableDisplayProfiles.Clear();
            foreach (var profile in profiles)
            {
                AvailableDisplayProfiles.Add(profile);
            }
            
            var activeProfileId = await _settingsService.GetActiveDisplayProfileIdAsync();
            if (activeProfileId.HasValue)
            {
                ActiveDisplayProfile = AvailableDisplayProfiles.FirstOrDefault(p => p.Id == activeProfileId.Value);
            }
            if (ActiveDisplayProfile == null)
            {
                ActiveDisplayProfile = AvailableDisplayProfiles.FirstOrDefault(p => p.IsDefault) ?? AvailableDisplayProfiles.FirstOrDefault();
            }

            // Load audio devices
            var devices = _audioPlayer.GetOutputDevices();
            AvailableAudioDevices.Clear();
            foreach (var device in devices)
            {
                AvailableAudioDevices.Add(device);
            }
            // Select the default/first device
            SelectedAudioDevice = AvailableAudioDevices.FirstOrDefault();

            // Load installed packages
            await RefreshInstalledPackagesAsync();

            // Load available packages
            await RefreshAvailablePackagesAsync();

            StatusMessage = "Settings loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading settings: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        try
        {
            // General
            await _settingsService.SetLanguageAsync(SelectedLanguage);
            await _settingsService.SetThemeAsync(SelectedTheme);

            // Display
            await _settingsService.SetIsAspectRatio43Async(IsAspectRatio43);
            await _settingsService.SetActiveDisplayProfileIdAsync(ActiveDisplayProfile?.Id);

            // Audio
            await _settingsService.SetAudioLibraryPathAsync(AudioLibraryPath);
            await _settingsService.SetAutoPlayDelayAsync(AutoPlayDelay);
            await _settingsService.SetGlobalVolumeAsync((float)(GlobalVolume / 100.0));

            // Set audio output device
            if (SelectedAudioDevice != null)
            {
                _audioPlayer.SetOutputDevice(SelectedAudioDevice.DeviceNumber);
            }

            StatusMessage = "Settings saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshInstalledPackagesAsync()
    {
        try
        {
            var packages = await _downloadService.GetInstalledPackagesAsync();
            InstalledPackages.Clear();
            foreach (var package in packages)
            {
                InstalledPackages.Add(package);
            }

            TotalLibrarySize = await _libraryService.GetTotalLibrarySizeAsync();
            StatusMessage = $"Found {packages.Count} installed packages";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading installed packages: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAvailablePackagesAsync()
    {
        try
        {
            var packages = await _downloadService.GetAvailablePackagesAsync();
            AvailablePackages.Clear();
            foreach (var package in packages)
            {
                AvailablePackages.Add(package);
            }

            StatusMessage = $"Found {packages.Count} available packages";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading available packages: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DownloadCategoryAsync(string categorySlug)
    {
        if (IsDownloading)
            return;

        try
        {
            IsDownloading = true;
            StatusMessage = $"Downloading {categorySlug}...";

            var progress = new Progress<DownloadProgress>(p =>
            {
                DownloadProgress = p.PercentComplete;
                DownloadStatusText = $"{p.State}: {p.CurrentFile} ({p.PercentComplete}%)";
            });

            await _downloadService.DownloadCategoryAsync(categorySlug, progress);

            StatusMessage = $"Downloaded {categorySlug} successfully";
            await RefreshInstalledPackagesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
            DownloadStatusText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(string categorySlug)
    {
        try
        {
            var success = await _libraryService.DeleteCategoryAsync(categorySlug);
            if (success)
            {
                StatusMessage = $"Deleted {categorySlug}";
                await RefreshInstalledPackagesAsync();
            }
            else
            {
                StatusMessage = $"Failed to delete {categorySlug}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting category: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VerifyCategoryAsync(string categorySlug)
    {
        try
        {
            StatusMessage = $"Verifying {categorySlug}...";

            var progress = new Progress<int>(p =>
            {
                DownloadProgress = p;
            });

            var isValid = await _downloadService.VerifyCategoryAsync(categorySlug, progress);

            StatusMessage = isValid
                ? $"{categorySlug} verified successfully"
                : $"{categorySlug} has issues";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Verification failed: {ex.Message}";
        }
        finally
        {
            DownloadProgress = 0;
        }
    }

    [RelayCommand]
    public async Task MigrateLibraryWithPathAsync(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath))
        {
            StatusMessage = "Invalid path selected";
            return;
        }

        try
        {
            StatusMessage = "Migrating library...";
            IsDownloading = true; // Reuse the downloading flag for progress

            var progress = new Progress<int>(p =>
            {
                DownloadProgress = p;
                DownloadStatusText = $"Migrating files... {p}%";
            });

            var success = await _libraryService.MigrateLibraryAsync(newPath, progress);

            if (success)
            {
                AudioLibraryPath = newPath;
                StatusMessage = "Library migrated successfully";
                await RefreshInstalledPackagesAsync();
            }
            else
            {
                StatusMessage = "Migration failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Migration error: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
            DownloadStatusText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task ScanLibraryHealthAsync()
    {
        try
        {
            StatusMessage = "Scanning library health...";
            var health = await _libraryService.ScanLibraryHealthAsync();

            StatusMessage = health.IsHealthy
                ? $"Library is healthy ({health.TotalFiles} files)"
                : $"Found {health.CorruptedFiles} corrupted files, {health.MissingMetadata} missing metadata";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Health scan failed: {ex.Message}";
        }
    }

    public string FormattedLibrarySize => FormatBytes(TotalLibrarySize);

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
