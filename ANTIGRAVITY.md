# ANTIGRAVITY.md

This file provides guidance to Antigravity when working with code in this repository.

## Project Overview

SDAHymns is a modern, cross-platform desktop application for displaying hymns in church services, built for the Romanian Seventh Day Adventist Church. 

**Origin:** This project is a complete rewrite from scratch of the legacy SDAHymns application, based on the work from the [ThorSPB/SDAHymns](https://github.com/ThorSPB/SDAHymns) repository. It focuses on improved UX, streaming optimization, and modern broadcast capabilities.

## Technology Stack

- **Language:** C# (.NET 10)
- **UI Framework:** Avalonia UI 11.3.x (cross-platform XAML-based)
- **MVVM:** CommunityToolkit.Mvvm 8.4.0
- **Database:** SQLite with Entity Framework Core 10
- **Audio:** NAudio 2.2.1
- **Updates:** Velopack 0.0.1298 (via GitHub Releases)

## Core Features

- **Dual-Window System:** 
  - **RemoteWidget:** Compact, widget-style controller (Default).
  - **DisplayWindow:** Full-screen projection display with dynamic styling.
- **Enhanced Search:** Real-time, diacritic-insensitive search.
- **Display Profiles:** Comprehensive styling system (fonts, colors, backgrounds).
- **English UI:** Fully translated interface and projection labels.

## Recent Enhancements (Modern Rewrite)

Since the original fork, the following professional features have been implemented:

### 1. Smart Projection Layouts
- **Context-Aware Styling**: The system automatically switches between **Inline Layout** for numbered verses (1. Lyrics...) and **Above/Left Layout** for the Chorus (Refren), providing a premium, book-like appearance.
- **Smart Label Extraction**: If the database is missing labels, the system automatically extracts numeric prefixes from the lyrics to ensure gold-colored numbering is always present.
- **Improved Scaling**: Removed fixed width constraints to ensure long lines in hymns (like "Exploratori") are never cut off, regardless of screen aspect ratio.

### 2. Advanced Remote Control
- **Full Metadata Display**: The Remote Widget now shows the full hymn number and title (e.g., "362 Tată, azi, Te rugăm").
- **Live Slide Counter**: Added a real-time indicator showing current slide type and position (e.g., "Refren (2/8)").
- **Auto-Sync Engine**: The Remote Widget is now perfectly synchronized with the Main Window and Hotkeys; changing a slide anywhere updates the remote counter instantly.
- **State Cleanup**: Automatically resets to "No hymn loaded" when a presentation is finished or closed.

### 3. Workflow & Stability
- **One-Tap Navigation**: Optimized the loading engine to eliminate the "double-tap" lag when advancing from the title slide.
- **Unified Closure**: Closing the Lyrics display, the Presenter View, or finishing the last slide will automatically shut down the entire projection system for a clean broadcast exit.
- **Code Health**: Fixed all `MVVMTK0034` backing field warnings and optimized logging performance (`CA1873`).

## Developer Commands

```bash
# Setup & Build
make setup           # Restore + build + install git hooks
make build           # Build solution (Debug)
make run             # Run desktop app

# Testing & Quality
make test            # Run all tests
make format          # Auto-format code

# Database
make db-update       # Apply EF Core migrations
```

## Important Notes

- **UTF-8 Encoding:** Essential for Romanian diacritics (ă, â, î, ș, ț).
- **Multi-Monitor:** Always test projection on secondary displays.
- **Gold Styling:** Hymn numbers and "Chorus" labels use Gold (#FFD700) by default in the UI.
- **Translation:** The UI has been translated to English, but internal database slugs (e.g., categories) remain in Romanian to maintain data integrity.
