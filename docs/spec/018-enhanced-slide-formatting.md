# Spec 018: Enhanced Hymn Slide Formatting

**Status:** 📋 Planned
**Created:** 2025-12-26
**Dependencies:** 007-display-profiles.md, 017-ui-ux-overhaul.md

## Overview

Improve the hymn slide display formatting to create professional, visually appealing, and easy-to-read presentations. The current display window shows verses as plain text blocks, but lacks advanced formatting features that make slides more readable and aesthetically pleasing.

**Goal:** Transform hymn slides into professionally formatted presentations with better typography, layout, and visual hierarchy.

## Current State

**What Works:**
- Basic verse display with configurable fonts/colors (Spec 007)
- Auto-scaling text to fit screen
- Display profiles for different scenarios
- Background images and opacity

**Limitations:**
1. **Verse Numbers:** Not visually distinct from verse text
2. **Chorus Formatting:** Choruses look identical to verses
3. **Long Verses:** No smart line breaking or hyphenation
4. **Typography:** Simple single-column layout
5. **Visual Hierarchy:** No emphasis on key words/phrases
6. **Spacing:** Uniform spacing between all lines
7. **Alignment:** Only left/center/right (no justified or smart alignment)

## Goals

1. **Verse Number Styling:** Make verse numbers visually distinct
2. **Chorus Differentiation:** Style choruses differently (indent, italics, color)
3. **Smart Text Layout:** Better line breaking and hyphenation
4. **Typography Enhancements:** Line spacing, kerning, drop caps
5. **Multi-Column Layout:** Support 2-column layout for short verses
6. **Emphasis Styling:** Bold/italics for specific words (if marked)
7. **Slide Metadata:** Show hymn number/title in corner (optional)
8. **Transition Effects:** Smooth transitions between verses

## Design Specifications

### 1. Verse Number Formatting

**Current:** `1. Verse text here...`

**Enhanced Options:**

**Style A: Badge Style**
```
┌─────┐
│  1  │  Verse text here starting
└─────┘  on the same line or next line
```
- Small colored box with number
- Configurable: Same line or separate line
- Accent color background

**Style B: Large Decorative**
```
  1  Verse text here starting
     on the second line, first line
     reserved for large number
```
- Large number (2-3x verse text size)
- Muted color
- Number on separate line

**Style C: Inline Emphasized**
```
1. Verse text here with number
   in same line but styled differently
```
- Number in accent color
- Bold or different font
- Inline with text

**Profile Setting:**
```csharp
public enum VerseNumberStyle
{
    None,           // No verse numbers shown
    InlinePlain,    // "1. Text" (current)
    InlineBold,     // "1. Text" (bold number)
    Badge,          // Badge box with number
    LargeDecorative,// Large number on own line
    Superscript     // Small superscript number
}

public class DisplayProfile
{
    // ... existing properties

    public VerseNumberStyle VerseNumberStyle { get; set; }
    public bool VerseNumberSeparateLine { get; set; }
    public string VerseNumberColor { get; set; }  // Hex color
    public int VerseNumberSize { get; set; }      // Font size override
}
```

### 2. Chorus Formatting

**Visual Differentiation:**

**Option 1: Indentation**
```
1. First verse text here
   continuing on second line

   Refrain: Chorus text here
            indented from left margin
            with lighter color

2. Second verse text here
   back to normal margin
```

**Option 2: Italics**
```
1. First verse text here
   in normal style

   Refrain: Chorus text here
            in italic style
            same alignment

2. Second verse text
```

**Option 3: Background Highlight**
```
1. First verse text

   ┌──────────────────────┐
   │ Refrain:             │
   │ Chorus with subtle   │
   │ background color     │
   └──────────────────────┘

2. Second verse
```

**Profile Settings:**
```csharp
public enum ChorusStyle
{
    SameAsVerse,        // No differentiation
    Indented,           // Indent left margin
    Italic,             // Italic font style
    ColoredText,        // Different text color
    BackgroundHighlight,// Background panel
    Combined            // Multiple styles together
}

public class DisplayProfile
{
    // ... existing properties

    public ChorusStyle ChorusStyle { get; set; }
    public int ChorusIndentAmount { get; set; }      // Pixels
    public string ChorusTextColor { get; set; }       // Hex color
    public string ChorusBackgroundColor { get; set; } // Hex color
    public bool ChorusItalic { get; set; }
    public bool ShowChorusLabel { get; set; }         // "Refrain:" prefix
}
```

### 3. Smart Text Layout

**Line Breaking:**
- Avoid orphans (single word on last line)
- Avoid widows (single line in new column)
- Prefer breaking at punctuation
- Hyphenation for long words (Romanian language rules)

**Implementation:**
```csharp
public class TextLayoutEngine
{
    public FormattedText LayoutVerse(string text, LayoutConstraints constraints)
    {
        // 1. Split into words
        // 2. Apply hyphenation rules (Romanian)
        // 3. Calculate optimal line breaks
        // 4. Apply widow/orphan prevention
        // 5. Return formatted text with line positions
    }
}
```

