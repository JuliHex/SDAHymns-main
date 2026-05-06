# Spec 002: Data Layer & Entity Framework Core

**Status:** ✅ Implemented
**Created:** 2025-12-03
**Implemented:** 2025-12-03
**Dependencies:** 001-project-structure.md

## Overview

Implement the data layer using Entity Framework Core with SQLite. This includes defining all entity models, DbContext configuration, relationships, and initial database migration. The data layer serves as the foundation for storing hymn data, user preferences, and application state.

## Goals

1. Define all entity models with proper relationships
2. Configure EF Core DbContext with SQLite provider
3. Implement repository pattern for data access
4. Create initial database migration
5. Set up proper indexing for search performance
6. Ensure Romanian text encoding (UTF-8) support

## Entity Models

### 1. Hymn

Primary entity representing a hymn.

```csharp
namespace SDAHymns.Core.Data.Models;

public class Hymn
{
    public int Id { get; set; }

    // Hymn identification
    public int Number { get; set; }
    public required string Title { get; set; }

    // Category relationship
    public int CategoryId { get; set; }
    public HymnCategory Category { get; set; } = null!;

    // Navigation properties
    public ICollection<Verse> Verses { get; set; } = new List<Verse>();
    public ICollection<AudioRecording> AudioRecordings { get; set; } = new List<AudioRecording>();
    public ICollection<UsageStatistic> UsageStatistics { get; set; } = new List<UsageStatistic>();
    public ICollection<ServicePlanItem> ServicePlanItems { get; set; } = new List<ServicePlanItem>();

    // Legacy reference (optional)
    public string? LegacyPowerPointPath { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Unique: `(Number, CategoryId)`
- Index: `Number`, `Title`, `CategoryId`
- `Title` is required, max length 200

### 2. Verse

Represents a single verse/slide of a hymn.

```csharp
namespace SDAHymns.Core.Data.Models;

public class Verse
{
    public int Id { get; set; }

    // Hymn relationship
    public int HymnId { get; set; }
    public Hymn Hymn { get; set; } = null!;

    // Verse data
    public int VerseNumber { get; set; }  // 1, 2, 3, etc.
    public required string Content { get; set; }  // Verse text/lyrics

    // Optional metadata
    public string? Label { get; set; }  // e.g., "Refren", "Strofa 1"

    // Order and display
    public int DisplayOrder { get; set; }  // For custom ordering

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Unique: `(HymnId, VerseNumber)`
- Index: `HymnId`, `DisplayOrder`
- `Content` required, max length 2000

### 3. HymnCategory

Represents hymn book categories.

```csharp
namespace SDAHymns.Core.Data.Models;

public class HymnCategory
{
    public int Id { get; set; }

    public required string Name { get; set; }  // "Imnuri crestine", etc.
    public required string Slug { get; set; }  // "crestine", "companioni"
    public string? Description { get; set; }

    // Display
    public int DisplayOrder { get; set; }
    public string? IconPath { get; set; }

    // Legacy reference
    public string? LegacyFolderPath { get; set; }

    // Navigation
    public ICollection<Hymn> Hymns { get; set; } = new List<Hymn>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Unique: `Name`, `Slug`
- Index: `Slug`, `DisplayOrder`

**Seed Data:**
```
1. Imnuri crestine (slug: crestine)
2. Imnuri companioni (slug: companioni)
3. Imnuri exploratori (slug: exploratori)
4. Imnuri licurici (slug: licurici)
5. Imnuri tineret (slug: tineret)
```

### 4. AudioRecording

Piano recording metadata.

```csharp
namespace SDAHymns.Core.Data.Models;

public class AudioRecording
{
    public int Id { get; set; }

    // Hymn relationship
    public int HymnId { get; set; }
    public Hymn Hymn { get; set; } = null!;

    // File information
    public required string FilePath { get; set; }  // Relative path
    public string FileFormat { get; set; } = "mp3";  // mp3, opus, etc.
    public long FileSizeBytes { get; set; }

    // Recording metadata
    public DateTime? RecordingDate { get; set; }
    public string? Tempo { get; set; }  // "moderato", "allegro", etc.
    public int DurationSeconds { get; set; }
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }

