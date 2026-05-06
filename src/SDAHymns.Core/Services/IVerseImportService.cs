namespace SDAHymns.Core.Services;

/// <summary>
/// Service for importing verses from PowerPoint files into the database
/// </summary>
public interface IVerseImportService
{
    /// <summary>
    /// Imports verses for a specific hymn by ID
    /// </summary>
    Task<VerseImportResult> ImportVersesForHymnAsync(int hymnId, bool force = false);

    /// <summary>
    /// Imports verses for all hymns in a category
    /// </summary>
    Task<VerseImportResult> ImportVersesForCategoryAsync(string categorySlug, bool force = false, int? limit = null, int? startFromNumber = null);

    /// <summary>
    /// Imports verses for all hymns in the database
    /// </summary>
    Task<VerseImportResult> ImportAllVersesAsync(bool force = false, int? limit = null, int? startFromNumber = null);

    /// <summary>
    /// Gets statistics about verse import status
    /// </summary>
    Task<VerseImportStatistics> GetStatisticsAsync();
}

/// <summary>
/// Result of a verse import operation
/// </summary>
public class VerseImportResult
{
    public int HymnsProcessed { get; set; }
    public int VersesImported { get; set; }
    public int SkippedHymns { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Statistics about verse imports
/// </summary>
public class VerseImportStatistics
{
    public int TotalHymns { get; set; }
    public int HymnsWithVerses { get; set; }
    public int HymnsWithoutVerses { get; set; }
    public int TotalVerses { get; set; }
    public Dictionary<string, int> VersesPerCategory { get; set; } = new();
    public Dictionary<string, int> HymnsWithVersesPerCategory { get; set; } = new();
}
