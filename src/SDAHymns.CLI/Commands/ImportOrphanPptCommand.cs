using CommandLine;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;

namespace SDAHymns.CLI.Commands;

[Verb("import-orphan-ppt", HelpText = "Import hymns from PPT files that are not in the XML index")]
public class ImportOrphanPptCommand
{
    [Option('c', "category", Required = true, HelpText = "Hymn category (crestine, companioni, exploratori, licurici, tineret)")]
    public string Category { get; set; } = string.Empty;

    [Option('d', "dry-run", Required = false, Default = false, HelpText = "Preview orphan hymns without importing")]
    public bool DryRun { get; set; }

    [Option('p', "ppt-directory", Required = false, HelpText = "Path to PPT directory (defaults to Imnuri Azs/Resurse/Imnuri {category}/ppt)")]
    public string? PptDirectory { get; set; }
}

public class ImportOrphanPptCommandHandler
{
    private readonly HymnsContext _context;
    private readonly PowerPointParserService _parser;

    public ImportOrphanPptCommandHandler(HymnsContext context, PowerPointParserService parser)
    {
        _context = context;
        _parser = parser;
    }

    public async Task<int> ExecuteAsync(ImportOrphanPptCommand options)
    {
        Console.WriteLine($"🔍 Scanning for orphan PPT files in category: {options.Category}");
        Console.WriteLine();

        // Determine PPT directory
        var pptDir = options.PptDirectory ?? GetDefaultPptDirectory(options.Category);
        if (!Directory.Exists(pptDir))
        {
            Console.WriteLine($"❌ Error: PPT directory not found: {pptDir}");
            return 1;
        }

        // Get all PPT files (deduplicate case-insensitive)
        var pptFiles = Directory.GetFiles(pptDir, "*.PPT", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(pptDir, "*.ppt", SearchOption.TopDirectoryOnly))
            .GroupBy(f => f.ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

        Console.WriteLine($"📁 Found {pptFiles.Count} PPT files in directory");

        // Get existing hymn numbers from database
        var category = await _context.HymnCategories
            .FirstOrDefaultAsync(c => c.Slug == options.Category.ToLowerInvariant());

        if (category == null)
        {
            Console.WriteLine($"❌ Error: Category not found in database: {options.Category}");
            Console.WriteLine("Available categories: crestine, companioni, exploratori, licurici, tineret");
            return 1;
        }

        var existingNumbers = await _context.Hymns
            .Where(h => h.CategoryId == category.Id)
            .Select(h => h.Number)
            .ToListAsync();

        Console.WriteLine($"💾 Found {existingNumbers.Count} existing hymns in database");

        // Find orphan files (PPT exists but not in database)
        var orphanFiles = new List<(string filePath, int hymnNumber)>();

        foreach (var pptFile in pptFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(pptFile);
            if (int.TryParse(fileName, out var hymnNumber))
            {
                if (!existingNumbers.Contains(hymnNumber))
                {
                    orphanFiles.Add((pptFile, hymnNumber));
                }
            }
        }

        Console.WriteLine($"🔎 Found {orphanFiles.Count} orphan PPT files (not in database)");
        Console.WriteLine();

        if (orphanFiles.Count == 0)
        {
            Console.WriteLine("✅ No orphan files found. All PPT files are already in the database!");
            return 0;
        }

        if (options.DryRun)
        {
            Console.WriteLine("🔍 DRY RUN - Orphan hymns that would be imported:");
            Console.WriteLine();
            foreach (var (filePath, hymnNumber) in orphanFiles.OrderBy(o => o.hymnNumber))
            {
                Console.WriteLine($"  #{hymnNumber:D3} - {Path.GetFileName(filePath)}");
            }
            Console.WriteLine();
            Console.WriteLine($"Total: {orphanFiles.Count} orphan hymns");
            Console.WriteLine("Run without --dry-run to import these hymns.");
            return 0;
        }

        // Parse PPT files and extract titles
        Console.WriteLine("📖 Extracting titles from PowerPoint files...");
        Console.WriteLine("This may take a few minutes...");
        Console.WriteLine();

        var progress = new Progress<(int current, int total, string fileName)>(p =>
        {
            Console.Write($"\r⏳ Processing: {p.current}/{p.total} - {p.fileName}".PadRight(80));
        });

        var extractionResults = await _parser.ExtractBatchAsync(
            orphanFiles.Select(o => o.filePath),
            progress);

        Console.WriteLine();
        Console.WriteLine();

        // Import into database
        var successCount = 0;
        var failedCount = 0;
        var failedFiles = new List<string>();

        foreach (var (filePath, expectedNumber) in orphanFiles)
        {
            var result = extractionResults[filePath];

            if (result == null)
            {
                Console.WriteLine($"❌ Failed to parse: {Path.GetFileName(filePath)}");
                failedCount++;
                failedFiles.Add(filePath);
                continue;
            }

            var (hymnNumber, title) = result.Value;

            // Verify hymn number matches file name
            if (hymnNumber != expectedNumber)
            {
                Console.WriteLine($"⚠️  Warning: Hymn number mismatch for {Path.GetFileName(filePath)}");
                Console.WriteLine($"   File name: {expectedNumber}, Extracted: {hymnNumber}");
                Console.WriteLine($"   Using file name ({expectedNumber}) and extracted title: {title}");
                hymnNumber = expectedNumber;
            }

            // Create hymn entity
            var hymn = new Hymn
            {
                Number = hymnNumber,
                Title = title,
                CategoryId = category.Id,
                LegacyPowerPointPath = Path.GetFullPath(filePath)
            };

            // Create a single verse placeholder (will be populated later if needed)
            var verse = new Verse
            {
                VerseNumber = 1,
                Content = $"[Content not yet imported from PowerPoint]\n\nOriginal file: {Path.GetFileName(filePath)}"
            };

            hymn.Verses.Add(verse);

            _context.Hymns.Add(hymn);
            successCount++;

            Console.WriteLine($"✅ #{hymnNumber:D3} - {title}");
        }

        // Save to database
        await _context.SaveChangesAsync();

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine($"✅ Successfully imported: {successCount} hymns");

        if (failedCount > 0)
        {
            Console.WriteLine($"❌ Failed to parse: {failedCount} hymns");
            Console.WriteLine();
            Console.WriteLine("Failed files:");
            foreach (var file in failedFiles)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)}");
            }
        }

        Console.WriteLine("═══════════════════════════════════════════════════════════");

        return failedCount > 0 ? 1 : 0;
    }

    private static string GetDefaultPptDirectory(string category)
    {
        var categoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "crestine", "Imnuri crestine" },
            { "companioni", "Imnuri companioni" },
            { "exploratori", "Imnuri exploratori" },
            { "licurici", "Imnuri licurici" },
            { "tineret", "Imnuri tineret" }
        };

        var folderName = categoryMap.GetValueOrDefault(category.ToLowerInvariant(), $"Imnuri {category}");
        return Path.Combine("Imnuri Azs", "Resurse", folderName, "ppt");
    }
}
