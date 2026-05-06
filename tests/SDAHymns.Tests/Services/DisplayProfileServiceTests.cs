using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;
using Xunit;

namespace SDAHymns.Tests.Services;

public class DisplayProfileServiceTests : IDisposable
{
    private readonly HymnsContext _context;
    private readonly DisplayProfileService _service;

    public DisplayProfileServiceTests()
    {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<HymnsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HymnsContext(options);
        _context.Database.EnsureCreated();
        _service = new DisplayProfileService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAllProfilesAsync_ReturnsProfilesOrderedByDefault()
    {
        // Act
        var profiles = await _service.GetAllProfilesAsync();

        // Assert
        profiles.Should().NotBeEmpty();
        profiles.First().IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileByIdAsync_ExistingProfile_ReturnsProfile()
    {
        // Arrange
        var existingProfile = _context.DisplayProfiles.First();

        // Act
        var profile = await _service.GetProfileByIdAsync(existingProfile.Id);

        // Assert
        profile.Should().NotBeNull();
        profile!.Name.Should().Be(existingProfile.Name);
    }

    [Fact]
    public async Task GetProfileByIdAsync_NonExistentProfile_ReturnsNull()
    {
        // Act
        var profile = await _service.GetProfileByIdAsync(9999);

        // Assert
        profile.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveProfileAsync_ReturnsDefaultProfile()
    {
        // Act
        var profile = await _service.GetActiveProfileAsync();

        // Assert
        profile.Should().NotBeNull();
        profile.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProfileAsync_CreatesNewProfile()
    {
        // Arrange
        var newProfile = new DisplayProfile
        {
            Name = "Test Profile",
            Description = "Test description",
            FontFamily = "Arial",
            VerseFontSize = 48
        };

        // Act
        var created = await _service.CreateProfileAsync(newProfile);

        // Assert
        created.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("Test Profile");
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        created.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify in database
        var fromDb = await _context.DisplayProfiles.FindAsync(created.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Test Profile");
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesExistingProfile()
    {
        // Arrange
        var profile = await _context.DisplayProfiles.FirstAsync();
        var originalName = profile.Name;
        profile.Name = "Updated Name";
        profile.VerseFontSize = 72;

        // Act
        var updated = await _service.UpdateProfileAsync(profile);

        // Assert
        updated.Name.Should().Be("Updated Name");
        updated.VerseFontSize.Should().Be(72);
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify in database
        var fromDb = await _context.DisplayProfiles.FindAsync(profile.Id);
        fromDb!.Name.Should().Be("Updated Name");
        fromDb.VerseFontSize.Should().Be(72);
    }

    [Fact]
    public async Task UpdateProfileAsync_NonExistentProfile_ThrowsException()
    {
        // Arrange
        var nonExistentProfile = new DisplayProfile
        {
            Id = 9999,
            Name = "Non-existent",
            FontFamily = "Arial"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateProfileAsync(nonExistentProfile));
    }

    [Fact]
    public async Task DeleteProfileAsync_CustomProfile_DeletesProfile()
    {
        // Arrange
        var customProfile = new DisplayProfile
        {
            Name = "Custom Profile",
            Description = "Can be deleted",
            IsSystemProfile = false,
            FontFamily = "Arial"
        };
        var created = await _service.CreateProfileAsync(customProfile);

        // Act
        await _service.DeleteProfileAsync(created.Id);

        // Assert
        var fromDb = await _context.DisplayProfiles.FindAsync(created.Id);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProfileAsync_SystemProfile_ThrowsException()
    {
        // Arrange
        var systemProfile = await _context.DisplayProfiles
            .FirstAsync(p => p.IsSystemProfile);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteProfileAsync(systemProfile.Id));
    }

    [Fact]
    public async Task DeleteProfileAsync_NonExistentProfile_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteProfileAsync(9999));
    }

    [Fact]
    public async Task SetActiveProfileAsync_UpdatesActiveProfile()
    {
        // Arrange
        var profile = await _context.DisplayProfiles.Skip(1).FirstAsync();

        // Act
        await _service.SetActiveProfileAsync(profile.Id);

        // Assert
        var setting = await _context.AppSettingsKeyValue
            .FirstOrDefaultAsync(s => s.Key == "ActiveDisplayProfileId");
        setting.Should().NotBeNull();
        setting!.Value.Should().Be(profile.Id.ToString());
    }

    [Fact]
    public async Task SetActiveProfileAsync_NonExistentProfile_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SetActiveProfileAsync(9999));
    }

    [Fact]
    public async Task DuplicateProfileAsync_CreatesIdenticalProfile()
    {
        // Arrange
        var original = await _context.DisplayProfiles.FirstAsync();

        // Act
        var duplicate = await _service.DuplicateProfileAsync(original.Id, "Duplicated Profile");

        // Assert
        duplicate.Id.Should().NotBe(original.Id);
        duplicate.Name.Should().Be("Duplicated Profile");
        duplicate.FontFamily.Should().Be(original.FontFamily);
        duplicate.VerseFontSize.Should().Be(original.VerseFontSize);
        duplicate.TextColor.Should().Be(original.TextColor);
        duplicate.BackgroundColor.Should().Be(original.BackgroundColor);
        duplicate.IsDefault.Should().BeFalse();
        duplicate.IsSystemProfile.Should().BeFalse();
    }

    [Fact]
    public async Task DuplicateProfileAsync_NonExistentProfile_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DuplicateProfileAsync(9999, "New Name"));
    }

    [Fact]
    public async Task ExportProfileAsync_ReturnsValidJson()
    {
        // Arrange
        var profile = await _context.DisplayProfiles.FirstAsync();

        // Act
        var json = await _service.ExportProfileAsync(profile.Id);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"fontFamily\":");
        json.Should().Contain(profile.Name);
    }

    [Fact]
    public async Task ExportProfileAsync_NonExistentProfile_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExportProfileAsync(9999));
    }

    [Fact]
    public async Task ImportProfileAsync_CreatesNewProfile()
    {
        // Arrange
        var json = @"{
            ""name"": ""Imported Profile"",
            ""description"": ""Imported from JSON"",
            ""fontFamily"": ""Verdana"",
            ""verseFontSize"": 60,
            ""backgroundColor"": ""#001122"",
            ""textColor"": ""#FFDDAA""
        }";

        // Act
        var imported = await _service.ImportProfileAsync(json);

        // Assert
        imported.Id.Should().BeGreaterThan(0);
        imported.Name.Should().Be("Imported Profile");
        imported.FontFamily.Should().Be("Verdana");
        imported.VerseFontSize.Should().Be(60);
        imported.BackgroundColor.Should().Be("#001122");
        imported.IsDefault.Should().BeFalse();
        imported.IsSystemProfile.Should().BeFalse();

        // Verify in database
        var fromDb = await _context.DisplayProfiles.FindAsync(imported.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task ImportProfileAsync_InvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => _service.ImportProfileAsync(invalidJson));
    }
}
