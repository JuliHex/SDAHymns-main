using CommandLine;
using SDAHymns.Core.Services;

namespace SDAHymns.CLI.Commands;

[Verb("test-verses", HelpText = "Test verse extraction from PPT file")]
public class TestVerseExtractionCommand
{
    [Option('f', "file", Required = true, HelpText = "Path to PPT file")]
    public string FilePath { get; set; } = string.Empty;
}

public class TestVerseExtractionCommandHandler
{
    private readonly PowerPointParserService _parser;

    public TestVerseExtractionCommandHandler(PowerPointParserService parser)
    {
        _parser = parser;
    }

    public async Task<int> ExecuteAsync(TestVerseExtractionCommand options)
    {
        Console.WriteLine($"Testing verse extraction: {options.FilePath}");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();

        var verses = await _parser.ExtractVersesAsync(options.FilePath);

        if (!verses.Any())
        {
            Console.WriteLine("❌ No verses found!");
            return 1;
        }

        Console.WriteLine($"✅ Extracted {verses.Count} verses");
        Console.WriteLine();

        foreach (var verse in verses)
        {
            Console.WriteLine(new string('-', 60));
            var flags = new List<string>();
            if (verse.IsInline)
                flags.Add("[INLINE]");
            if (verse.IsContinuation)
                flags.Add("[CONTINUATION]");
            var flagString = flags.Any() ? " " + string.Join(" ", flags) : "";

            if (verse.Label == "Refren")
            {
                Console.WriteLine($"🎵 CHORUS (DisplayOrder: {verse.DisplayOrder}){flagString}");
            }
            else
            {
                Console.WriteLine($"📝 VERSE {verse.VerseNumber} (DisplayOrder: {verse.DisplayOrder}){flagString}");
            }
            Console.WriteLine(new string('-', 60));
            Console.WriteLine(verse.Content);
            Console.WriteLine();
        }

        Console.WriteLine(new string('=', 80));
        Console.WriteLine($"✅ Total: {verses.Count} verses extracted");

        return 0;
    }
}
