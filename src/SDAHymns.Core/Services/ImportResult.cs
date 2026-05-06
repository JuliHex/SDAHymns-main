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
