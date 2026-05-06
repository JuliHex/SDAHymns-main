using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;
using Velopack;

namespace SDAHymns.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private bool _disposed;
    private readonly IHymnDisplayService _hymnService;
    private readonly IUpdateService _updateService;
    private readonly ISearchService _searchService;
    private readonly IDisplayProfileService _profileService;
    private readonly IAudioPlayerService _audioPlayer;
    private readonly HymnSynchronizer _synchronizer;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private Hymn? _currentHymn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextVerseCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousVerseCommand))]
    private List<Verse> _verses = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextVerseCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousVerseCommand))]
    private int _currentVerseIndex = 0;

    [ObservableProperty]
    private int _hymnNumber = 1;

    [ObservableProperty]
    private string _selectedCategory = "crestine";

    [ObservableProperty]
    private string _statusMessage = "Enter a hymn number and click Load";

    [ObservableProperty]
    private bool _isAspectRatio43 = true;

    [ObservableProperty]
    private bool _isDisplayWindowOpen = false;

    // Search properties
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<HymnSearchResult> _searchResults = new();

    [ObservableProperty]
    private HymnSearchResult? _selectedSearchResult;

    [ObservableProperty]
    private List<Hymn> _recentHymns = new();

    // Update notification properties
    [ObservableProperty]
    private bool _isUpdateAvailable = false;

    [ObservableProperty]
    private string? _latestVersion;

    [ObservableProperty]
    private bool _isDownloadingUpdate = false;

    [ObservableProperty]
    private int _downloadProgress = 0;

    private UpdateInfo? _pendingUpdate;

    // Profile properties
    [ObservableProperty]
    private List<DisplayProfile> _availableProfiles = new();

    [ObservableProperty]
    private DisplayProfile? _activeProfile;

    // Audio properties
    [ObservableProperty]
    private AudioRecording? _currentAudioRecording;

    [ObservableProperty]
    private bool _isAudioLoaded = false;

    [ObservableProperty]
    private double _audioPosition = 0;  // Seconds

    [ObservableProperty]
    private double _audioDuration = 0;  // Seconds

    [ObservableProperty]
    private double _audioVolume = 80;  // 0-100

    [ObservableProperty]
    private bool _autoAdvanceEnabled = false;

    [ObservableProperty]
    private string _playPauseIcon = "▶";

    [ObservableProperty]
    private string _playPauseTooltip = "Play";

    [ObservableProperty]
    private bool _isCountdownActive = false;

    [ObservableProperty]
    private int _countdownSeconds = 0;

    private AutoPlayCountdown? _countdown;

    public string AudioTimeDisplay =>
        $"{FormatTime(AudioPosition)} / {FormatTime(AudioDuration)}";

    public MainWindowViewModel(IHymnDisplayService hymnService, IUpdateService updateService, ISearchService searchService, IDisplayProfileService profileService, IAudioPlayerService audioPlayer, ISettingsService settingsService)
    {
        _hymnService = hymnService;
        _updateService = updateService;
        _searchService = searchService;
        _profileService = profileService;
        _audioPlayer = audioPlayer;
        _settingsService = settingsService;
        _synchronizer = new HymnSynchronizer(_audioPlayer);

        // Subscribe to audio events
        _audioPlayer.PositionChanged += OnAudioPositionChanged;
        _audioPlayer.PlaybackEnded += OnAudioPlaybackEnded;
        _audioPlayer.StateChanged += OnAudioStateChanged;
        _synchronizer.VerseChangeRequested += OnSynchronizerVerseChangeRequested;

        // Initialize search results and recent hymns
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Load initial search results (all hymns in default category)
            await PerformSearchAsync();

            // Load recent hymns
            await LoadRecentHymnsAsync();

            // Load display profiles
            await LoadProfilesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        try
        {
            AvailableProfiles = await _profileService.GetAllProfilesAsync();
            ActiveProfile = await _profileService.GetActiveProfileAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading profiles: {ex.Message}";
        }
    }

    partial void OnActiveProfileChanged(DisplayProfile? value)
    {
        if (value != null)
        {
            _ = SetActiveProfileAsync(value);
        }
    }

    private async Task SetActiveProfileAsync(DisplayProfile profile)
    {
        try
        {
            await _profileService.SetActiveProfileAsync(profile.Id);
            StatusMessage = $"Active profile: {profile.Name}";
            // TODO: Notify display window to refresh with new profile
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error setting active profile: {ex.Message}";
        }
    }

    public string DisplayWindowButtonLabel => IsDisplayWindowOpen ? "Hide Display" : "Show Display";

    // Display dimensions based on aspect ratio
    public double DisplayWidth => IsAspectRatio43 ? 800 : 1920;
    public double DisplayHeight => IsAspectRatio43 ? 600 : 1080;

    public string AspectRatioLabel => IsAspectRatio43 ? "4:3" : "16:9";

    public string VerseIndicator =>
        Verses.Any()
            ? $"Verse {CurrentVerseIndex + 1} of {Verses.Count}"
            : "No verses loaded";

    partial void OnIsAspectRatio43Changed(bool value)
    {
        OnPropertyChanged(nameof(DisplayWidth));
        OnPropertyChanged(nameof(DisplayHeight));
        OnPropertyChanged(nameof(AspectRatioLabel));
    }

    partial void OnIsDisplayWindowOpenChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayWindowButtonLabel));
    }

    partial void OnCurrentVerseIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentVerseContent));
        OnPropertyChanged(nameof(CurrentVerseLabel));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(VerseIndicator));
        OnPropertyChanged(nameof(IsTitleSlide));
    }

    partial void OnVersesChanged(List<Verse> value)
    {
        OnPropertyChanged(nameof(CurrentVerseContent));
        OnPropertyChanged(nameof(CurrentVerseLabel));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(VerseIndicator));
        OnPropertyChanged(nameof(IsTitleSlide));
    }

    public string? CurrentVerseContent =>
        Verses.Any() && CurrentVerseIndex >= 0 && CurrentVerseIndex < Verses.Count
            ? Verses[CurrentVerseIndex].Content
            : null;

    public string? CurrentVerseLabel =>
        Verses.Any() && CurrentVerseIndex >= 0 && CurrentVerseIndex < Verses.Count
            ? Verses[CurrentVerseIndex].Label
            : null;

    public bool IsTitleSlide => 
        Verses.Any() && CurrentVerseIndex >= 0 && CurrentVerseIndex < Verses.Count && Verses[CurrentVerseIndex].Label == "Titlu";

    public string? HymnTitle => CurrentHymn != null
        ? $"{CurrentHymn.Number}. {CurrentHymn.Title}"
        : null;

    public string? HymnTitleOnly => CurrentHymn?.Title;

    public bool CanGoNext => CurrentVerseIndex < Verses.Count - 1;
    public bool CanGoPrevious => CurrentVerseIndex > 0;

    [RelayCommand]
    public async Task LoadHymnAsync()
    {
        try
        {
            StatusMessage = $"Loading hymn {HymnNumber} from {SelectedCategory}...";

            CurrentHymn = await _hymnService.GetHymnByNumberAsync(HymnNumber, SelectedCategory);

            if (CurrentHymn != null)
            {
                Verses = await _hymnService.GetVersesForHymnAsync(CurrentHymn.Id);
                CurrentVerseIndex = 0;

                OnPropertyChanged(nameof(CurrentVerseContent));
                OnPropertyChanged(nameof(CurrentVerseLabel));
                OnPropertyChanged(nameof(HymnTitle));
                OnPropertyChanged(nameof(HymnTitleOnly));
                OnPropertyChanged(nameof(IsTitleSlide));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(VerseIndicator));

                StatusMessage = $"Loaded: {CurrentHymn.Title} ({Verses.Count} verses)";
            }
            else
            {
                StatusMessage = $"Hymn {HymnNumber} not found in {SelectedCategory}";
                Verses = new();
                CurrentHymn = null;

                OnPropertyChanged(nameof(CurrentVerseContent));
                OnPropertyChanged(nameof(CurrentVerseLabel));
                OnPropertyChanged(nameof(HymnTitle));
                OnPropertyChanged(nameof(HymnTitleOnly));
                OnPropertyChanged(nameof(IsTitleSlide));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading hymn: {ex.Message}";
        }
    }

    /// <summary>
    /// Load a hymn directly by hymn object and verse index (used by RemoteWidget)
    /// </summary>
    public async Task LoadHymnDirectlyAsync(Hymn hymn, int verseIndex = 0)
    {
        try
        {
            CurrentHymn = hymn;
            Verses = await _hymnService.GetVersesForHymnAsync(hymn.Id);
            CurrentVerseIndex = Math.Min(verseIndex, Verses.Count - 1);

            OnPropertyChanged(nameof(CurrentVerseContent));
            OnPropertyChanged(nameof(CurrentVerseLabel));
            OnPropertyChanged(nameof(HymnTitle));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(VerseIndicator));

            StatusMessage = $"Loaded: {CurrentHymn.Title} ({Verses.Count} verses)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading hymn: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    public void NextVerse()
    {
        if (CanGoNext)
        {
            CurrentVerseIndex++;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    public void PreviousVerse()
    {
        if (CanGoPrevious)
        {
            CurrentVerseIndex--;
        }
    }

    // Search methods
    partial void OnSearchQueryChanged(string value)
    {
        _ = PerformSearchAsync();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        _ = PerformSearchAsync();
    }

    private async Task PerformSearchAsync()
    {
        try
        {
            List<HymnSearchResult> results;

            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Show all hymns in category or recent
                results = await _searchService.SearchHymnsAsync("", SelectedCategory);
            }
            else
            {
                results = await _searchService.SearchHymnsAsync(SearchQuery, SelectedCategory);
            }

            // Update ObservableCollection
            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }

            StatusMessage = $"Found {SearchResults.Count} hymns";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    partial void OnSelectedSearchResultChanged(HymnSearchResult? value)
    {
        if (value != null)
        {
            _ = LoadAndDisplayHymnAsync(value.Number, value.CategorySlug);
        }
    }

    /// <summary>
    /// Unified helper method for loading and displaying a hymn
    /// </summary>
    private async Task LoadAndDisplayHymnAsync(int number, string categorySlug)
    {
        try
        {
            CurrentHymn = await _hymnService.GetHymnByNumberAsync(number, categorySlug);

            if (CurrentHymn != null)
            {
                Verses = await _hymnService.GetVersesForHymnAsync(CurrentHymn.Id);
                CurrentVerseIndex = 0;

                // Track recent access
                await _searchService.AddToRecentAsync(CurrentHymn.Id);
                await LoadRecentHymnsAsync();

                // Try to load audio if available
                await TryLoadAudioAsync();

                OnPropertyChanged(nameof(CurrentVerseContent));
                OnPropertyChanged(nameof(CurrentVerseLabel));
                OnPropertyChanged(nameof(HymnTitle));
                OnPropertyChanged(nameof(FavoriteIcon));
                StatusMessage = $"Loaded: {CurrentHymn.Number}. {CurrentHymn.Title}";
            }
            else
            {
                StatusMessage = $"Hymn {number} not found in {categorySlug}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading hymn: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadRecentHymnsAsync()
    {
        try
        {
            RecentHymns = await _searchService.GetRecentHymnsAsync(5);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading recent hymns: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (CurrentHymn != null)
        {
            try
            {
                await _searchService.ToggleFavoriteAsync(CurrentHymn.Id);
                CurrentHymn.IsFavorite = !CurrentHymn.IsFavorite;
                OnPropertyChanged(nameof(FavoriteIcon));

                // Update the favorite status in the search results list
                var searchResult = SearchResults.FirstOrDefault(r => r.Id == CurrentHymn.Id);
                if (searchResult != null)
                {
                    searchResult.IsFavorite = CurrentHymn.IsFavorite;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error toggling favorite: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task LoadHymnFromRecentAsync(Hymn hymn)
    {
        await LoadAndDisplayHymnAsync(hymn.Number, hymn.Category.Slug);
    }

    public string FavoriteIcon => CurrentHymn?.IsFavorite == true ? "⭐" : "☆";

    // Update notification methods
    public void ShowUpdateNotification(UpdateInfo updateInfo)
    {
        _pendingUpdate = updateInfo;
        LatestVersion = updateInfo.TargetFullRelease.Version.ToString();
        IsUpdateAvailable = true;
    }

    [RelayCommand]
    private async Task UpdateNowAsync()
    {
        if (_pendingUpdate == null)
            return;

        IsDownloadingUpdate = true;
        DownloadProgress = 0;

        var progress = new Progress<int>(p => DownloadProgress = p);
        var success = await _updateService.DownloadUpdatesAsync(_pendingUpdate, progress);

        if (success)
        {
            // Apply and restart
            _updateService.ApplyUpdatesAndRestart(_pendingUpdate);
        }
        else
        {
            // Show error - keep banner visible so user can retry
            StatusMessage = "Failed to download update. Please try again later.";
            IsDownloadingUpdate = false;
            // Note: Keep IsUpdateAvailable = true so the banner stays visible for retry
        }
    }

    [RelayCommand]
    private void DismissUpdate()
    {
        IsUpdateAvailable = false;
    }

    // Audio methods
    private string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    partial void OnAudioPositionChanged(double value)
    {
        OnPropertyChanged(nameof(AudioTimeDisplay));
    }

    partial void OnAudioDurationChanged(double value)
    {
        OnPropertyChanged(nameof(AudioTimeDisplay));
    }

    partial void OnAudioVolumeChanged(double value)
    {
        _audioPlayer.Volume = (float)(value / 100.0);
    }

    partial void OnAutoAdvanceEnabledChanged(bool value)
    {
        _synchronizer.SetEnabled(value);
    }

    private void OnAudioPositionChanged(object? sender, TimeSpan position)
    {
        AudioPosition = position.TotalSeconds;
    }

    private void OnAudioPlaybackEnded(object? sender, EventArgs e)
    {
        PlayPauseIcon = "▶";
        PlayPauseTooltip = "Play";
        AudioPosition = 0;
    }

    private void OnAudioStateChanged(object? sender, PlaybackState newState)
    {
        switch (newState)
        {
            case PlaybackState.Playing:
                PlayPauseIcon = "⏸";
                PlayPauseTooltip = "Pause";
                break;
            case PlaybackState.Paused:
            case PlaybackState.Stopped:
                PlayPauseIcon = "▶";
                PlayPauseTooltip = "Play";
                break;
        }
    }

    private void OnSynchronizerVerseChangeRequested(object? sender, int verseNumber)
    {
        // Find the verse index (verseNumber is 1-based, index is 0-based)
        var index = Verses.FindIndex(v => v.VerseNumber == verseNumber);
        if (index >= 0)
        {
            CurrentVerseIndex = index;
            StatusMessage = $"Auto-advanced to verse {verseNumber}";
        }
    }

    [RelayCommand]
    private async Task PlayPauseAudioAsync()
    {
        if (!IsAudioLoaded)
            return;

        try
        {
            if (_audioPlayer.PlaybackState == PlaybackState.Playing)
            {
                await _audioPlayer.PauseAsync();
            }
            else
            {
                await _audioPlayer.PlayAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Audio error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StopAudioAsync()
    {
        if (!IsAudioLoaded)
            return;

        try
        {
            await _audioPlayer.StopAsync();
            AudioPosition = 0;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Audio error: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SaveAudioTimingsAsync(string timingMapJson)
    {
        if (CurrentAudioRecording == null)
            return;

        try
        {
            // Update the timing map
            CurrentAudioRecording.TimingMapJson = timingMapJson;

            // Save to database
            await _hymnService.UpdateAudioRecordingAsync(CurrentAudioRecording);

            // Reload timing map in synchronizer
            _synchronizer.LoadTimingMap(timingMapJson);

            StatusMessage = "Timings saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving timings: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelCountdown()
    {
        _countdown?.Cancel();
        IsCountdownActive = false;
        CountdownSeconds = 0;
        StatusMessage = "Auto-play cancelled";
    }

    private async Task StartAutoPlayCountdownAsync()
    {
        if (!IsAudioLoaded)
            return;

        try
        {
            // Get auto-play delay from settings
            var delaySeconds = await _settingsService.GetAutoPlayDelayAsync();

            // Initialize countdown if not already created
            if (_countdown == null)
            {
                _countdown = new AutoPlayCountdown();

                _countdown.CountdownTick += (s, remaining) =>
                {
                    CountdownSeconds = remaining;
                };

                _countdown.CountdownCompleted += async (s, e) =>
                {
                    IsCountdownActive = false;
                    await _audioPlayer.PlayAsync();
                };

                _countdown.CountdownCancelled += (s, e) =>
                {
                    IsCountdownActive = false;
                    CountdownSeconds = 0;
                };
            }

            IsCountdownActive = true;
            _countdown.Start(delaySeconds);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Countdown error: {ex.Message}";
            IsCountdownActive = false;
        }
    }

    private async Task TryLoadAudioAsync()
    {
        if (CurrentHymn == null)
            return;

        try
        {
            // Check if audio recording exists for this hymn
            var recording = CurrentHymn.AudioRecordings.FirstOrDefault();
            if (recording != null)
            {
                // Get audio library path from settings
                var audioLibraryPath = await _settingsService.GetAudioLibraryPathAsync();
                await _audioPlayer.LoadAsync(recording, audioLibraryPath);

                CurrentAudioRecording = recording;
                AudioDuration = _audioPlayer.TotalDuration.TotalSeconds;
                AudioPosition = 0;
                IsAudioLoaded = true;

                // Load timing map if available
                if (!string.IsNullOrEmpty(recording.TimingMapJson))
                {
                    _synchronizer.LoadTimingMap(recording.TimingMapJson);
                }

                StatusMessage = $"Audio loaded: {recording.FilePath}";
            }
            else
            {
                IsAudioLoaded = false;
                StatusMessage = "No audio file available for this hymn";
            }
        }
        catch (FileNotFoundException)
        {
            IsAudioLoaded = false;
            StatusMessage = "Audio file not found. Check audio library path in settings.";
        }
        catch (Exception ex)
        {
            IsAudioLoaded = false;
            StatusMessage = $"Error loading audio: {ex.Message}";
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        // Unsubscribe from audio player events to prevent memory leaks
        _audioPlayer.PositionChanged -= OnAudioPositionChanged;
        _audioPlayer.PlaybackEnded -= OnAudioPlaybackEnded;
        _audioPlayer.StateChanged -= OnAudioStateChanged;
        _synchronizer.VerseChangeRequested -= OnSynchronizerVerseChangeRequested;

        _synchronizer?.Dispose();
        _countdown?.Dispose();

        _disposed = true;
    }
}
