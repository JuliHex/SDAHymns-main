# Spec 009: Service Planner

**Status:** 📋 Planned
**Created:** 2025-12-04
**Dependencies:** 006-enhanced-control-window.md

## Overview

Implement a service planner that allows users to pre-plan hymn orders for church services. Users can create setlists, reorder hymns, add notes, and follow along during live services with one-click hymn switching.

**Goal:** Enable worship leaders to prepare services in advance and operators to follow a pre-planned order during live services, reducing mistakes and improving flow.

## Goals

1. Create, save, and load service plans
2. Add hymns to service plan via drag-and-drop or search
3. Reorder hymns within the plan
4. Add notes and metadata to plans (service name, date, speaker, etc.)
5. Live mode: Step through plan during service
6. Mark hymns as "played" during service
7. Export plans to PDF for printing bulletins
8. Template system for recurring services

**Non-Goals (Future Phases):**
- Cloud sync across devices
- Real-time collaborative editing
- Integration with church management software
- Automatic chord/key transposition
- Music notation display

## Architecture

### ServicePlan Model

**File:** `src/SDAHymns.Core/Data/Models/ServicePlan.cs`

```csharp
public class ServicePlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public string? Speaker { get; set; }
    public string? Notes { get; set; }
    public string? Theme { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsTemplate { get; set; }

    public List<ServicePlanItem> Items { get; set; } = new();
}

public class ServicePlanItem
{
    public int Id { get; set; }
    public int ServicePlanId { get; set; }
    public int HymnId { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }
    public bool IsPlayed { get; set; } // Marked during live service
    public string? SelectedVerses { get; set; } // e.g., "1,2,4" - skip verse 3

    public ServicePlan ServicePlan { get; set; } = null!;
    public Hymn Hymn { get; set; } = null!;
}
```

### UI Layout

```
┌────────────────────────────────────────────────────┐
│  Service Planner - Sabbath Service (Dec 7, 2024)  │
├────────────────────────────────────────────────────┤
│  Speaker: [Elder John Smith      ]  Date: [12/7]  │
│  Theme:   [The Love of Christ              ]      │
├──────────────┬─────────────────────────────────────┤
│              │  Service Plan                       │
│ Hymn Library │  ┌─────────────────────────────────┐│
│              │  │ ☐ 1. Spre slava Ta uniţi        ││
│ [Search...]  │  │    Opening Hymn - All verses    ││
│              │  │                                  ││
│ Recent:      │  │ ☐ 20. Aleluia! Răsună cântec   ││
│  [1] [20]    │  │    Before Prayer - v1,2,4 only  ││
│              │  │                                  ││
│ 📁 Crestine  │  │ ☑ 45. Domnul e stânca mea      ││
│   1. Spre    │  │    Special Music (PLAYED)       ││
│   20. Alel.. │  │                                  ││
│   45. Domn.. │  │ ☐ 99. O, ce prieten avem       ││
│   ...        │  │    Closing - All                ││
│              │  └─────────────────────────────────┘│
│              │                                      │
│              │  [+ Add Selected] [▲ Move Up]       │
│              │  [- Remove]       [▼ Move Down]     │
├──────────────┴─────────────────────────────────────┤
│  [Save] [Load] [Export PDF] [Live Mode] [Close]   │
└────────────────────────────────────────────────────┘
```

## Implementation Plan

### Step 1: Database Migration

```bash
dotnet ef migrations add AddServicePlanner --project src/SDAHymns.Core
```

```csharp
public partial class AddServicePlanner : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ServicePlans",
            columns: table => new
            {
                Id = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: false),
                ServiceDate = table.Column<DateTime>(nullable: false),
                Speaker = table.Column<string>(nullable: true),
                Notes = table.Column<string>(nullable: true),
                Theme = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: false),
                IsTemplate = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ServicePlans", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ServicePlanItems",
            columns: table => new
            {
                Id = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                ServicePlanId = table.Column<int>(nullable: false),
                HymnId = table.Column<int>(nullable: false),
                DisplayOrder = table.Column<int>(nullable: false),
                Notes = table.Column<string>(nullable: true),
                IsPlayed = table.Column<bool>(nullable: false),
                SelectedVerses = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ServicePlanItems", x => x.Id);
                table.ForeignKey("FK_ServicePlanItems_ServicePlans",
                    x => x.ServicePlanId,
                    "ServicePlans", "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ServicePlanItems_Hymns",
                    x => x.HymnId,
                    "Hymns", "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }
}
```

### Step 2: ServicePlanService

**File:** `src/SDAHymns.Core/Services/ServicePlanService.cs`

