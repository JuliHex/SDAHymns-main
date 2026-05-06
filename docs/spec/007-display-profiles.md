# Spec 007: Display Profiles System

**Status:** 📋 Planned
**Created:** 2025-12-04
**Dependencies:** 005-basic-hymn-display.md

## Overview

Implement a comprehensive display profiles system allowing users to customize hymn display appearance for different scenarios (projector, OBS streaming, practice, etc.). Each profile stores font, color, background, opacity, and layout settings.

**Goal:** Give users full control over hymn display appearance to match different projection equipment, lighting conditions, and streaming requirements.

## Goals

1. Create, save, and switch between multiple display profiles
2. Customize fonts (family, size, weight, style)
3. Customize colors (text, background, accent)
4. Customize background opacity and images
5. Profile-specific layout settings (alignment, margins, spacing)
6. Quick profile switching during services
7. Import/export profiles for sharing
8. Preset profiles for common scenarios

**Non-Goals (Future Phases):**
- Per-verse customization
- Animated transitions
- Video backgrounds
- Real-time collaborative editing

## Architecture

### DisplayProfile Model

**File:** `src/SDAHymns.Core/Data/Models/DisplayProfile.cs`

```csharp
public class DisplayProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Typography
    public string FontFamily { get; set; } = "Inter";
    public int TitleFontSize { get; set; } = 36;
    public int VerseFontSize { get; set; } = 48;
    public int LabelFontSize { get; set; } = 28;
    public string FontWeight { get; set; } = "Normal"; // Normal, SemiBold, Bold
    public double LineHeight { get; set; } = 1.4;
    public double LetterSpacing { get; set; } = 0;

    // Colors
    public string BackgroundColor { get; set; } = "#000000";
    public string TextColor { get; set; } = "#FFFFFF";
    public string TitleColor { get; set; } = "#FFFFFF";
    public string LabelColor { get; set; } = "#CCCCCC";
    public string AccentColor { get; set; } = "#0078D4";

    // Background
    public double BackgroundOpacity { get; set; } = 1.0;
    public string? BackgroundImagePath { get; set; }
    public string BackgroundImageMode { get; set; } = "Fill"; // Fill, Fit, Stretch, Tile
    public double BackgroundImageOpacity { get; set; } = 0.3;

    // Layout
    public string TextAlignment { get; set; } = "Left"; // Left, Center, Right
    public string VerticalAlignment { get; set; } = "Center"; // Top, Center, Bottom
    public int MarginLeft { get; set; } = 100;
    public int MarginRight { get; set; } = 100;
    public int MarginTop { get; set; } = 60;
    public int MarginBottom { get; set; } = 60;

    // Effects
    public bool EnableTextShadow { get; set; } = false;
    public string ShadowColor { get; set; } = "#000000";
    public int ShadowBlurRadius { get; set; } = 10;
    public int ShadowOffsetX { get; set; } = 2;
    public int ShadowOffsetY { get; set; } = 2;

    public bool EnableTextOutline { get; set; } = false;
    public string OutlineColor { get; set; } = "#000000";
    public int OutlineThickness { get; set; } = 2;

    // Advanced
    public bool TransparentBackground { get; set; } = false;
    public bool ShowVerseNumbers { get; set; } = true;
    public bool ShowHymnTitle { get; set; } = true;
}
```

### Preset Profiles

**Built-in profiles** shipped with the application:

1. **Classic Dark (Default)**
   - Black background, white text
   - Inter font, 48px verse size
   - Left-aligned, standard margins
   - No effects

