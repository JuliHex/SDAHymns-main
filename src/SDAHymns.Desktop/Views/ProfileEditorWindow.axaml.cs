using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SDAHymns.Desktop.ViewModels;

namespace SDAHymns.Desktop.Views;

public partial class ProfileEditorWindow : Window
{
    public ProfileEditorWindow()
    {
        InitializeComponent();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Load profiles when window opens
        if (DataContext is ProfileEditorViewModel viewModel)
        {
            await viewModel.LoadProfilesAsync();
        }
    }

    private async void ExportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ProfileEditorViewModel viewModel)
            return;

        // Get the JSON from the ViewModel
        var json = await viewModel.GetExportJsonAsync();
        if (json == null)
            return;

        // Show file picker
        var storageProvider = StorageProvider;
        var suggestedFileName = viewModel.SelectedProfile != null
            ? $"{viewModel.SelectedProfile.Name.Replace(" ", "_")}_profile.json"
            : "profile.json";

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Display Profile",
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON Profile")
                {
                    Patterns = new[] { "*.json" }
                }
            }
        });

        if (file != null)
        {
            // Save the JSON to the selected file
            await using var stream = await file.OpenWriteAsync();
            await using var writer = new System.IO.StreamWriter(stream);
            await writer.WriteAsync(json);

            // Notify ViewModel of success
            viewModel.OnExportSuccess(file.Path.LocalPath);
        }
    }
}
