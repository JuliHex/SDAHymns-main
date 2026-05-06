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
