using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.ViewModels;
using System;
using System.Linq;

namespace SDAHymns.Desktop.Views;

public partial class RemoteWidget : Window
{
    private RemoteWidgetViewModel? ViewModel => DataContext as RemoteWidgetViewModel;
    private DisplayWindow? _displayWindow;
    private PresenterView? _presenterView;
    private MainWindowViewModel? _sharedProjectionViewModel;
    private IServiceProvider? _serviceProvider;

    public RemoteWidget()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        // Add global keyboard handling for arrows (Tunnel captures keys BEFORE buttons can steal them)
        AddHandler(KeyDownEvent, RemoteWidget_KeyDown!, RoutingStrategies.Tunnel);

        // Set initial position
        Opened += OnOpened;
        PositionChanged += OnPositionChanged;
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private void RemoteWidget_KeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel == null) return;

        // If focus is in a TextBox, don't intercept arrow keys (so user can still type/edit)
        if (this.FocusManager?.GetFocusedElement() is TextBox && e.Key != Key.Enter)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Right:
            case Key.Down:
                if (ViewModel.NextVerseCommand.CanExecute(null))
                    ViewModel.NextVerseCommand.Execute(null);
                e.Handled = true;
                break;


            case Key.Left:
            case Key.Up:
                if (ViewModel.PreviousVerseCommand.CanExecute(null))
                    ViewModel.PreviousVerseCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Enter:
                // Unfocus the input box by focusing the window
                this.Focus();
                e.Handled = true;
                break;
        }
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (ViewModel == null) return;

        // Initialize ViewModel asynchronously
        await ViewModel.InitializeAsync();

        // Wire up callbacks
        ViewModel.OnShowHymnRequested = ShowHymnOnDisplay;
        ViewModel.OnBlankDisplayRequested = BlankDisplay;
        ViewModel.OnTogglePresenterViewRequested = TogglePresenterView;
        ViewModel.OnRequestFocusClear = () => this.Focus();

        // Sync with shared ViewModel
        var sharedVm = GetSharedProjectionViewModel();
        if (sharedVm != null)
        {
            sharedVm.PropertyChanged += SharedVm_PropertyChanged;
        }

        // Load saved position or use default
        if (!double.IsNaN(ViewModel.Settings.PositionX) && !double.IsNaN(ViewModel.Settings.PositionY))
        {
            // Restore saved position
            Position = new PixelPoint((int)ViewModel.Settings.PositionX, (int)ViewModel.Settings.PositionY);
        }
        else
        {
            // Use default position (bottom-right corner)
            Position = GetDefaultPosition();
        }

        // Focus the input box on startup
        var hymnInput = this.FindControl<TextBox>("HymnInput");
        hymnInput?.Focus();
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        // Save position if not locked
        if (ViewModel != null && !ViewModel.Settings.IsLocked)
        {
            ViewModel.UpdatePosition(e.Point.X, e.Point.Y);
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Only allow dragging if position is not locked
        if (ViewModel?.Settings.IsLocked == false)
        {
            BeginMoveDrag(e);
        }
        else
        {
            // TODO: Show tooltip "Position locked"
        }
    }

    private void Minimize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Close DisplayWindow if open
        _displayWindow?.Close();
        Close();
    }

    private void AdvancedMode_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_serviceProvider == null) return;

        // Open MainWindow
        var hotKeyManager = _serviceProvider.GetRequiredService<IHotKeyManager>();
        var mainWindow = new MainWindow(hotKeyManager)
        {
            DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
        };

        mainWindow.SetServiceProvider(_serviceProvider);
        mainWindow.Show();
    }

    private void ShowPresenter()
    {
        if (_presenterView != null) return;
        
        _presenterView = new PresenterView();
        _presenterView.Closed += (s, e) => {
            _presenterView = null;
            if (_displayWindow == null) ViewModel?.BlankDisplay();
        };
        _presenterView.Show();
    }

    public void BlankDisplay()
    {
        _displayWindow?.Close();
        _displayWindow = null;
        _presenterView?.Close();
        _presenterView = null;
    }

    private MainWindowViewModel? GetSharedProjectionViewModel()
    {
        if (_sharedProjectionViewModel == null && _serviceProvider != null)
        {
            _sharedProjectionViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        }
        return _sharedProjectionViewModel;
    }

    public void TogglePresenterView()
    {
        if (_presenterView != null)
        {
            _presenterView.Close();
            _presenterView = null;
            return;
        }

        var presenterViewModel = GetSharedProjectionViewModel();
        if (presenterViewModel == null) return;

        _presenterView = new PresenterView
        {
            DataContext = presenterViewModel
        };

        _presenterView.Closed += (s, e) => {
            _presenterView = null;
            if (_displayWindow != null)
            {
                _displayWindow.Close();
                _displayWindow = null;
            }
            if (ViewModel != null) ViewModel.BlankDisplay();
        };
        _presenterView.Show();
    }

    public async void ShowHymnOnDisplay(Hymn hymn, int verseIndex = 0)
    {
        try
        {
            if (_serviceProvider == null || ViewModel == null || hymn == null) return;

            var displayViewModel = GetSharedProjectionViewModel();
            if (displayViewModel == null) return;

            // Create DisplayWindow if it doesn't exist
            if (_displayWindow == null)
            {
                _displayWindow = new DisplayWindow
                {
                    DataContext = displayViewModel,
                    Width = 1920, // Default 16:9
                    Height = 1080
                };

                // Apply active profile
                if (displayViewModel.ActiveProfile != null)
                {
                    _displayWindow.ApplyProfile(displayViewModel.ActiveProfile);
                }

                // Setup synchronized closure
                displayViewModel.RequestClose += () => {
                    ViewModel.BlankDisplay();
                    _displayWindow?.Close();
                    _displayWindow = null;
                    _presenterView?.Close();
                    _presenterView = null;
                };

                _displayWindow.Closed += (s, e) =>
                {
                    _displayWindow = null;
                    _presenterView?.Close();
                    _presenterView = null;
                    if (ViewModel != null) ViewModel.BlankDisplay();
                };
            }

            // Ensure PresenterView is also using the same VM if open
            if (_presenterView != null && _presenterView.DataContext != displayViewModel)
            {
                _presenterView.DataContext = displayViewModel;
            }

            // Load the hymn into the shared ViewModel
            await displayViewModel.LoadHymnDirectlyAsync(hymn, verseIndex);

            // Show fullscreen on secondary monitor
            if (!_displayWindow.IsVisible)
            {
                // Find secondary screen
                var secondaryScreen = Screens.All.FirstOrDefault(s => !s.IsPrimary) ?? Screens.Primary;
                if (secondaryScreen != null)
                {
                    _displayWindow.Position = secondaryScreen.Bounds.Position;
                    _displayWindow.WindowState = WindowState.FullScreen;
                }
                
                _displayWindow.Show();

                // Automatically show Presenter View if it's not already open
                if (_presenterView == null)
                {
                    TogglePresenterView();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing hymn: {ex.Message}");
            if (ViewModel != null)
            {
                ViewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }
    }

    private PixelPoint GetDefaultPosition()
    {
        // Get primary screen
        var screen = Screens.Primary;
        if (screen == null) return new PixelPoint(100, 100);

        var workingArea = screen.WorkingArea;

        // Calculate bottom-right position with 20px margin
        int x = workingArea.X + workingArea.Width - (int)Width - 20;
        int y = workingArea.Y + workingArea.Height - (int)Height - 20;

        return new PixelPoint(x, y);
    }

    private void SharedVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ViewModel == null || sender is not MainWindowViewModel sharedVm) return;

        if (e.PropertyName == nameof(MainWindowViewModel.CurrentVerseIndex))
        {
            ViewModel.SyncVerseIndex(sharedVm.CurrentVerseIndex);
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.CurrentHymn))
        {
            if (sharedVm.CurrentHymn != null)
            {
                ViewModel.SyncHymn(sharedVm.CurrentHymn, sharedVm.CurrentVerseIndex);
            }
            else
            {
                ViewModel.BlankDisplay();
            }
        }
    }
}
