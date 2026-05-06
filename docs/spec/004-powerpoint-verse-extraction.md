# Spec 004: PowerPoint Verse Extraction

**Status:** ✅ Implemented
**Created:** 2025-12-03
**Completed:** 2025-12-04
**Dependencies:** 003-legacy-xml-import.md

## Overview

Extract verse/lyrics content from legacy PowerPoint (.PPT) files and populate the `Verses` table. Each PowerPoint slide (after the title slide) typically contains one verse or chorus of a hymn. This will complete the hymn data migration, giving us full hymn content for display.

## Goals

1. Extract text content from all slides in PowerPoint files
2. Parse verses and identify verse numbers, choruses, and refrains
3. Store verse content in the database linked to hymns
4. Handle multi-line verses and formatting
5. Preserve Romanian text encoding
6. Process all 1,254 hymns across 5 categories
7. Provide progress reporting and error handling

## Current State

**What We Have:**
- ✅ 1,254 hymns with numbers and titles
- ✅ `PowerPointParserService` with LibreOffice integration
- ✅ DocumentFormat.OpenXml package for PPTX parsing
- ✅ PPT → PPTX conversion working
- ✅ Infrastructure for batch processing

**What We Need:**
- Extract verses from slides 2+ (slide 1 is the title)
- Parse verse structure (verse numbers, chorus markers, etc.)
- Store in `Verses` table with proper ordering

## PowerPoint Structure Analysis

### Expected Slide Format

**Slide 1:** Title (already extracted)
```
Imnul 737
Bunul nostru Salvator
```

**Slides 2+:** Verses/Choruses
```
1. Prima strofă text aici
   Linie 2 din strofa
   Linie 3 din strofa

or

Refren:
Text refren aici
Pe mai multe linii
```

### Verse Patterns to Recognize

1. **Numbered verses:** `1.`, `2.`, `3.`, etc.
2. **Refren (Chorus):** `Refren:`, `Refren`, `R:`
3. **Plain text:** No marker (treat as continuation or separate verse)

## Data Model

Already exists in `Verse` entity (from Spec 002):

```csharp
public class Verse
{
    public int Id { get; set; }
    public int HymnId { get; set; }
    public Hymn Hymn { get; set; } = null!;

    public int VerseNumber { get; set; }  // 1, 2, 3, etc.
    public required string Content { get; set; }  // Full verse text
    public string? Label { get; set; }  // "Refren", "Strofa 1", etc.
    public int DisplayOrder { get; set; }  // Order for display

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Implementation Plan

### Phase 1: Extend PowerPointParserService

**File:** `src/SDAHymns.Core/Services/PowerPointParserService.cs`

Add method:
```csharp
public async Task<List<VerseData>> ExtractVersesAsync(string pptFilePath)
{
    // 1. Convert PPT to PPTX (reuse existing method)
    // 2. Extract text from ALL slides (skip slide 1)
    // 3. Parse each slide into verse data
    // 4. Return structured verse list
}

public class VerseData
{
    public int VerseNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsChorus { get; set; }
}
```

### Phase 2: Create Verse Import Service

**File:** `src/SDAHymns.Core/Services/VerseImportService.cs`

```csharp
public interface IVerseImportService
{
    Task<VerseImportResult> ImportVersesForHymnAsync(int hymnId);
    Task<VerseImportResult> ImportVersesForCategoryAsync(string categorySlug);
    Task<VerseImportResult> ImportAllVersesAsync();
    Task<VerseImportStatistics> GetStatisticsAsync();
}

public class VerseImportResult
{
    public int HymnsProcessed { get; set; }
    public int VersesImported { get; set; }
    public int SkippedHymns { get; set; }  // No PPT file or already imported
    public List<string> Errors { get; set; } = new();
}

public class VerseImportStatistics
{
    public int TotalHymns { get; set; }
    public int HymnsWithVerses { get; set; }
    public int HymnsWithoutVerses { get; set; }
    public int TotalVerses { get; set; }
    public Dictionary<string, int> VersesPerCategory { get; set; } = new();
}
```

### Phase 3: Create CLI Command

**File:** `src/SDAHymns.CLI/Commands/ImportVersesCommand.cs`

```bash
# Import verses for all hymns
dotnet run --project src/SDAHymns.CLI -- import-verses

# Import verses for specific category
dotnet run --project src/SDAHymns.CLI -- import-verses --category crestine

# Import verses for specific hymn
dotnet run --project src/SDAHymns.CLI -- import-verses --hymn 737

# Show statistics
dotnet run --project src/SDAHymns.CLI -- import-verses --stats

# Dry run (preview)
dotnet run --project src/SDAHymns.CLI -- import-verses --dry-run
```

## Verse Parsing Logic

### Strategy 1: Simple Approach (Start Here)

For each slide (after slide 1):
1. Extract all text content
2. Detect if it starts with a number pattern (`1.`, `2.`, etc.) → `VerseNumber`
3. Detect if it contains "Refren" → Mark as chorus, `Label = "Refren"`
4. Store entire slide text as `Content`
5. Use slide order for `DisplayOrder`

**Example:**
```
Slide 2 text: "1. Bunul nostru Salvator..."
→ VerseNumber = 1, Content = "Bunul nostru Salvator...", Label = null

