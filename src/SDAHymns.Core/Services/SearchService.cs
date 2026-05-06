using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;

namespace SDAHymns.Core.Services;

public interface ISearchService
{
    Task<List<HymnSearchResult>> SearchHymnsAsync(string query, string? categorySlug = null);
    Task<List<Hymn>> GetRecentHymnsAsync(int count = 10);
    Task<List<Hymn>> GetFavoriteHymnsAsync();
    Task AddToRecentAsync(int hymnId);
    Task ToggleFavoriteAsync(int hymnId);
}

public partial class HymnSearchResult : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isFavorite;

    public int VerseCount { get; set; }
}

public class SearchService : ISearchService
{
    private readonly HymnsContext _context;

    public SearchService(HymnsContext context)
    {
        _context = context;
    }

    public async Task<List<HymnSearchResult>> SearchHymnsAsync(string query, string? categorySlug = null)
    {
        var hymnsQuery = _context.Hymns
            .Include(h => h.Category)
            .Include(h => h.Verses)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            hymnsQuery = hymnsQuery.Where(h => h.Category.Slug == categorySlug);
        }

        List<HymnSearchResult> results;

        if (!string.IsNullOrWhiteSpace(query))
        {
            // Get all hymns from the query first
            var allHymns = await hymnsQuery
                .Select(h => new HymnSearchResult
                {
                    Id = h.Id,
                    Number = h.Number,
                    Title = h.Title,
                    CategoryName = h.Category.Name,
                    CategorySlug = h.Category.Slug,
                    IsFavorite = h.IsFavorite,
                    VerseCount = h.Verses.Count
                })
                .ToListAsync();

            // Search by number or title with smart matching
            if (int.TryParse(query, out int number))
            {
                // If it's a number, match by number first, then by title
                results = allHymns
                    .Where(h => h.Number == number ||
                                SmartMatch(h.Title, query))
                    .OrderBy(h => h.Number == number ? 0 : 1) // Exact number match first
                    .ThenBy(h => h.Number)
                    .ToList();
            }
            else
            {
                // Text search - use smart matching
                var normalizedQuery = NormalizeText(query);
                results = allHymns
                    .Where(h => SmartMatch(h.Title, query))
                    .OrderBy(h => h.Number)
                    .ToList();
            }
        }
        else
        {
            // No query - return all
            results = await hymnsQuery
                .OrderBy(h => h.Number)
                .Select(h => new HymnSearchResult
                {
                    Id = h.Id,
                    Number = h.Number,
                    Title = h.Title,
                    CategoryName = h.Category.Name,
                    CategorySlug = h.Category.Slug,
                    IsFavorite = h.IsFavorite,
                    VerseCount = h.Verses.Count
                })
                .ToListAsync();
        }

        return results;
    }

    /// <summary>
    /// Smart text matching: case-insensitive, diacritic-insensitive, punctuation-insensitive
    /// </summary>
    private bool SmartMatch(string text, string query)
    {
        var normalizedText = NormalizeText(text);
        var normalizedQuery = NormalizeText(query);
        return normalizedText.Contains(normalizedQuery);
    }

    /// <summary>
    /// Normalizes text by:
    /// - Converting to lowercase
    /// - Removing diacritics (ă→a, â→a, î→i, ș→s, ț→t)
    /// - Removing punctuation
    /// - Trimming whitespace
    /// </summary>
    private string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase
        text = text.ToLowerInvariant();

        // Remove diacritics
        text = RemoveDiacritics(text);

        // Remove punctuation and special characters, keep only letters, numbers, and spaces
        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        // Normalize whitespace
        return string.Join(" ", sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Removes diacritics from text (Romanian: ă→a, â→a, î→i, ș→s, ț→t)
    /// </summary>
    private string RemoveDiacritics(string text)
    {
        // Normalize to decomposed form (NFD) where diacritics are separate characters
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            // Skip non-spacing marks (diacritics)
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // Return to composed form (NFC)
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public async Task<List<Hymn>> GetRecentHymnsAsync(int count = 10)
    {
        return await _context.Hymns
            .Include(h => h.Category)
            .Where(h => h.LastAccessedAt != null)
            .OrderByDescending(h => h.LastAccessedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Hymn>> GetFavoriteHymnsAsync()
    {
        return await _context.Hymns
            .Include(h => h.Category)
            .Where(h => h.IsFavorite)
            .OrderBy(h => h.Number)
            .ToListAsync();
    }

    public async Task AddToRecentAsync(int hymnId)
    {
        var hymn = await _context.Hymns.FindAsync(hymnId);
        if (hymn != null)
        {
            hymn.LastAccessedAt = DateTime.UtcNow;
            hymn.AccessCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleFavoriteAsync(int hymnId)
    {
        var hymn = await _context.Hymns.FindAsync(hymnId);
        if (hymn != null)
        {
            hymn.IsFavorite = !hymn.IsFavorite;
            await _context.SaveChangesAsync();
        }
    }
}