    // Playback settings
    public double DefaultPlaybackSpeed { get; set; } = 1.0;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Index: `HymnId`, `FilePath`
- `FilePath` required, max length 500

### 5. DisplayProfile

Display configuration presets.

```csharp
namespace SDAHymns.Core.Data.Models;

public class DisplayProfile
{
    public int Id { get; set; }

    public required string Name { get; set; }  // "Projector", "OBS Stream"
    public string? Description { get; set; }

    // Background
    public string BackgroundColor { get; set; } = "#000000";
    public double BackgroundOpacity { get; set; } = 1.0;
    public string? BackgroundImagePath { get; set; }

    // Text styling
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 48;
    public string TextColor { get; set; } = "#FFFFFF";
    public string TextAlignment { get; set; } = "Center";  // Left, Center, Right
    public bool EnableTextShadow { get; set; } = true;
    public string? ShadowColor { get; set; } = "#000000";

    // Layout
    public int PaddingHorizontal { get; set; } = 40;
    public int PaddingVertical { get; set; } = 40;
    public int LineSpacing { get; set; } = 10;

    // Special modes
    public bool TransparentBackground { get; set; } = false;

    // Metadata
    public bool IsDefault { get; set; } = false;
    public bool IsSystemProfile { get; set; } = false;  // Cannot be deleted

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Unique: `Name`
- Index: `Name`, `IsDefault`

**Seed Data:**
```
1. "Projector" - Default projector settings
2. "OBS Stream" - Transparent background, optimized for streaming
3. "Practice Mode" - Lower opacity, smaller text
```

### 6. ServicePlan

Service planning feature.

```csharp
namespace SDAHymns.Core.Data.Models;

public class ServicePlan
{
    public int Id { get; set; }

    public required string Name { get; set; }  // "Sabbath Morning - Dec 3"
    public string? Description { get; set; }
    public DateTime? ServiceDate { get; set; }

    // Navigation
    public ICollection<ServicePlanItem> Items { get; set; } = new List<ServicePlanItem>();

    // Status
    public bool IsActive { get; set; } = false;  // Currently active plan

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Index: `ServiceDate`, `IsActive`, `Name`

### 7. ServicePlanItem

Individual items in a service plan.

```csharp
namespace SDAHymns.Core.Data.Models;

public class ServicePlanItem
{
    public int Id { get; set; }

    // Service plan relationship
    public int ServicePlanId { get; set; }
    public ServicePlan ServicePlan { get; set; } = null!;

    // Hymn relationship
    public int HymnId { get; set; }
    public Hymn Hymn { get; set; } = null!;

    // Ordering
    public int DisplayOrder { get; set; }

    // Selected verses (JSON array of verse numbers)
    public string? SelectedVerses { get; set; }  // e.g., "[1,3,4]"

    // Notes
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

**Constraints:**
- Index: `ServicePlanId`, `DisplayOrder`

### 8. UsageStatistic

Track hymn usage for statistics.

```csharp
namespace SDAHymns.Core.Data.Models;

public class UsageStatistic
{
    public int Id { get; set; }

    // Hymn relationship
    public int HymnId { get; set; }
    public Hymn Hymn { get; set; } = null!;

    // Usage data
    public DateTime UsedAt { get; set; }
    public int DisplayDurationSeconds { get; set; }
    public bool AudioPlayed { get; set; }

