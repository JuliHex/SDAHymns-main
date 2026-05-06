using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    private IServiceProvider? _serviceProvider;

    public RemoteWidget()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        // Set initial position
        Opened += OnOpened;
        PositionChanged += OnPositionChanged;
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (ViewModel == null) return;

        // Initialize ViewModel asynchronously
        await ViewModel.InitializeAsync();

        // Wire up callbacks
        ViewModel.OnShowHymnRequested = ShowHymnOnDisplay;
        ViewModel.OnBlankDisplayRequested = BlankDisplay;

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

    public async void ShowHymnOnDisplay(Hymn hymn, int verseIndex = 0)
    {
        if (_serviceProvider == null || ViewModel == null || hymn == null) return;

        // Create DisplayWindow with a MainWindowViewModel if it doesn't exist
        if (_displayWindow == null)
        {
            var displayViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

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

            _displayWindow.Closed += (s, e) => _displayWindow = null;
        }

        // Load the hymn into the display ViewModel
        if (_displayWindow.DataContext is MainWindowViewModel displayViewModel2)
        {
            // Set the hymn on the display ViewModel
            await displayViewModel2.LoadHymnDirectlyAsync(hymn, verseIndex);
        }

        // Show fullscreen
        _displayWindow.Show();
        _displayWindow.WindowState = WindowState.FullScreen;
    }

    public void BlankDisplay()
    {
        _displayWindow?.Close();
        _displayWindow = null;
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

}
