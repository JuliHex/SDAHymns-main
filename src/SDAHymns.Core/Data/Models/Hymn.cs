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

    // Usage tracking
    public DateTime? LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
    public bool IsFavorite { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
