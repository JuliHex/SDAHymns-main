# Spec 017: UI/UX Overhaul & Modernization

**Status:** 📋 Planned
**Created:** 2025-12-26
**Updated:** 2025-12-26 (Added RemoteWidget/DisplayWindow polish requirements)
**Dependencies:** All previous specs (foundation for visual refinement), Spec 019 (RemoteWidget provides approved color scheme)

## Overview

Revamp the entire application UI/UX to create a modern, polished, professional appearance. The current interface is functional but lacks visual polish, consistency, and modern design patterns. This spec focuses on transforming the application into a visually appealing, intuitive, and delightful user experience.

**Goal:** Transform the functional interface into a modern, beautiful, and professional application worthy of church presentation software.

**Note:** Spec 019 (Compact Remote Widget) has been implemented with a beautiful color scheme that has been approved by the user. This spec will apply that color scheme throughout the application and polish all windows including RemoteWidget and DisplayWindow.

## Current Issues

1. **Visual Design:**
   - Basic, unpolished appearance
   - Inconsistent spacing and alignment
   - Lack of visual hierarchy
   - No animations or transitions
   - Colors feel generic (standard dark theme)

2. **Layout:**
   - Cramped controls in some areas
   - Inconsistent padding/margins
   - No breathing room between elements
   - Search results feel cluttered

3. **Typography:**
   - Limited font usage
   - No emphasis or hierarchy
   - Small font sizes in some areas

4. **Icons & Imagery:**
   - Text-only buttons (no icons)
   - No visual cues for actions
   - Missing app icon/branding

5. **Interactions:**
   - No feedback animations
   - Instant state changes feel jarring
   - No loading states
   - No success/error animations

## Goals

1. **Modern Design Language:** Implement a cohesive, modern design system
2. **Visual Polish:** Add smooth animations, transitions, and micro-interactions
3. **Improved Layout:** Better spacing, alignment, and visual hierarchy
4. **Icon System:** Comprehensive icon set for all actions
5. **Theme System:** Refined dark/light themes with accent colors
6. **Responsive Design:** Adapt layouts to different window sizes
7. **Accessibility:** Maintain keyboard navigation while improving visuals
8. **Branding:** Create app identity with logo and color palette

## Design System

### Color Palette

**Dark Theme (Default):**
```
Primary:     #6366F1 (Indigo 500) - Accent color for CTAs
Background:  #1E1E2E (Dark slate) - Main background
Surface:     #2A2A3C (Slightly lighter) - Cards, panels
Border:      #3E3E52 (Subtle borders)
Text:        #E5E7EB (Off-white) - Primary text
TextMuted:   #9CA3AF (Gray 400) - Secondary text
Success:     #10B981 (Green 500)
Warning:     #F59E0B (Amber 500)
Error:       #EF4444 (Red 500)
```

**Light Theme:**
```
Primary:     #6366F1 (Indigo 500)
Background:  #F9FAFB (Gray 50)
Surface:     #FFFFFF (White)
Border:      #E5E7EB (Gray 200)
Text:        #1F2937 (Gray 800)
TextMuted:   #6B7280 (Gray 500)
Success:     #10B981
Warning:     #F59E0B
Error:       #EF4444
```

### Typography Scale

```
Display:  32px / Bold   (Main window title)
Heading1: 24px / SemiBold (Section headers)
Heading2: 20px / SemiBold (Subsection headers)
Heading3: 18px / Medium  (Card titles)
Body:     14px / Regular (Main text)
Small:    12px / Regular (Metadata, labels)
Caption:  11px / Regular (Status bar, hints)
```

### Spacing System

```
xs:  4px
sm:  8px
md:  16px
lg:  24px
xl:  32px
2xl: 48px
```

### Border Radius

```
sm: 4px  (Small buttons, inputs)
md: 8px  (Cards, panels)
lg: 12px (Major containers)
xl: 16px (Modal dialogs)
```

### Icon System

Use **Fluent UI Icons** or **Material Design Icons** (2,000+ icons available):
- 20px for inline icons (buttons, labels)
- 24px for toolbar icons
- 32px for feature cards