2. **High Contrast**
   - Pure black background (#000)
   - Pure white text (#FFF)
   - Bold font weight
   - Larger font sizes (56px)
   - Text shadow for better visibility

3. **OBS Stream Optimized**
   - Transparent background
   - White text with black outline
   - Centered alignment
   - Larger margins
   - Text effects for readability over video

4. **Projector - Bright Room**
   - Dark blue background (#001F3F)
   - Yellow text (#FFD700)
   - Bold weight
   - High contrast for bright environments

5. **Minimalist**
   - Pure black background
   - Light gray text (#EEEEEE)
   - Thin font weight
   - Minimal margins
   - Clean, simple appearance

6. **Traditional**
   - Navy background (#00274D)
   - Gold text (#D4AF37)
   - Serif font (Georgia)
   - Centered alignment
   - Classic church aesthetic

## Implementation Plan

### Step 1: Database Migration

```bash
dotnet ef migrations add AddDisplayProfiles --project src/SDAHymns.Core
```

```csharp
public partial class AddDisplayProfiles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DisplayProfiles",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                IsDefault = table.Column<bool>(nullable: false),
                // ... all properties from model
            });

        // Seed default profiles
        migrationBuilder.InsertData(
            table: "DisplayProfiles",
            columns: new[] { "Name", "IsDefault", "BackgroundColor", /* ... */ },
            values: new object[] { "Classic Dark", true, "#000000", /* ... */ });
    }
}
```

### Step 2: DisplayProfileService

**File:** `src/SDAHymns.Core/Services/DisplayProfileService.cs`

```csharp
public interface IDisplayProfileService
{
    Task<List<DisplayProfile>> GetAllProfilesAsync();
    Task<DisplayProfile?> GetProfileByIdAsync(int id);
    Task<DisplayProfile> GetActiveProfileAsync();
    Task<DisplayProfile> CreateProfileAsync(DisplayProfile profile);
    Task<DisplayProfile> UpdateProfileAsync(DisplayProfile profile);
    Task DeleteProfileAsync(int id);
    Task SetActiveProfileAsync(int id);
    Task<DisplayProfile> DuplicateProfileAsync(int id, string newName);
    Task<string> ExportProfileAsync(int id);
    Task<DisplayProfile> ImportProfileAsync(string json);
}
```

### Step 3: Profile Editor UI

**File:** `src/SDAHymns.Desktop/Views/ProfileEditorWindow.axaml`

```xml
<Window Title="Display Profile Editor" Width="900" Height="700">
    <Grid ColumnDefinitions="250,*">
        <!-- Left: Profile List -->
        <Border Grid.Column="0" Background="#1A1A1A" BorderBrush="#333" BorderThickness="0,0,1,0">
            <StackPanel>
                <TextBlock Text="Profiles" FontSize="18" Margin="15" FontWeight="Bold"/>

                <ListBox ItemsSource="{Binding Profiles}"
                         SelectedItem="{Binding SelectedProfile}">
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <StackPanel>
                                <TextBlock Text="{Binding Name}" FontWeight="SemiBold"/>
                                <TextBlock Text="{Binding Description}"
                                          FontSize="11"
                                          Foreground="#888"/>
                            </StackPanel>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>

                <StackPanel Margin="10">
                    <Button Content="+ New Profile" Command="{Binding CreateProfileCommand}"/>
                    <Button Content="Duplicate" Command="{Binding DuplicateProfileCommand}"/>
                    <Button Content="Delete" Command="{Binding DeleteProfileCommand}"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- Right: Profile Editor -->
        <ScrollViewer Grid.Column="1">
            <StackPanel Margin="20" Spacing="15">
                <!-- Profile Info -->
                <TextBlock Text="Profile Settings" FontSize="20" FontWeight="Bold"/>

                <TextBox Header="Profile Name" Text="{Binding SelectedProfile.Name}"/>
                <TextBox Header="Description" Text="{Binding SelectedProfile.Description}"/>

                <!-- Typography Section -->
                <Separator/>
                <TextBlock Text="Typography" FontSize="16" FontWeight="SemiBold"/>

                <ComboBox Header="Font Family"
                          SelectedItem="{Binding SelectedProfile.FontFamily}">
                    <ComboBoxItem Content="Inter"/>
                    <ComboBoxItem Content="Arial"/>
                    <ComboBoxItem Content="Georgia"/>
                    <ComboBoxItem Content="Times New Roman"/>
                    <ComboBoxItem Content="Verdana"/>
                </ComboBox>

                <Grid ColumnDefinitions="*,*,*">
                    <NumericUpDown Grid.Column="0" Header="Title Size"
                                  Value="{Binding SelectedProfile.TitleFontSize}"
                                  Minimum="12" Maximum="100"/>
                    <NumericUpDown Grid.Column="1" Header="Verse Size"
                                  Value="{Binding SelectedProfile.VerseFontSize}"
                                  Minimum="12" Maximum="100"/>
                    <NumericUpDown Grid.Column="2" Header="Label Size"
                                  Value="{Binding SelectedProfile.LabelFontSize}"
                                  Minimum="12" Maximum="100"/>
                </Grid>

                <ComboBox Header="Font Weight"
                          SelectedItem="{Binding SelectedProfile.FontWeight}">
                    <ComboBoxItem Content="Normal"/>
                    <ComboBoxItem Content="SemiBold"/>
                    <ComboBoxItem Content="Bold"/>
                </ComboBox>

                <!-- Colors Section -->
                <Separator/>
                <TextBlock Text="Colors" FontSize="16" FontWeight="SemiBold"/>

                <Grid ColumnDefinitions="*,*">
                    <ColorPicker Grid.Column="0" Header="Background"
                                Color="{Binding SelectedProfile.BackgroundColor}"/>
                    <ColorPicker Grid.Column="1" Header="Text"
                                Color="{Binding SelectedProfile.TextColor}"/>
                </Grid>

                <Grid ColumnDefinitions="*,*">
                    <ColorPicker Grid.Column="0" Header="Title"
                                Color="{Binding SelectedProfile.TitleColor}"/>
                    <ColorPicker Grid.Column="1" Header="Label"
                                Color="{Binding SelectedProfile.LabelColor}"/>
                </Grid>

                <!-- Background Section -->
                <Separator/>
                <TextBlock Text="Background" FontSize="16" FontWeight="SemiBold"/>

                <Slider Header="Background Opacity"
                        Value="{Binding SelectedProfile.BackgroundOpacity}"
                        Minimum="0" Maximum="1" TickFrequency="0.1"/>

                <CheckBox Content="Transparent Background"
                         IsChecked="{Binding SelectedProfile.TransparentBackground}"/>

                <!-- Layout Section -->
                <Separator/>
                <TextBlock Text="Layout" FontSize="16" FontWeight="SemiBold"/>

                <ComboBox Header="Text Alignment"
                          SelectedItem="{Binding SelectedProfile.TextAlignment}">
                    <ComboBoxItem Content="Left"/>
                    <ComboBoxItem Content="Center"/>
                    <ComboBoxItem Content="Right"/>
                </ComboBox>

                <Grid ColumnDefinitions="*,*,*,*">
                    <NumericUpDown Grid.Column="0" Header="Margin Left"
                                  Value="{Binding SelectedProfile.MarginLeft}"/>
                    <NumericUpDown Grid.Column="1" Header="Margin Right"
                                  Value="{Binding SelectedProfile.MarginRight}"/>
                    <NumericUpDown Grid.Column="2" Header="Margin Top"
                                  Value="{Binding SelectedProfile.MarginTop}"/>
                    <NumericUpDown Grid.Column="3" Header="Margin Bottom"
                                  Value="{Binding SelectedProfile.MarginBottom}"/>
                </Grid>

                <!-- Effects Section -->
                <Separator/>
                <TextBlock Text="Effects" FontSize="16" FontWeight="SemiBold"/>

                <CheckBox Content="Enable Text Shadow"
                         IsChecked="{Binding SelectedProfile.EnableTextShadow}"/>

                <CheckBox Content="Enable Text Outline"
                         IsChecked="{Binding SelectedProfile.EnableTextOutline}"/>

                <!-- Action Buttons -->
                <Separator/>
                <StackPanel Orientation="Horizontal" Spacing="10">
                    <Button Content="Save Changes" Command="{Binding SaveProfileCommand}"/>
                    <Button Content="Reset to Default" Command="{Binding ResetProfileCommand}"/>
                    <Button Content="Preview" Command="{Binding PreviewProfileCommand}"/>
                    <Button Content="Export" Command="{Binding ExportProfileCommand}"/>
                    <Button Content="Import" Command="{Binding ImportProfileCommand}"/>
                </StackPanel>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</Window>
```

### Step 4: Apply Profile to DisplayWindow

**File:** `src/SDAHymns.Desktop/Views/DisplayWindow.axaml.cs`

```csharp
public void ApplyProfile(DisplayProfile profile)
{
    // Apply to root border
    if (this.FindControl<Border>("RootBorder") is Border rootBorder)
    {
        rootBorder.Background = new SolidColorBrush(Color.Parse(profile.BackgroundColor));
        rootBorder.Opacity = profile.BackgroundOpacity;
    }

    // Apply to title
    if (this.FindControl<TextBlock>("HymnTitleText") is TextBlock titleText)
    {
        titleText.FontFamily = new FontFamily(profile.FontFamily);
        titleText.FontSize = profile.TitleFontSize;
        titleText.Foreground = new SolidColorBrush(Color.Parse(profile.TitleColor));
        titleText.TextAlignment = Enum.Parse<TextAlignment>(profile.TextAlignment);
    }

    // Apply to verse content
    if (this.FindControl<TextBlock>("VerseContentText") is TextBlock verseText)
    {
        verseText.FontFamily = new FontFamily(profile.FontFamily);
        verseText.FontSize = profile.VerseFontSize;
        verseText.Foreground = new SolidColorBrush(Color.Parse(profile.TextColor));
        verseText.TextAlignment = Enum.Parse<TextAlignment>(profile.TextAlignment);
        verseText.LineHeight = profile.LineHeight * profile.VerseFontSize;

        // Apply effects
        if (profile.EnableTextShadow)
        {
            verseText.Effect = new DropShadowEffect
            {
                Color = Color.Parse(profile.ShadowColor),
                BlurRadius = profile.ShadowBlurRadius,
                OffsetX = profile.ShadowOffsetX,
                OffsetY = profile.ShadowOffsetY
            };
        }
    }

    // Apply margins
    if (this.FindControl<StackPanel>("ContentPanel") is StackPanel contentPanel)
    {
        contentPanel.Margin = new Thickness(
            profile.MarginLeft,
            profile.MarginTop,
            profile.MarginRight,
            profile.MarginBottom
        );
    }
}
```

### Step 5: Quick Profile Switcher

Add to MainWindow toolbar:

```xml
<ComboBox Header="Profile:"
          ItemsSource="{Binding AvailableProfiles}"
          SelectedItem="{Binding ActiveProfile}"
          DisplayMemberPath="Name"
          Width="150"/>

<Button Content="⚙ Edit Profiles" Command="{Binding OpenProfileEditorCommand}"/>
```

## Features

### 1. Profile Management
- **Create** new profiles from scratch
- **Duplicate** existing profiles as starting point
- **Delete** custom profiles (cannot delete presets)
- **Set default** profile for startup

### 2. Live Preview
- **Real-time preview** as settings change
- **Test hymn** button to preview with actual content
- **Side-by-side** comparison of profiles

### 3. Import/Export
- **JSON format** for profiles
- **Share profiles** with other users/computers
- **Backup profiles** before changes

### 4. Quick Switching
- **Dropdown in toolbar** for instant profile switching
- **Keyboard shortcut** (Ctrl+1-9 for profiles 1-9)
- **Profile applies immediately** to display window

## Testing Strategy

### Manual Testing
1. Create new profile with custom colors
2. Change font size, verify display updates
3. Toggle transparent background for OBS
4. Export profile, import on different machine
5. Switch profiles during live display
6. Test all preset profiles

### Unit Tests
```csharp
[Fact]
public async Task CreateProfileAsync_CreatesNewProfile()
{
    var profile = new DisplayProfile { Name = "Test Profile" };
    var created = await _profileService.CreateProfileAsync(profile);
    Assert.NotEqual(0, created.Id);
}

[Fact]
public async Task GetActiveProfileAsync_ReturnsDefaultWhenNoneSet()
{
    var profile = await _profileService.GetActiveProfileAsync();
    Assert.True(profile.IsDefault);
}
```

## Acceptance Criteria

- [ ] Can create, edit, delete custom profiles
- [ ] Preset profiles available on first run
- [ ] Profile changes apply to display window in real-time
- [ ] Can switch profiles from dropdown
- [ ] Export/import profiles as JSON
- [ ] Font family, size, weight, color all apply correctly
- [ ] Background color and opacity work
- [ ] Text alignment (left/center/right) works
- [ ] Margins apply correctly
- [ ] Text shadow and outline effects work
- [ ] Transparent background works for OBS
- [ ] Profile persists after app restart
- [ ] Cannot delete preset profiles

## Future Enhancements (Phase 3)

- Background image support
- Video backgrounds
- Gradient backgrounds
- Per-verse overrides
- Animated transitions between verses
- Profile templates/themes marketplace
- Cloud sync for profiles

## Related Specs

- **Previous:** 005-basic-hymn-display.md
- **Next:** 008-keyboard-shortcuts.md
- **Related:** 006-enhanced-control-window.md

## Notes

- Keep UI simple - too many options overwhelm users
- Provide good presets so most users don't need customization
- Live preview is critical for understanding changes
- Export/import enables sharing best practices across churches
