using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for managing display profiles (CRUD operations, import/export, active profile management)
/// </summary>
public interface IDisplayProfileService
{
    /// <summary>
    /// Gets all display profiles ordered by IsDefault descending, then by Name
    /// </summary>
    Task<List<DisplayProfile>> GetAllProfilesAsync();

    /// <summary>
    /// Gets a specific profile by ID
    /// </summary>
    Task<DisplayProfile?> GetProfileByIdAsync(int id);

    /// <summary>
    /// Gets the currently active profile (from AppSettings or default)
    /// </summary>
    Task<DisplayProfile> GetActiveProfileAsync();

    /// <summary>
    /// Creates a new profile
    /// </summary>
    Task<DisplayProfile> CreateProfileAsync(DisplayProfile profile);

    /// <summary>
    /// Updates an existing profile
    /// </summary>
    Task<DisplayProfile> UpdateProfileAsync(DisplayProfile profile);

    /// <summary>
    /// Deletes a profile (cannot delete system profiles)
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if attempting to delete a system profile</exception>
    Task DeleteProfileAsync(int id);

    /// <summary>
    /// Sets the active profile and saves it to AppSettings
    /// </summary>
    Task SetActiveProfileAsync(int id);

    /// <summary>
    /// Duplicates an existing profile with a new name
    /// </summary>
    Task<DisplayProfile> DuplicateProfileAsync(int id, string newName);

    /// <summary>
    /// Exports a profile as JSON string
    /// </summary>
    Task<string> ExportProfileAsync(int id);

    /// <summary>
    /// Imports a profile from JSON string
    /// </summary>
    Task<DisplayProfile> ImportProfileAsync(string json);
}