**Key Icons Needed:**
- Search: `Search` 🔍
- Play/Pause: `Play`, `Pause` ▶️⏸️
- Navigation: `ChevronLeft`, `ChevronRight`, `ChevronUp`, `ChevronDown`
- Actions: `Star`, `StarFilled`, `Add`, `Delete`, `Edit`, `Settings`
- Status: `CheckCircle`, `ErrorCircle`, `InfoCircle`
- Display: `ScreenShare`, `Monitor`, `Fullscreen`
- Audio: `Speaker`, `VolumeUp`, `VolumeDown`, `VolumeMute`

## UI Components

### 1. MainWindow Redesign

**Layout Structure:**
```
┌─────────────────────────────────────────────────────────┐
│ ⚙ SDAHymns                          [Profile ▾] [●●●]  │ ← Toolbar
├─────────────────────────────────────────────────────────┤
│ 🔍 Search hymns...                                   │ ← Search Bar (larger)
├─────────────────────────────────────────────────────────┤
│ 📌 Recent Hymns                                         │
│  [1] Hymn 123   [2] Hymn 45   [3] Hymn 67  ...        │ ← Chip-style buttons
├─────────────────────────────────────────────────────────┤
│ ┌─────────────────┐  ┌────────────────────────────────┐│
│ │ Search Results  │  │ Now Playing: Hymn 123          ││
│ │                 │  │ ────────────────────────────── ││ ← Two-column layout
│ │ [List of hymns] │  │                                ││
│ │                 │  │ [Large verse preview]          ││
│ │                 │  │                                ││
│ │                 │  │ ◀  Verse 1/3  ▶               ││
│ │                 │  │                                ││
│ │                 │  │ ▶ 00:45 ─────●──── 02:30      ││
│ └─────────────────┘  └────────────────────────────────┘│
├─────────────────────────────────────────────────────────┤
│ Ready • Press F1 for shortcuts        [Display: Off]  │ ← Status bar
└─────────────────────────────────────────────────────────┘
```

**Improvements:**
- Larger search bar with icon
- Recent hymns as horizontal chips (not cramped buttons)
- Two-column layout with clear separation
- Verse preview with larger text
- Audio controls integrated into preview card
- Visual status indicators (colored dots)

### 2. Card-Based Layout

Wrap key sections in cards with elevation:
```xml
<Border CornerRadius="8" Background="{StaticResource SurfaceColor}"
        BoxShadow="0 2 8 0 #00000020">
    <StackPanel Padding="16">
        <!-- Content -->
    </StackPanel>
</Border>
```

### 3. Modern Buttons

**Primary Button (CTA):**
- Background: Primary color
- Text: White
- Border radius: 6px
- Height: 36px
- Hover: Lighten 10%
- Active: Darken 5%

**Secondary Button:**
- Background: Transparent
- Border: 1px Primary color
- Text: Primary color
- Same hover/active states

**Icon Button:**
- 32x32px clickable area
- Icon centered
- Hover: Background overlay

### 4. Smooth Animations

**Transitions:**
- Property changes: 150ms ease-out
- Hover states: 100ms ease-in-out
- Page transitions: 250ms ease-in-out
- Modal open: 200ms scale + fade

**Micro-interactions:**
- Button click: Scale down to 0.95 for 50ms
- Star favorite: Scale up to 1.2, then back to 1.0
- Search results appear: Fade in + slide up
- Audio progress: Smooth interpolation

**Loading States:**
- Skeleton screens for hymn loading
- Spinner for long operations
- Progress bars for downloads

### 5. Improved Search UI

**Search Bar:**
- Height: 44px (larger touch target)
- Icon on left (🔍)
- Placeholder with hint text
- Clear button on right (×) when text present
- Focus: Border glow with Primary color

**Search Results:**
- Card-based list items
- Hover: Background highlight
- Selected: Border + background
- Metadata in muted text (category, verses count)

### 6. Settings Window Redesign

**Tab Navigation:**
- Vertical sidebar tabs (icons + text)
- Active tab highlighted
- Smooth transition between tabs

**Form Controls:**
- Labels above inputs (not inline)
- Helper text below inputs
- Validation feedback (red/green borders)

### 7. Toast Notifications

Replace status bar messages with toast notifications:
- Slide in from top-right
- Auto-dismiss after 3 seconds
- Success/Warning/Error colors
- Dismissible with × button

## Implementation Plan

