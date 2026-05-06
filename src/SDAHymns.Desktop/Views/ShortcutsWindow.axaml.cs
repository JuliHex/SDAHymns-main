using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SDAHymns.Desktop.ViewModels;

namespace SDAHymns.Desktop.Views;

public partial class ShortcutsWindow : Window
{
    public ShortcutsWindow()
    {
        InitializeComponent();

        // Global key handler for capturing shortcuts
        KeyDown += Window_KeyDown;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShortcutButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is ShortcutDisplay shortcut &&
            DataContext is ShortcutsWindowViewModel viewModel)
        {
            viewModel.StartListening(shortcut);
        }
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ShortcutsWindowViewModel viewModel)
            return;
        if (viewModel.ListeningShortcut == null)
            return;

        // Don't process modifier keys alone
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or
            Key.LeftAlt or Key.RightAlt or
            Key.LeftShift or Key.RightShift or
            Key.LWin or Key.RWin)
        {
            return;
        }

        // Build the modifiers string
        var parts = new List<string>();

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Meta))
            parts.Add("Meta");

        var modifiers = string.Join("+", parts.OrderBy(p => p));
        var key = e.Key.ToString();

        // Capture the key
        viewModel.CaptureKey(key, modifiers);

        // Prevent default handling
        e.Handled = true;
    }
}
