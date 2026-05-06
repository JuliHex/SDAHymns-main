# SDAHymns

A modern, cross-platform desktop application for displaying hymns in church services, built for the Romanian Seventh Day Adventist Church.

## Overview

SDAHymns is a complete rewrite of a legacy hymn display application, designed to improve user experience, support streaming workflows, and enable automation. The application displays hymns on projectors and supports customizable styling for OBS/streaming integration.

## Features

### Core Functionality
- **Hymn Display & Projection** - Dual-window system with control and full-screen display
- **Multi-Category Support** - Includes five hymn categories:
  - Imnuri crestine (Christian hymns - main hymnbook)
  - Imnuri companioni (Pathfinder hymns)
  - Imnuri exploratori (Explorer hymns)
  - Imnuri licurici (Firefly hymns - children)
  - Imnuri tineret (Youth hymns)

### Advanced Features
- ✅ **Auto-Updates** - Seamless update mechanism via GitHub Releases with Velopack
- ✅ **Piano Recordings** - Audio playback with sync, recorder mode, countdown, device selection
- ✅ **Audio Library Management** - Download from GitHub/Pi, verify checksums, migrate library
- ✅ **Display Profiles** - 6 preset profiles + full editor (fonts, colors, backgrounds, effects)
- ✅ **Keyboard-Driven** - Full hotkey support with F1 overlay and customizable shortcuts
- ✅ **OBS/Streaming Optimization** - Transparent backgrounds, custom fonts/colors for streaming
- ✅ **Offline-First** - Fully functional without internet
- 🚧 **CLI Support** - Full command-line interface for automation (planned)
- 🚧 **Service Planner** - Pre-plan hymn orders for services (planned)
- 🚧 **Export** - Generate PDFs and images for bulletins (planned)
- 🚧 **Statistics** - Track hymn usage frequency (planned)

## Tech Stack

- **Language:** C# (.NET 10 LTS)
- **UI Framework:** Avalonia UI 11.x (cross-platform XAML)
- **Database:** SQLite with Entity Framework Core 10.x
- **Audio:** NAudio 2.x
- **CLI:** CommandLineParser 2.x
- **Testing:** xUnit, FluentAssertions, Moq
- **License:** AGPL-3.0

## Platform Support

- Windows 10/11 (primary target)
- Windows 7/8 (if feasible)
- macOS ARM (M1/M2/M3)
- macOS Intel

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (LTS release)
- Git
- A code editor (Visual Studio 2022, VS Code, or Rider recommended)

### Clone the Repository

```bash
git clone https://github.com/yourusername/SDAHymns.git
cd SDAHymns
```

### Build the Solution

```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Run tests
dotnet test
```

### Run the Application

```bash
# Run desktop application
dotnet run --project src/SDAHymns.Desktop

# Run CLI
dotnet run --project src/SDAHymns.CLI -- --help
```

## Project Structure

```
SDAHymns/
├── src/
│   ├── SDAHymns.Core/          # Shared business logic and data layer
│   ├── SDAHymns.Desktop/       # Avalonia UI application
│   └── SDAHymns.CLI/           # Command-line interface
├── tests/
│   └── SDAHymns.Tests/         # Unit and integration tests
├── Resources/                   # Application resources
│   ├── audio/                  # Piano recordings (by category)
│   ├── config/                 # Configuration files
│   └── legacy/                 # Legacy app (reference only)
├── docs/
│   └── spec/                   # Specification files
├── CLAUDE.md                   # Developer guidance for Claude Code
├── README.md                   # This file
└── Directory.Build.props       # Common MSBuild properties
```

## Development

### Spec-Driven Development

This project follows a specification-driven approach. All features are documented in `docs/spec/` before implementation:

- **Check `docs/spec/_context.md`** for current project status
- Each spec defines requirements, implementation details, and acceptance criteria
- Specs are tracked through statuses: Planned → In Progress → Implemented → Tested

### Building for Production

```bash
# Windows (self-contained)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# macOS ARM (self-contained)
dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/SDAHymns.Tests
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/SDAHymns.Core

# Update database
dotnet ef database update --project src/SDAHymns.Core

# Remove last migration
dotnet ef migrations remove --project src/SDAHymns.Core
```

## Contributing

Contributions are welcome! This project aims for industry-standard testing coverage and code quality.

### Development Workflow

1. Check `docs/spec/_context.md` for current priorities
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Write tests first (TDD approach)
4. Implement the feature
5. Ensure all tests pass
6. Update relevant spec files
7. Submit a pull request

### Code Style

- Follow C# naming conventions
- Use nullable reference types
- Write XML documentation for public APIs
- Maintain test coverage above 80%

## Documentation

- **[CLAUDE.md](CLAUDE.md)** - Comprehensive developer guide and architecture documentation
- **[Spec Context](docs/spec/_context.md)** - Current project status and spec tracking
- **[Spec 001](docs/spec/001-project-structure.md)** - Project structure specification

## Roadmap

### Phase 1: Foundation ✅
- [x] Project structure setup
- [x] Data layer and models (1,254 hymns, 4,629 verses)
- [x] Legacy XML import functionality
- [x] PowerPoint verse extraction
- [x] Basic hymn display (dual-window system)

### Phase 2: Core Features ✅ COMPLETE
- [x] Enhanced control window (search, browse, recent/favorites)
- [x] Display profiles system (6 presets, full editor, background images)
- [x] Keyboard shortcuts (F1 overlay, inline editing, customizable)
- [ ] Service planner (planned)

### Phase 3: Enhanced Features ✅ MOSTLY COMPLETE
- [x] **Audio playback system** (NAudio player, sync, recorder mode, countdown, device selector - 19/19 criteria)
- [x] **Audio download & settings** (GitHub/Pi downloads, library management, folder picker - 18/20 app features)
- [ ] Export functionality (PDF/image generation - planned)
- [ ] Statistics tracking (usage analytics - planned)

### Phase 4: Advanced Features 🚧 In Progress
- [x] **Auto-update system** (Velopack, GitHub Releases, delta updates)
- [ ] CLI interface with IPC
- [ ] OBS integration

### Phase 5: UI/UX Refinement & Polish 📋 Planned
- [ ] **UI/UX Overhaul** (Modern design system, animations, themes, icons)
- [ ] **Enhanced Slide Formatting** (Verse numbers, chorus styling, transitions, multi-column)
- [ ] **Compact Remote Widget** (Widget-style default GUI, custom chrome, borderless display)
- [ ] **Remote Control API** (Web-based mobile control, SignalR, Vue.js interface)

## License

This project is licensed under the AGPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built for the Romanian Seventh Day Adventist Church
- Successor to the legacy "Imnuri Crestine" application
- Developed with [Claude Code](https://claude.ai/code)

## Contact & Support

For questions, issues, or feature requests, please open an issue on GitHub.

---

**Note:** The `Imnuri Azs/` folder contains the legacy application and is kept only as a reference for hymn resources. It is not part of the new application.
