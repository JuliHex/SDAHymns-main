using CommandLine;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SDAHymns.Core.Services;

namespace SDAHymns.CLI.Commands;

[Verb("test-ppt", HelpText = "Test PPT file structure analysis")]
public class TestPptCommand
{
    [Option('f', "file", Required = true, HelpText = "Path to PPT file to analyze")]
    public string FilePath { get; set; } = string.Empty;
}

public class TestPptCommandHandler
{
    private readonly PowerPointParserService _parser;

    public TestPptCommandHandler(PowerPointParserService parser)
    {
        _parser = parser;
    }

    public async Task<int> ExecuteAsync(TestPptCommand options)
    {
        Console.WriteLine($"Analyzing PPT file: {options.FilePath}");
        Console.WriteLine();

        // Extract hymn info using the parser service
        Console.WriteLine("Extracting hymn information...");
        var hymnInfo = await _parser.ExtractHymnInfoAsync(options.FilePath);

        if (hymnInfo == null)
        {
            Console.WriteLine("❌ Failed to extract hymn information from PPT file");
            return 1;
        }

        Console.WriteLine($"✅ Successfully extracted hymn info:");
        Console.WriteLine($"   Hymn Number: {hymnInfo.Value.hymnNumber}");
        Console.WriteLine($"   Title: {hymnInfo.Value.title}");
        Console.WriteLine();

        // Extract and display verses
        Console.WriteLine("Extracting verses...");
        var verses = await _parser.ExtractVersesAsync(options.FilePath);

        Console.WriteLine($"✅ Extracted {verses.Count} verses");
        Console.WriteLine();
        Console.WriteLine(new string('=', 80));

        foreach (var verse in verses)
        {
            var label = verse.Label ?? $"Verse {verse.VerseNumber}";
            Console.WriteLine($"📄 {label} (Display Order: {verse.DisplayOrder})");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine(verse.Content);
            Console.WriteLine();
        }

        Console.WriteLine(new string('=', 80));
        Console.WriteLine("✅ Analysis Complete!");

        return 0;
    }
}