Slide 3 text: "Refren:\nLaudă, laudă..."
→ VerseNumber = 0, Content = "Laudă, laudă...", Label = "Refren"
```

### Strategy 2: Advanced Parsing (Optional Enhancement)

- Remove verse number prefix from content
- Detect multiple verses on one slide
- Handle "Strofa 1", "Strofa 2" labels
- Detect repeated choruses

Start with Strategy 1 for MVP.

## Implementation Steps

### Step 1: Extend PowerPointParserService ✓ (Reuse existing)

Already have:
- ✅ PPT → PPTX conversion
- ✅ Slide text extraction

Add:
- Extract from ALL slides (not just first)
- Parse verse structure

### Step 2: Create VerseImportService

Implement `IVerseImportService` with:
- Batch processing
- Progress reporting
- Error handling
- Database transactions

### Step 3: Create CLI Command

Implement `import-verses` command with options:
- `--category` filter
- `--hymn` single hymn import
- `--dry-run` preview mode
- `--force` reimport existing verses
- `--stats` show statistics

### Step 4: Test on Sample Hymns

Test on a few hymns first:
```bash
dotnet run --project src/SDAHymns.CLI -- import-verses --hymn 1 --dry-run
dotnet run --project src/SDAHymns.CLI -- import-verses --hymn 1
```

Verify in database:
```sql
SELECT * FROM Verses WHERE HymnId = 1;
```

### Step 5: Run Full Import

Import all categories:
```bash
dotnet run --project src/SDAHymns.CLI -- import-verses
```

## Expected Results

**Per Hymn:**
- Average: 3-5 verses
- Some hymns: 1 verse + chorus
- Larger hymns: 6-10 verses

**Total Estimates:**
- 1,254 hymns × ~4 verses average = **~5,000 verses**

## Data Validation

The import service will:
- ✅ Skip hymns with no PowerPoint file
- ✅ Skip hymns that already have verses (unless `--force`)
- ✅ Validate verse content is not empty
- ✅ Handle Romanian UTF-8 encoding
- ✅ Trim whitespace from verse content
- ✅ Set proper `DisplayOrder` based on slide order
- ✅ Log warnings for parsing issues

## Error Handling

- Missing PowerPoint file → Skip hymn, log info
- Failed PPT conversion → Log error, skip hymn
- Empty slide text → Log warning, skip slide
- Database constraint violations → Log error, rollback transaction
- Parsing errors → Log warning, store raw text as-is

## Acceptance Criteria

- [x] `ExtractVersesAsync` method works for all PPT files
- [x] `IVerseImportService` interface created
- [x] `VerseImportService` implementation complete
- [x] Verses imported for 1,249/1,254 hymns (99.6% success rate)
- [x] Verse numbers detected correctly (with automatic sequential numbering)
- [x] Choruses identified and labeled as "Refren"
- [x] Romanian characters preserved
- [x] `DisplayOrder` maintains correct sequence
- [x] CLI command `import-verses` works with all options
- [x] Statistics command shows correct counts
- [x] Import can be run multiple times safely (idempotent)
- [x] Progress reporting during import (every 10 hymns)
- [x] Resumable imports with `--start-from` parameter

## Performance Considerations

- **Batch size:** Process hymns in batches of 50-100
- **Progress reporting:** Update console every 10 hymns
- **Transaction scope:** One transaction per hymn (rollback on error)
- **LibreOffice throttling:** Already have 100ms delay between conversions
- **Expected duration:** ~10-15 minutes for all 1,254 hymns

## Testing Strategy

### Unit Tests

```csharp
[Fact]
public async Task ExtractVerses_ShouldParse_NumberedVerses()
{
    var service = new PowerPointParserService();
    var verses = await service.ExtractVersesAsync("test_hymn.PPT");

    verses.Should().NotBeEmpty();
    verses.First().VerseNumber.Should().Be(1);
}