```csharp
public interface IServicePlanService
{
    Task<List<ServicePlan>> GetAllPlansAsync();
    Task<ServicePlan?> GetPlanByIdAsync(int id);
    Task<ServicePlan> CreatePlanAsync(ServicePlan plan);
    Task<ServicePlan> UpdatePlanAsync(ServicePlan plan);
    Task DeletePlanAsync(int id);
    Task<ServicePlanItem> AddHymnToPlanAsync(int planId, int hymnId, string? notes = null);
    Task RemoveHymnFromPlanAsync(int planId, int itemId);
    Task ReorderPlanItemsAsync(int planId, List<int> itemIds);
    Task MarkItemAsPlayedAsync(int itemId, bool isPlayed);
    Task<ServicePlan> DuplicatePlanAsync(int planId, string newName);
    Task<ServicePlan> CreateFromTemplateAsync(int templateId, DateTime serviceDate);
    Task<byte[]> ExportToPdfAsync(int planId);
}
```

### Step 3: ServicePlannerViewModel

**File:** `src/SDAHymns.Desktop/ViewModels/ServicePlannerViewModel.cs`

```csharp
public partial class ServicePlannerViewModel : ViewModelBase
{
    private readonly IServicePlanService _planService;
    private readonly ISearchService _searchService;

    [ObservableProperty]
    private ServicePlan? _currentPlan;

    [ObservableProperty]
    private ObservableCollection<ServicePlanItem> _planItems = new();

    [ObservableProperty]
    private ObservableCollection<HymnSearchResult> _availableHymns = new();

    [ObservableProperty]
    private HymnSearchResult? _selectedHymn;

    [ObservableProperty]
    private ServicePlanItem? _selectedPlanItem;

    [ObservableProperty]
    private bool _isLiveMode = false;

    [RelayCommand]
    private async Task CreateNewPlanAsync()
    {
        CurrentPlan = new ServicePlan
        {
            Name = $"Service - {DateTime.Now:MMM d, yyyy}",
            ServiceDate = DateTime.Now,
            CreatedAt = DateTime.UtcNow
        };

        CurrentPlan = await _planService.CreatePlanAsync(CurrentPlan);
        PlanItems.Clear();
    }

    [RelayCommand]
    private async Task LoadPlanAsync(int planId)
    {
        CurrentPlan = await _planService.GetPlanByIdAsync(planId);
        if (CurrentPlan != null)
        {
            PlanItems = new ObservableCollection<ServicePlanItem>(
                CurrentPlan.Items.OrderBy(i => i.DisplayOrder));
        }
    }

    [RelayCommand]
    private async Task AddSelectedHymnAsync()
    {
        if (SelectedHymn == null || CurrentPlan == null) return;

        var item = await _planService.AddHymnToPlanAsync(
            CurrentPlan.Id,
            SelectedHymn.Id);

        PlanItems.Add(item);
    }

    [RelayCommand]
    private async Task RemoveSelectedItemAsync()
    {
        if (SelectedPlanItem == null || CurrentPlan == null) return;

        await _planService.RemoveHymnFromPlanAsync(
            CurrentPlan.Id,
            SelectedPlanItem.Id);

        PlanItems.Remove(SelectedPlanItem);
    }

    [RelayCommand]
    private async Task MoveItemUpAsync()
    {
        if (SelectedPlanItem == null) return;

        var index = PlanItems.IndexOf(SelectedPlanItem);
        if (index > 0)
        {
            PlanItems.Move(index, index - 1);
            await SaveOrderAsync();
        }
    }

    [RelayCommand]
    private async Task MoveItemDownAsync()
    {
        if (SelectedPlanItem == null) return;

        var index = PlanItems.IndexOf(SelectedPlanItem);
        if (index < PlanItems.Count - 1)
        {
            PlanItems.Move(index, index + 1);
            await SaveOrderAsync();
        }
    }

    private async Task SaveOrderAsync()
    {
        if (CurrentPlan == null) return;

        var itemIds = PlanItems.Select(i => i.Id).ToList();
        await _planService.ReorderPlanItemsAsync(CurrentPlan.Id, itemIds);
    }

    [RelayCommand]
    private async Task EnterLiveModeAsync()
    {
        IsLiveMode = true;
        // Auto-select first unplayed item
        SelectedPlanItem = PlanItems.FirstOrDefault(i => !i.IsPlayed);
    }

    [RelayCommand]
    private async Task NextInPlanAsync()
    {
        if (!IsLiveMode || SelectedPlanItem == null) return;

        // Mark current as played
        await _planService.MarkItemAsPlayedAsync(SelectedPlanItem.Id, true);
        SelectedPlanItem.IsPlayed = true;

        // Move to next
        var index = PlanItems.IndexOf(SelectedPlanItem);
        if (index < PlanItems.Count - 1)
        {
            SelectedPlanItem = PlanItems[index + 1];
            // Load this hymn to display
            // ... trigger load in main window
        }
    }

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        if (CurrentPlan == null) return;

        var pdfBytes = await _planService.ExportToPdfAsync(CurrentPlan.Id);

        var saveDialog = new SaveFileDialog
        {
            DefaultExtension = "pdf",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
        }
    }
}
```

