# Spec 003: Legacy XML Import

**Status:** ✅ Implemented
**Created:** 2025-12-03
**Implemented:** 2025-12-03
**Dependencies:** 002-data-layer.md

## Overview

Automatically parse and import hymn data from the legacy application's XML index files. This will populate the database with all hymn metadata (numbers and titles) from the five hymn categories. PowerPoint content parsing will be handled in a separate spec.

## Goals

1. Parse XML index files from all 5 hymn categories
2. Import hymn metadata (number, title) into database
3. Link hymns to correct categories
4. Store PowerPoint file path references
5. Handle Romanian text encoding properly
6. Validate data integrity (no duplicates, proper formatting)
7. Provide import statistics and error reporting

## Data Source

**Location:** `Imnuri Azs/Resurse/`

**5 Categories:**
```
Imnuri Azs/Resurse/
├── Imnuri companioni/
│   └── index.xml
├── Imnuri crestine/
│   └── index.xml
├── Imnuri exploratori/
│   └── index.xml
├── Imnuri licurici/
│   └── index.xml
└── Imnuri tineret/
    └── index.xml
```

**XML Format:**
```xml
<Imnuri>
  <Imn>
    <Numar>1</Numar>
    <Titlu> Iubirea Ta nemăsurată </Titlu>
  </Imn>
  <Imn>
    <Numar>2</Numar>
    <Titlu> Tatăl nostru </Titlu>
  </Imn>
  <!-- ... -->
</Imnuri>
```

## Implementation

### 1. Create XML Parser Service

**File:** `src/SDAHymns.Core/Services/LegacyXmlImportService.cs`

