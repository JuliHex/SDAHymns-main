using Microsoft.EntityFrameworkCore.Design;

namespace SDAHymns.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HymnsContext>
{
    public HymnsContext CreateDbContext(string[] args)
    {
        // Find solution root by looking for .sln file
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var solutionRoot = currentDir;

        while (solutionRoot != null && !solutionRoot.GetFiles("*.sln").Any())
        {
            solutionRoot = solutionRoot.Parent;
        }

        if (solutionRoot == null)
        {
            throw new InvalidOperationException("Could not find solution root (no .sln file found in parent directories)");
        }

        var dbPath = Path.Combine(solutionRoot.FullName, "Resources", "hymns.db");

        var optionsBuilder = new DbContextOptionsBuilder<HymnsContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new HymnsContext(optionsBuilder.Options);
    }
}
