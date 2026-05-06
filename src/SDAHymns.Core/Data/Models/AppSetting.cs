namespace SDAHymns.Core.Data.Models;

public class AppSetting
{
    public int Id { get; set; }

    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
