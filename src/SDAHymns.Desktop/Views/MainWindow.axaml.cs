using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.ViewModels;

namespace SDAHymns.Desktop.Views;

public partial class MainWindow : Window
{
    private DisplayWindow? _displayWindow;
    private readonly IHotKeyManager _hotKeyManager;
    private IServiceProvider? _serviceProvider;

    public MainWindow(IHotKeyManager hotKeyManager)
    {
        _hotKeyManager = hotKeyManager;

        InitializeComponent();

        // Handle window closing to clean up display window
        Closing += MainWindow_Closing;

        // Handle aspect ratio changes
        DataContextChanged += (s, e) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                RegisterKeyboardActions(viewModel);
            }
        };

        // Global keyboard handler
        KeyDown += MainWindow_KeyDown;

        // Load custom shortcuts
        _hotKeyManager.LoadCustomBindings();
    }

    private void RegisterKeyboardActions(MainWindowViewModel viewModel)
    {
        // Global shortcuts
        _hotKeyManager.RegisterAction("FocusSearch", () =>
        {
            SearchTextBox.Focus();
        });

        _hotKeyManager.RegisterAction("ClearSearch", () =>
        {
            if (SearchTextBox.IsFocused && !string.IsNullOrEmpty(viewModel.SearchQuery))
            {
                viewModel.SearchQuery = string.Empty;
            }
        });

        _hotKeyManager.RegisterAction("ToggleDisplay", () =>
        {
            ToggleDisplayWindow();
        });

        _hotKeyManager.RegisterAction("ToggleFullscreen", () =>
        {
            if (_displayWindow != null)
            {
                _displayWindow.WindowState = _displayWindow.WindowState == WindowState.FullScreen
                    ? WindowState.Normal
                    : WindowState.FullScreen;
            }
        });

        // Navigation shortcuts
        _hotKeyManager.RegisterAction("NextVerse", () =>
        {
            if (viewModel.NextVerseCommand.CanExecute(null))
                viewModel.NextVerseCommand.Execute(null);
        });

        _hotKeyManager.RegisterAction("PreviousVerse", () =>
        {
            if (viewModel.PreviousVerseCommand.CanExecute(null))
                viewModel.PreviousVerseCommand.Execute(null);
        });

        _hotKeyManager.RegisterAction("NextVerseArrow", () =>
        {
            // Only if search box is not focused
            if (!SearchTextBox.IsFocused && viewModel.NextVerseCommand.CanExecute(null))
                viewModel.NextVerseCommand.Execute(null);
        });

        _hotKeyManager.RegisterAction("PreviousVerseArrow", () =>
        {
            // Only if search box is not focused
            if (!SearchTextBox.IsFocused && viewModel.PreviousVerseCommand.CanExecute(null))
                viewModel.PreviousVerseCommand.Execute(null);
        });

        _hotKeyManager.RegisterAction("NextVersePage", () =>
        {
            if (viewModel.NextVerseCommand.CanExecute(null))
                viewModel.NextVerseCommand.Execute(null);
        });

        _hotKeyManager.RegisterAction("PreviousVersePage", () =>
        {
            if (viewModel.PreviousVerseCommand.CanExecute(null))
                viewModel.PreviousVerseCommand.Execute(null);
        });

        _hotKeyManager.RegisterAction("FirstVerse", () =>
        {
            if (viewModel.CurrentHymn != null && viewModel.Verses.Count > 0)
            {
                viewModel.CurrentVerseIndex = 0;
            }
        });

        _hotKeyManager.RegisterAction("LastVerse", () =>
        {
            if (viewModel.CurrentHymn != null && viewModel.Verses.Count > 0)
            {
                viewModel.CurrentVerseIndex = viewModel.Verses.Count - 1;
            }
        });

        // Search shortcuts
        _hotKeyManager.RegisterAction("SelectHymn", () =>
        {
            // Only if search results are visible and search box is focused
            if (SearchTextBox.IsFocused && viewModel.SelectedSearchResult != null)
            {
                // Loading is already handled by SelectedSearchResult setter
            }
        });

        _hotKeyManager.RegisterAction("ToggleFavorite", () =>
        {
            if (viewModel.ToggleFavoriteCommand.CanExecute(null))
                viewModel.ToggleFavoriteCommand.Execute(null);
        });

        // Recent hymns shortcuts (Ctrl+1 through Ctrl+5)
        for (int i = 1; i <= 5; i++)
        {
            int index = i - 1; // Capture for closure
            _hotKeyManager.RegisterAction($"LoadRecent{i}", () =>
            {
                if (viewModel.RecentHymns.Count > index)
                {
                    var recentHymn = viewModel.RecentHymns[index];
                    viewModel.LoadHymnFromRecentCommand.Execute(recentHymn);
                }
            });
        }

        // Display control (blank screen)
        _hotKeyManager.RegisterAction("BlankDisplay", () =>
        {
            // TODO: Implement blank display functionality in future
        });

        // Help overlay
        _hotKeyManager.RegisterAction("ShowHelpOverlay", () =>
        {
            ShowShortcutsWindow();
        });
    }

    private void ShowShortcutsWindow()
    {
        var shortcutsWindow = new ShortcutsWindow
        {
            DataContext = new ShortcutsWindowViewModel(_hotKeyManager)
        };

        shortcutsWindow.ShowDialog(this);
    }

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        // Convert Avalonia types to strings for the hot key manager
        var key = e.Key.ToString();
        var modifiers = ConvertModifiersToString(e.KeyModifiers);

        // Let the hotkey manager handle the key press
        var handled = _hotKeyManager.HandleKeyPress(key, modifiers);
        e.Handled = handled;
    }

    private static string ConvertModifiersToString(KeyModifiers modifiers)
    {
        var parts = new List<string>();

        if (modifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (modifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (modifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        if (modifiers.HasFlag(KeyModifiers.Meta))
            parts.Add("Meta");

        return string.Join("+", parts.OrderBy(p => p));
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var viewModel = (MainWindowViewModel)sender!;

        if (e.PropertyName == nameof(MainWindowViewModel.IsAspectRatio43) && _displayWindow != null)
        {
            _displayWindow.Width = viewModel.DisplayWidth;
            _displayWindow.Height = viewModel.DisplayHeight;
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.ActiveProfile) && _displayWindow != null)
        {
            // Apply the new profile to the display window
            if (viewModel.ActiveProfile != null)
            {
                _displayWindow.ApplyProfile(viewModel.ActiveProfile);
            }
        }
    }

    public void ToggleDisplayWindow()
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        if (_displayWindow == null || !_displayWindow.IsVisible)
        {
            // Create and show display window with correct aspect ratio
            _displayWindow = new DisplayWindow
            {
                DataContext = viewModel,
                Width = viewModel.DisplayWidth,
                Height = viewModel.DisplayHeight
            };

            // Apply the active profile to the display window
            if (viewModel.ActiveProfile != null)
            {
                _displayWindow.ApplyProfile(viewModel.ActiveProfile);
            }

            _displayWindow.Closed += (s, e) =>
            {
                viewModel.IsDisplayWindowOpen = false;
                _displayWindow = null;
            };

            _displayWindow.Show();
            viewModel.IsDisplayWindowOpen = true;
        }
        else
        {
            // Hide display window
            _displayWindow.Close();
            viewModel.IsDisplayWindowOpen = false;
            _displayWindow = null;
        }
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        // Close display window when main window closes
        _displayWindow?.Close();
    }

    private void ToggleDisplayWindow_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ToggleDisplayWindow();
    }

    private async void EditProfiles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || _serviceProvider == null)
            return;

        // Get the profile service from DI
        var profileService = _serviceProvider.GetService<IDisplayProfileService>();
        if (profileService == null)
            return;

        // Create the profile editor window
        var profileEditorViewModel = new ProfileEditorViewModel(profileService);
        var profileEditorWindow = new ProfileEditorWindow
        {
            DataContext = profileEditorViewModel
        };

        // Show as dialog
        await profileEditorWindow.ShowDialog(this);

        // Reload profiles after editing
        await viewModel.LoadProfilesCommand.ExecuteAsync(null);
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private async void OpenSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_serviceProvider == null)
            return;

        // Get the SettingsWindowViewModel from DI
        var settingsViewModel = _serviceProvider.GetService<SettingsWindowViewModel>();
        if (settingsViewModel == null)
            return;

        // Create the settings window
        var settingsWindow = new SettingsWindow
        {
            DataContext = settingsViewModel
        };

        // Show as dialog
        await settingsWindow.ShowDialog(this);
    }

    private async void RecordTimings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || _serviceProvider == null)
            return;

        // Check if we have a hymn loaded with audio
        if (viewModel.CurrentHymn == null || viewModel.CurrentAudioRecording == null)
        {
            // TODO: Show message to user
            return;
        }

        // Get the audio player service from DI
        var audioPlayer = _serviceProvider.GetService<IAudioPlayerService>();
        if (audioPlayer == null)
            return;

        // Create the recorder mode ViewModel
        var recorderViewModel = new RecorderModeViewModel(audioPlayer);
        recorderViewModel.LoadHymn(viewModel.CurrentHymn, viewModel.CurrentAudioRecording);

        // Load existing timings if any
        if (!string.IsNullOrWhiteSpace(viewModel.CurrentAudioRecording.TimingMapJson))
        {
            var timingRecorder = new TimingRecorder(audioPlayer);
            timingRecorder.LoadTimingMap(viewModel.CurrentAudioRecording.TimingMapJson);
        }

        // Create and show the recorder window
        var recorderWindow = new RecorderModeWindow
        {
            DataContext = recorderViewModel
        };

        var result = await recorderWindow.ShowDialog<bool?>(this);

        // If user saved timings, update the database
        if (result == true)
        {
            var timingMapJson = recorderViewModel.GetTimingMapJson();
            await viewModel.SaveAudioTimingsCommand.ExecuteAsync(timingMapJson);
        }
    }
}