### Phase 1: Design System Foundation (2-3 hours)
1. Create `Styles/Colors.axaml` - Color resources
2. Create `Styles/Typography.axaml` - Text styles
3. Create `Styles/Spacing.axaml` - Margin/padding constants
4. Create `Styles/Controls.axaml` - Reusable control styles
5. Create `Styles/Animations.axaml` - Transition definitions
6. Update `App.axaml` to merge all style dictionaries

### Phase 2: Icon Integration (1 hour)
1. Add `FluentIcons.Avalonia` or `Material.Icons.Avalonia` NuGet package
2. Create icon constants/helpers
3. Replace text-only buttons with icon+text or icon-only buttons

### Phase 3: MainWindow Redesign (3-4 hours)
1. Implement two-column layout
2. Add card wrappers around sections
3. Style search bar with icon
4. Redesign recent hymns bar (horizontal chips)
5. Improve verse preview card
6. Add smooth transitions

### Phase 4: Other Windows (2-3 hours)
1. Redesign `DisplayWindow.axaml` - Already has good profiles
2. Redesign `SettingsWindow.axaml` - Vertical tabs
3. Redesign `ShortcutsWindow.axaml` - Better table layout
4. Redesign `ProfileEditorWindow.axaml` - Card-based sections

### Phase 5: Animations & Polish (2 hours)
1. Add property change transitions
2. Add hover/active states
3. Add loading states
4. Add toast notification system
5. Add fade-in animations for window opens

### Phase 6: Theme System (1-2 hours)
1. Implement theme switcher (Dark/Light)
2. Add theme setting to AppSettings
3. Add theme toggle in Settings window
4. Test all windows in both themes

## Acceptance Criteria

**Visual Design:**
- [ ] Consistent color palette across entire application
- [ ] All interactive elements have hover/active states
- [ ] Clear visual hierarchy (size, weight, color)
- [ ] Proper spacing (no cramped or cluttered areas)
- [ ] Icons for all major actions

**Layout:**
- [ ] Two-column layout in MainWindow
- [ ] Card-based sections with elevation/shadows
- [ ] Consistent padding/margins throughout
- [ ] Responsive to window resizing

**Animations:**
- [ ] Smooth property transitions (150ms standard)
- [ ] Button click feedback
- [ ] Page/tab transitions
- [ ] Toast notifications slide in/out
- [ ] Loading states for long operations

**Typography:**
- [ ] Consistent font sizes across app
- [ ] Clear hierarchy (headings vs body)
- [ ] Readable text (proper contrast ratios)
- [ ] Romanian diacritics render correctly

**Themes:**
- [ ] Dark theme (default) looks modern and polished
- [ ] Light theme available and looks equally polished
- [ ] Theme persists across app restarts
- [ ] All windows support both themes

**Accessibility:**
- [ ] Keyboard navigation still fully functional
- [ ] Focus indicators visible
- [ ] All interactive elements have 44px minimum touch target
- [ ] Sufficient color contrast (WCAG AA)

## Technical Considerations

### Avalonia Styling

Use Avalonia's powerful styling system:
```xml
<Style Selector="Button.primary">
    <Setter Property="Background" Value="{StaticResource PrimaryColor}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="CornerRadius" Value="6"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="Transitions">
        <Transitions>
            <BrushTransition Property="Background" Duration="0:0:0.1"/>
        </Transitions>
    </Setter>
</Style>
<Style Selector="Button.primary:pointerover">
    <Setter Property="Background" Value="{StaticResource PrimaryColorHover}"/>
</Style>
```

### Icon Libraries

**Option 1: FluentIcons.Avalonia**
```bash
dotnet add package FluentIcons.Avalonia
```

**Option 2: Material.Icons.Avalonia**
```bash
dotnet add package Material.Icons.Avalonia
```

### Animation Performance

- Use GPU-accelerated properties (Opacity, Transform)
- Avoid animating Width/Height (use ScaleTransform)
- Keep animations under 300ms for snappiness

## Future Enhancements

- **Custom Branding:** Allow churches to customize app logo/colors
- **Window Chrome:** Custom title bar with app branding
- **Glassmorphism:** Frosted glass effects for overlays
- **Sound Effects:** Subtle audio feedback for actions
- **Onboarding:** First-run tutorial with animations
- **Easter Eggs:** Celebratory animations for special hymns

## Related Specs

- **Spec 018:** Enhanced Hymn Slide Formatting (display window improvements)
- **Spec 015:** Remote Control API (mobile UI also needs polish)
- All existing specs benefit from improved visual design