```csharp
namespace SDAHymns.Core.Services;

public interface ILegacyXmlImportService
{
    Task<ImportResult> ImportAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ImportResult> ImportCategoryAsync(string categorySlug, CancellationToken cancellationToken = default);
    Task<ImportStatistics> GetImportStatisticsAsync();
}

public class LegacyXmlImportService : ILegacyXmlImportService
{
    private readonly HymnsContext _context;
    private readonly ILogger<LegacyXmlImportService> _logger;

    public LegacyXmlImportService(HymnsContext context, ILogger<LegacyXmlImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportResult> ImportAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = new ImportResult();
        var categories = await _context.HymnCategories.OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            try
            {
                var categoryResult = await ImportCategoryAsync(category.Slug, cancellationToken);
                result.Merge(categoryResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import category {CategorySlug}", category.Slug);
                result.Errors.Add($"Category {category.Slug}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<ImportResult> ImportCategoryAsync(string categorySlug, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult { CategorySlug = categorySlug };

        // Get category from database
        var category = await _context.HymnCategories
            .FirstOrDefaultAsync(c => c.Slug == categorySlug, cancellationToken);

        if (category == null)
        {
            result.Errors.Add($"Category '{categorySlug}' not found in database");
            return result;
        }

        // Build path to XML file
        var solutionRoot = GetSolutionRoot();
        var xmlPath = Path.Combine(solutionRoot, category.LegacyFolderPath!, "index.xml");

        if (!File.Exists(xmlPath))
        {
            result.Errors.Add($"XML file not found: {xmlPath}");
            return result;
        }

        // Parse XML
        var hymns = ParseXmlFile(xmlPath, category.Id);
        result.ParsedCount = hymns.Count;

        // Check for existing hymns in this category
        var existingNumbers = await _context.Hymns
            .Where(h => h.CategoryId == category.Id)
            .Select(h => h.Number)
            .ToListAsync(cancellationToken);

        // Filter out duplicates
        var newHymns = hymns.Where(h => !existingNumbers.Contains(h.Number)).ToList();
        result.SkippedCount = hymns.Count - newHymns.Count;

        // Batch insert
        if (newHymns.Any())
        {
            await _context.Hymns.AddRangeAsync(newHymns, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            result.ImportedCount = newHymns.Count;
        }

        _logger.LogInformation(
            "Imported {ImportedCount} hymns from category {CategorySlug} ({SkippedCount} skipped)",
            result.ImportedCount, categorySlug, result.SkippedCount);

        return result;
    }

    private List<Hymn> ParseXmlFile(string xmlPath, int categoryId)
    {
        var hymns = new List<Hymn>();
        var pptDirectory = Path.Combine(Path.GetDirectoryName(xmlPath)!, "ppt");

        var doc = XDocument.Load(xmlPath);
        var imnuriElements = doc.Root?.Elements("Imn") ?? Enumerable.Empty<XElement>();

        foreach (var imn in imnuriElements)
        {
            var numarStr = imn.Element("Numar")?.Value?.Trim();
            var titlu = imn.Element("Titlu")?.Value?.Trim();

            if (string.IsNullOrEmpty(numarStr) || string.IsNullOrEmpty(titlu))
            {
                continue; // Skip invalid entries
            }

            if (!int.TryParse(numarStr, out var numar))
            {
                _logger.LogWarning("Invalid hymn number: {Number} in {XmlPath}", numarStr, xmlPath);
                continue;
            }

            // Build PowerPoint file path (try both .PPT and .ppt)
            var pptFileName = $"{numar:D3}.PPT";
            var pptPath = Path.Combine(pptDirectory, pptFileName);
            if (!File.Exists(pptPath))
            {
                pptFileName = $"{numar:D3}.ppt";
                pptPath = Path.Combine(pptDirectory, pptFileName);
            }

            var legacyPath = File.Exists(pptPath)
                ? Path.GetRelativePath(GetSolutionRoot(), pptPath)
                : null;

            var hymn = new Hymn
            {
                Number = numar,
                Title = titlu,
                CategoryId = categoryId,
                LegacyPowerPointPath = legacyPath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            hymns.Add(hymn);
        }

        return hymns;
    }

    private string GetSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir, "SDAHymns.sln")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        throw new InvalidOperationException("Could not find solution root directory");
    }

    public async Task<ImportStatistics> GetImportStatisticsAsync()
    {
        var stats = new ImportStatistics();

        var categories = await _context.HymnCategories
            .Select(c => new
            {
                c.Slug,
                c.Name,
                HymnCount = c.Hymns.Count
            })
            .ToListAsync();

        stats.TotalHymns = categories.Sum(c => c.HymnCount);
        stats.CategoryCounts = categories.ToDictionary(c => c.Slug, c => c.HymnCount);

        var lastImportSetting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "LastDatabaseImportDate");

        if (lastImportSetting != null && !string.IsNullOrEmpty(lastImportSetting.Value))
        {
            stats.LastImportDate = DateTime.Parse(lastImportSetting.Value);
        }

        return stats;
    }
}
```

### 2. Result Models

**File:** `src/SDAHymns.Core/Services/ImportResult.cs`

```csharp
namespace SDAHymns.Core.Services;

public class ImportResult
{
    public string? CategorySlug { get; set; }
    public int ParsedCount { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();

    public bool IsSuccess => !Errors.Any();

    public void Merge(ImportResult other)
    {
        ParsedCount += other.ParsedCount;
        ImportedCount += other.ImportedCount;
        SkippedCount += other.SkippedCount;
        Errors.AddRange(other.Errors);
    }
}

public class ImportStatistics
{
    public int TotalHymns { get; set; }
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public DateTime? LastImportDate { get; set; }
}
```

### 3. CLI Command for Import

**File:** `src/SDAHymns.CLI/Commands/ImportCommand.cs`

