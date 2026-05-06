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
