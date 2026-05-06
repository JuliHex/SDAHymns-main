using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SDAHymns.Desktop.ViewModels;

namespace SDAHymns.Desktop.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private async void BrowseLibraryPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SettingsWindowViewModel viewModel)
            return;

        // Open folder picker
        var folderPicker = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Audio Library Folder",
            AllowMultiple = false
        });

        if (folderPicker.Count > 0)
        {
            var selectedPath = folderPicker[0].Path.LocalPath;
            viewModel.AudioLibraryPath = selectedPath;

            // Auto-save the setting
            await viewModel.SaveSettingsCommand.ExecuteAsync(null);
        }
    }

    private async void MigrateLibrary_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SettingsWindowViewModel viewModel)
            return;

        // Open folder picker for new location
        var folderPicker = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select New Audio Library Location",
            AllowMultiple = false
        });

        if (folderPicker.Count > 0)
        {
            var newPath = folderPicker[0].Path.LocalPath;

            // Call the migration command with the new path
            await viewModel.MigrateLibraryWithPathCommand.ExecuteAsync(newPath);
        }
    }
}
