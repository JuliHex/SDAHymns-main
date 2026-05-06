using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

public class LegacyXmlImportService : ILegacyXmlImportService
{
    private readonly HymnsContext _context;
    private readonly ILogger<LegacyXmlImportService> _logger;

    public LegacyXmlImportService(HymnsContext context, ILogger<LegacyXmlImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportResult> ImportAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = new ImportResult();
        var categories = await _context.HymnCategories.OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken);

        _logger.LogInformation("Starting import of all {CategoryCount} categories", categories.Count);

        foreach (var category in categories)
        {
            try
            {
                var categoryResult = await ImportCategoryAsync(category.Slug, cancellationToken);
                result.Merge(categoryResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import category {CategorySlug}", category.Slug);
                result.Errors.Add($"Category {category.Slug}: {ex.Message}");
            }
        }

        _logger.LogInformation(
            "Import completed: {ImportedCount} imported, {SkippedCount} skipped, {ErrorCount} errors",
            result.ImportedCount, result.SkippedCount, result.Errors.Count);

        return result;
    }

    public async Task<ImportResult> ImportCategoryAsync(string categorySlug, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult { CategorySlug = categorySlug };

        _logger.LogInformation("Importing category: {CategorySlug}", categorySlug);

        // Get category from database
        var category = await _context.HymnCategories
            .FirstOrDefaultAsync(c => c.Slug == categorySlug, cancellationToken);

        if (category == null)
        {
            result.Errors.Add($"Category '{categorySlug}' not found in database");
            return result;
        }

        if (string.IsNullOrEmpty(category.LegacyFolderPath))
        {
            result.Errors.Add($"Category '{categorySlug}' has no legacy folder path configured");
            return result;
        }

        // Build path to XML file
        var solutionRoot = GetSolutionRoot();
        var xmlPath = Path.Combine(solutionRoot, category.LegacyFolderPath, "index.xml");

        if (!File.Exists(xmlPath))
        {
            result.Errors.Add($"XML file not found: {xmlPath}");
            return result;
        }

        _logger.LogDebug("Parsing XML file: {XmlPath}", xmlPath);

        // Parse XML
        List<Hymn> hymns;
        try
        {
            hymns = ParseXmlFile(xmlPath, category.Id);
            result.ParsedCount = hymns.Count;
            _logger.LogDebug("Parsed {HymnCount} hymns from XML", hymns.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse XML file: {XmlPath}", xmlPath);
            result.Errors.Add($"Failed to parse XML: {ex.Message}");
            return result;
        }

        // Check for existing hymns in this category
        var existingNumbers = await _context.Hymns
            .Where(h => h.CategoryId == category.Id)
            .Select(h => h.Number)
            .ToListAsync(cancellationToken);

        // Filter out duplicates
        var newHymns = hymns.Where(h => !existingNumbers.Contains(h.Number)).ToList();
        result.SkippedCount = hymns.Count - newHymns.Count;

        // Batch insert
        if (newHymns.Any())
        {
            await _context.Hymns.AddRangeAsync(newHymns, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            result.ImportedCount = newHymns.Count;

            _logger.LogInformation(
                "Category {CategorySlug}: Imported {ImportedCount} hymns ({SkippedCount} skipped)",
                categorySlug, result.ImportedCount, result.SkippedCount);
        }
        else
        {
            _logger.LogInformation(
                "Category {CategorySlug}: No new hymns to import ({SkippedCount} already exist)",
                categorySlug, result.SkippedCount);
        }

        return result;
    }

    private List<Hymn> ParseXmlFile(string xmlPath, int categoryId)
    {
        var hymns = new List<Hymn>();
        var pptDirectory = Path.Combine(Path.GetDirectoryName(xmlPath)!, "ppt");

        var doc = XDocument.Load(xmlPath);
        var imnuriElements = doc.Root?.Elements("Imn") ?? Enumerable.Empty<XElement>();

        foreach (var imn in imnuriElements)
        {
            var numarStr = imn.Element("Numar")?.Value?.Trim();
            var titlu = imn.Element("Titlu")?.Value?.Trim();

            if (string.IsNullOrEmpty(numarStr) || string.IsNullOrEmpty(titlu))
            {
                _logger.LogWarning("Skipping entry with missing number or title in {XmlPath}", xmlPath);
                continue;
            }

            if (!int.TryParse(numarStr, out var numar))
            {
                _logger.LogWarning("Invalid hymn number: {Number} in {XmlPath}", numarStr, xmlPath);
                continue;
            }

            // Build PowerPoint file path (try both .PPT and .ppt)
            var pptFileName = $"{numar:D3}.PPT";
            var pptPath = Path.Combine(pptDirectory, pptFileName);
            if (!File.Exists(pptPath))
            {
                pptFileName = $"{numar:D3}.ppt";
                pptPath = Path.Combine(pptDirectory, pptFileName);
            }

            var legacyPath = File.Exists(pptPath)
                ? Path.GetRelativePath(GetSolutionRoot(), pptPath).Replace("\\", "/")
                : null;

            if (legacyPath == null)
            {
                _logger.LogDebug("PowerPoint file not found for hymn {Number} in {Category}", numar, Path.GetFileName(Path.GetDirectoryName(xmlPath)));
            }

            var hymn = new Hymn
            {
                Number = numar,
                Title = titlu,
                CategoryId = categoryId,
                LegacyPowerPointPath = legacyPath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            hymns.Add(hymn);
        }

        return hymns;
    }

    private string GetSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir, "SDAHymns.sln")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        throw new InvalidOperationException("Could not find solution root directory");
    }

    public async Task<ImportStatistics> GetImportStatisticsAsync()
    {
        var stats = new ImportStatistics();

        var categories = await _context.HymnCategories
            .Select(c => new
            {
                c.Slug,
                c.Name,
                HymnCount = c.Hymns.Count
            })
            .ToListAsync();

        stats.TotalHymns = categories.Sum(c => c.HymnCount);
        stats.CategoryCounts = categories.ToDictionary(c => c.Slug, c => c.HymnCount);

        var lastImportSetting = await _context.AppSettingsKeyValue
            .FirstOrDefaultAsync(s => s.Key == "LastDatabaseImportDate");

        if (lastImportSetting != null && !string.IsNullOrEmpty(lastImportSetting.Value))
        {
            if (DateTime.TryParse(lastImportSetting.Value, out var lastImport))
            {
                stats.LastImportDate = lastImport;
            }
        }

        return stats;
    }
}
