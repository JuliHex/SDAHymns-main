using CommandLine;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Services;

namespace SDAHymns.CLI.Commands;

[Verb("import", HelpText = "Import hymns from legacy XML files")]
public class ImportOptions
{
    [Option('c', "category", Required = false, HelpText = "Category slug to import (or all if not specified)")]
    public string? Category { get; set; }

    [Option('s', "stats", Required = false, Default = false, HelpText = "Show import statistics only")]
    public bool ShowStats { get; set; }
}

public class ImportCommandHandler
{
    private readonly HymnsContext _context;
    private readonly ILegacyXmlImportService _importService;

    public ImportCommandHandler(HymnsContext context, ILegacyXmlImportService importService)
    {
        _context = context;
        _importService = importService;
    }

    public async Task<int> ExecuteAsync(ImportOptions options)
    {
        if (options.ShowStats)
        {
            return await ShowStatisticsAsync();
        }

        Console.WriteLine("SDA Hymns - Legacy XML Import");
        Console.WriteLine("==============================");
        Console.WriteLine();

        ImportResult result;

        if (!string.IsNullOrEmpty(options.Category))
        {
            Console.WriteLine($"Importing category: {options.Category}");
            Console.WriteLine();
            result = await _importService.ImportCategoryAsync(options.Category);
        }
        else
        {
            Console.WriteLine("Importing all categories...");
            Console.WriteLine();
            result = await _importService.ImportAllCategoriesAsync();
        }

        PrintResults(result);

        // Update last import date
        if (result.IsSuccess && result.ImportedCount > 0)
        {
            var lastImportSetting = await _context.AppSettingsKeyValue
                .FirstOrDefaultAsync(s => s.Key == "LastDatabaseImportDate");

            if (lastImportSetting != null)
            {
                lastImportSetting.Value = DateTime.UtcNow.ToString("O");
                lastImportSetting.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        return result.IsSuccess ? 0 : 1;
    }

    private async Task<int> ShowStatisticsAsync()
    {
        var stats = await _importService.GetImportStatisticsAsync();

        Console.WriteLine("Import Statistics");
        Console.WriteLine("=================");
        Console.WriteLine($"Total Hymns: {stats.TotalHymns}");
        Console.WriteLine();
        Console.WriteLine("By Category:");

        foreach (var (slug, count) in stats.CategoryCounts.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"  {slug,-15} {count,4} hymns");
        }

        if (stats.LastImportDate.HasValue)
        {
            Console.WriteLine();
            Console.WriteLine($"Last Import: {stats.LastImportDate.Value:yyyy-MM-dd HH:mm:ss} UTC");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Last Import: Never");
        }

        return 0;
    }

    private void PrintResults(ImportResult result)
    {
        Console.WriteLine();
        Console.WriteLine("Import Results");
        Console.WriteLine("==============");
        Console.WriteLine($"Parsed:   {result.ParsedCount,4} hymns");
        Console.WriteLine($"Imported: {result.ImportedCount,4} hymns");
        Console.WriteLine($"Skipped:  {result.SkippedCount,4} hymns (already exist)");

        if (result.Errors.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Errors:");
            foreach (var error in result.Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  - {error}");
                Console.ResetColor();
            }
        }
        else if (result.ImportedCount > 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Import completed successfully!");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No new hymns to import.");
            Console.ResetColor();
        }
    }
}
