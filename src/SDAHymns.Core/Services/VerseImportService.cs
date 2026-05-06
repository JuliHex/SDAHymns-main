using System.Linq;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

/// <summary>
/// Service for importing verses from PowerPoint files into the database
/// </summary>
public class VerseImportService : IVerseImportService
{
    private readonly HymnsContext _context;
    private readonly PowerPointParserService _parserService;

    public VerseImportService(HymnsContext context, PowerPointParserService parserService)
    {
        _context = context;
        _parserService = parserService;
    }

    /// <summary>
    /// Imports verses for a specific hymn by ID
    /// </summary>
    public async Task<VerseImportResult> ImportVersesForHymnAsync(int hymnId, bool force = false)
    {
        // Clear tracker to ensure clean state for every hymn (prevents zombie entities after errors)
        _context.ChangeTracker.Clear();

        var result = new VerseImportResult();

        var hymn = await _context.Hymns
            .Include(h => h.Category)
            .FirstOrDefaultAsync(h => h.Id == hymnId);

        if (hymn == null)
        {
            result.Errors.Add($"Hymn with ID {hymnId} not found");
            return result;
        }

        // Check if verses already exist (unless force is true)
        if (!force)
        {
            var existingCount = await _context.Verses.CountAsync(v => v.HymnId == hymnId);
            if (existingCount > 0)
            {
                result.SkippedHymns++;
                return result;
            }
        }

        // Check if PowerPoint file exists
        if (string.IsNullOrEmpty(hymn.LegacyPowerPointPath))
        {
            result.SkippedHymns++;
            result.Warnings.Add($"Hymn #{hymn.Number} has no PowerPoint path");
            return result;
        }

        if (!File.Exists(hymn.LegacyPowerPointPath))
        {
            result.SkippedHymns++;
            result.Warnings.Add($"Hymn #{hymn.Number}: File not found: {hymn.LegacyPowerPointPath}");
            return result;
        }

        try
        {
            // Extract verses from PowerPoint
            var versesData = await _parserService.ExtractVersesAsync(hymn.LegacyPowerPointPath);

            if (!versesData.Any())
            {
                result.Warnings.Add($"Hymn #{hymn.Number}: No verses extracted from PowerPoint");
                result.HymnsProcessed++;
                return result;
            }

            // Use transaction for atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ALWAYS delete existing verses first to avoid UNIQUE constraint violations
                // Use raw SQL to ensure deletion happens immediately
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Verses WHERE HymnId = {0}", hymnId);

                // Convert to Verse entities
                var now = DateTime.UtcNow;
                var verses = versesData.Select(v => new Verse
                {
                    HymnId = hymn.Id,
                    VerseNumber = v.VerseNumber,
                    Content = v.Content,
                    Label = v.Label,
                    DisplayOrder = v.DisplayOrder,
                    IsInline = v.IsInline,
                    IsContinuation = v.IsContinuation,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();

                // Add to database
                await _context.Verses.AddRangeAsync(verses);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                result.HymnsProcessed++;
                result.VersesImported += verses.Count;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMsg = ex.InnerException != null
                    ? $"{ex.Message} Inner: {ex.InnerException.Message}"
                    : ex.Message;
                result.Errors.Add($"Hymn #{hymn.Number}: Database error: {errorMsg}");
            }
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException != null
                ? $"{ex.Message} Inner: {ex.InnerException.Message}"
                : ex.Message;
            result.Errors.Add($"Hymn #{hymn.Number}: Failed to extract verses: {errorMsg}");
        }

        return result;
    }

    /// <summary>
    /// Imports verses for all hymns in a category
    /// </summary>
    public async Task<VerseImportResult> ImportVersesForCategoryAsync(string categorySlug, bool force = false, int? limit = null, int? startFromNumber = null)
    {
        var totalResult = new VerseImportResult();

        var query = _context.Hymns
            .Include(h => h.Category)
            .Include(h => h.Verses)
            .Where(h => h.Category.Slug == categorySlug);

        // Apply startFromNumber filter
        if (startFromNumber.HasValue)
        {
            query = query.Where(h => h.Number >= startFromNumber.Value);
        }

        var orderedQuery = query.OrderBy(h => h.Number);

        // Apply limit if specified
        IQueryable<Hymn> finalQuery = orderedQuery;
        if (limit.HasValue)
        {
            finalQuery = orderedQuery.Take(limit.Value);
        }

        var hymns = await finalQuery.ToListAsync();

        if (!hymns.Any())
        {
            // Only report error if we weren't just filtering
            if (!startFromNumber.HasValue)
            {
                totalResult.Errors.Add($"No hymns found for category: {categorySlug}");
            }
            else
            {
                Console.WriteLine($"ℹ️ No hymns found in {categorySlug} starting from #{startFromNumber}");
            }
            return totalResult;
        }

        Console.WriteLine($"📖 Importing verses for category: {categorySlug} ({hymns.Count} hymns)");
        if (startFromNumber.HasValue)
        {
            Console.WriteLine($"   Starting from Hymn #{startFromNumber}");
        }
        Console.WriteLine();

        var progress = 0;
        var total = hymns.Count;

        foreach (var hymn in hymns)
        {
            progress++;

            var result = await ImportVersesForHymnAsync(hymn.Id, force);

            // Merge results
            totalResult.HymnsProcessed += result.HymnsProcessed;
            totalResult.VersesImported += result.VersesImported;
            totalResult.SkippedHymns += result.SkippedHymns;
            totalResult.Errors.AddRange(result.Errors);
            totalResult.Warnings.AddRange(result.Warnings);

            // Progress reporting every 10 hymns
            if (progress % 10 == 0 || progress == total)
            {
                Console.WriteLine($"⏳ Progress: {progress}/{total} hymns processed...");
            }

            // Show success for individual hymns
            if (result.VersesImported > 0)
            {
                Console.WriteLine($"   ✅ Hymn #{hymn.Number}: {result.VersesImported} verses imported");
            }
            else if (result.Errors.Any())
            {
                Console.WriteLine($"   ❌ Hymn #{hymn.Number} FAILED:");
                foreach (var err in result.Errors)
                {
                    Console.WriteLine($"      - {err}");
                }
            }
        }

        return totalResult;
    }

    /// <summary>
    /// Imports verses for all hymns in the database
    /// </summary>
    public async Task<VerseImportResult> ImportAllVersesAsync(bool force = false, int? limit = null, int? startFromNumber = null)
    {
        var totalResult = new VerseImportResult();

        var categories = await _context.HymnCategories
            .OrderBy(c => c.Name)
            .ToListAsync();

        Console.WriteLine($"📚 Starting full verse import across {categories.Count} categories");
        if (startFromNumber.HasValue)
        {
            Console.WriteLine($"   Starting from Hymn #{startFromNumber}");
        }
        Console.WriteLine();

        foreach (var category in categories)
        {
            Console.WriteLine($"═══════════════════════════════════════════════════");
            Console.WriteLine($"📖 Category: {category.Name}");
            Console.WriteLine($"═══════════════════════════════════════════════════");

            var result = await ImportVersesForCategoryAsync(category.Slug, force, limit, startFromNumber);

            // Merge results
            totalResult.HymnsProcessed += result.HymnsProcessed;
            totalResult.VersesImported += result.VersesImported;
            totalResult.SkippedHymns += result.SkippedHymns;
            totalResult.Errors.AddRange(result.Errors);
            totalResult.Warnings.AddRange(result.Warnings);

            Console.WriteLine();
            Console.WriteLine($"   Category Summary:");
            Console.WriteLine($"   - Processed: {result.HymnsProcessed} hymns");
            Console.WriteLine($"   - Imported: {result.VersesImported} verses");
            Console.WriteLine($"   - Skipped: {result.SkippedHymns} hymns");
            if (result.Errors.Any())
            {
                Console.WriteLine($"   - Errors: {result.Errors.Count}");
            }
            Console.WriteLine();
        }

        return totalResult;
    }

    /// <summary>
    /// Gets statistics about verse import status
    /// </summary>
    public async Task<VerseImportStatistics> GetStatisticsAsync()
    {
        var stats = new VerseImportStatistics
        {
            TotalHymns = await _context.Hymns.CountAsync(),
            TotalVerses = await _context.Verses.CountAsync()
        };

        // Hymns with verses
        var hymnsWithVerses = await _context.Hymns
            .Include(h => h.Verses)
            .Where(h => h.Verses.Any())
            .CountAsync();

        stats.HymnsWithVerses = hymnsWithVerses;
        stats.HymnsWithoutVerses = stats.TotalHymns - hymnsWithVerses;

        // Verses per category
        var categories = await _context.HymnCategories
            .Include(c => c.Hymns)
            .ThenInclude(h => h.Verses)
            .ToListAsync();

        foreach (var category in categories)
        {
            var versesCount = category.Hymns.Sum(h => h.Verses.Count);
            var hymnsWithVersesCount = category.Hymns.Count(h => h.Verses.Any());

            stats.VersesPerCategory[category.Name] = versesCount;
            stats.HymnsWithVersesPerCategory[category.Name] = hymnsWithVersesCount;
        }

        return stats;
    }
}
