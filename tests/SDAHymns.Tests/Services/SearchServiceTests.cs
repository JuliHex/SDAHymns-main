using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;
using SDAHymns.Core.Services;
using Xunit;

namespace SDAHymns.Tests.Services;

public class SearchServiceTests : IDisposable
{
    private readonly HymnsContext _context;
    private readonly SearchService _searchService;

    public SearchServiceTests()
    {
        var options = new DbContextOptionsBuilder<HymnsContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new HymnsContext(options);
        _searchService = new SearchService(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var crestineCategory = new HymnCategory
        {
            Name = "Imnuri Crestine",
            Slug = "crestine",
            DisplayOrder = 1
        };

        var exploratoriCategory = new HymnCategory
        {
            Name = "Imnuri Exploratori",
            Slug = "exploratori",
            DisplayOrder = 2
        };

        _context.HymnCategories.AddRange(crestineCategory, exploratoriCategory);
        _context.SaveChanges();

        var hymns = new List<Hymn>
        {
            new Hymn
            {
                Number = 1,
                Title = "Spre slava Ta uniţi",
                CategoryId = crestineCategory.Id,
                IsFavorite = false,
                AccessCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Verses = new List<Verse>
                {
                    new Verse { Label = "1.", Content = "Verse 1 content", DisplayOrder = 1 },
                    new Verse { Label = "2.", Content = "Verse 2 content", DisplayOrder = 2 }
                }
            },
            new Hymn
            {
                Number = 20,
                Title = "Aleluia! Răsună cântec",
                CategoryId = crestineCategory.Id,
                IsFavorite = true,
                AccessCount = 5,
                LastAccessedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Verses = new List<Verse>
                {
                    new Verse { Label = "1.", Content = "Verse 1", DisplayOrder = 1 },
                    new Verse { Label = "2.", Content = "Verse 2", DisplayOrder = 2 },
                    new Verse { Label = "3.", Content = "Verse 3", DisplayOrder = 3 }
                }
            },
            new Hymn
            {
                Number = 45,
                Title = "Domnul e stânca mea",
                CategoryId = crestineCategory.Id,
                IsFavorite = false,
                AccessCount = 3,
                LastAccessedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Verses = new List<Verse>
                {
                    new Verse { Label = "1.", Content = "Verse 1", DisplayOrder = 1 }
                }
            },
            new Hymn
            {
                Number = 99,
                Title = "O, ce prieten avem în Isus",
                CategoryId = crestineCategory.Id,
                IsFavorite = false,
                AccessCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Verses = new List<Verse>
                {
                    new Verse { Label = "1.", Content = "Verse 1", DisplayOrder = 1 },
                    new Verse { Label = "Refren", Content = "Chorus", DisplayOrder = 2 }
                }
            },
            new Hymn
            {
                Number = 10,
                Title = "Exploratori march",
                CategoryId = exploratoriCategory.Id,
                IsFavorite = false,
                AccessCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Verses = new List<Verse>
                {
                    new Verse { Label = "1.", Content = "Verse 1", DisplayOrder = 1 }
                }
            }
        };

        _context.Hymns.AddRange(hymns);
        _context.SaveChanges();
    }

    [Fact]
    public async Task SearchHymnsAsync_ByNumber_ReturnsMatchingHymn()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("20", null);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].Number.Should().Be(20);
        results[0].Title.Should().Be("Aleluia! Răsună cântec");
        results[0].VerseCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchHymnsAsync_ByPartialTitle_ReturnsMatchingHymns()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("slava", null);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].Title.Should().Contain("slava");
    }

    [Fact]
    public async Task SearchHymnsAsync_WithCategoryFilter_ReturnsOnlyFromCategory()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("", "exploratori");

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].CategorySlug.Should().Be("exploratori");
    }

    [Fact]
    public async Task SearchHymnsAsync_EmptyQuery_ReturnsAllHymnsInCategory()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("", "crestine");

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(4); // All crestine hymns
        results.Should().OnlyContain(r => r.CategorySlug == "crestine");
    }

    [Fact]
    public async Task SearchHymnsAsync_NoMatch_ReturnsEmptyList()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("xyz123", null);

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchHymnsAsync_IncludesFavoriteStatus()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("", "crestine");

        // Assert
        var favoriteHymn = results.FirstOrDefault(r => r.Number == 20);
        favoriteHymn.Should().NotBeNull();
        favoriteHymn!.IsFavorite.Should().BeTrue();

        var nonFavoriteHymn = results.FirstOrDefault(r => r.Number == 1);
        nonFavoriteHymn.Should().NotBeNull();
        nonFavoriteHymn!.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task GetRecentHymnsAsync_ReturnsHymnsOrderedByLastAccessed()
    {
        // Act
        var results = await _searchService.GetRecentHymnsAsync(10);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2); // Only 2 hymns have LastAccessedAt set
        results[0].Number.Should().Be(20); // Most recent (1 day ago)
        results[1].Number.Should().Be(45); // Older (2 days ago)
    }

    [Fact]
    public async Task GetRecentHymnsAsync_LimitsResultCount()
    {
        // Act
        var results = await _searchService.GetRecentHymnsAsync(1);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].Number.Should().Be(20); // Most recent
    }

    [Fact]
    public async Task GetFavoriteHymnsAsync_ReturnsOnlyFavorites()
    {
        // Act
        var results = await _searchService.GetFavoriteHymnsAsync();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].Number.Should().Be(20);
        results[0].IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task GetFavoriteHymnsAsync_OrdersByNumber()
    {
        // Arrange - add another favorite
        var hymn = await _context.Hymns.FirstAsync(h => h.Number == 1);
        hymn.IsFavorite = true;
        await _context.SaveChangesAsync();

        // Act
        var results = await _searchService.GetFavoriteHymnsAsync();

        // Assert
        results.Should().HaveCount(2);
        results[0].Number.Should().Be(1);
        results[1].Number.Should().Be(20);
    }

    [Fact]
    public async Task AddToRecentAsync_UpdatesLastAccessedAndCount()
    {
        // Arrange
        var hymn = await _context.Hymns.FirstAsync(h => h.Number == 1);
        var initialAccessCount = hymn.AccessCount;
        var initialLastAccessed = hymn.LastAccessedAt;

        // Act
        await _searchService.AddToRecentAsync(hymn.Id);

        // Assert
        var updatedHymn = await _context.Hymns.FirstAsync(h => h.Number == 1);
        updatedHymn.AccessCount.Should().Be(initialAccessCount + 1);
        updatedHymn.LastAccessedAt.Should().NotBeNull();
        updatedHymn.LastAccessedAt.Should().BeAfter(initialLastAccessed ?? DateTime.MinValue);
    }

    [Fact]
    public async Task AddToRecentAsync_WithInvalidId_DoesNotThrow()
    {
        // Act
        var action = async () => await _searchService.AddToRecentAsync(99999);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ToggleFavoriteAsync_TogglesIsFavorite()
    {
        // Arrange
        var hymn = await _context.Hymns.FirstAsync(h => h.Number == 1);
        var initialFavoriteStatus = hymn.IsFavorite;

        // Act - Toggle on
        await _searchService.ToggleFavoriteAsync(hymn.Id);

        // Assert - Should be opposite
        var updatedHymn = await _context.Hymns.FirstAsync(h => h.Number == 1);
        updatedHymn.IsFavorite.Should().Be(!initialFavoriteStatus);

        // Act - Toggle off
        await _searchService.ToggleFavoriteAsync(hymn.Id);

        // Assert - Should be back to original
        updatedHymn = await _context.Hymns.FirstAsync(h => h.Number == 1);
        updatedHymn.IsFavorite.Should().Be(initialFavoriteStatus);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_WithInvalidId_DoesNotThrow()
    {
        // Act
        var action = async () => await _searchService.ToggleFavoriteAsync(99999);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SearchHymnsAsync_PartialMatch_FindsHymns()
    {
        // Act - Search with partial title match (using exact casing from test data)
        var resultsAleluia = await _searchService.SearchHymnsAsync("Aleluia", null);
        var resultsRasuna = await _searchService.SearchHymnsAsync("Răsună", null);
        var resultsCantec = await _searchService.SearchHymnsAsync("cântec", null);

        // Assert - Should find the hymn with matching text
        resultsAleluia.Should().HaveCount(1);
        resultsAleluia[0].Number.Should().Be(20);

        resultsRasuna.Should().HaveCount(1);
        resultsRasuna[0].Number.Should().Be(20);

        resultsCantec.Should().HaveCount(1);
        resultsCantec[0].Number.Should().Be(20);
    }

    [Fact]
    public async Task SearchHymnsAsync_ReturnsCorrectVerseCount()
    {
        // Act
        var results = await _searchService.SearchHymnsAsync("", null);

        // Assert
        var hymn1 = results.First(r => r.Number == 1);
        hymn1.VerseCount.Should().Be(2);

        var hymn20 = results.First(r => r.Number == 20);
        hymn20.VerseCount.Should().Be(3);

        var hymn99 = results.First(r => r.Number == 99);
        hymn99.VerseCount.Should().Be(2); // Including chorus
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