**Profile Settings:**
```csharp
public class DisplayProfile
{
    // ... existing properties

    public bool EnableSmartLineBreaking { get; set; }
    public bool EnableHyphenation { get; set; }
    public int MinimumWordsPerLine { get; set; }      // Default: 2
    public int PreferredWordsPerLine { get; set; }    // Default: 6-8
}
```

### 4. Typography Enhancements

**Line Height (Leading):**
- Current: Fixed line height
- Enhanced: Configurable line height multiplier (1.0 - 2.5)
- Default: 1.5 for better readability

**Letter Spacing (Kerning):**
- Adjustable tracking for better fit
- Range: -50 to +200 (pixels)

**Paragraph Spacing:**
- Space between verse number and text
- Space between verses/choruses

**Profile Settings:**
```csharp
public class DisplayProfile
{
    // ... existing properties

    public double LineHeightMultiplier { get; set; }  // 1.0 - 2.5
    public int LetterSpacing { get; set; }            // -50 to +200
    public int ParagraphSpacing { get; set; }         // Pixels
    public int VerseSpacing { get; set; }             // Pixels between verses
}
```

### 5. Multi-Column Layout

**Use Case:** When verses are short, use 2-column layout to fill screen

**Layout:**
```
┌────────────────────────────────────┐
│  1. First verse      2. Second     │
│     text here           verse text │
│                         here        │
│                                     │
│     Refrain:            Refrain:   │
│     Chorus text         (repeated) │
│                                     │
│  3. Third verse      4. Fourth     │
│     text here           verse      │
└────────────────────────────────────┘
```

**Profile Settings:**
```csharp
public enum ColumnLayout
{
    SingleColumn,       // Always one column
    TwoColumn,          // Always two columns
    Auto                // Auto-detect based on verse length
}

public class DisplayProfile
{
    // ... existing properties

    public ColumnLayout ColumnLayout { get; set; }
    public int AutoColumnThreshold { get; set; }      // Chars per verse for auto
    public int ColumnGap { get; set; }                // Pixels between columns
}
```

### 6. Slide Metadata Display

**Optional Corner Info:**

**Top-Left:**
```
┌─────────────────────────┐
│ Hymn 123                │
│                         │
│   Verse text here       │
│                         │
└─────────────────────────┘
```

**Top-Right:**
```
┌─────────────────────────┐
│           Imnuri Crestine│
│                         │
│   Verse text here       │
│                         │
└─────────────────────────┘
```

**Bottom-Right:**
```
┌─────────────────────────┐
│                         │
│   Verse text here       │
│                         │
│               Verse 2/4 │
└─────────────────────────┘
```

**Profile Settings:**
```csharp
public enum MetadataPosition
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public class DisplayProfile
{
    // ... existing properties

    public bool ShowHymnNumber { get; set; }
    public bool ShowHymnTitle { get; set; }
    public bool ShowCategory { get; set; }
    public bool ShowVerseIndicator { get; set; }       // "Verse 2/4"
    public MetadataPosition MetadataPosition { get; set; }
    public int MetadataFontSize { get; set; }
    public string MetadataColor { get; set; }
    public double MetadataOpacity { get; set; }
}
```

### 7. Transition Effects

**Between Verses:**

**Fade:**
- Old verse fades out (200ms)
- New verse fades in (200ms)

**Slide:**
- Old verse slides left and fades
- New verse slides in from right

**Dissolve:**
- Cross-fade with slight blur

**Cut:**
- Instant change (current behavior)

**Profile Settings:**
```csharp
public enum VerseTransition
{
    None,           // Instant
    Fade,           // Fade in/out
    Slide,          // Slide left/right
    Dissolve,       // Cross-fade with blur
    FadeToBlack     // Fade to black, then new verse
}

public class DisplayProfile
{
    // ... existing properties

    public VerseTransition VerseTransition { get; set; }
    public int TransitionDuration { get; set; }        // Milliseconds (100-1000)
}
```

## Implementation Plan

### Phase 1: Profile Model Extensions (1 hour)
1. Add new properties to `DisplayProfile` model
2. Create migration: `AddEnhancedSlideFormatting`
3. Update seed data with default values
4. Update `DisplayProfileService` to handle new properties

### Phase 2: Verse Number Styling (2 hours)
1. Create `VerseNumberRenderer` component
2. Implement 5 verse number styles
3. Add style selector in ProfileEditorWindow
4. Update DisplayWindow to use new renderer
5. Test all styles with Romanian text

### Phase 3: Chorus Formatting (2 hours)
1. Extend `Verse` model to detect chorus type
2. Create `ChorusStyleRenderer` component
3. Implement 5 chorus styles
4. Add chorus style controls in ProfileEditor
5. Update DisplayWindow rendering logic

