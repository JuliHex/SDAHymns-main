using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.Services;
using Velopack;

using SDAHymns.Desktop.Models;

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

    public event Action? RequestClose;
    
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
    private LocalizedCategory? _selectedCategoryItem;

    public List<LocalizedCategory> Categories { get; } = new()
    {
        new LocalizedCategory { Slug = "crestine", Name = LocalizationManager.Instance.GetString("Category.crestine", "Christian") },
        new LocalizedCategory { Slug = "companioni", Name = LocalizationManager.Instance.GetString("Category.companioni", "Companions") },
        new LocalizedCategory { Slug = "exploratori", Name = LocalizationManager.Instance.GetString("Category.exploratori", "Explorers") },
        new LocalizedCategory { Slug = "licurici", Name = LocalizationManager.Instance.GetString("Category.licurici", "Little Stars") },
        new LocalizedCategory { Slug = "tineret", Name = LocalizationManager.Instance.GetString("Category.tineret", "Youth") },
        new LocalizedCategory { Slug = "diverse", Name = LocalizationManager.Instance.GetString("Category.diverse", "Miscellaneous") }
    };

    partial void OnSelectedCategoryItemChanged(LocalizedCategory? value)
    {
        if (value != null)
        {
            SelectedCategory = value.Slug;
        }
    }

    [ObservableProperty]
    private string _statusMessage = LocalizationManager.Instance.GetString("Status.Ready");

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
    private string _playPauseTooltip = LocalizationManager.Instance.GetString("Text.Play");

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
        SelectedCategoryItem = Categories.FirstOrDefault(c => c.Slug == SelectedCategory);
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Load Settings
            IsAspectRatio43 = await _settingsService.GetIsAspectRatio43Async();

            // Load initial search results (all hymns in default category)
            await PerformSearchAsync();

            // Load recent hymns
            await LoadRecentHymnsAsync();

            // Load display profiles
            await LoadProfilesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
        }
    }

    partial void OnActiveProfileChanged(DisplayProfile? value)
    {
        if (value != null)
        {
            _ = SetActiveProfileAsync(value);
            OnPropertyChanged(nameof(CurrentVerseLabelColor));
        }
    }

    private async Task SetActiveProfileAsync(DisplayProfile profile)
    {
        try
        {
            await _profileService.SetActiveProfileAsync(profile.Id);
            StatusMessage = $"Profil activ: {profile.Name}";
            // TODO: Notify display window to refresh with new profile
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
        }
    }

    public string DisplayWindowButtonLabel => IsDisplayWindowOpen ? "Ascunde Afișajul" : "Arată Afișajul";

    // Display dimensions based on aspect ratio
    public double DisplayWidth => IsAspectRatio43 ? 1600 : 1920;
    public double DisplayHeight => IsAspectRatio43 ? 1200 : 1080;

    public string AspectRatioLabel => IsAspectRatio43 ? "4:3" : "16:9";

    public string VerseIndicator
    {
        get
        {
            if (!Verses.Any()) return LocalizationManager.Instance.GetString("Text.NoVerses");
            if (IsTitleSlide) return LocalizationManager.Instance.GetString("Text.Title");
            
            var currentLabel = CurrentVerseLabel;
            if (currentLabel == LocalizationManager.Instance.GetString("Text.Chorus")) return LocalizationManager.Instance.GetString("Text.Chorus");
            
            // For numbered verses, show "Verse X of Y"
            // Count total unique numbered verses
            var totalNumbered = Verses.Where(v => int.TryParse(ExtractNumberPrefix(v.Content)?.TrimEnd('.') ?? v.Label?.TrimEnd('.'), out _)).Select(v => v.VerseNumber).Distinct().Count();
            
            // Find current verse number from the slide content or label
            var currentLabelStr = ExtractNumberPrefix(Verses[CurrentVerseIndex].Content) ?? Verses[CurrentVerseIndex].Label;
            if (int.TryParse(currentLabelStr?.TrimEnd('.'), out var num))
            {
                return string.Format(LocalizationManager.Instance.GetString("Text.VerseIndicator"), num, totalNumbered);
            }
            
            return currentLabel ?? string.Empty;
        }
    }

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
        OnPropertyChanged(nameof(CurrentVerseLabelColor));
        OnPropertyChanged(nameof(NextVerseContent));
        OnPropertyChanged(nameof(NextVerseLabel));
        OnPropertyChanged(nameof(NextVerseLabelColor));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(VerseIndicator));
        OnPropertyChanged(nameof(IsTitleSlide));
        OnPropertyChanged(nameof(IsNotTitleSlide));
        OnPropertyChanged(nameof(IsChorus));
        OnPropertyChanged(nameof(IsVerseNumber));
        OnPropertyChanged(nameof(IsUnlabeledVerse));
        OnPropertyChanged(nameof(IsNextChorus));
        OnPropertyChanged(nameof(IsNextVerseNumber));
        OnPropertyChanged(nameof(IsNextUnlabeledVerse));
        OnPropertyChanged(nameof(IsNextTitleSlide));
        OnPropertyChanged(nameof(IsNotNextTitleSlide));
    }

    partial void OnVersesChanged(List<Verse> value)
    {
        OnPropertyChanged(nameof(CurrentVerseContent));
        OnPropertyChanged(nameof(CurrentVerseLabel));
        OnPropertyChanged(nameof(CurrentVerseLabelColor));
        OnPropertyChanged(nameof(NextVerseContent));
        OnPropertyChanged(nameof(NextVerseLabel));
        OnPropertyChanged(nameof(NextVerseLabelColor));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(VerseIndicator));
        OnPropertyChanged(nameof(IsTitleSlide));
        OnPropertyChanged(nameof(IsNotTitleSlide));
    }

    public string? CurrentVerseContent =>
        Verses.Any() && CurrentVerseIndex >= 0 && CurrentVerseIndex < Verses.Count
            ? StripNumberPrefix(Verses[CurrentVerseIndex].Content)
            : null;

    public string? CurrentVerseLabel
    {
        get
        {
            if (!Verses.Any() || CurrentVerseIndex < 0 || CurrentVerseIndex >= Verses.Count) return null;
            var verse = Verses[CurrentVerseIndex];
            if (verse.Label == "Title") return LocalizationManager.Instance.GetString("Text.Title");
            
            var extracted = ExtractNumberPrefix(verse.Content);
            if (!string.IsNullOrEmpty(extracted)) return extracted;
            
            if (!string.IsNullOrEmpty(verse.Label) && !verse.Label.StartsWith("Verse", StringComparison.Ordinal)) 
                return (verse.Label == "Refren" || verse.Label == "Chorus") ? LocalizationManager.Instance.GetString("Text.Chorus") : verse.Label;
                
            if (verse.VerseNumber > 0) return $"{verse.VerseNumber}.";
            
            return "";
        }
    }

    public string? PreviousVerseContent =>
        Verses.Any() && CurrentVerseIndex > 0
            ? Verses[CurrentVerseIndex - 1].Content
            : null;

    public string? NextVerseContent =>
        Verses.Any() && CurrentVerseIndex < Verses.Count - 1
            ? StripNumberPrefix(Verses[CurrentVerseIndex + 1].Content)
            : null;

    public bool IsChorus => CurrentVerseLabel == "Refren";
    public bool IsVerseNumber => !string.IsNullOrEmpty(CurrentVerseLabel) && CurrentVerseLabel != "Refren" && CurrentVerseLabel != "Title";
    public bool IsUnlabeledVerse => !IsChorus && !IsVerseNumber && !IsTitleSlide;

    public bool IsTitleSlide => CurrentVerseLabel == LocalizationManager.Instance.GetString("Text.Title");
    public bool IsNotTitleSlide => !IsTitleSlide && !string.IsNullOrEmpty(CurrentVerseContent);

    public bool IsNextChorus => NextVerseLabel == "Refren";
    public bool IsNextVerseNumber => !string.IsNullOrEmpty(NextVerseLabel) && NextVerseLabel != "Refren" && NextVerseLabel != "Title";
    public bool IsNextUnlabeledVerse => !IsNextChorus && !IsNextVerseNumber && !string.IsNullOrEmpty(NextVerseContent);
    public bool IsNextTitleSlide => NextVerseLabel == LocalizationManager.Instance.GetString("Text.Title");
    public bool IsNotNextTitleSlide => !IsNextTitleSlide && !string.IsNullOrEmpty(NextVerseContent);

    public string? NextVerseLabel =>
        Verses.Any() && CurrentVerseIndex < Verses.Count - 1
            ? (ExtractNumberPrefix(Verses[CurrentVerseIndex + 1].Content) ?? 
               (Verses[CurrentVerseIndex + 1].Label == "Refren" || Verses[CurrentVerseIndex + 1].Label == "Chorus" ? LocalizationManager.Instance.GetString("Text.Chorus") : Verses[CurrentVerseIndex + 1].Label))
            : null;

    public string? HymnTitle => CurrentHymn != null
        ? $"{CurrentHymn.Number}. {CurrentHymn.Title}"
        : null;

    public string? LocalizedCategoryName => CurrentHymn?.Category != null
        ? LocalizationManager.Instance.GetString($"Category.{CurrentHymn.Category.Slug}", CurrentHymn.Category.Name)
        : null;

    public string? HymnTitleOnly => CurrentHymn?.Title;
    
    public string CurrentVerseLabelColor
    {
        get
        {
            if (ActiveProfile != null && !ActiveProfile.EnableCustomChorusStyling)
                return ActiveProfile.LabelColor;

            var label = CurrentVerseLabel;
            if (string.IsNullOrEmpty(label)) return ActiveProfile?.LabelColor ?? "#CCCCCC";

            // Check if it's a Refren or a Number (e.g., "1.", "2.", etc.)
            var isChorusOrNumber = label.Contains(LocalizationManager.Instance.GetString("Text.Chorus"), StringComparison.OrdinalIgnoreCase) || 
                                  label.Contains("Chorus", StringComparison.OrdinalIgnoreCase) ||
                                  (int.TryParse(label.TrimEnd('.'), out _));
            
            if (isChorusOrNumber)
            {
                return ActiveProfile?.CustomChorusColor ?? "#FFD700";
            }
            
            return ActiveProfile?.LabelColor ?? "#CCCCCC";
        }
    }

    public string NextVerseLabelColor
    {
        get
        {
            if (ActiveProfile != null && !ActiveProfile.EnableCustomChorusStyling)
                return ActiveProfile.LabelColor;

            var label = NextVerseLabel;
            if (string.IsNullOrEmpty(label)) return ActiveProfile?.LabelColor ?? "#CCCCCC";

            var isChorusOrNumber = label.Contains("Chorus", StringComparison.OrdinalIgnoreCase) || 
                                  (int.TryParse(label.TrimEnd('.'), out _));
            
            if (isChorusOrNumber)
            {
                return ActiveProfile?.CustomChorusColor ?? "#FFD700";
            }
            
            return ActiveProfile?.LabelColor ?? "#CCCCCC";
        }
    }

    public bool CanGoNext => Verses.Any();
    public bool CanGoPrevious => CurrentVerseIndex > 0;

    [RelayCommand]
    public async Task LoadHymnAsync()
    {
        try
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.LoadingHymn"), HymnNumber, SelectedCategory);

            CurrentHymn = await _hymnService.GetHymnByNumberAsync(HymnNumber, SelectedCategory);

            if (CurrentHymn != null)
            {
                var baseVerses = await _hymnService.GetVersesForHymnAsync(CurrentHymn.Id);
                Verses = ExpandVersesWithChorus(baseVerses);
                CurrentVerseIndex = 0;

                OnPropertyChanged(nameof(CurrentVerseContent));
                OnPropertyChanged(nameof(CurrentVerseLabel));
                OnPropertyChanged(nameof(HymnTitle));
                OnPropertyChanged(nameof(HymnTitleOnly));
                OnPropertyChanged(nameof(IsTitleSlide));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(VerseIndicator));

                StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.HymnLoaded"), CurrentHymn.Title, Verses.Count);
            }
            else
            {
                StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.HymnNotFound"), HymnNumber, SelectedCategory);
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
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
        }
    }

    /// <summary>
    /// Load a hymn directly by hymn object and verse index (used by RemoteWidget)
    /// </summary>
    public async Task LoadHymnDirectlyAsync(Hymn hymn, int verseIndex = 0)
    {
        try
        {
            // Only load verses if it's a new hymn or verses are empty
            if (CurrentHymn?.Id != hymn.Id || Verses == null || Verses.Count == 0)
            {
                CurrentHymn = hymn;
                HymnNumber = hymn.Number;
                var baseVerses = await _hymnService.GetVersesForHymnAsync(hymn.Id);
                Verses = ExpandVersesWithChorus(baseVerses);
            }

            CurrentVerseIndex = Math.Min(verseIndex, Verses.Count - 1);

            OnPropertyChanged(nameof(CurrentVerseContent));
            OnPropertyChanged(nameof(CurrentVerseLabel));
            OnPropertyChanged(nameof(CurrentVerseLabelColor));
            OnPropertyChanged(nameof(HymnTitle));
            OnPropertyChanged(nameof(HymnTitleOnly));
            OnPropertyChanged(nameof(IsTitleSlide));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(VerseIndicator));
            OnPropertyChanged(nameof(IsChorus));
            OnPropertyChanged(nameof(IsVerseNumber));
            OnPropertyChanged(nameof(IsUnlabeledVerse));
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.SyncError"), ex.Message);
        }
    }

    [RelayCommand]
    public void NextVerse()
    {
        if (CanGoNext)
        {
            if (CurrentVerseIndex < Verses.Count - 1)
            {
                CurrentVerseIndex++;
            }
            else
            {
                // On last slide, exit the display
                StatusMessage = LocalizationManager.Instance.GetString("Status.Finalized");
                RequestClose?.Invoke();
                IsDisplayWindowOpen = false;
            }
        }
    }

    [RelayCommand]
    public void CloseDisplay()
    {
        IsDisplayWindowOpen = false;
        RequestClose?.Invoke();
    }

    [RelayCommand]
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

            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Searching"), SearchResults.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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
                StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.HymnLoaded"), CurrentHymn.Title, Verses.Count);
            }
            else
            {
                StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.HymnNotFound"), number, categorySlug);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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
                StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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
            StatusMessage = LocalizationManager.Instance.GetString("Status.Error", "Error downloading update.");
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
        PlayPauseTooltip = LocalizationManager.Instance.GetString("Text.Play");
        AudioPosition = 0;
    }

    private void OnAudioStateChanged(object? sender, PlaybackState newState)
    {
        switch (newState)
        {
            case PlaybackState.Playing:
                PlayPauseIcon = "⏸";
                PlayPauseTooltip = LocalizationManager.Instance.GetString("Text.Pause");
                break;
            case PlaybackState.Paused:
            case PlaybackState.Stopped:
                PlayPauseIcon = "▶";
                PlayPauseTooltip = LocalizationManager.Instance.GetString("Text.Play");
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
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Text.Verse"), verseNumber);
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
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message);
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

            StatusMessage = "Timpi salvați cu succes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Eroare la salvarea timpilor: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelCountdown()
    {
        _countdown?.Cancel();
        IsCountdownActive = false;
        CountdownSeconds = 0;
        StatusMessage = "Redare automată anulată";
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
            StatusMessage = $"Eroare numărătoare inversă: {ex.Message}";
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

                StatusMessage = $"Audio încărcat: {recording.FilePath}";
            }
            else
            {
                IsAudioLoaded = false;
                StatusMessage = "Niciun fișier audio disponibil pentru acest imn";
            }
        }
        catch (FileNotFoundException)
        {
            IsAudioLoaded = false;
            StatusMessage = "Fișierul audio nu a fost găsit. Verificați calea bibliotecii audio în setări.";
        }
        catch (Exception ex)
        {
            IsAudioLoaded = false;
            StatusMessage = $"Eroare la încărcarea audio: {ex.Message}";
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

    private string? ExtractNumberPrefix(string? content)
    {
        if (string.IsNullOrEmpty(content)) return null;
        var firstSpace = content.IndexOf(' ');
        if (firstSpace > 0)
        {
            var firstWord = content.Substring(0, firstSpace);
            if (int.TryParse(firstWord.TrimEnd('.'), out _))
            {
                return firstWord;
            }
        }
        return null;
    }

    private string? StripNumberPrefix(string? content)
    {
        if (string.IsNullOrEmpty(content)) return null;
        var prefix = ExtractNumberPrefix(content);
        if (prefix != null)
        {
            return content.Substring(prefix.Length).TrimStart();
        }
        return content;
    }

    private List<Verse> ExpandVersesWithChorus(List<Verse> baseVerses)
    {
        if (baseVerses == null || baseVerses.Count <= 2) return baseVerses ?? new();

        var expanded = new List<Verse>();
        var titleSlide = baseVerses.FirstOrDefault(v => v.Label == "Title");
        var actualVerses = baseVerses.Where(v => v.Label != "Title").ToList();
        
        if (titleSlide != null) expanded.Add(titleSlide);

        var chorus = actualVerses.FirstOrDefault(v => v.Label == "Refren" || v.Label == "Chorus");
        
        // If there's no chorus, just return the original list (with title)
        if (chorus == null) return baseVerses;

        // Check if the chorus is already repeated in the list
        var chorusCount = actualVerses.Count(v => v.Label == "Refren" || v.Label == "Chorus");
        if (chorusCount > 1) return baseVerses;

        // Expand: Verse 1, Chorus, Verse 2, Chorus...
        var numberedVerses = actualVerses.Where(v => v.Label != "Refren" && v.Label != "Chorus").ToList();
        
        for (int i = 0; i < numberedVerses.Count; i++)
        {
            expanded.Add(numberedVerses[i]);
            expanded.Add(chorus);
        }

        return expanded;
    }
}