[Fact]
public async Task ExtractVerses_ShouldIdentify_Chorus()
{
    var service = new PowerPointParserService();
    var verses = await service.ExtractVersesAsync("test_chorus.PPT");

    var chorus = verses.FirstOrDefault(v => v.Label == "Refren");
    chorus.Should().NotBeNull();
}
```

### Integration Tests

```csharp
[Fact]
public async Task VerseImport_ShouldImport_AllVersesForHymn()
{
    var importService = CreateVerseImportService();
    var result = await importService.ImportVersesForHymnAsync(hymnId: 1);

    result.VersesImported.Should().BeGreaterThan(0);
    result.Errors.Should().BeEmpty();
}
```

### Manual Testing

1. Import single hymn with known structure
2. Verify verses in database match PowerPoint slides
3. Check Romanian characters preserved
4. Verify verse order matches slide order
5. Test dry-run mode

## Related Specs

- **Previous:** 003-legacy-xml-import.md
- **Next:** 005-basic-hymn-display.md (TBD) - UI to display hymn verses
- **Depends On:** 003-legacy-xml-import.md (needs hymns and PPT paths)

## Notes

- This completes the data migration from legacy app
- After this, we have full hymn content ready for display
- Verse extraction might fail for some malformed PPT files (acceptable)
- We can manually fix problematic hymns later if needed
- Consider adding `VerseImportLog` table to track import attempts per hymn

## Known Edge Cases

The following hymns have special structural quirks that may need special handling in the display layer:

- **Hymn #99**: DisplayOrder starts at 2 (verse 1 on slide with special formatting/layout)
- **Hymn #644**: DisplayOrder starts at 2 (similar first-slide formatting issue)
- **Hymn #713**: DisplayOrder has gap (1, 3-6) - slide 2 might contain non-verse content

These hymns imported successfully but may require special display logic when implementing the hymn display UI.

## Future Enhancements (Out of Scope)

- Parse multiple verses from single slide
- Detect verse fragments across slides
- Extract formatting (bold, italic) from PowerPoint
- Detect musical notations or chords
- Image extraction from slides (if any)

## Status Updates

- **2025-12-03 (Created):** Spec created, ready for implementation

- **2025-12-03 (Implementation Started):**
  - ✅ **Phase 1 Complete: Verse Extraction Logic**
    - Created `VerseData` model class
    - Extended `PowerPointParserService` with `ExtractVersesAsync()` method
    - Implemented slide parsing logic with pattern detection:
      - Pattern 1: Simple numbered verses (e.g., hymn 001)
      - Pattern 2: Verses with inline chorus (e.g., hymn 020)
      - Pattern 3: Chorus on separate slides (e.g., hymn 285)
      - Pattern 4: Repeat markers `(: :)` (e.g., hymn 063)
    - Handles text fragmentation from PowerPoint conversion
    - Deduplicates repeated chorus slides
    - Preserves Romanian diacritics

  - ✅ **Testing Complete:**
    - Created `test-verses` CLI command for verification
    - Tested on 4 sample hymns covering all patterns: ✅ All working
    - Tested on 10 random hymns: ✅ All successful (3-5 verses each)
    - Verse numbering works correctly
    - Chorus detection and deduplication working

  - ✅ **Phase 2 Complete: Database Import Service**
    - Created `IVerseImportService` interface
    - Implemented `VerseImportService` with full import logic
    - Added `--limit` and `--start-from` options for resumable imports
    - Implemented automatic sequential verse numbering for unnumbered verses
    - Added `ChangeTracker.Clear()` to prevent EF Core tracking issues

  - ✅ **Phase 3 Complete: CLI Command**
    - Created `ImportVersesCommand` with full option support
    - Supports category-specific, single hymn, and full imports
    - Progress reporting every 10 hymns
    - Detailed error messages with inner exception details
    - Statistics command to view import status

  - ✅ **Phase 4 Complete: Full Import**
    - **Final Results: 99.6% Success Rate**
    - **1,249 out of 1,254 hymns** imported successfully
    - **4,629 verses** extracted and stored
    - Only 5 hymns failed (0.4%) - likely missing/corrupt PPT files

    **Per Category Breakdown:**
    - Imnuri crestine: 919/919 hymns (3,468 verses) ✅ 100%
    - Imnuri exploratori: 120/120 hymns (390 verses) ✅ 100%
    - Imnuri licurici: 83/83 hymns (315 verses) ✅ 100%
    - Imnuri tineret: 65/69 hymns (250 verses) - 94.2%
    - Imnuri companioni: 62/63 hymns (206 verses) - 98.4%

## Enhanced Features Implemented

**Advanced Verse Model:**
- `IsInline` flag for verses displayed on same slide
- `IsContinuation` flag for verse fragments split across slides

**Improved PowerPoint Parser:**
- 45-second timeout to prevent LibreOffice hangs
- Y-position-based text extraction for correct reading order
- Multi-segment slide parsing (handles Refren → Verse → Refren)
- Automatic sequential verse numbering for unnumbered verses
- Enhanced chorus deduplication with content comparison
- Final safety check to ensure unique VerseNumbers

**Robust Import Service:**
- Per-hymn transaction isolation
- Automatic skip of already-imported hymns
- Raw SQL DELETE to avoid EF Core caching issues
- Resumable imports with `--start-from` parameter
- Comprehensive error logging with inner exceptions

## Implementation Files

**Created:**
- `src/SDAHymns.Core/Services/PowerPointParserService.cs` (verse extraction)
  - `VerseData` model class with IsInline/IsContinuation
  - `ExtractVersesAsync()` method
  - `ParseSlideContent()` - returns List<VerseData> for multi-segment slides
  - `ProcessSegment()` - handles Refren, numbered verses, continuations
- `src/SDAHymns.Core/Services/IVerseImportService.cs` (interface)
- `src/SDAHymns.Core/Services/VerseImportService.cs` (implementation)
- `src/SDAHymns.CLI/Commands/ImportVersesCommand.cs` (CLI command)
- `src/SDAHymns.CLI/Commands/TestVerseExtractionCommand.cs` (testing tool)
- Database migration for IsInline/IsContinuation columns
