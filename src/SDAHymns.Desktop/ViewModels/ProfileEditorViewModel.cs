using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;

namespace SDAHymns.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Profile Editor window
/// </summary>
public partial class ProfileEditorViewModel : ObservableObject
{
    private readonly IDisplayProfileService _profileService;

    [ObservableProperty]
    private ObservableCollection<DisplayProfile> _profiles = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviewProfileCommand))]
    private DisplayProfile? _selectedProfile;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ProfileEditorViewModel(IDisplayProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Loads all profiles from database
    /// </summary>
    public async Task LoadProfilesAsync()
    {
        var profiles = await _profileService.GetAllProfilesAsync();
        Profiles.Clear();
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }

        // Select the first profile by default
        if (Profiles.Count > 0)
        {
            SelectedProfile = Profiles[0];
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    private async Task SaveProfileAsync()
    {
        if (SelectedProfile == null)
            return;

        try
        {
            await _profileService.UpdateProfileAsync(SelectedProfile);
            StatusMessage = $"Profile '{SelectedProfile.Name}' saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving profile: {ex.Message}";
        }
    }

    private bool CanSaveProfile() => SelectedProfile != null;

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        var newProfile = new DisplayProfile
        {
            Name = "New Profile",
            Description = "Custom profile",
            IsDefault = false,
            IsSystemProfile = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            var created = await _profileService.CreateProfileAsync(newProfile);
            Profiles.Add(created);
            SelectedProfile = created;
            StatusMessage = $"Profile '{created.Name}' created";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating profile: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteProfile))]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile == null)
            return;

        try
        {
            await _profileService.DeleteProfileAsync(SelectedProfile.Id);
            Profiles.Remove(SelectedProfile);
            SelectedProfile = Profiles.FirstOrDefault();
            StatusMessage = "Profile deleted successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting profile: {ex.Message}";
        }
    }

    private bool CanDeleteProfile()
    {
        return SelectedProfile != null && !SelectedProfile.IsSystemProfile;
    }

    [RelayCommand(CanExecute = nameof(CanDuplicateProfile))]
    private async Task DuplicateProfileAsync()
    {
        if (SelectedProfile == null)
            return;

        try
        {
            var duplicate = await _profileService.DuplicateProfileAsync(
                SelectedProfile.Id,
                $"{SelectedProfile.Name} (Copy)"
            );
            Profiles.Add(duplicate);
            SelectedProfile = duplicate;
            StatusMessage = $"Profile duplicated as '{duplicate.Name}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error duplicating profile: {ex.Message}";
        }
    }

    private bool CanDuplicateProfile() => SelectedProfile != null;

    /// <summary>
    /// Export profile to JSON - returns the JSON string for the window to save using file picker
    /// </summary>
    public async Task<string?> GetExportJsonAsync()
    {
        if (SelectedProfile == null)
            return null;

        try
        {
            return await _profileService.ExportProfileAsync(SelectedProfile.Id);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting profile: {ex.Message}";
            return null;
        }
    }

    /// <summary>
    /// Called after successful export with the file path
    /// </summary>
    public void OnExportSuccess(string filePath)
    {
        StatusMessage = $"Profile exported to {filePath}";
    }

    [RelayCommand]
    private async Task ImportProfileAsync()
    {
        // TODO: Implement file picker and import logic
        // For now, this is a placeholder
        StatusMessage = "Import functionality coming soon";
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanResetProfile))]
    private async Task ResetProfileAsync()
    {
        if (SelectedProfile == null)
            return;

        // Reload profile from database to discard changes
        try
        {
            var fresh = await _profileService.GetProfileByIdAsync(SelectedProfile.Id);
            if (fresh != null)
            {
                var index = Profiles.IndexOf(SelectedProfile);
                Profiles[index] = fresh;
                SelectedProfile = fresh;
                StatusMessage = "Profile reset to saved state";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error resetting profile: {ex.Message}";
        }
    }

    private bool CanResetProfile() => SelectedProfile != null;

    [RelayCommand(CanExecute = nameof(CanPreviewProfile))]
    private Task PreviewProfileAsync()
    {
        if (SelectedProfile == null)
            return Task.CompletedTask;

        // TODO: Implement preview functionality (show in display window)
        StatusMessage = "Preview functionality coming soon";
        return Task.CompletedTask;
    }

    private bool CanPreviewProfile() => SelectedProfile != null;
}
