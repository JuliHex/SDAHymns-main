using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Models;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.Services;
using System;
using System.Threading.Tasks;

using SDAHymns.Desktop.Models;

namespace SDAHymns.Desktop.ViewModels;

public partial class RemoteWidgetViewModel : ViewModelBase
{
    private readonly IHymnDisplayService _hymnService;
    private readonly IAudioPlayerService? _audioService;
    private readonly DesktopRemoteControlService? _remoteControlService;
    private readonly ISettingsService _settingsService;
    private readonly BroadcastSyncService _broadcastSync;
    private readonly ISearchService _searchService;

    [ObservableProperty]
    private string _hymnNumberInput = "";

    partial void OnHymnNumberInputChanged(string value)
    {
        UpdateRealTimePreview(value);
    }

    private async void UpdateRealTimePreview(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            if (_currentHymn == null)
            {
                ActiveHymnNumber = "";
                ActiveHymnTitle = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
            }
            else
            {
                ActiveHymnNumber = _currentHymn.Number.ToString();
                ActiveHymnTitle = _currentHymn.Title;
            }
            return;
        }

        try
        {
            if (int.TryParse(query, out int number))
            {
                var hymn = await _hymnService.GetHymnByNumberAsync(number, SelectedCategory);
                if (hymn != null)
                {
                    ActiveHymnNumber = hymn.Number.ToString();
                    ActiveHymnTitle = hymn.Title;
                }
                else
                {
                    ActiveHymnNumber = query;
                    ActiveHymnTitle = "";
                }
            }
            else if (query.Length >= 2)
            {
                var results = await _searchService.SearchHymnsAsync(query, SelectedCategory);
                if (results != null && results.Any())
                {
                    var bestMatch = results.First();
                    ActiveHymnNumber = bestMatch.Number.ToString();
                    ActiveHymnTitle = bestMatch.Title;
                }
            }
        }
        catch
        {
            // Ignore errors in preview
        }
    }

    [ObservableProperty]
    private string _currentHymnDisplay = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");

    [ObservableProperty]
    private string _verseIndicator = "";

    [ObservableProperty]
    private string _activeHymnNumber = "";

    [ObservableProperty]
    private string _activeHymnTitle = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");

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
        if (value != null && !_isInitializing)
        {
            SelectedCategory = value.Slug;
        }
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (!_isInitializing)
        {
            SelectedCategoryItem = Categories.FirstOrDefault(c => c.Slug == value);
        }
    }

    [ObservableProperty]
    private bool _isAlwaysOnTop = true;

    [ObservableProperty]
    private bool _isPositionLocked = false;

    [ObservableProperty]
    private bool _showNumberPad = true;

    [ObservableProperty]
    private bool _canNavigateNext = false;

    [ObservableProperty]
    private bool _canNavigatePrevious = false;

    [ObservableProperty]
    private bool _isAudioAvailable = false;

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private string _statusMessage = LocalizationManager.Instance.GetString("Status.Ready");

    public RemoteWidgetSettings Settings { get; set; }

    private Hymn? _currentHymn;
    private int _currentVerseIndex = 0;
    private bool _isInitializing = false;

    // Window callbacks
    public Action<Hymn, int>? OnShowHymnRequested { get; set; }
    public Action? OnBlankDisplayRequested { get; set; }
    public Action? OnRequestFocusClear { get; set; }
    public Action? OnTogglePresenterViewRequested { get; set; }

    public RemoteWidgetViewModel(
        IHymnDisplayService hymnService,
        ISettingsService settingsService,
        BroadcastSyncService broadcastSync,
        ISearchService searchService,
        IAudioPlayerService? audioService = null,
        DesktopRemoteControlService? remoteControlService = null)
    {
        _hymnService = hymnService;
        _settingsService = settingsService;
        _broadcastSync = broadcastSync;
        _searchService = searchService;
        _audioService = audioService;
        _remoteControlService = remoteControlService;

        if (_remoteControlService != null)
        {
            _remoteControlService.OnNextRequested += () => Avalonia.Threading.Dispatcher.UIThread.Post(NextVerse);
            _remoteControlService.OnPrevRequested += () => Avalonia.Threading.Dispatcher.UIThread.Post(PreviousVerse);
            _remoteControlService.OnLoadRequested += (num, cat) => Avalonia.Threading.Dispatcher.UIThread.Post(async () => 
            {
                SelectedCategory = cat;
                HymnNumberInput = num.ToString();
                await LoadHymnAsync();
            });
            _remoteControlService.OnToggleBlankRequested += () => Avalonia.Threading.Dispatcher.UIThread.Post(BlankDisplay);
        }

        // Settings will be loaded asynchronously in InitializeAsync
        Settings = new RemoteWidgetSettings();

        // Subscribe to audio events if available
        if (_audioService != null)
        {
            _audioService.StateChanged += OnPlaybackStateChanged;
        }
    }

    /// <summary>
    /// Initialize the ViewModel asynchronously - call this after construction
    /// </summary>
    public async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            // Load settings asynchronously
            Settings = await _settingsService.LoadRemoteWidgetSettingsAsync();
            IsAlwaysOnTop = Settings.AlwaysOnTop;
            IsPositionLocked = Settings.IsLocked;
            ShowNumberPad = Settings.ShowNumberPad;
            if (!string.IsNullOrEmpty(Settings.LastCategory))
            {
                SelectedCategory = Settings.LastCategory;
                SelectedCategoryItem = Categories.FirstOrDefault(c => c.Slug == SelectedCategory);
            }
            else
            {
                SelectedCategoryItem = Categories.FirstOrDefault(c => c.Slug == "crestine");
            }

            // Ensure localization strings are loaded for initial state
            if (string.IsNullOrEmpty(ActiveHymnTitle))
            {
                ActiveHymnTitle = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
                CurrentHymnDisplay = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
            }

            UpdateSlotLabels();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void UpdateSlotLabels()
    {
        Slot1Text = Settings.QuickSlots[0] > 0 ? Settings.QuickSlots[0].ToString() : "";
        Slot2Text = Settings.QuickSlots[1] > 0 ? Settings.QuickSlots[1].ToString() : "";
        Slot3Text = Settings.QuickSlots[2] > 0 ? Settings.QuickSlots[2].ToString() : "";
        Slot4Text = Settings.QuickSlots[3] > 0 ? Settings.QuickSlots[3].ToString() : "";
        Slot5Text = Settings.QuickSlots[4] > 0 ? Settings.QuickSlots[4].ToString() : "";
        Slot6Text = Settings.QuickSlots[5] > 0 ? Settings.QuickSlots[5].ToString() : "";
        Slot7Text = Settings.QuickSlots[6] > 0 ? Settings.QuickSlots[6].ToString() : "";
        Slot8Text = Settings.QuickSlots[7] > 0 ? Settings.QuickSlots[7].ToString() : "";

        Slot1Label = Settings.QuickSlotLabels[0];
        Slot2Label = Settings.QuickSlotLabels[1];
        Slot3Label = Settings.QuickSlotLabels[2];
        Slot4Label = Settings.QuickSlotLabels[3];
        Slot5Label = Settings.QuickSlotLabels[4];
        Slot6Label = Settings.QuickSlotLabels[5];
        Slot7Label = Settings.QuickSlotLabels[6];
        Slot8Label = Settings.QuickSlotLabels[7];
    }

    [ObservableProperty]
    private string _slot1Text = "";

    [ObservableProperty]
    private string _slot2Text = "";

    [ObservableProperty]
    private string _slot3Text = "";

    [ObservableProperty]
    private string _slot4Text = "";

    [ObservableProperty]
    private string _slot5Text = "";

    [ObservableProperty]
    private string _slot6Text = "";

    [ObservableProperty]
    private string _slot7Text = "";

    [ObservableProperty]
    private string _slot8Text = "";

    private string _slot1Label = "";
    public string Slot1Label
    {
        get => _slot1Label;
        set
        {
            if (SetProperty(ref _slot1Label, value))
            {
                UpdateSlotLabel(0, value);
            }
        }
    }

    private string _slot2Label = "";
    public string Slot2Label
    {
        get => _slot2Label;
        set
        {
            if (SetProperty(ref _slot2Label, value))
            {
                UpdateSlotLabel(1, value);
            }
        }
    }

    private string _slot3Label = "";
    public string Slot3Label
    {
        get => _slot3Label;
        set
        {
            if (SetProperty(ref _slot3Label, value))
            {
                UpdateSlotLabel(2, value);
            }
        }
    }

    private string _slot4Label = "";
    public string Slot4Label
    {
        get => _slot4Label;
        set
        {
            if (SetProperty(ref _slot4Label, value))
            {
                UpdateSlotLabel(3, value);
            }
        }
    }

    private string _slot5Label = "";
    public string Slot5Label
    {
        get => _slot5Label;
        set
        {
            if (SetProperty(ref _slot5Label, value))
            {
                UpdateSlotLabel(4, value);
            }
        }
    }

    private string _slot6Label = "";
    public string Slot6Label
    {
        get => _slot6Label;
        set
        {
            if (SetProperty(ref _slot6Label, value))
            {
                UpdateSlotLabel(5, value);
            }
        }
    }

    private string _slot7Label = "";
    public string Slot7Label
    {
        get => _slot7Label;
        set
        {
            if (SetProperty(ref _slot7Label, value))
            {
                UpdateSlotLabel(6, value);
            }
        }
    }

    private string _slot8Label = "";
    public string Slot8Label
    {
        get => _slot8Label;
        set
        {
            if (SetProperty(ref _slot8Label, value))
            {
                UpdateSlotLabel(7, value);
            }
        }
    }

    partial void OnSlot1TextChanged(string value) => UpdateSlot(0, value);
    partial void OnSlot2TextChanged(string value) => UpdateSlot(1, value);
    partial void OnSlot3TextChanged(string value) => UpdateSlot(2, value);
    partial void OnSlot4TextChanged(string value) => UpdateSlot(3, value);
    partial void OnSlot5TextChanged(string value) => UpdateSlot(4, value);
    partial void OnSlot6TextChanged(string value) => UpdateSlot(5, value);
    partial void OnSlot7TextChanged(string value) => UpdateSlot(6, value);
    partial void OnSlot8TextChanged(string value) => UpdateSlot(7, value);

    // Label change handlers removed as we handle them in the setters now

    private void UpdateSlotLabel(int index, string value)
    {
        if (Settings.QuickSlotLabels == null) Settings.QuickSlotLabels = new List<string> { "", "", "", "", "", "", "", "" };
        while (Settings.QuickSlotLabels.Count < 8) Settings.QuickSlotLabels.Add("");
        
        if (Settings.QuickSlotLabels.Count > index)
        {
            Settings.QuickSlotLabels[index] = value ?? "";
            SaveSettings();
        }
    }

    private void UpdateSlot(int index, string value)
    {
        if (int.TryParse(value, out int number))
        {
            Settings.QuickSlots[index] = number;
            SaveSettings();
        }
        else if (string.IsNullOrEmpty(value))
        {
            Settings.QuickSlots[index] = 0;
            SaveSettings();
        }
    }


    [RelayCommand]
    private async Task SaveCurrentToSlot(string slotIndexStr)
    {
        if (int.TryParse(slotIndexStr, out int index) && index >= 1 && index <= 8)
        {
            int number = 0;
            string title = "";
            
            if (_currentHymn != null)
            {
                number = _currentHymn.Number;
                title = _currentHymn.Title;
            }
            else if (int.TryParse(HymnNumberInput, out number))
            {
                // If not loaded but number in box, try to get title for label
                var hymn = await _hymnService.GetHymnByNumberAsync(number, SelectedCategory);
                if (hymn != null) title = hymn.Title;
            }

            if (number > 0)
            {
                Settings.QuickSlots[index - 1] = number;
                if (!string.IsNullOrEmpty(title))
                {
                    Settings.QuickSlotLabels[index - 1] = title;
                }
                
                UpdateSlotLabels();
                SaveSettings();
                StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.SettingsSaved"), number, index);
            }
        }
    }

    [RelayCommand]
    private async Task LoadFromSlot(string slotIndexStr)
    {
        if (int.TryParse(slotIndexStr, out int index) && index >= 1 && index <= 8)
        {
            // Try to use the text in the textbox first (it might be fresher)
            string text = index switch { 
                1 => Slot1Text, 2 => Slot2Text, 3 => Slot3Text, 4 => Slot4Text, 
                5 => Slot5Text, 6 => Slot6Text, 7 => Slot7Text, 8 => Slot8Text,
                _ => "" 
            };
            
            if (int.TryParse(text, out int number) && number > 0)
            {
                HymnNumberInput = number.ToString();
                await LoadHymnAsync();
            }
            else
            {
                number = Settings.QuickSlots[index - 1];
                if (number > 0)
                {
                    HymnNumberInput = number.ToString();
                    await LoadHymnAsync();
                }
            }
        }
    }

    [RelayCommand]
    private async Task LoadHymnAsync()
    {
        if (string.IsNullOrWhiteSpace(HymnNumberInput))
        {
            ShowError(LocalizationManager.Instance.GetString("Status.Error", "Please enter a hymn number"));
            return;
        }

        Hymn? hymn = null;
        if (int.TryParse(HymnNumberInput, out int hymnNumber))
        {
            hymn = await _hymnService.GetHymnByNumberAsync(hymnNumber, SelectedCategory);
        }
        else
        {
            // Title search
            StatusMessage = LocalizationManager.Instance.GetString("Status.Searching", "Searching...");
            var results = await _searchService.SearchHymnsAsync(HymnNumberInput, SelectedCategory);
            if (results.Any())
            {
                // Take the first best match
                var bestMatch = results.First();
                hymn = await _hymnService.GetHymnByNumberAsync(bestMatch.Number, bestMatch.CategorySlug);
            }
        }

        try
        {
            if (hymn == null)
            {
                ShowError(string.Format(LocalizationManager.Instance.GetString("Status.HymnNotFound"), HymnNumberInput, SelectedCategory));
                return;
            }

            _currentHymn = hymn;
            if (_currentHymn.Verses != null)
            {
                _currentHymn.Verses = ExpandVersesWithChorus(_currentHymn.Verses.ToList());
            }
            _currentVerseIndex = 0;

            // Update display
            ActiveHymnNumber = hymn.Number.ToString();
            ActiveHymnTitle = hymn.Title;
            CurrentHymnDisplay = $"{hymn.Number} {hymn.Title}";
            UpdateVerseIndicator();
            HymnNumberInput = ""; // Clear for next input

            // Check audio availability
            IsAudioAvailable = hymn.AudioRecordings?.Any() == true;

            UpdateNavigationButtons();
            StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.HymnLoaded"), hymn.Title, hymn.Verses?.Count ?? 0);

            // Sync to Broadcast Center
            _ = _broadcastSync.SyncHymnAsync(hymn.Number, hymn.Title);

            // Save last hymn number
            Settings.LastHymnNumber = hymnNumber;
            Settings.LastCategory = SelectedCategory;
            SaveSettings();

            // Show hymn on display
            OnShowHymnRequested?.Invoke(hymn, 0);

            // Clear focus from input to enable arrow keys immediately
            OnRequestFocusClear?.Invoke();
        }
        catch (Exception ex)
        {
            ShowError(string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message));
        }
    }

    [RelayCommand]
    private void NumberPadPress(string digit)
    {
        HymnNumberInput += digit;
        // The UI should stay focused on the TextBox if possible, 
        // but since buttons take focus, we don't need to do much here
        // as the LoadHymnCommand will clear it anyway.
    }

    [RelayCommand]
    private void NumberPadBackspace()
    {
        if (!string.IsNullOrEmpty(HymnNumberInput))
        {
            HymnNumberInput = HymnNumberInput[..^1];
        }
    }

    [RelayCommand]
    private void NextVerse()
    {
        if (_currentHymn != null)
        {
            if (_currentVerseIndex < (_currentHymn.Verses?.Count ?? 0) - 1)
            {
                _currentVerseIndex++;
                UpdateVerseIndicator();
                UpdateNavigationButtons();

                // Update DisplayWindow with new verse
                OnShowHymnRequested?.Invoke(_currentHymn, _currentVerseIndex);
            }
            else
            {
                // On last slide, exit the display
                StatusMessage = LocalizationManager.Instance.GetString("Status.Finalized");
                BlankDisplay();
            }
        }
    }

    [RelayCommand]
    private void PreviousVerse()
    {
        if (_currentHymn != null && _currentVerseIndex > 0)
        {
            _currentVerseIndex--;
            UpdateVerseIndicator();
            UpdateNavigationButtons();

            // Update DisplayWindow with new verse
            if (_currentHymn != null)
            {
                OnShowHymnRequested?.Invoke(_currentHymn, _currentVerseIndex);
            }
        }
    }

    [RelayCommand]
    private void TogglePresenterView()
    {
        OnTogglePresenterViewRequested?.Invoke();
    }

    [RelayCommand]
    public void BlankDisplay()
    {
        _currentHymn = null;
        _currentVerseIndex = 0;
        ActiveHymnNumber = "";
        ActiveHymnTitle = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
        CurrentHymnDisplay = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
        VerseIndicator = "";
        UpdateNavigationButtons();
        OnBlankDisplayRequested?.Invoke();
        _ = _broadcastSync.HideHymnAsync();
        StatusMessage = LocalizationManager.Instance.GetString("Status.Ready");

        if (_remoteControlService != null)
        {
            _remoteControlService.CurrentHymnNumber = "";
            _remoteControlService.CurrentHymnTitle = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
            _remoteControlService.CurrentVerseIndicator = "";
        }
    }

    [RelayCommand(CanExecute = nameof(IsAudioAvailable))]
    private async Task ToggleAudioAsync()
    {
        if (_audioService == null || _currentHymn == null)
            return;

        try
        {
            if (IsPlaying)
            {
                await _audioService.PauseAsync();
            }
            else
            {
                if (!_audioService.IsLoaded && _currentHymn.AudioRecordings?.Any() == true)
                {
                    // Load audio file first
                    var audioRecording = _currentHymn.AudioRecordings.First();
                    var audioLibraryPath = await _settingsService.GetAudioLibraryPathAsync();
                    await _audioService.LoadAsync(audioRecording, audioLibraryPath);
                }
                await _audioService.PlayAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError(string.Format(LocalizationManager.Instance.GetString("Status.Error"), ex.Message));
        }
    }

    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        IsAlwaysOnTop = !IsAlwaysOnTop;
        Settings.AlwaysOnTop = IsAlwaysOnTop;
        SaveSettings();
    }

    [RelayCommand]
    private void ToggleLockPosition()
    {
        IsPositionLocked = !IsPositionLocked;
        Settings.IsLocked = IsPositionLocked;
        SaveSettings();
    }

    [RelayCommand]
    private void ToggleNumberPad()
    {
        ShowNumberPad = !ShowNumberPad;
        Settings.ShowNumberPad = ShowNumberPad;
        SaveSettings();
    }

    [RelayCommand]
    private void OpenAdvancedMode()
    {
        // This will be handled by the RemoteWidget code-behind
    }

    private void UpdateNavigationButtons()
    {
        if (_currentHymn?.Verses == null)
        {
            CanNavigateNext = false;
            CanNavigatePrevious = false;
            return;
        }

        CanNavigatePrevious = _currentVerseIndex > 0;
        CanNavigateNext = _currentHymn?.Verses != null;

        // Notify command can-execute changed
        NextVerseCommand.NotifyCanExecuteChanged();
        PreviousVerseCommand.NotifyCanExecuteChanged();
    }

    private void OnPlaybackStateChanged(object? sender, PlaybackState state)
    {
        if (_audioService != null)
        {
            IsPlaying = _audioService.PlaybackState == PlaybackState.Playing;
            ToggleAudioCommand.NotifyCanExecuteChanged();
        }
    }

    private void ShowError(string message)
    {
        StatusMessage = string.Format(LocalizationManager.Instance.GetString("Status.Error"), message);
        // TODO: Show toast notification
    }

    private async void SaveSettings()
    {
        if (_isInitializing) return;
        await _settingsService.SaveRemoteWidgetSettingsAsync(Settings);
        StatusMessage = LocalizationManager.Instance.GetString("Status.SettingsSaved");
        // Reset status message after 3 seconds
        _ = Task.Delay(3000).ContinueWith(_ => StatusMessage = LocalizationManager.Instance.GetString("Status.Ready"));
    }

    public void UpdatePosition(double x, double y)
    {
        if (!Settings.IsLocked)
        {
            Settings.PositionX = x;
            Settings.PositionY = y;
            SaveSettings();
        }
    }

    private void UpdateVerseIndicator()
    {
        if (_currentHymn == null || _currentHymn.Verses == null)
        {
            VerseIndicator = LocalizationManager.Instance.GetString("Main.NoHymnLoaded");
            return;
        }

        var verse = _currentHymn.Verses.ElementAtOrDefault(_currentVerseIndex);
        if (verse == null) return;

        if (verse.Label == "Title")
        {
            VerseIndicator = LocalizationManager.Instance.GetString("Text.Title");
            return;
        }

        var label = ExtractNumberPrefix(verse.Content) ?? verse.Label;
        if (label == "Refren" || label == "Chorus") label = LocalizationManager.Instance.GetString("Text.Chorus");
        
        VerseIndicator = $"{label} ({_currentVerseIndex + 1}/{_currentHymn.Verses.Count})";

        if (_remoteControlService != null)
        {
            _remoteControlService.CurrentHymnNumber = ActiveHymnNumber;
            _remoteControlService.CurrentHymnTitle = ActiveHymnTitle;
            _remoteControlService.CurrentVerseIndicator = VerseIndicator;
        }
    }

    private string? ExtractNumberPrefix(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        var firstLine = content.Split('\n')[0].Trim();
        var match = System.Text.RegularExpressions.Regex.Match(firstLine, @"^(\d+[\.:])\s*");
        return match.Success ? match.Groups[1].Value : null;
    }

    private string StripNumberPrefix(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return content;
        var lines = content.Split('\n');
        var firstLine = lines[0].Trim();
        var match = System.Text.RegularExpressions.Regex.Match(firstLine, @"^\d+[\.:]\s*(.*)");
        if (match.Success)
        {
            lines[0] = match.Groups[1].Value;
            return string.Join('\n', lines);
        }
        return content;
    }

    private List<Verse> ExpandVersesWithChorus(List<Verse> baseVerses)
    {
        if (baseVerses == null || baseVerses.Count <= 1) return baseVerses ?? new();

        var expanded = new List<Verse>();
        var actualVerses = baseVerses.ToList();
        
        // Handle Title slide if present (RemoteWidget might have it at index 0)
        var titleSlide = actualVerses.FirstOrDefault(v => v.Label == "Title");
        if (titleSlide != null)
        {
            expanded.Add(titleSlide);
            actualVerses.Remove(titleSlide);
        }

        var chorus = actualVerses.FirstOrDefault(v => v.Label == "Refren" || v.Label == "Chorus");
        
        // If there's no chorus or it's already repeated, return as is
        if (chorus == null || actualVerses.Count(v => v.Label == "Refren" || v.Label == "Chorus") > 1)
        {
            return baseVerses;
        }

        // Expand: Verse 1, Chorus, Verse 2, Chorus...
        var numberedVerses = actualVerses.Where(v => v.Label != "Refren" && v.Label != "Chorus").ToList();
        
        for (int i = 0; i < numberedVerses.Count; i++)
        {
            expanded.Add(numberedVerses[i]);
            expanded.Add(chorus);
        }

        return expanded;
    }

    public void SyncVerseIndex(int newIndex)
    {
        // Only sync if it's actually different AND we're not currently in the middle of a command
        if (_currentVerseIndex != newIndex && newIndex >= 0 && (_currentHymn != null && newIndex < _currentHymn.Verses.Count))
        {
            _currentVerseIndex = newIndex;
            UpdateVerseIndicator();
            UpdateNavigationButtons();
        }
    }

    public void SyncHymn(Hymn hymn, int verseIndex)
    {
        _currentHymn = hymn;
        _currentVerseIndex = verseIndex;
        ActiveHymnNumber = hymn.Number.ToString();
        ActiveHymnTitle = hymn.Title;
        CurrentHymnDisplay = $"{hymn.Number} {hymn.Title}";
        UpdateVerseIndicator();
        UpdateNavigationButtons();
        IsAudioAvailable = hymn.AudioRecordings?.Any() == true;
    }
}