```csharp
using CommandLine;

namespace SDAHymns.CLI.Commands;

[Verb("import", HelpText = "Import hymns from legacy XML files")]
public class ImportOptions
{
    [Option('c', "category", Required = false, HelpText = "Category slug to import (or all if not specified)")]
    public string? Category { get; set; }

    [Option('f', "force", Required = false, Default = false, HelpText = "Force reimport (skip duplicate check)")]
    public bool Force { get; set; }

    [Option('s', "stats", Required = false, Default = false, HelpText = "Show import statistics only")]
    public bool ShowStats { get; set; }
}

public class ImportCommandHandler
{
    private readonly ILegacyXmlImportService _importService;

    public ImportCommandHandler(ILegacyXmlImportService importService)
    {
        _importService = importService;
    }

    public async Task<int> ExecuteAsync(ImportOptions options)
    {
        if (options.ShowStats)
        {
            return await ShowStatisticsAsync();
        }

        Console.WriteLine("Starting import from legacy XML files...");
        Console.WriteLine();

        ImportResult result;

        if (!string.IsNullOrEmpty(options.Category))
        {
            Console.WriteLine($"Importing category: {options.Category}");
            result = await _importService.ImportCategoryAsync(options.Category);
        }
        else
        {
            Console.WriteLine("Importing all categories...");
            result = await _importService.ImportAllCategoriesAsync();
        }

        PrintResults(result);

        // Update last import date
        // ... (implementation)

        return result.IsSuccess ? 0 : 1;
    }

    private async Task<int> ShowStatisticsAsync()
    {
        var stats = await _importService.GetImportStatisticsAsync();

        Console.WriteLine("Import Statistics");
        Console.WriteLine("=================");
        Console.WriteLine($"Total Hymns: {stats.TotalHymns}");
        Console.WriteLine();
        Console.WriteLine("By Category:");
        foreach (var (slug, count) in stats.CategoryCounts)
        {
            Console.WriteLine($"  {slug}: {count} hymns");
        }

        if (stats.LastImportDate.HasValue)
        {
            Console.WriteLine();
            Console.WriteLine($"Last Import: {stats.LastImportDate.Value:yyyy-MM-dd HH:mm:ss} UTC");
        }

        return 0;
    }

    private void PrintResults(ImportResult result)
    {
        Console.WriteLine();
        Console.WriteLine("Import Results");
        Console.WriteLine("==============");
        Console.WriteLine($"Parsed:   {result.ParsedCount} hymns");
        Console.WriteLine($"Imported: {result.ImportedCount} hymns");
        Console.WriteLine($"Skipped:  {result.SkippedCount} hymns (already exist)");

        if (result.Errors.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("✓ Import completed successfully!");
        }
    }
}
```

## Implementation Steps

### Step 1: Create Service Interface and Implementation

Create `ILegacyXmlImportService` and `LegacyXmlImportService` in `src/SDAHymns.Core/Services/`

### Step 2: Create Result Models

Create `ImportResult` and `ImportStatistics` classes

### Step 3: Add System.Xml.Linq Usage

Already included in .NET, but add to GlobalUsings if needed:
```csharp
global using System.Xml.Linq;
```

### Step 4: Add Logging Support

Add `Microsoft.Extensions.Logging.Abstractions` package to Core project:
```bash
dotnet add src/SDAHymns.Core package Microsoft.Extensions.Logging.Abstractions
```

### Step 5: Create CLI Command

Implement `ImportCommand` in CLI project

### Step 6: Register Services

In Desktop/CLI startup, register the import service:
```csharp
services.AddScoped<ILegacyXmlImportService, LegacyXmlImportService>();
```

### Step 7: Run Import

```bash
# Import all categories
dotnet run --project src/SDAHymns.CLI -- import

# Import specific category
dotnet run --project src/SDAHymns.CLI -- import --category crestine

# Show statistics
dotnet run --project src/SDAHymns.CLI -- import --stats
```

## Expected Results

Based on visual inspection of the legacy app:

**Estimated Hymn Counts:**
- Imnuri crestine: ~500-700 hymns (main hymnbook)
- Imnuri companioni: ~60-100 hymns
- Imnuri exploratori: ~50-100 hymns
- Imnuri licurici: ~50-100 hymns
- Imnuri tineret: ~50-100 hymns

**Total: ~800-1200 hymns**

## Data Validation

The import service will:
- ✅ Skip entries with invalid numbers
- ✅ Skip entries with missing titles
- ✅ Trim whitespace from titles
- ✅ Handle Romanian UTF-8 encoding
- ✅ Check for duplicate (Number, CategoryId) pairs
- ✅ Log warnings for missing PowerPoint files
- ✅ Store relative paths to PowerPoint files

