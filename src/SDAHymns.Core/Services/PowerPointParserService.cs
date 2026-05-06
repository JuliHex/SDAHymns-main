using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation; // Using P = DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing; // Using A = DocumentFormat.OpenXml.Drawing;

namespace SDAHymns.Core.Services;

/// <summary>
/// Data structure for extracted verse information
/// </summary>
public class VerseData
{
    public int VerseNumber { get; set; }
    public required string Content { get; set; }
    public string? Label { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsInline { get; set; }
    public bool IsContinuation { get; set; }
}

/// <summary>
/// Service for parsing hymn titles and verses from legacy PowerPoint (.PPT) files
/// </summary>
public class PowerPointParserService
{
    private readonly string _libreOfficePath;
    private readonly string _tempDirectory;

    /// <summary>
    /// Creates a new PowerPointParserService with auto-detected LibreOffice path
    /// </summary>
    public PowerPointParserService() : this(null)
    {
    }

    /// <summary>
    /// Creates a new PowerPointParserService with a custom LibreOffice path
    /// </summary>
    /// <param name="libreOfficePath">Path to LibreOffice executable, or null to auto-detect</param>
    public PowerPointParserService(string? libreOfficePath)
    {
        _libreOfficePath = libreOfficePath ?? FindLibreOfficePath();
        _tempDirectory = Path.Combine(Path.GetTempPath(), "sdahymns-ppt-parser");
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Automatically detects LibreOffice installation path based on the operating system
    /// </summary>
    private static string FindLibreOfficePath()
    {
        var possiblePaths = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            possiblePaths.AddRange(new[]
            {
                @"C:\Program Files\LibreOffice\program\soffice.com",
                @"C:\Program Files\LibreOffice\program\soffice.exe",
                @"C:\Program Files (x86)\LibreOffice\program\soffice.com",
                @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            possiblePaths.AddRange(new[]
            {
                "/Applications/LibreOffice.app/Contents/MacOS/soffice",
                "/usr/local/bin/soffice",
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            possiblePaths.AddRange(new[]
            {
                "/usr/bin/soffice",
                "/usr/local/bin/soffice",
                "/opt/libreoffice/program/soffice",
            });
        }

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new InvalidOperationException(
            $"LibreOffice not found. Searched locations: {string.Join(", ", possiblePaths)}. " +
            "Please install LibreOffice or provide a custom path.");
    }

    /// <summary>
    /// Extracts hymn number and title from a .PPT file
    /// </summary>
    /// <param name="pptFilePath">Path to the .PPT file</param>
    /// <returns>Tuple of (hymn number, title) or null if parsing failed</returns>
    public async Task<(int hymnNumber, string title)?> ExtractHymnInfoAsync(string pptFilePath)
    {
        if (!File.Exists(pptFilePath))
        {
            throw new FileNotFoundException($"PPT file not found: {pptFilePath}");
        }

        try
        {
            // Step 1: Convert PPT to PPTX using LibreOffice
            var pptxPath = await ConvertPptToPptxAsync(pptFilePath);

            if (pptxPath == null)
            {
                return null;
            }

            // Step 2: Extract text from first slide
            var texts = ExtractFirstSlideText(pptxPath);

            // Step 3: Parse hymn number and title
            var result = ParseHymnInfo(texts);

            // Clean up temporary PPTX file
            try
            {
                File.Delete(pptxPath);
            }
            catch
            {
                // Ignore cleanup errors
            }

            return result;
        }
        catch (Exception)
        {
            // Silent failure - will be reported by calling code
            return null;
        }
    }

    /// <summary>
    /// Converts a .PPT file to .PPTX using LibreOffice
    /// </summary>
    private async Task<string?> ConvertPptToPptxAsync(string pptFilePath)
    {
        if (!File.Exists(_libreOfficePath))
        {
            throw new InvalidOperationException($"LibreOffice not found at: {_libreOfficePath}");
        }

        var fullPptPath = Path.GetFullPath(pptFilePath);
        var outputDir = _tempDirectory;

        var startInfo = new ProcessStartInfo
        {
            FileName = _libreOfficePath,
            Arguments = $"--headless --convert-to pptx --outdir \"{outputDir}\" \"{fullPptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return null;
        }

        try
        {
            // Add timeout to prevent hangs (45 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill();
            }
            catch { /* Ignore if already exited */ }

            // Log warning but don't crash the whole app - return null to skip this file
            Console.WriteLine($"   ⚠️ Timeout converting: {Path.GetFileName(pptFilePath)}");
            return null;
        }

        // Construct expected output path
        var fileName = Path.GetFileNameWithoutExtension(pptFilePath);
        var pptxPath = Path.Combine(outputDir, $"{fileName}.pptx");

        return File.Exists(pptxPath) ? pptxPath : null;
    }

    /// <summary>
    /// Extracts all text from the first slide of a PPTX file
    /// </summary>
    private List<string> ExtractFirstSlideText(string pptxPath)
    {
        var texts = new List<string>();

        using var presentationDocument = PresentationDocument.Open(pptxPath, false);
        var presentationPart = presentationDocument.PresentationPart;
        if (presentationPart == null)
        {
            return texts;
        }

        // Get first slide
        var slideIdList = presentationPart.Presentation.SlideIdList;
        if (slideIdList == null)
        {
            return texts;
        }

        var firstSlideId = slideIdList.ChildElements.OfType<SlideId>().FirstOrDefault();
        if (firstSlideId?.RelationshipId?.Value == null)
        {
            return texts;
        }

        var slidePart = (SlidePart)presentationPart.GetPartById(firstSlideId.RelationshipId.Value);

        // Extract all text from the slide
        var textElements = slidePart.Slide.Descendants<A.Text>();
        foreach (var textElement in textElements)
        {
            if (!string.IsNullOrWhiteSpace(textElement.Text))
            {
                texts.Add(textElement.Text.Trim());
            }
        }

        return texts;
    }

    /// <summary>
    /// Parses hymn number and title from extracted text
    /// Format found in actual files:
    /// - Element 0: Title (e.g., "Bunul nostru Salvator")
    /// - Element 1: "Imnul" (literal text)
    /// - Elements 2+: Number parts (e.g., "7", "37" -> combine to "737")
    /// </summary>
    private (int hymnNumber, string title)? ParseHymnInfo(List<string> texts)
    {
        if (texts.Count < 3)
        {
            return null;
        }

        // First text is the title
        var title = texts[0].Trim();
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        // Collect all numeric text elements and combine them
        var numberParts = new List<string>();
        for (int i = 1; i < texts.Count; i++)
        {
            // Extract any digits from this element
            var digits = Regex.Match(texts[i], @"\d+");
            if (digits.Success)
            {
                numberParts.Add(digits.Value);
            }
        }

        if (numberParts.Count == 0)
        {
            return null;
        }

        // Combine all number parts (e.g., ["7", "37"] -> "737")
        var fullNumber = string.Join("", numberParts);
        if (!int.TryParse(fullNumber, out var hymnNumber))
        {
            return null;
        }

        return (hymnNumber, title);
    }

    /// <summary>
    /// Batch process multiple PPT files
    /// </summary>
    public async Task<Dictionary<string, (int hymnNumber, string title)?>> ExtractBatchAsync(
        IEnumerable<string> pptFiles,
        IProgress<(int current, int total, string fileName)>? progress = null)
    {
        var results = new Dictionary<string, (int hymnNumber, string title)?>();
        var fileList = pptFiles.ToList();
        var total = fileList.Count;
        var current = 0;

        foreach (var pptFile in fileList)
        {
            current++;
            var fileName = Path.GetFileName(pptFile);
            progress?.Report((current, total, fileName));

            var result = await ExtractHymnInfoAsync(pptFile);
            results[pptFile] = result;

            // Small delay to avoid overwhelming LibreOffice
            await Task.Delay(100);
        }

        return results;
    }

    /// <summary>
    /// Extracts all verses from a PPT file (slides 2+)
    /// </summary>
    public async Task<List<VerseData>> ExtractVersesAsync(string pptFilePath)
    {
        var verses = new List<VerseData>();

        if (!File.Exists(pptFilePath))
        {
            throw new FileNotFoundException($"PPT file not found: {pptFilePath}");
        }

        try
        {
            // Convert PPT to PPTX
            var pptxPath = await ConvertPptToPptxAsync(pptFilePath);
            if (pptxPath == null)
            {
                return verses;
            }

            // Extract verses from all slides (skip slide 1 = title)
            using var doc = PresentationDocument.Open(pptxPath, false);
            var presentationPart = doc.PresentationPart;

            if (presentationPart?.Presentation.SlideIdList == null)
            {
                return verses;
            }

            var slideIds = presentationPart.Presentation.SlideIdList.ChildElements.OfType<SlideId>().ToList();

            // Process slides 2+ (slide 1 is title)
            var displayOrder = 1;
            VerseData? lastChorus = null;

            for (int i = 1; i < slideIds.Count; i++)
            {
                var slideId = slideIds[i];
                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!.Value!);

                // Extract text from shapes, sorted by Y-position to ensure reading order
                var texts = new List<string>();

                if (slidePart.Slide.CommonSlideData?.ShapeTree != null)
                {
                    var shapes = slidePart.Slide.CommonSlideData.ShapeTree.Elements<Shape>()
                        .Select(s => new
                        {
                            Shape = s,
                            Y = s.ShapeProperties?.Transform2D?.Offset?.Y?.Value ?? 0
                        })
                        .OrderBy(x => x.Y);

                    foreach (var item in shapes)
                    {
                        var paragraphs = item.Shape.Descendants<A.Paragraph>();
                        foreach (var para in paragraphs)
                        {
                            var sb = new StringBuilder();
                            foreach (var child in para.Elements())
                            {
                                if (child is A.Run run)
                                {
                                    sb.Append(run.Text?.Text ?? "");
                                }
                                else if (child is A.Break)
                                {
                                    sb.AppendLine();
                                }
                            }
                            var line = sb.ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                texts.Add(line);
                            }
                        }
                    }
                }

                // Fallback: If no shapes found (e.g. weird layout), try generic descendant search
                if (!texts.Any())
                {
                    texts = slidePart.Slide.Descendants<A.Text>()
                        .Where(t => !string.IsNullOrWhiteSpace(t.Text))
                        .Select(t => t.Text.Trim())
                        .ToList();
                }

                if (!texts.Any())
                    continue;

                // Join text intelligently
                var slideContent = string.Join("\n", texts);

                // Parse the slide
                var extractedVerseParts = ParseSlideContent(slideContent);

                foreach (var versePart in extractedVerseParts)
                {
                    if (versePart.Label == "Refren")
                    {
                        // Deduplication logic for chorus:
                        // Only add if it's the first chorus encountered or if its content is different from the last one
                        if (lastChorus == null || !string.Equals(lastChorus.Content, versePart.Content, StringComparison.Ordinal))
                        {
                            versePart.DisplayOrder = displayOrder;
                            verses.Add(versePart);
                            lastChorus = versePart;
                            displayOrder++;
                        }
                        // Else: Skip duplicate chorus
                    }
                    else
                    {
                        versePart.DisplayOrder = displayOrder;
                        verses.Add(versePart);
                        displayOrder++;
                    }
                }
            }

            // Cleanup
            try
            {
                File.Delete(pptxPath);
            }
            catch { }

            // Post-processing: Assign sequential verse numbers to unnumbered verses
            var nextVerseNumber = 1;
            foreach (var verse in verses)
            {
                if (verse.VerseNumber == 0 && verse.Label != "Refren")
                {
                    verse.VerseNumber = nextVerseNumber++;
                }
                else if (verse.VerseNumber > 0)
                {
                    nextVerseNumber = Math.Max(nextVerseNumber, verse.VerseNumber + 1);
                }
            }

            // Final safety check: Ensure unique VerseNumbers
            var usedNumbers = new HashSet<int>();
            int safeNextNum = verses.Any(v => v.VerseNumber > 0) ? verses.Max(v => v.VerseNumber) + 1 : 1;

            foreach (var verse in verses)
            {
                if (verse.VerseNumber == 0)
                    continue;

                if (usedNumbers.Contains(verse.VerseNumber))
                {
                    verse.VerseNumber = safeNextNum++;
                }
                usedNumbers.Add(verse.VerseNumber);
            }

            return verses;
        }
        catch (Exception)
        {
            return verses;
        }
    }

    /// <summary>
    /// Parse slide content into one or more segments (Verse, Chorus, etc.)
    /// Handles complex slides with multiple sections (e.g. Refren -> Verse -> Refren).
    /// </summary>
    private List<VerseData> ParseSlideContent(string slideContent)
    {
        var results = new List<VerseData>();

        if (string.IsNullOrWhiteSpace(slideContent))
        {
            return results;
        }

        var lines = slideContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .ToList();

        if (!lines.Any())
            return results;

        // 1. Identify Section Headers
        var splitIndices = new List<int>();

        for (int i = 0; i < lines.Count; i++)
        {
            // Chorus header
            if (lines[i].StartsWith("Refren", StringComparison.OrdinalIgnoreCase) && lines[i].Length < 15)
            {
                splitIndices.Add(i);
            }
            // Verse header (numbered)
            else if (Regex.IsMatch(lines[i], @"^\d+\."))
            {
                splitIndices.Add(i);
            }
        }

        // Ensure start of content is covered
        if (!splitIndices.Contains(0))
        {
            splitIndices.Insert(0, 0);
        }

        splitIndices.Sort();

        // 2. Extract Segments
        for (int k = 0; k < splitIndices.Count; k++)
        {
            int startIndex = splitIndices[k];
            int endIndex = (k + 1 < splitIndices.Count) ? splitIndices[k + 1] : lines.Count;
            int count = endIndex - startIndex;

            if (count <= 0)
                continue;

            var segmentLines = lines.GetRange(startIndex, count);

            // 3. Process Segment
            ProcessSegment(segmentLines, results);
        }

        return results;
    }

    private void ProcessSegment(List<string> lines, List<VerseData> results)
    {
        if (!lines.Any())
            return;

        var firstLine = lines[0];

        // Case A: Chorus
        if (firstLine.StartsWith("Refren", StringComparison.OrdinalIgnoreCase) && firstLine.Length < 15)
        {
            // Strip "Refren" line
            var content = string.Join("\n", lines.Skip(1));
            if (!string.IsNullOrWhiteSpace(content))
            {
                results.Add(new VerseData
                {
                    VerseNumber = 0,
                    Content = content.Trim(),
                    Label = "Refren",
                    IsInline = (results.Count > 0 && results.Last().Label == null)
                });

                if (results.Count > 1 && results[^1].Label == "Refren")
                {
                    results.Last().IsInline = (results.Count > 1);
                }
            }
            return;
        }

        // Case B: Numbered Verse
        var match = Regex.Match(firstLine, @"^(\d+)\.");
        if (match.Success)
        {
            var verseNum = int.Parse(match.Groups[1].Value);
            var content = string.Join("\n", lines);
            results.Add(new VerseData
            {
                VerseNumber = verseNum,
                Content = content.Trim(),
                Label = null,
                IsContinuation = false
            });
            return;
        }

        // Case C: Continuation / Unnumbered
        var unnumberedContent = string.Join("\n", lines);
        results.Add(new VerseData
        {
            VerseNumber = 0,
            Content = unnumberedContent.Trim(),
            Label = null,
            IsContinuation = true
        });
    }
}
