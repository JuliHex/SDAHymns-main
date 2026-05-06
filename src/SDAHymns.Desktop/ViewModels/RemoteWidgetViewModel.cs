using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Models;
using SDAHymns.Core.Services;
using System;
using System.Threading.Tasks;

namespace SDAHymns.Desktop.ViewModels;

public partial class RemoteWidgetViewModel : ViewModelBase
{
    private readonly IHymnDisplayService _hymnService;
    private readonly IAudioPlayerService? _audioService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _hymnNumberInput = "";

    [ObservableProperty]
    private string _currentHymnDisplay = "No hymn loaded";

    [ObservableProperty]
    private string _verseIndicator = "";

    [ObservableProperty]
    private string _selectedCategory = "crestine";

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
    private string _statusMessage = "Ready";

    public RemoteWidgetSettings Settings { get; set; }

    private Hymn? _currentHymn;
    private int _currentVerseIndex = 0;

    // Window callbacks
    public Action<Hymn, int>? OnShowHymnRequested { get; set; }
    public Action? OnBlankDisplayRequested { get; set; }

    public RemoteWidgetViewModel(
        IHymnDisplayService hymnService,
        ISettingsService settingsService,
        IAudioPlayerService? audioService = null)
    {
        _hymnService = hymnService;
        _settingsService = settingsService;
        _audioService = audioService;

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
        // Load settings asynchronously
        Settings = await _settingsService.LoadRemoteWidgetSettingsAsync();
        IsAlwaysOnTop = Settings.AlwaysOnTop;
        IsPositionLocked = Settings.IsLocked;
        ShowNumberPad = Settings.ShowNumberPad;
        if (!string.IsNullOrEmpty(Settings.LastCategory))
        {
            SelectedCategory = Settings.LastCategory;
        }
    }

    [RelayCommand]
    private async Task LoadHymnAsync()
    {
        if (string.IsNullOrWhiteSpace(HymnNumberInput))
        {
            ShowError("Please enter a hymn number");
            return;
        }

        if (!int.TryParse(HymnNumberInput, out int hymnNumber))
        {
            ShowError("Invalid hymn number");
            return;
        }

        try
        {
            var hymn = await _hymnService.GetHymnByNumberAsync(hymnNumber, SelectedCategory);

            if (hymn == null)
            {
                ShowError($"Hymn {hymnNumber} not found");
                return;
            }

            _currentHymn = hymn;
            _currentVerseIndex = 0;

            // Update display
            CurrentHymnDisplay = $"Hymn {hymn.Number}";
            var currentVerseLabel = hymn.Verses?.FirstOrDefault()?.Label ?? "Verse 1";
            VerseIndicator = $"{currentVerseLabel}/{hymn.Verses?.Count ?? 0}";
            HymnNumberInput = ""; // Clear for next input

            // Check audio availability
            IsAudioAvailable = hymn.AudioRecordings?.Any() == true;

            UpdateNavigationButtons();
            StatusMessage = $"Loaded: {hymn.Title}";

            // Save last hymn number
            Settings.LastHymnNumber = hymnNumber;
            Settings.LastCategory = SelectedCategory;
            SaveSettings();

            // Show hymn on display
            OnShowHymnRequested?.Invoke(hymn, 0);
        }
        catch (Exception ex)
        {
            ShowError($"Error loading hymn: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NumberPadPress(string digit)
    {
        HymnNumberInput += digit;
    }

    [RelayCommand]
    private void NumberPadBackspace()
    {
        if (!string.IsNullOrEmpty(HymnNumberInput))
        {
            HymnNumberInput = HymnNumberInput[..^1];
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void NextVerse()
    {
        if (_currentHymn != null && _currentVerseIndex < (_currentHymn.Verses?.Count ?? 0) - 1)
        {
            _currentVerseIndex++;
            var currentVerseLabel = _currentHymn.Verses?.ElementAtOrDefault(_currentVerseIndex)?.Label ?? $"Verse {_currentVerseIndex + 1}";
            VerseIndicator = $"{currentVerseLabel}/{_currentHymn.Verses?.Count ?? 0}";
            UpdateNavigationButtons();

            // Update DisplayWindow with new verse
            if (_currentHymn != null)
            {
                OnShowHymnRequested?.Invoke(_currentHymn, _currentVerseIndex);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrevious))]
    private void PreviousVerse()
    {
        if (_currentHymn != null && _currentVerseIndex > 0)
        {
            _currentVerseIndex--;
            var currentVerseLabel = _currentHymn.Verses?.ElementAtOrDefault(_currentVerseIndex)?.Label ?? $"Verse {_currentVerseIndex + 1}";
            VerseIndicator = $"{currentVerseLabel}/{_currentHymn.Verses?.Count ?? 0}";
            UpdateNavigationButtons();

            // Update DisplayWindow with new verse
            if (_currentHymn != null)
            {
                OnShowHymnRequested?.Invoke(_currentHymn, _currentVerseIndex);
            }
        }
    }

    [RelayCommand]
    private void BlankDisplay()
    {
        OnBlankDisplayRequested?.Invoke();
        StatusMessage = "Display blanked";
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
            ShowError($"Audio error: {ex.Message}");
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
        CanNavigateNext = _currentVerseIndex < _currentHymn.Verses.Count - 1;

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
        StatusMessage = $"Error: {message}";
        // TODO: Show toast notification
    }

    private async void SaveSettings()
    {
        await _settingsService.SaveRemoteWidgetSettingsAsync(Settings);
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
}
