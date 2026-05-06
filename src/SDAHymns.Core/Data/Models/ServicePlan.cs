namespace SDAHymns.Core.Data.Models;

public class ServicePlan
{
    public int Id { get; set; }

    public required string Name { get; set; }  // "Sabbath Morning - Dec 3"
    public string? Description { get; set; }
    public DateTime? ServiceDate { get; set; }

    // Navigation
    public ICollection<ServicePlanItem> Items { get; set; } = new List<ServicePlanItem>();

    // Status
    public bool IsActive { get; set; } = false;  // Currently active plan

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
