using SDAHymns.Core.Data.Models;

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
    public DbSet<AppSetting> AppSettingsKeyValue => Set<AppSetting>();  // Key-value store
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();  // Singleton app configuration

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
        ConfigureAppSettings(modelBuilder);

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

            entity.Property(d => d.FontWeight)
                .HasMaxLength(20);

            entity.Property(d => d.BackgroundColor)
                .HasMaxLength(20);

            entity.Property(d => d.TextColor)
                .HasMaxLength(20);

            entity.Property(d => d.TitleColor)
                .HasMaxLength(20);

            entity.Property(d => d.LabelColor)
                .HasMaxLength(20);

            entity.Property(d => d.AccentColor)
                .HasMaxLength(20);

            entity.Property(d => d.ShadowColor)
                .HasMaxLength(20);

            entity.Property(d => d.OutlineColor)
                .HasMaxLength(20);

            entity.Property(d => d.TextAlignment)
                .HasMaxLength(20);

            entity.Property(d => d.VerticalAlignment)
                .HasMaxLength(20);

            entity.Property(d => d.BackgroundImagePath)
                .HasMaxLength(500);

            entity.Property(d => d.BackgroundImageMode)
                .HasMaxLength(20);

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

    private void ConfigureAppSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.AudioLibraryPath)
                .HasMaxLength(500);

            entity.Property(a => a.AudioOutputDeviceId)
                .HasMaxLength(200);

            entity.Property(a => a.LastWindowPosition)
                .HasMaxLength(100);

            entity.Property(a => a.LastWindowSize)
                .HasMaxLength(100);

            entity.HasOne(a => a.ActiveDisplayProfile)
                .WithMany()
                .HasForeignKey(a => a.ActiveDisplayProfileId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Use static date for seed data (required by EF Core)
        var now = new DateTime(2025, 12, 3, 0, 0, 0, DateTimeKind.Utc);

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
                CreatedAt = now,
                UpdatedAt = now
            },
            new HymnCategory
            {
                Id = 2,
                Name = "Imnuri companioni",
                Slug = "companioni",
                Description = "Pathfinder/Companion hymns",
                DisplayOrder = 2,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri companioni",
                CreatedAt = now,
                UpdatedAt = now
            },
            new HymnCategory
            {
                Id = 3,
                Name = "Imnuri exploratori",
                Slug = "exploratori",
                Description = "Explorer hymns",
                DisplayOrder = 3,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri exploratori",
                CreatedAt = now,
                UpdatedAt = now
            },
            new HymnCategory
            {
                Id = 4,
                Name = "Imnuri licurici",
                Slug = "licurici",
                Description = "Firefly hymns (children's songs)",
                DisplayOrder = 4,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri licurici",
                CreatedAt = now,
                UpdatedAt = now
            },
            new HymnCategory
            {
                Id = 5,
                Name = "Imnuri tineret",
                Slug = "tineret",
                Description = "Youth hymns",
                DisplayOrder = 5,
                LegacyFolderPath = "Imnuri Azs/Resurse/Imnuri tineret",
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        // Seed DisplayProfiles
        modelBuilder.Entity<DisplayProfile>().HasData(
            // 1. Classic Dark (Default)
            new DisplayProfile
            {
                Id = 1,
                Name = "Classic Dark",
                Description = "Default black background with white text, left-aligned",
                IsDefault = true,
                IsSystemProfile = true,
                CreatedAt = now,
                UpdatedAt = now,
                // Typography
                FontFamily = "Inter",
                TitleFontSize = 36,
                VerseFontSize = 48,
                LabelFontSize = 28,
                FontWeight = "Normal",
                LineHeight = 1.4,
                LetterSpacing = 0,
                // Colors
                BackgroundColor = "#000000",
                TextColor = "#FFFFFF",
                TitleColor = "#FFFFFF",
                LabelColor = "#CCCCCC",
                AccentColor = "#0078D4",
                // Background
                BackgroundOpacity = 1.0,
                BackgroundImagePath = null,
                BackgroundImageMode = "Fill",
                BackgroundImageOpacity = 0.3,
                // Layout
                TextAlignment = "Left",
                VerticalAlignment = "Center",
                MarginLeft = 100,
                MarginRight = 100,
                MarginTop = 60,
                MarginBottom = 60,
                // Effects
                EnableTextShadow = false,
                ShadowColor = "#000000",
                ShadowBlurRadius = 10,
                ShadowOffsetX = 2,
                ShadowOffsetY = 2,
                EnableTextOutline = false,
                OutlineColor = "#000000",
                OutlineThickness = 2,
                // Advanced
                TransparentBackground = false,
                ShowVerseNumbers = true,
                ShowHymnTitle = true
            },
            // 2. High Contrast
            new DisplayProfile
            {
                Id = 2,
                Name = "High Contrast",
                Description = "Pure black and white with bold text and shadow for maximum visibility",
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = now,
                UpdatedAt = now,
                // Typography
                FontFamily = "Inter",
                TitleFontSize = 40,
                VerseFontSize = 56,
                LabelFontSize = 32,
                FontWeight = "Bold",
                LineHeight = 1.4,
                LetterSpacing = 0,
                // Colors
                BackgroundColor = "#000000",
                TextColor = "#FFFFFF",
                TitleColor = "#FFFFFF",
                LabelColor = "#FFFFFF",
                AccentColor = "#FFFFFF",
                // Background
                BackgroundOpacity = 1.0,
                BackgroundImagePath = null,
                BackgroundImageMode = "Fill",
                BackgroundImageOpacity = 0.3,
                // Layout
                TextAlignment = "Left",
                VerticalAlignment = "Center",
                MarginLeft = 100,
                MarginRight = 100,
                MarginTop = 60,
                MarginBottom = 60,
                // Effects
                EnableTextShadow = true,
                ShadowColor = "#000000",
                ShadowBlurRadius = 15,
                ShadowOffsetX = 3,
                ShadowOffsetY = 3,
                EnableTextOutline = false,
                OutlineColor = "#000000",
                OutlineThickness = 2,
                // Advanced
                TransparentBackground = false,
                ShowVerseNumbers = true,
                ShowHymnTitle = true
            },
            // 3. OBS Stream Optimized
            new DisplayProfile
            {
                Id = 3,
                Name = "OBS Stream",
                Description = "Transparent background with text outline for streaming over video",
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = now,
                UpdatedAt = now,
                // Typography
                FontFamily = "Inter",
                TitleFontSize = 40,
                VerseFontSize = 52,
                LabelFontSize = 30,
                FontWeight = "SemiBold",
                LineHeight = 1.4,
                LetterSpacing = 0,
                // Colors
                BackgroundColor = "#000000",
                TextColor = "#FFFFFF",
                TitleColor = "#FFFFFF",
                LabelColor = "#EEEEEE",
                AccentColor = "#0078D4",
                // Background
                BackgroundOpacity = 0.0,
                BackgroundImagePath = null,
                BackgroundImageMode = "Fill",
                BackgroundImageOpacity = 0.3,
                // Layout
                TextAlignment = "Center",
                VerticalAlignment = "Center",
                MarginLeft = 120,
                MarginRight = 120,
                MarginTop = 80,
                MarginBottom = 80,
                // Effects
                EnableTextShadow = false,
                ShadowColor = "#000000",
                ShadowBlurRadius = 10,
                ShadowOffsetX = 2,
                ShadowOffsetY = 2,
                EnableTextOutline = true,
                OutlineColor = "#000000",
                OutlineThickness = 3,
                // Advanced
                TransparentBackground = true,
                ShowVerseNumbers = true,
                ShowHymnTitle = true
            },
            // 4. Projector - Bright Room
            new DisplayProfile
            {
                Id = 4,
                Name = "Bright Room",
                Description = "Dark blue background with yellow text for bright environments",
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = now,
                UpdatedAt = now,
                // Typography
                FontFamily = "Inter",
                TitleFontSize = 38,
                VerseFontSize = 52,
                LabelFontSize = 30,
                FontWeight = "Bold",
                LineHeight = 1.4,
                LetterSpacing = 0,
                // Colors
                BackgroundColor = "#001F3F",
                TextColor = "#FFD700",
                TitleColor = "#FFD700",
                LabelColor = "#FFC700",
                AccentColor = "#FFD700",
                // Background
                BackgroundOpacity = 1.0,
                BackgroundImagePath = null,
                BackgroundImageMode = "Fill",
                BackgroundImageOpacity = 0.3,
                // Layout
                TextAlignment = "Center",
                VerticalAlignment = "Center",
                MarginLeft = 100,
                MarginRight = 100,
                MarginTop = 60,
                MarginBottom = 60,
                // Effects
                EnableTextShadow = true,
                ShadowColor = "#000000",
                ShadowBlurRadius = 12,
                ShadowOffsetX = 2,
                ShadowOffsetY = 2,
                EnableTextOutline = false,
                OutlineColor = "#000000",
                OutlineThickness = 2,
                // Advanced
                TransparentBackground = false,
                ShowVerseNumbers = true,
                ShowHymnTitle = true
            },
            // 5. Minimalist
            new DisplayProfile
            {
                Id = 5,
                Name = "Minimalist",
                Description = "Clean and simple with minimal styling",
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = now,
                UpdatedAt = now,
                // Typography
                FontFamily = "Inter",
                TitleFontSize = 32,
                VerseFontSize = 44,
                LabelFontSize = 26,
                FontWeight = "Normal",
                LineHeight = 1.5,
                LetterSpacing = 0,
                // Colors
                BackgroundColor = "#000000",
                TextColor = "#EEEEEE",
                TitleColor = "#EEEEEE",
                LabelColor = "#CCCCCC",
                AccentColor = "#666666",
                // Background
                BackgroundOpacity = 1.0,
                BackgroundImagePath = null,
                BackgroundImageMode = "Fill",
                BackgroundImageOpacity = 0.3,
                // Layout
                TextAlignment = "Left",
                VerticalAlignment = "Center",
                MarginLeft = 80,
                MarginRight = 80,
                MarginTop = 50,
                MarginBottom = 50,
                // Effects
                EnableTextShadow = false,
                ShadowColor = "#000000",
                ShadowBlurRadius = 10,
                ShadowOffsetX = 2,
                ShadowOffsetY = 2,
                EnableTextOutline = false,
                OutlineColor = "#000000",
                OutlineThickness = 2,
                // Advanced
                TransparentBackground = false,
                ShowVerseNumbers = true,
                ShowHymnTitle = true
            },
            // 6. Traditional
            new DisplayProfile
            {
                Id = 6,
                Name = "Traditional",
                Description = "Classic church aesthetic with navy and gold colors",
                IsDefault = false,
                IsSystemProfile = true,
                CreatedAt = now,
                UpdatedAt = now,
                // Typography
                FontFamily = "Georgia",
                TitleFontSize = 40,
                VerseFontSize = 50,
                LabelFontSize = 32,
                FontWeight = "Normal",
                LineHeight = 1.5,
                LetterSpacing = 0,
                // Colors
                BackgroundColor = "#00274D",
                TextColor = "#D4AF37",
                TitleColor = "#D4AF37",
                LabelColor = "#C4A030",
                AccentColor = "#D4AF37",
                // Background
                BackgroundOpacity = 1.0,
                BackgroundImagePath = null,
                BackgroundImageMode = "Fill",
                BackgroundImageOpacity = 0.3,
                // Layout
                TextAlignment = "Center",
                VerticalAlignment = "Center",
                MarginLeft = 120,
                MarginRight = 120,
                MarginTop = 70,
                MarginBottom = 70,
                // Effects
                EnableTextShadow = false,
                ShadowColor = "#000000",
                ShadowBlurRadius = 10,
                ShadowOffsetX = 2,
                ShadowOffsetY = 2,
                EnableTextOutline = false,
                OutlineColor = "#000000",
                OutlineThickness = 2,
                // Advanced
                TransparentBackground = false,
                ShowVerseNumbers = true,
                ShowHymnTitle = true
            }
        );

        // Seed initial AppSettings (key-value store)
        modelBuilder.Entity<AppSetting>().HasData(
            new AppSetting
            {
                Id = 1,
                Key = "LastDatabaseImportDate",
                Value = "",
                Description = "Last time legacy XML was imported",
                CreatedAt = now,
                UpdatedAt = now
            },
            new AppSetting
            {
                Id = 2,
                Key = "DefaultDisplayProfileId",
                Value = "1",
                Description = "Default display profile ID",
                CreatedAt = now,
                UpdatedAt = now
            },
            new AppSetting
            {
                Id = 3,
                Key = "EnableStatisticsTracking",
                Value = "true",
                Description = "Track hymn usage statistics",
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        // Seed initial AppSettings (singleton)
        modelBuilder.Entity<AppSettings>().HasData(
            new AppSettings
            {
                Id = 1,
                AudioLibraryPath = null,  // User will configure
                AudioOutputDeviceId = null,  // Use default device
                AudioAutoPlayDelay = 5,
                GlobalVolume = 0.8f,
                AutoAdvanceEnabled = false,
                ActiveDisplayProfileId = 1,  // Classic Dark
                LastWindowPosition = null,
                LastWindowSize = null,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
