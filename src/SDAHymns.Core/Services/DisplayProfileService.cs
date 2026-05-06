using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for managing display profiles (CRUD operations, import/export, active profile management)
/// </summary>
public class DisplayProfileService : IDisplayProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CamelCaseJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HymnsContext _context;
    private const string ActiveProfileIdKey = "ActiveDisplayProfileId";

    public DisplayProfileService(HymnsContext context)
    {
        _context = context;
    }

    public async Task<List<DisplayProfile>> GetAllProfilesAsync()
    {
        return await _context.DisplayProfiles
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<DisplayProfile?> GetProfileByIdAsync(int id)
    {
        return await _context.DisplayProfiles.FindAsync(id);
    }

    public async Task<DisplayProfile> GetActiveProfileAsync()
    {
        // Try to get the active profile ID from AppSettings
        var setting = await _context.AppSettingsKeyValue
            .FirstOrDefaultAsync(s => s.Key == ActiveProfileIdKey);

        if (setting != null && int.TryParse(setting.Value, out int profileId))
        {
            var profile = await GetProfileByIdAsync(profileId);
            if (profile != null)
                return profile;
        }

        // Fallback to default profile
        var defaultProfile = await _context.DisplayProfiles
            .FirstOrDefaultAsync(p => p.IsDefault);

        if (defaultProfile == null)
            throw new InvalidOperationException("No default display profile found in database");

        return defaultProfile;
    }

    public async Task<DisplayProfile> CreateProfileAsync(DisplayProfile profile)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        _context.DisplayProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<DisplayProfile> UpdateProfileAsync(DisplayProfile profile)
    {
        var existing = await GetProfileByIdAsync(profile.Id);
        if (existing == null)
            throw new InvalidOperationException($"Profile with ID {profile.Id} not found");

        // Update all properties
        _context.Entry(existing).CurrentValues.SetValues(profile);
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task DeleteProfileAsync(int id)
    {
        var profile = await GetProfileByIdAsync(id);
        if (profile == null)
            throw new InvalidOperationException($"Profile with ID {id} not found");

        if (profile.IsSystemProfile)
            throw new InvalidOperationException("Cannot delete system profiles");

        _context.DisplayProfiles.Remove(profile);
        await _context.SaveChangesAsync();
    }

    public async Task SetActiveProfileAsync(int id)
    {
        var profile = await GetProfileByIdAsync(id);
        if (profile == null)
            throw new InvalidOperationException($"Profile with ID {id} not found");

        // Update or create the setting
        var setting = await _context.AppSettingsKeyValue
            .FirstOrDefaultAsync(s => s.Key == ActiveProfileIdKey);

        if (setting != null)
        {
            setting.Value = id.ToString();
            setting.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            setting = new AppSetting
            {
                Key = ActiveProfileIdKey,
                Value = id.ToString(),
                Description = "Currently active display profile ID",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AppSettingsKeyValue.Add(setting);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<DisplayProfile> DuplicateProfileAsync(int id, string newName)
    {
        var source = await GetProfileByIdAsync(id);
        if (source == null)
            throw new InvalidOperationException($"Profile with ID {id} not found");

        var duplicate = new DisplayProfile
        {
            Name = newName,
            Description = source.Description,
            IsDefault = false,
            IsSystemProfile = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            // Typography
            FontFamily = source.FontFamily,
            TitleFontSize = source.TitleFontSize,
            VerseFontSize = source.VerseFontSize,
            LabelFontSize = source.LabelFontSize,
            FontWeight = source.FontWeight,
            LineHeight = source.LineHeight,
            LetterSpacing = source.LetterSpacing,
            // Colors
            BackgroundColor = source.BackgroundColor,
            TextColor = source.TextColor,
            TitleColor = source.TitleColor,
            LabelColor = source.LabelColor,
            AccentColor = source.AccentColor,
            // Background
            BackgroundOpacity = source.BackgroundOpacity,
            BackgroundImagePath = source.BackgroundImagePath,
            BackgroundImageMode = source.BackgroundImageMode,
            BackgroundImageOpacity = source.BackgroundImageOpacity,
            // Layout
            TextAlignment = source.TextAlignment,
            VerticalAlignment = source.VerticalAlignment,
            MarginLeft = source.MarginLeft,
            MarginRight = source.MarginRight,
            MarginTop = source.MarginTop,
            MarginBottom = source.MarginBottom,
            // Effects
            EnableTextShadow = source.EnableTextShadow,
            ShadowColor = source.ShadowColor,
            ShadowBlurRadius = source.ShadowBlurRadius,
            ShadowOffsetX = source.ShadowOffsetX,
            ShadowOffsetY = source.ShadowOffsetY,
            EnableTextOutline = source.EnableTextOutline,
            OutlineColor = source.OutlineColor,
            OutlineThickness = source.OutlineThickness,
            // Advanced
            TransparentBackground = source.TransparentBackground,
            ShowVerseNumbers = source.ShowVerseNumbers,
            ShowHymnTitle = source.ShowHymnTitle
        };

        return await CreateProfileAsync(duplicate);
    }

    public async Task<string> ExportProfileAsync(int id)
    {
        var profile = await GetProfileByIdAsync(id);
        if (profile == null)
            throw new InvalidOperationException($"Profile with ID {id} not found");

        return JsonSerializer.Serialize(profile, CamelCaseJsonOptions);
    }

    public async Task<DisplayProfile> ImportProfileAsync(string json)
    {
        var profile = JsonSerializer.Deserialize<DisplayProfile>(json, CamelCaseJsonOptions);
        if (profile == null)
            throw new InvalidOperationException("Failed to deserialize profile JSON");

        // Reset ID and system properties for imported profiles
        profile.Id = 0;
        profile.IsDefault = false;
        profile.IsSystemProfile = false;

        return await CreateProfileAsync(profile);
    }
}
