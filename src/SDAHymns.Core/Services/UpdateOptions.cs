namespace SDAHymns.Core.Services;

/// <summary>
/// Configuration options for the update service
/// </summary>
public class UpdateOptions
{
    /// <summary>
    /// GitHub repository URL for checking updates (e.g., "https://github.com/owner/repo")
    /// </summary>
    public string GitHubRepoUrl { get; set; } = "https://github.com/ThorSPB/SDAHymns";
}
