using CommandLine;
using SDAHymns.Core.Services;

namespace SDAHymns.CLI.Commands;

[Verb("import-verses", HelpText = "Import verses from PowerPoint files into the database")]
public class ImportVersesCommand
{
    [Option('c', "category", Required = false, HelpText = "Import verses for specific category (crestine, companioni, etc.)")]
    public string? Category { get; set; }

    [Option('h', "hymn", Required = false, HelpText = "Import verses for a specific hymn ID")]
    public int? HymnId { get; set; }

    [Option('f', "force", Required = false, Default = false, HelpText = "Force reimport even if verses already exist")]
    public bool Force { get; set; }

    [Option('s', "stats", Required = false, Default = false, HelpText = "Show statistics only (don't import)")]
    public bool ShowStats { get; set; }

    [Option('d', "dry-run", Required = false, Default = false, HelpText = "Preview what would be imported without making changes")]
    public bool DryRun { get; set; }

    [Option('l', "limit", Required = false, HelpText = "Limit the number of hymns to process (useful for testing)")]
    public int? Limit { get; set; }

    [Option("start-from", Required = false, HelpText = "Start import from a specific hymn number (skip previous)")]
    public int? StartFrom { get; set; }

    public static async Task<int> ExecuteAsync(ImportVersesCommand options, IVerseImportService importService)
    {
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("📖 Verse Import");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine();

        try
        {
            // Show statistics only
            if (options.ShowStats)
            {
                await ShowStatisticsAsync(importService);
                return 0;
            }

            // Dry run mode
            if (options.DryRun)
            {
                Console.WriteLine("🔍 DRY RUN MODE - No changes will be made");
                Console.WriteLine();
                // For now, just show stats in dry run
                await ShowStatisticsAsync(importService);
                return 0;
            }

            // Execute import
            VerseImportResult result;

            if (options.HymnId.HasValue)
            {
                // Import single hymn
                Console.WriteLine($"Importing verses for Hymn ID: {options.HymnId.Value}");
                Console.WriteLine();
                result = await importService.ImportVersesForHymnAsync(options.HymnId.Value, options.Force);
            }
            else if (!string.IsNullOrEmpty(options.Category))
            {
                // Import category
                if (options.Limit.HasValue)
                {
                    Console.WriteLine($"⚠️  Limit: Processing only first {options.Limit.Value} hymns");
                    Console.WriteLine();
                }
                result = await importService.ImportVersesForCategoryAsync(options.Category, options.Force, options.Limit, options.StartFrom);
            }
            else
            {
                // Import all
                if (options.Limit.HasValue)
                {
                    Console.WriteLine($"⚠️  Limit: Processing only first {options.Limit.Value} hymns per category");
                    Console.WriteLine();
                }
                result = await importService.ImportAllVersesAsync(options.Force, options.Limit, options.StartFrom);
            }

            // Show results
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("📊 Import Summary");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine($"✅ Hymns Processed: {result.HymnsProcessed}");
            Console.WriteLine($"📝 Verses Imported: {result.VersesImported}");
            Console.WriteLine($"⏭️  Hymns Skipped: {result.SkippedHymns}");

            if (result.Warnings.Any())
            {
                Console.WriteLine($"⚠️  Warnings: {result.Warnings.Count}");
                if (result.Warnings.Count <= 10)
                {
                    foreach (var warning in result.Warnings)
                    {
                        Console.WriteLine($"   - {warning}");
                    }
                }
            }

            if (result.Errors.Any())
            {
                Console.WriteLine($"❌ Errors: {result.Errors.Count}");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error}");
                }
            }

            Console.WriteLine();

            // Show updated statistics
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("📊 Current Database Statistics");
            Console.WriteLine("═══════════════════════════════════════════════════");
            await ShowStatisticsAsync(importService);

            return result.Errors.Any() ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
            return 1;
        }
    }

    private static async Task ShowStatisticsAsync(IVerseImportService importService)
    {
        var stats = await importService.GetStatisticsAsync();

        Console.WriteLine($"Total Hymns: {stats.TotalHymns}");
        Console.WriteLine($"Hymns with Verses: {stats.HymnsWithVerses} ({GetPercentage(stats.HymnsWithVerses, stats.TotalHymns)}%)");
        Console.WriteLine($"Hymns without Verses: {stats.HymnsWithoutVerses} ({GetPercentage(stats.HymnsWithoutVerses, stats.TotalHymns)}%)");
        Console.WriteLine($"Total Verses: {stats.TotalVerses}");
        Console.WriteLine();

        if (stats.VersesPerCategory.Any())
        {
            Console.WriteLine("Verses per Category:");
            foreach (var (category, count) in stats.VersesPerCategory.OrderBy(kvp => kvp.Key))
            {
                var hymnsWithVerses = stats.HymnsWithVersesPerCategory.GetValueOrDefault(category, 0);
                Console.WriteLine($"  {category}: {count} verses ({hymnsWithVerses} hymns)");
            }
        }

        Console.WriteLine();
    }

    private static double GetPercentage(int part, int total)
    {
        if (total == 0)
            return 0;
        return Math.Round((double)part / total * 100, 1);
    }
}