    // Context
    public string? ServicePlanName { get; set; }
}
```

**Constraints:**
- Index: `HymnId`, `UsedAt`

### 9. AppSettings

Application-wide settings (key-value store).

```csharp
namespace SDAHymns.Core.Data.Models;

public class AppSetting
{
    public int Id { get; set; }

    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Constraints:**
- Unique: `Key`
- Index: `Key`

## DbContext Implementation

```csharp
namespace SDAHymns.Core.Data;

public class HymnsContext : DbContext
{
    public HymnsContext(DbContextOptions<HymnsContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Hymn> Hymns => Set<Hymn>();
    public DbSet<Verse> Verses => Set<Verse>();
    public DbSet<HymnCategory> HymnCategories => Set<HymnCategory>();
    public DbSet<AudioRecording> AudioRecordings => Set<AudioRecording>();
    public DbSet<DisplayProfile> DisplayProfiles => Set<DisplayProfile>();
    public DbSet<ServicePlan> ServicePlans => Set<ServicePlan>();
    public DbSet<ServicePlanItem> ServicePlanItems => Set<ServicePlanItem>();
    public DbSet<UsageStatistic> UsageStatistics => Set<UsageStatistic>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureHymn(modelBuilder);
        ConfigureVerse(modelBuilder);
        ConfigureHymnCategory(modelBuilder);
        ConfigureAudioRecording(modelBuilder);
        ConfigureDisplayProfile(modelBuilder);
        ConfigureServicePlan(modelBuilder);
        ConfigureServicePlanItem(modelBuilder);
        ConfigureUsageStatistic(modelBuilder);
        ConfigureAppSetting(modelBuilder);

        SeedData(modelBuilder);
    }

    private void ConfigureHymn(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hymn>(entity =>
        {
            entity.HasKey(h => h.Id);

            entity.Property(h => h.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(h => h.LegacyPowerPointPath)
                .HasMaxLength(500);

            entity.HasIndex(h => h.Number);
            entity.HasIndex(h => h.Title);
            entity.HasIndex(h => h.CategoryId);
            entity.HasIndex(h => new { h.Number, h.CategoryId })
                .IsUnique();

            entity.HasOne(h => h.Category)
                .WithMany(c => c.Hymns)
                .HasForeignKey(h => h.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureVerse(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Verse>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(v => v.Label)
                .HasMaxLength(50);

            entity.HasIndex(v => v.HymnId);
            entity.HasIndex(v => new { v.HymnId, v.VerseNumber })
                .IsUnique();
            entity.HasIndex(v => new { v.HymnId, v.DisplayOrder });

            entity.HasOne(v => v.Hymn)
                .WithMany(h => h.Verses)
                .HasForeignKey(v => v.HymnId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureHymnCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HymnCategory>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.Property(c => c.IconPath)
                .HasMaxLength(500);

            entity.Property(c => c.LegacyFolderPath)
                .HasMaxLength(500);

            entity.HasIndex(c => c.Name)
                .IsUnique();
            entity.HasIndex(c => c.Slug)
                .IsUnique();
            entity.HasIndex(c => c.DisplayOrder);
        });
    }

    private void ConfigureAudioRecording(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AudioRecording>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.FileFormat)
                .HasMaxLength(10);

            entity.Property(a => a.Tempo)
                .HasMaxLength(50);

            entity.Property(a => a.RecordedBy)
                .HasMaxLength(200);

            entity.Property(a => a.Notes)
                .HasMaxLength(1000);

            entity.HasIndex(a => a.HymnId);
            entity.HasIndex(a => a.FilePath);

            entity.HasOne(a => a.Hymn)
                .WithMany(h => h.AudioRecordings)
                .HasForeignKey(a => a.HymnId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureDisplayProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DisplayProfile>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.Description)
                .HasMaxLength(500);

            entity.Property(d => d.FontFamily)
                .HasMaxLength(100);

            entity.Property(d => d.BackgroundColor)
                .HasMaxLength(20);

            entity.Property(d => d.TextColor)
                .HasMaxLength(20);

            entity.Property(d => d.ShadowColor)
                .HasMaxLength(20);

            entity.Property(d => d.TextAlignment)
                .HasMaxLength(20);

            entity.Property(d => d.BackgroundImagePath)
                .HasMaxLength(500);

            entity.HasIndex(d => d.Name)
                .IsUnique();
            entity.HasIndex(d => d.IsDefault);
        });
    }

    private void ConfigureServicePlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServicePlan>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.Description)
                .HasMaxLength(1000);

            entity.HasIndex(s => s.ServiceDate);
            entity.HasIndex(s => s.IsActive);
            entity.HasIndex(s => s.Name);
        });
    }

    private void ConfigureServicePlanItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServicePlanItem>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.SelectedVerses)
                .HasMaxLength(200);

            entity.Property(i => i.Notes)
                .HasMaxLength(500);

            entity.HasIndex(i => new { i.ServicePlanId, i.DisplayOrder });

            entity.HasOne(i => i.ServicePlan)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.ServicePlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Hymn)
                .WithMany(h => h.ServicePlanItems)
                .HasForeignKey(i => i.HymnId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureUsageStatistic(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsageStatistic>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.ServicePlanName)
                .HasMaxLength(200);

            entity.HasIndex(u => u.HymnId);
            entity.HasIndex(u => u.UsedAt);

            entity.HasOne(u => u.Hymn)
                .WithMany(h => h.UsageStatistics)
                .HasForeignKey(u => u.HymnId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureAppSetting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Key)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Value)
                .IsRequired();

            entity.Property(a => a.Description)
                .HasMaxLength(500);

            entity.HasIndex(a => a.Key)
                .IsUnique();
        });
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed HymnCategories
        modelBuilder.Entity<HymnCategory>().HasData(
            new HymnCategory
            {
                Id = 1,
                Name = "Imnuri crestine",
                Slug = "crestine",
                Description = "Main Christian hymnbook",
                DisplayOrder = 1,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri crestine",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new HymnCategory
            {
                Id = 2,
                Name = "Imnuri companioni",
                Slug = "companioni",
                Description = "Pathfinder/Companion hymns",
                DisplayOrder = 2,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri companioni",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new HymnCategory
            {
                Id = 3,
                Name = "Imnuri exploratori",
                Slug = "exploratori",
                Description = "Explorer hymns",
                DisplayOrder = 3,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri exploratori",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new HymnCategory
            {
                Id = 4,
                Name = "Imnuri licurici",
                Slug = "licurici",
                Description = "Firefly hymns (children's songs)",
                DisplayOrder = 4,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri licurici",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new HymnCategory
            {
                Id = 5,
                Name = "Imnuri tineret",
                Slug = "tineret",
                Description = "Youth hymns",
                DisplayOrder = 5,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri tineret",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed DisplayProfiles
        modelBuilder.Entity<DisplayProfile>().HasData(
            new DisplayProfile
            {
                Id = 1,
                Name = "Projector",
                Description = "Default projector display settings",
                BackgroundColor = "#000000",
                BackgroundOpacity = 1.0,
                FontFamily = "Arial",
                FontSize = 48,
                TextColor = "#FFFFFF",
                TextAlignment = "Center",
                EnableTextShadow = true,
                ShadowColor = "#000000",
                PaddingHorizontal = 60,
                PaddingVertical = 40,
                LineSpacing = 15,
                TransparentBackground = false,
                IsDefault = true,
                IsSystemProfile = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DisplayProfile
            {
                Id = 2,
                Name = "OBS Stream",
                Description = "Transparent background for OBS/streaming",
                BackgroundColor = "#000000",
                BackgroundOpacity = 0.0,
                FontFamily = "Arial",
                FontSize = 52,
                TextColor = "#FFFFFF",
                TextAlignment = "Center",
                EnableTextShadow = true,
                ShadowColor = "#000000",
                PaddingHorizontal = 80,
                PaddingVertical = 60,
                LineSpacing = 18,
                TransparentBackground = true,
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DisplayProfile
            {
                Id = 3,
                Name = "Practice Mode",
                Description = "Lower opacity for practice/rehearsal",
                BackgroundColor = "#1a1a1a",
                BackgroundOpacity = 0.7,
                FontFamily = "Arial",
                FontSize = 36,
                TextColor = "#CCCCCC",
                TextAlignment = "Center",
                EnableTextShadow = false,
                PaddingHorizontal = 40,
                PaddingVertical = 30,
                LineSpacing = 12,
                TransparentBackground = false,
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed initial AppSettings
        modelBuilder.Entity<AppSetting>().HasData(
            new AppSetting
            {
                Id = 1,
                Key = "LastDatabaseImportDate",
                Value = "",
                Description = "Last time legacy XML was imported",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AppSetting
            {
                Id = 2,
                Key = "DefaultDisplayProfileId",
                Value = "1",
                Description = "Default display profile ID",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AppSetting
            {
                Id = 3,
                Key = "EnableStatisticsTracking",
                Value = "true",
                Description = "Track hymn usage statistics",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
```

## Implementation Steps

### Step 1: Create Entity Models

Create all entity classes in `src/SDAHymns.Core/Data/Models/`:

```bash
# Create model files
touch src/SDAHymns.Core/Data/Models/{Hymn,Verse,HymnCategory,AudioRecording,DisplayProfile,ServicePlan,ServicePlanItem,UsageStatistic,AppSetting}.cs
```

### Step 2: Create DbContext

Create `HymnsContext.cs` in `src/SDAHymns.Core/Data/` with all configurations.

### Step 3: Add EF Core Design-Time Factory

For migrations to work, create `DesignTimeDbContextFactory.cs`:

```csharp
namespace SDAHymns.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HymnsContext>
{
    public HymnsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HymnsContext>();
        optionsBuilder.UseSqlite("Data Source=Resources/hymns.db");

        return new HymnsContext(optionsBuilder.Options);
    }
}
```

### Step 4: Create Initial Migration

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project src/SDAHymns.Core --startup-project src/SDAHymns.Desktop

# Apply migration (creates database)
dotnet ef database update --project src/SDAHymns.Core --startup-project src/SDAHymns.Desktop
```

### Step 5: Write Unit Tests

Create tests in `tests/SDAHymns.Tests/Core/Data/`:

- `HymnsContextTests.cs` - Test DbContext configuration
- `HymnModelTests.cs` - Test entity constraints and relationships
- `DataSeeding Tests.cs` - Verify seed data

## Acceptance Criteria

- [ ] All 9 entity models created with proper properties
- [ ] `HymnsContext` implemented with all DbSet properties
- [ ] All `OnModelCreating` configurations complete (indexes, relationships, constraints)
- [ ] Seed data for HymnCategories, DisplayProfiles, and AppSettings
- [ ] Initial migration created and applies successfully
- [ ] Database file created at `Resources/hymns.db`
- [ ] All unique constraints working
- [ ] All foreign key relationships configured correctly
- [ ] Romanian text (UTF-8) properly stored and retrieved
- [ ] Unit tests for data layer pass
- [ ] Can query and insert data programmatically

## Testing Verification

```csharp
// Example test: Verify category seed data
[Fact]
public async Task Database_ShouldHave_FiveCategories()
{
    // Arrange
    using var context = CreateTestContext();

    // Act
    var categories = await context.HymnCategories.ToListAsync();

    // Assert
    categories.Should().HaveCount(5);
    categories.Should().Contain(c => c.Slug == "crestine");
    categories.Should().Contain(c => c.Slug == "companioni");
}

// Example test: Verify unique constraint
[Fact]
public async Task Hymn_ShouldNotAllow_DuplicateNumberInCategory()
{
    // Arrange
    using var context = CreateTestContext();
    var category = await context.HymnCategories.FirstAsync();

    context.Hymns.Add(new Hymn { Number = 1, Title = "Test 1", CategoryId = category.Id });
    await context.SaveChangesAsync();

    // Act & Assert
    context.Hymns.Add(new Hymn { Number = 1, Title = "Test 2", CategoryId = category.Id });

    await Assert.ThrowsAsync<DbUpdateException>(async () =>
        await context.SaveChangesAsync());
}
```

## Notes

- Use `DateTime.UtcNow` for operational timestamps (Created/Updated At)
- Use a static `DateTime` (e.g., `new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)`) for seed data to ensure deterministic migrations
- SQLite connection string: `Data Source=Resources/hymns.db`
- Ensure proper cascading deletes (Cascade for owned data, Restrict for references)
- `SelectedVerses` in `ServicePlanItem` stored as JSON array string
- Consider adding soft delete pattern in future if needed
- Performance: Index all commonly-queried fields

## Related Specs

- **Previous:** 001-project-structure.md
- **Next:** 003-legacy-xml-import.md (TBD) - Import hymn data from legacy XML files

## Status Updates

- **2025-12-03:** Spec created, ready for implementation
