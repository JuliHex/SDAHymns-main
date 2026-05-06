namespace SDAHymns.Core.Data.Models;

public class HymnCategory
{
    public int Id { get; set; }

    public required string Name { get; set; }  // "Imnuri crestine", etc.
    public required string Slug { get; set; }  // "crestine", "companioni"
    public string? Description { get; set; }

    // Display
    public int DisplayOrder { get; set; }
    public string? IconPath { get; set; }

    // Legacy reference
    public string? LegacyFolderPath { get; set; }

    // Navigation
    public ICollection<Hymn> Hymns { get; set; } = new List<Hymn>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