## Error Handling

- Missing XML files → Log error, skip category
- Invalid XML format → Log error, skip file
- Invalid hymn numbers → Log warning, skip entry
- Missing titles → Log warning, skip entry
- Duplicate hymns → Skip, log info

## Acceptance Criteria

- [ ] `ILegacyXmlImportService` interface created
- [ ] `LegacyXmlImportService` implementation complete
- [ ] Import all 5 categories successfully
- [ ] All hymn numbers and titles imported correctly
- [ ] Romanian characters (ă, â, î, ș, ț) preserved
- [ ] PowerPoint file paths stored (where files exist)
- [ ] Duplicate detection works correctly
- [ ] CLI command `import` works
- [ ] Statistics command shows correct counts
- [ ] Import can be run multiple times safely (idempotent)
- [ ] Logging provides useful information
- [ ] Error handling is robust

## Testing Verification

```csharp
[Fact]
public async Task ImportService_ShouldImport_AllCategories()
{
    // Arrange
    var service = CreateImportService();

    // Act
    var result = await service.ImportAllCategoriesAsync();

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.ImportedCount.Should().BeGreaterThan(800);
    result.Errors.Should().BeEmpty();
}

[Fact]
public async Task ImportService_ShouldSkip_DuplicateHymns()
{
    // Arrange
    var service = CreateImportService();
    await service.ImportAllCategoriesAsync(); // First import

    // Act
    var result = await service.ImportAllCategoriesAsync(); // Second import

    // Assert
    result.ImportedCount.Should().Be(0);
    result.SkippedCount.Should().BeGreaterThan(0);
}

[Fact]
public async Task ImportService_ShouldPreserve_RomanianCharacters()
{
    // Arrange
    var service = CreateImportService();
    await service.ImportCategoryAsync("crestine");

    // Act
    var hymn = await _context.Hymns
        .FirstOrDefaultAsync(h => h.Title.Contains("ă") || h.Title.Contains("ș"));

    // Assert
    hymn.Should().NotBeNull();
    hymn!.Title.Should().ContainAny("ă", "â", "î", "ș", "ț");
}
```

## Notes

- This spec only imports **metadata** (number, title)
- **Verse content** parsing from PowerPoint will be in a future spec (004)
- PowerPoint parsing is complex (COM interop or library required)
- For now, we store the PowerPoint file path for manual/future extraction
- Import is idempotent - can be run multiple times safely
- Database validates unique (Number, CategoryId) constraint

## Performance Considerations

- Batch insert for better performance (AddRangeAsync)
- Use transactions for consistency
- ~1000 hymns should import in < 5 seconds
- Could add progress reporting for large imports

## Related Specs

- **Previous:** 002-data-layer.md
- **Next:** 004-powerpoint-verse-extraction.md (TBD) - Extract verse content from PowerPoint files
- **Depends On:** 002-data-layer.md

## Status Updates

- **2025-12-03 (Morning):** Spec created and implemented
  - ✅ XML parsing complete: 1,070 hymns imported from all 5 categories
  - ✅ All hymn numbers and titles imported correctly
  - ✅ Romanian characters preserved
  - ✅ CLI commands working (`import`, `import --stats`)

- **2025-12-03 (Afternoon):** Orphan PPT Import Enhancement
  - ✅ Discovered 184 PowerPoint files not in XML index
  - ✅ Installed LibreOffice for PPT→PPTX conversion
  - ✅ Added DocumentFormat.OpenXml package (v3.3.0)
  - ✅ Created `PowerPointParserService` with:
    - PPT to PPTX conversion using LibreOffice
    - Title extraction from first slide using OpenXML
    - Batch processing with progress reporting
  - ✅ Created `import-orphan-ppt` CLI command with dry-run support
  - ✅ Successfully imported all 184 orphan hymns with extracted titles
  - **Final Total: 1,254 hymns** (1,070 from XML + 184 from orphan PPTs)
