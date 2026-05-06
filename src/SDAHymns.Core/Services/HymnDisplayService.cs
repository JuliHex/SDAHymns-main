using Microsoft.EntityFrameworkCore;
using SDAHymns.Core.Data;
using SDAHymns.Core.Data.Models;
using System.Linq;
using System.Collections.Generic;

namespace SDAHymns.Core.Services;

public interface IHymnDisplayService
{
    Task<Hymn?> GetHymnByNumberAsync(int hymnNumber, string categorySlug);
    Task<List<Verse>> GetVersesForHymnAsync(int hymnId);
    Task UpdateAudioRecordingAsync(AudioRecording audioRecording);
}

public class HymnDisplayService : IHymnDisplayService
{
    private readonly HymnsContext _context;

    public HymnDisplayService(HymnsContext context)
    {
        _context = context;
    }

    public async Task<Hymn?> GetHymnByNumberAsync(int hymnNumber, string categorySlug)
    {
        var hymn = await _context.Hymns
            .Include(h => h.Category)
            .Include(h => h.Verses.OrderBy(v => v.DisplayOrder))
            .FirstOrDefaultAsync(h =>
                h.Number == hymnNumber &&
                h.Category.Slug == categorySlug);

        if (hymn != null && hymn.Verses != null)
        {
            var versesList = hymn.Verses.ToList();
            var titleVerse = new Verse
            {
                Id = -1,
                HymnId = hymn.Id,
                VerseNumber = 0,
                Label = "Titlu",
                Content = $"{hymn.Number}. {hymn.Title}",
                DisplayOrder = -1,
                IsInline = false,
                IsContinuation = false
            };
            versesList.Insert(0, titleVerse);
            hymn.Verses = versesList;
        }

        return hymn;
    }

    public async Task<List<Verse>> GetVersesForHymnAsync(int hymnId)
    {
        var verses = await _context.Verses
            .Where(v => v.HymnId == hymnId)
            .OrderBy(v => v.DisplayOrder)
            .ToListAsync();

        var hymn = await _context.Hymns.FindAsync(hymnId);
        if (hymn != null)
        {
            var titleVerse = new Verse
            {
                Id = -1,
                HymnId = hymn.Id,
                VerseNumber = 0,
                Label = "Titlu",
                Content = $"{hymn.Number}. {hymn.Title}",
                DisplayOrder = -1,
                IsInline = false,
                IsContinuation = false
            };
            verses.Insert(0, titleVerse);
        }

        return verses;
    }

    public async Task UpdateAudioRecordingAsync(AudioRecording audioRecording)
    {
        _context.AudioRecordings.Update(audioRecording);
        await _context.SaveChangesAsync();
    }
}