### Phase 4: Typography & Layout (3 hours)
1. Implement line height multiplier
2. Implement letter spacing
3. Implement paragraph spacing
4. Create `TextLayoutEngine` for smart line breaking
5. Add Romanian hyphenation rules
6. Test with various verse lengths

### Phase 5: Multi-Column Layout (2 hours)
1. Create `MultiColumnLayoutEngine`
2. Implement auto-detection logic
3. Add column layout controls in ProfileEditor
4. Update DisplayWindow to support columns
5. Test with short/long verses

### Phase 6: Slide Metadata (1 hour)
1. Create `SlideMetadataOverlay` component
2. Implement 4 corner positions
3. Add metadata controls in ProfileEditor
4. Update DisplayWindow with overlay

### Phase 7: Transition Effects (2 hours)
1. Create `VerseTransitionService`
2. Implement 5 transition types using Avalonia animations
3. Add transition controls in ProfileEditor
4. Update DisplayWindow verse navigation
5. Test transition smoothness

### Phase 8: Profile Editor UI (2 hours)
1. Add "Slide Formatting" section to ProfileEditor
2. Create collapsible sections for each feature
3. Add preview panel showing live changes
4. Add preset buttons for common combinations

### Phase 9: Testing & Polish (1 hour)
1. Test all combinations of settings
2. Verify Romanian text rendering in all styles
3. Test with real hymns on projector
4. Document best practices in tooltip hints
5. Create 2-3 new preset profiles showcasing features

## Acceptance Criteria

**Verse Numbers:**
- [ ] 5 verse number styles available
- [ ] Verse numbers can be hidden
- [ ] Number color/size configurable
- [ ] Badge style renders correctly
- [ ] Large decorative style readable

**Chorus Formatting:**
- [ ] Choruses visually differentiated from verses
- [ ] Indentation style works correctly
- [ ] Italic style applies properly
- [ ] Background highlight renders with opacity
- [ ] "Refrain:" label optional

**Typography:**
- [ ] Line height adjustable (1.0 - 2.5)
- [ ] Letter spacing adjustable
- [ ] Paragraph spacing configurable
- [ ] Smart line breaking prevents orphans/widows
- [ ] Romanian hyphenation works correctly

**Layout:**
- [ ] Single-column layout (default)
- [ ] Two-column layout renders correctly
- [ ] Auto-detect column count based on verse length
- [ ] Columns balanced (similar height)

**Metadata:**
- [ ] Hymn number displays in corner
- [ ] Category displays when enabled
- [ ] Verse indicator (2/4) shows correctly
- [ ] Metadata position configurable
- [ ] Metadata opacity configurable

**Transitions:**
- [ ] Fade transition smooth (200ms)
- [ ] Slide transition directional
- [ ] Dissolve transition with blur
- [ ] Transition duration configurable
- [ ] Transitions can be disabled

**Profiles:**
- [ ] All new properties saved to database
- [ ] ProfileEditor UI has new controls
- [ ] Live preview shows formatting changes
- [ ] Export/import includes new properties
- [ ] New presets showcase features

## Profile Preset Examples

### Preset 1: "Modern Worship"
- Verse Numbers: Badge style (accent color)
- Chorus: Indented + italic + lighter color
- Line Height: 1.6
- Transition: Fade
- Metadata: Verse indicator bottom-right

### Preset 2: "Classic Hymnal"
- Verse Numbers: Large decorative
- Chorus: Background highlight
- Line Height: 1.4
- Transition: None
- Metadata: Hymn number top-left

### Preset 3: "Compact Service"
- Verse Numbers: Inline bold
- Chorus: Same as verse
- Layout: Two-column auto
- Line Height: 1.3
- Transition: Slide

### Preset 4: "OBS Stream Enhanced"
- Verse Numbers: Superscript
- Chorus: Colored text with outline
- Transparent background (existing)
- Metadata: Category top-right
- Transition: Dissolve

## Technical Considerations

### Performance
- Pre-render verses when hymn loads
- Cache formatted layouts
- Use GPU-accelerated transitions
- Avoid layout recalculations during transitions

### Romanian Text
- Test hyphenation with diacritics (ă, â, î, ș, ț)
- Ensure all fonts support Romanian characters
- Verify letter spacing doesn't break diacritics

### Accessibility
- Maintain high contrast in all styles
- Ensure metadata doesn't obstruct main text
- Keep transitions under 500ms (avoid motion sickness)

## Future Enhancements

- **Live Captions:** Synchronized word highlighting during audio playback
- **Karaoke Mode:** Word-by-word highlighting following timing map
- **Custom Backgrounds:** Per-hymn background images
- **Animation Library:** More transition effects (zoom, rotate, etc.)
- **Text Effects:** Shadows, glows, gradients for special occasions
- **Responsive Font Sizing:** Auto-adjust based on verse length

## Related Specs

- **Spec 007:** Display Profiles (foundation for this spec)
- **Spec 017:** UI/UX Overhaul (consistent visual language)
- **Spec 011:** Audio Playback (potential for synchronized highlighting)
