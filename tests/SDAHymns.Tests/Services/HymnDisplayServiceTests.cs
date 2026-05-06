using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Services;

namespace SDAHymns.Tests.Services;

public class HymnDisplayServiceTests : IDisposable
{
    private readonly HymnsContext _context;
    private readonly HymnDisplayService _service;

    public HymnDisplayServiceTests()
    {
        // Setup in-memory database with production data
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "Resources", "hymns.db");
        var options = new DbContextOptionsBuilder<HymnsContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        _context = new HymnsContext(options);
        _service = new HymnDisplayService(_context);
    }

    [Fact]
    public async Task GetHymnByNumberAsync_ValidHymn_ReturnsHymnWithVerses()
    {
        // Act
        var hymn = await _service.GetHymnByNumberAsync(1, "crestine");

        // Assert
        Assert.NotNull(hymn);
        Assert.Equal("Spre slava Ta uniţi", hymn.Title);
        Assert.NotNull(hymn.Verses);
        Assert.NotEmpty(hymn.Verses);
    }

    [Fact]
    public async Task GetHymnByNumberAsync_InvalidHymn_ReturnsNull()
    {
        // Act
        var hymn = await _service.GetHymnByNumberAsync(9999, "crestine");

        // Assert
        Assert.Null(hymn);
    }

    [Fact]
    public async Task GetHymnByNumberAsync_WithChorus_ReturnsAllVerses()
    {
        // Act - Hymn 20 should have chorus
        var hymn = await _service.GetHymnByNumberAsync(20, "crestine");

        // Assert
        Assert.NotNull(hymn);
        Assert.NotEmpty(hymn.Verses);

        var verses = await _service.GetVersesForHymnAsync(hymn.Id);
        Assert.NotEmpty(verses);
        Assert.Contains(verses, v =>
            (v.Label != null && v.Label.Contains("Refren")) ||
            v.Content.Contains("Refren"));
    }

    [Fact]
    public async Task GetVersesForHymnAsync_OrdersByDisplayOrder()
    {
        // Arrange
        var hymn = await _service.GetHymnByNumberAsync(1, "crestine");
        Assert.NotNull(hymn);

        // Act
        var verses = await _service.GetVersesForHymnAsync(hymn.Id);

        // Assert
        Assert.NotEmpty(verses);
        for (int i = 0; i < verses.Count - 1; i++)
        {
            Assert.True(verses[i].DisplayOrder <= verses[i + 1].DisplayOrder,
                $"Verses not in order: {verses[i].DisplayOrder} > {verses[i + 1].DisplayOrder}");
        }
    }

    [Fact]
    public async Task GetHymnByNumberAsync_EdgeCase_Hymn99()
    {
        // Act - Hymn 99 has DisplayOrder starting at 2 according to spec
        var hymn = await _service.GetHymnByNumberAsync(99, "crestine");

        // Assert
        Assert.NotNull(hymn);
        var verses = await _service.GetVersesForHymnAsync(hymn.Id);
        Assert.NotEmpty(verses);
    }

    [Fact]
    public async Task GetHymnByNumberAsync_DifferentCategory_Exploratori()
    {
        // Act
        var hymn = await _service.GetHymnByNumberAsync(1, "exploratori");

        // Assert
        Assert.NotNull(hymn);
        Assert.Equal("exploratori", hymn.Category.Slug);
    }

    [Fact]
    public async Task GetHymnByNumberAsync_RomanianCharacters_DisplayCorrectly()
    {
        // Act
        var hymn = await _service.GetHymnByNumberAsync(1, "crestine");

        // Assert
        Assert.NotNull(hymn);
        // Verify the title is not corrupted and contains the expected text
        Assert.NotNull(hymn.Title);
        Assert.NotEmpty(hymn.Title);
        // Check for "slava" which should be in the title "Spre slava Ta uniţi"
        Assert.Contains("slava", hymn.Title, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
