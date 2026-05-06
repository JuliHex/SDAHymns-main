# Spec 013: Export Functionality

**Status:** 📋 Planned
**Created:** 2025-12-26
**Dependencies:** 006-enhanced-control-window.md, 007-display-profiles.md

## Overview

Implement a flexible export system allowing users to generate PDF documents for printing and image files for digital broadcasting. This enables the content of the application to be used outside the app itself, supporting musicians, elderly members (via large print), and video production teams.

**Goal:** Provide high-quality, formatted exports of hymn content in both document (PDF) and visual (PNG) formats.

## Goals

1.  **PDF Export:** Generate printable PDFs for single or multiple selected hymns.
2.  **Image Export:** Render hymn slides as high-resolution images (WYSIWYG).
3.  **Batch Processing:** Allow exporting multiple selected hymns at once.
4.  **Formatting:** Preserve verse/chorus structure with proper indentation and styling in PDFs.
5.  **Accessibility:** "Large Print" option for PDF exports.
6.  **Broadcasting:** Transparent background option for image exports (for OBS/video overlays).

## Architecture

### 1. Export Options Model

**File:** `src/SDAHymns.Core/Services/Export/ExportOptions.cs`

```csharp
public class ExportOptions
{
    public ExportFormat Format { get; set; } // PDF or Images
    public List<int> HymnIds { get; set; } = new();
    
    // PDF Specific
    public bool LargePrint { get; set; } = false;
    public bool IncludeChords { get; set; } = false; // Future use
    public PaperSize PaperSize { get; set; } = PaperSize.A4;
    
    // Image Specific
    public bool TransparentBackground { get; set; } = false;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int ProfileId { get; set; } // Which Display Profile to use for rendering
}

public enum ExportFormat { Pdf, PngImages }
public enum PaperSize { A4, Letter }
```

### 2. Export Service

**File:** `src/SDAHymns.Core/Services/Export/ExportService.cs`

We will use **QuestPDF** for generating documents due to its modern fluent API and layout engine.

```csharp
public interface IExportService
{
    Task ExportHymnsToPdfAsync(ExportOptions options, string outputPath);
    Task ExportHymnsToImagesAsync(ExportOptions options, string outputDirectory);
}
```

### 3. PDF Layout Strategy (QuestPDF)

The PDF generator will iterate through selected hymns and create a continuous document (or one file per hymn, based on user choice).

**Visual Style:**
*   **Header:** Hymn Number (Left), Category (Right).
*   **Title:** Bold, Centered, Large.
*   **Verses:** Numbered, Left Aligned.
*   **Chorus:** Italicized, Indented (2cm), labeled "Refren:".
*   **Footer:** Page X of Y.

**Code Snippet Concept (QuestPDF):**
```csharp
.Column(column => 
{
    column.Item().Text(hymn.Title).FontSize(24).Bold();
    
    foreach(var verse in hymn.Verses)
    {
        if (verse.IsChorus)
        {
             column.Item().PaddingLeft(20).Text(verse.Content).Italic();
        }
        else
        {
             column.Item().Text($"{verse.Number}. {verse.Content}");
        }
        column.Item().Height(10); // Spacing
    }
});
```

### 4. Image Rendering Strategy (Avalonia)

To ensure the images look *exactly* like the slides (WYSIWYG), we won't manually draw text on bitmaps. Instead, we will leverage Avalonia's `RenderTargetBitmap`.

1.  **Invisible Window:** Create an instance of `DisplayWindow` off-screen (or a specific `ExportControl`).
2.  **Apply Profile:** Apply the selected `DisplayProfile` (Spec 007) to this control.
3.  **Transparency:** If `TransparentBackground` is requested, set the Window/Control background to `Brushes.Transparent`.
4.  **Capture:** Iterate through every verse of every selected hymn:
    *   Bind verse data to control.
    *   Force layout update (`Measure` / `Arrange`).
    *   Render to `RenderTargetBitmap`.
    *   Save to stream as PNG.

## UI Implementation

### Export Dialog

A modal window triggered from the main menu or context menu.

**File:** `src/SDAHymns.Desktop/Views/Dialogs/ExportDialog.axaml`

**Layout:**
*   **Left Panel (Preview):** A thumbnail preview of what the output will look like.
*   **Right Panel (Settings):**
    *   **Format:** Radio Buttons [PDF Document] [Slide Images]
    *   **Selection:** "3 Hymns Selected" (ReadOnly summary)
    *   **PDF Options:**
        *   [ ] Large Print (18pt+)
        *   [ ] One Hymn Per Page
    *   **Image Options:**
        *   [ ] Transparent Background
        *   Resolution: [1920x1080 ▼]
    *   **Action:** [Export Button] (Opens Save File Dialog)

## User Workflows

### Scenario A: Printing for Elderly Member
1.  User searches for "Hymn 20" and "Hymn 45".
2.  User selects both in the list (Ctrl+Click).
3.  Right-click -> "Export/Print".
4.  Dialog opens. User selects **PDF** and checks **Large Print**.
5.  Click Export -> Save as `Hymns_Large.pdf`.
6.  Result: A clean, high-contrast PDF with 20pt font.

### Scenario B: Exporting for Video Editor
1.  User selects "Hymn 99".
2.  Right-click -> "Export/Print".
3.  User selects **Slide Images**.
4.  Checks **Transparent Background**.
5.  Click Export -> Selects folder.
6.  App generates:
    *   `099_01_Verse1.png`
    *   `099_02_Chorus.png`
    *   `099_03_Verse2.png`
7.  Result: PNG files with alpha transparency, white text (if using OBS profile), ready for video overlay.

## Acceptance Criteria

- [ ] **Dependency:** QuestPDF added to Core project (Community License enabled).
- [ ] **PDF Content:** Exported PDF contains correct title, number, and full text for all selected hymns.
- [ ] **PDF Formatting:** Choruses are visually distinct (indented/italic) from verses.
- [ ] **Image Rendering:** Image export matches the visual style of the `DisplayWindow`.
- [ ] **Transparency:** Image export supports transparent backgrounds correctly.
- [ ] **Batching:** Can handle exporting 10+ hymns in a single operation without crashing.
- [ ] **UI:** Export Dialog allows configuration of all specified options.

## Future Enhancements
-   **PPTX Export:** Generate editable PowerPoint files.
-   **Chord Sheets:** If we add chord data later, export "Lead Sheets".
-   **Booklet Imposition:** Reorder PDF pages for folding into a booklet.