### Step 4: ServicePlanner XAML

**File:** `src/SDAHymns.Desktop/Views/ServicePlannerWindow.axaml`

```xml
<Window Title="{Binding WindowTitle}" Width="1000" Height="700">
    <Grid RowDefinitions="Auto,*,Auto">
        <!-- Header: Plan Info -->
        <Grid Grid.Row="0" Background="#1A1A1A" Padding="15" ColumnDefinitions="*,Auto,Auto">
            <StackPanel Grid.Column="0">
                <TextBox Text="{Binding CurrentPlan.Name}"
                         FontSize="18" FontWeight="Bold"
                         Watermark="Service Name"/>
                <Grid ColumnDefinitions="Auto,*,Auto,Auto">
                    <TextBlock Grid.Column="0" Text="Speaker:" Margin="0,5,10,0"/>
                    <TextBox Grid.Column="1" Text="{Binding CurrentPlan.Speaker}"/>
                    <TextBlock Grid.Column="2" Text="Date:" Margin="20,5,10,0"/>
                    <DatePicker Grid.Column="3" SelectedDate="{Binding CurrentPlan.ServiceDate}"/>
                </Grid>
                <TextBox Text="{Binding CurrentPlan.Theme}"
                         Watermark="Service Theme"
                         Margin="0,5,0,0"/>
            </StackPanel>

            <Button Grid.Column="1"
                    Content="{Binding LiveModeButtonLabel}"
                    Command="{Binding ToggleLiveModeCommand}"
                    Background="#0078D4"
                    Width="120" Height="40"
                    Margin="10,0"/>

            <Button Grid.Column="2"
                    Content="💾 Save"
                    Command="{Binding SavePlanCommand}"
                    Width="100" Height="40"/>
        </Grid>

        <!-- Main Content -->
        <Grid Grid.Row="1" ColumnDefinitions="300,*">
            <!-- Left: Hymn Library -->
            <Border Grid.Column="0" Background="#0D0D0D" BorderBrush="#333" BorderThickness="0,0,1,0">
                <Grid RowDefinitions="Auto,Auto,*">
                    <TextBox Grid.Row="0"
                             Text="{Binding SearchQuery}"
                             Watermark="Search hymns..."
                             Margin="10"/>

                    <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="10,0,10,10">
                        <TextBlock Text="Recent:" Foreground="#888" Margin="0,0,10,0"/>
                        <!-- Recent hymns -->
                    </StackPanel>

                    <ListBox Grid.Row="2"
                             ItemsSource="{Binding AvailableHymns}"
                             SelectedItem="{Binding SelectedHymn}"
                             Background="#0D0D0D">
                        <ListBox.ItemTemplate>
                            <DataTemplate>
                                <StackPanel>
                                    <TextBlock>
                                        <Run Text="{Binding Number}"/>
                                        <Run Text=". "/>
                                        <Run Text="{Binding Title}"/>
                                    </TextBlock>
                                    <TextBlock Text="{Binding CategoryName}"
                                              FontSize="11" Foreground="#666"/>
                                </StackPanel>
                            </DataTemplate>
                        </ListBox.ItemTemplate>
                    </ListBox>
                </Grid>
            </Border>

            <!-- Right: Service Plan -->
            <Grid Grid.Column="1" RowDefinitions="Auto,*,Auto">
                <TextBlock Grid.Row="0"
                           Text="Service Plan"
                           FontSize="16" FontWeight="SemiBold"
                           Margin="15,10"/>

                <ListBox Grid.Row="1"
                         ItemsSource="{Binding PlanItems}"
                         SelectedItem="{Binding SelectedPlanItem}"
                         AllowDrop="True"
                         Background="Black"
                         Margin="10">
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <Grid ColumnDefinitions="Auto,Auto,*,Auto" Margin="5">
                                <CheckBox Grid.Column="0"
                                         IsChecked="{Binding IsPlayed}"
                                         Margin="0,0,10,0"/>

                                <TextBlock Grid.Column="1"
                                          Text="{Binding DisplayOrder}"
                                          Foreground="#666"
                                          Margin="0,0,10,0"/>

                                <StackPanel Grid.Column="2">
                                    <TextBlock FontWeight="SemiBold">
                                        <Run Text="{Binding Hymn.Number}"/>
                                        <Run Text=". "/>
                                        <Run Text="{Binding Hymn.Title}"/>
                                    </TextBlock>
                                    <TextBlock Text="{Binding Notes}"
                                              FontSize="11"
                                              Foreground="#888"
                                              TextWrapping="Wrap"/>
                                </StackPanel>

                                <TextBlock Grid.Column="3"
                                          Text="{Binding Hymn.Verses.Count}"
                                          Foreground="#666"
                                          Margin="10,0"/>
                            </Grid>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>

                <StackPanel Grid.Row="2" Orientation="Horizontal" Margin="10" Spacing="5">
                    <Button Content="+ Add Selected"
                            Command="{Binding AddSelectedHymnCommand}"/>
                    <Button Content="- Remove"
                            Command="{Binding RemoveSelectedItemCommand}"/>
                    <Button Content="▲ Move Up"
                            Command="{Binding MoveItemUpCommand}"/>
                    <Button Content="▼ Move Down"
                            Command="{Binding MoveItemDownCommand}"/>
                    <Button Content="✏ Edit Notes"
                            Command="{Binding EditNotesCommand}"/>
                </StackPanel>
            </Grid>
        </Grid>

        <!-- Footer: Actions -->
        <Border Grid.Row="2" Background="#1A1A1A" Padding="15">
            <StackPanel Orientation="Horizontal" Spacing="10">
                <Button Content="New Plan" Command="{Binding CreateNewPlanCommand}"/>
                <Button Content="Load Plan" Command="{Binding OpenLoadDialogCommand}"/>
                <Button Content="Save As" Command="{Binding SaveAsCommand}"/>
                <Button Content="Export PDF" Command="{Binding ExportToPdfCommand}"/>
                <Button Content="Templates" Command="{Binding OpenTemplatesCommand}"/>
                <Button Content="Close" Command="{Binding CloseCommand}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

## Features

### 1. Plan Management
- **Create new** service plan
- **Save/Load** plans from database
- **Save as template** for recurring services
- **Duplicate plan** for similar services

### 2. Hymn Addition
- **Search and add** hymns from library
- **Drag and drop** from search results to plan
- **Recent hymns** quick-add buttons
- **Double-click** to add hymn

### 3. Plan Editing
- **Reorder** hymns with up/down buttons or drag-and-drop
- **Remove** hymns from plan
- **Add notes** to each hymn (e.g., "Opening", "Special Music")
- **Select specific verses** to display (skip verses)

### 4. Live Mode
- **Follow along** during service
- **One-click** to load next hymn
- **Mark as played** automatically
- **Visual progress** indicator

### 5. Export
- **PDF export** for printing bulletins
- **Include** hymn numbers, titles, verse counts
- **Formatted** for church bulletin

### 6. Templates
- **Save as template** (recurring Sabbath services)
- **Create from template** with new date
- **Template library** for different service types

## Testing Strategy

### Manual Testing
1. Create new service plan
2. Add 5 hymns to plan
3. Reorder hymns
4. Add notes to hymns
5. Enter live mode
6. Step through plan
7. Export to PDF
8. Save and reload plan

### Unit Tests
```csharp
[Fact]
public async Task CreatePlanAsync_CreatesNewPlan()
{
    var plan = new ServicePlan { Name = "Test Service" };
    var created = await _planService.CreatePlanAsync(plan);
    Assert.NotEqual(0, created.Id);
}

