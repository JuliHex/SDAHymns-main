using Avalonia.Controls;
using Avalonia.Input;
using SDAHymns.Desktop.ViewModels;

namespace SDAHymns.Desktop.Views;

public partial class RecorderModeWindow : Window
{
    public RecorderModeWindow()
    {
        InitializeComponent();

        // Handle spacebar key press for tap timing
        KeyDown += RecorderModeWindow_KeyDown;
    }

    private void RecorderModeWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && DataContext is RecorderModeViewModel viewModel)
        {
            viewModel.TapTimingCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SaveTimings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Return DialogResult success
        Close(true);
    }

    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Return DialogResult cancel
        Close(false);
    }
}