[Fact]
public async Task AddHymnToPlanAsync_AddsHymn()
{
    var item = await _planService.AddHymnToPlanAsync(1, 20);
    Assert.Equal(1, item.ServicePlanId);
    Assert.Equal(20, item.HymnId);
}
```

## Acceptance Criteria

- [ ] Can create, save, load service plans
- [ ] Can add hymns to plan from search
- [ ] Can reorder hymns in plan
- [ ] Can remove hymns from plan
- [ ] Can add notes to each hymn
- [ ] Live mode steps through plan correctly
- [ ] Marks hymns as played
- [ ] Exports plan to PDF
- [ ] Can create templates
- [ ] Can create plan from template
- [ ] Plan persists after app restart
- [ ] Drag-and-drop reordering works

## Future Enhancements (Phase 3)

- Cloud sync across devices
- Share plans with other churches
- Recurring service automation (auto-create plans)
- Hymn usage statistics from plans
- Print-friendly bulletin templates
- Integration with worship planning software (Planning Center, etc.)

## Related Specs

- **Previous:** 006-enhanced-control-window.md (provides hymn search)
- **Related:** 008-keyboard-shortcuts.md (Ctrl+N for new plan)

## Notes

- Most churches have recurring order of service - templates are critical
- Live mode prevents mistakes during services
- PDF export is essential for printed bulletins
- Drag-and-drop makes reordering intuitive
