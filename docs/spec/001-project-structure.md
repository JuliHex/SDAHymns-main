# Spec 001: Project Structure

**Status:** ✅ Implemented
**Created:** 2025-12-03
**Updated:** 2025-12-03
**Implemented:** 2025-12-03

## Overview

Establish the foundational solution structure for SDAHymns with proper project separation, dependency management, and build configuration. This creates a maintainable, testable architecture following .NET best practices.

## Goals

1. Create multi-project solution structure
2. Configure proper dependencies between projects
3. Set up cross-platform build targets
4. Establish resource directory structure
5. Configure solution for .NET 8 with Avalonia UI

## Project Structure

### Solution Layout

```
SDAHymns.sln
├── src/
│   ├── SDAHymns.Desktop/          # Avalonia UI application
│   ├── SDAHymns.CLI/              # Command-line interface
│   └── SDAHymns.Core/             # Shared business logic
├── tests/
│   └── SDAHymns.Tests/            # Unit and integration tests
├── Resources/                      # Application resources (not in solution)
│   ├── audio/
│   ├── config/
│   └── legacy/
├── .claude/
│   └── spec/
├── docs/
├── .gitignore
├── README.md
└── CLAUDE.md
```

## Project Details

### 1. SDAHymns.Core (Class Library)

**Purpose:** Shared business logic, data models, services

**Target Framework:** `net8.0`

**Dependencies:**
- `Microsoft.EntityFrameworkCore` (8.x)
- `Microsoft.EntityFrameworkCore.Sqlite` (8.x)
- `Microsoft.EntityFrameworkCore.Design` (8.x) - for migrations
- `NAudio` (latest) - audio playback
- `System.Text.Json` (included in .NET 8) - configuration serialization

**Key Namespaces:**
```
SDAHymns.Core
├── Data/
│   ├── HymnsContext.cs
│   ├── Models/
│   └── Migrations/
├── Services/
│   ├── IHymnService.cs
│   ├── HymnService.cs
│   ├── IAudioService.cs
│   ├── AudioService.cs
│   ├── IDisplayService.cs
│   ├── DisplayService.cs
│   ├── IConfigService.cs
│   ├── ConfigService.cs
│   ├── IExportService.cs
│   ├── ExportService.cs
│   ├── IStatisticsService.cs
│   ├── StatisticsService.cs
│   └── IUpdateService.cs
└── Utilities/
    ├── XmlParser.cs
    └── FileHelper.cs
```

**Project File Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.*" />
    <PackageReference Include="NAudio" Version="2.*" />
  </ItemGroup>
</Project>
```

### 2. SDAHymns.Desktop (Avalonia Application)

**Purpose:** Main GUI application with Avalonia UI

**Target Framework:** `net8.0`

**Dependencies:**
- `Avalonia` (11.x)
- `Avalonia.Desktop` (11.x)
- `Avalonia.Themes.Fluent` (11.x)
- `Avalonia.Fonts.Inter` (11.x)
- `Avalonia.ReactiveUI` (11.x) - for MVVM
- Project reference: `SDAHymns.Core`

**Key Namespaces:**
```
SDAHymns.Desktop
├── Views/
│   ├── ControlWindow.axaml / .axaml.cs
│   ├── DisplayWindow.axaml / .axaml.cs
│   ├── SettingsWindow.axaml / .axaml.cs
│   └── ServicePlannerView.axaml / .axaml.cs
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── ControlWindowViewModel.cs
│   ├── DisplayWindowViewModel.cs
│   ├── SettingsViewModel.cs
│   └── ServicePlannerViewModel.cs
├── Models/
│   └── (UI-specific models only)
├── Services/
│   └── WindowManager.cs
├── Assets/
│   ├── Icons/
│   └── Fonts/
├── App.axaml / .axaml.cs
└── Program.cs
```

**Project File Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <AvaloniaResource Include="Assets\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.*" />
    <PackageReference Include="Avalonia.ReactiveUI" Version="11.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SDAHymns.Core\SDAHymns.Core.csproj" />
  </ItemGroup>
</Project>
```

### 3. SDAHymns.CLI (Console Application)

**Purpose:** Command-line interface for automation

**Target Framework:** `net8.0`

**Dependencies:**
- `CommandLineParser` (2.x)
- `System.IO.Pipes` (included) - for IPC with GUI
- Project reference: `SDAHymns.Core`

**Key Namespaces:**
```
SDAHymns.CLI
├── Commands/
│   ├── ShowCommand.cs
│   ├── SearchCommand.cs
│   ├── NavigateCommand.cs
│   ├── AudioCommand.cs
│   └── ConfigCommand.cs
├── Services/
│   └── IpcClient.cs
└── Program.cs
```

**Project File Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommandLineParser" Version="2.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SDAHymns.Core\SDAHymns.Core.csproj" />
  </ItemGroup>
</Project>
```

### 4. SDAHymns.Tests (Test Project)

**Purpose:** Unit and integration tests

**Target Framework:** `net8.0`

**Dependencies:**
- `xUnit` (2.x)
- `xUnit.runner.visualstudio` (2.x)
- `FluentAssertions` (6.x)
- `Moq` (4.x) - for mocking
- `Microsoft.NET.Test.Sdk` (17.x)
- Project reference: `SDAHymns.Core`

**Key Namespaces:**
```
SDAHymns.Tests
├── Core/
│   ├── Services/
│   │   ├── HymnServiceTests.cs
│   │   └── AudioServiceTests.cs
│   └── Data/
│       └── HymnsContextTests.cs
├── Integration/
│   └── DatabaseIntegrationTests.cs
└── Fixtures/
    └── TestData.cs
```

**Project File Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    <PackageReference Include="Moq" Version="4.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\SDAHymns.Core\SDAHymns.Core.csproj" />
  </ItemGroup>
</Project>
```

## Resources Directory Structure

**Location:** `Resources/` (at solution root, not in any project)

```
Resources/
├── audio/                          # Piano recordings
│   ├── companioni/
│   │   ├── 001.mp3
│   │   └── 001.metadata.json
│   ├── crestine/
│   ├── exploratori/
│   ├── licurici/
│   └── tineret/
├── config/                         # User configuration
│   ├── display-profiles.json
│   └── user-settings.json
├── hymns.db                        # SQLite database (generated)
└── legacy/                         # Reference only
    └── Imnuri Azs/
        └── Resurse/
```

**Note:** Resources directory is excluded from projects but accessed at runtime via relative paths.

## Git Configuration

### .gitignore Additions

Add to existing `.gitignore`:

```gitignore
# .NET build outputs
**/bin/
**/obj/
*.user

# Rider
.idea/

# Generated database
Resources/hymns.db
Resources/hymns.db-shm
Resources/hymns.db-wal

# User configuration
Resources/config/user-settings.json

# Audio files (too large, distribute separately)
Resources/audio/**/*.mp3
Resources/audio/**/*.opus
!Resources/audio/**/*.metadata.json

# Keep directory structure
!Resources/audio/**/.gitkeep
```

**Important:** Audio files should be distributed separately or via Git LFS, not committed directly.

## Build Configuration

### Solution-Level Configuration

Create `Directory.Build.props` at solution root:

```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <Version>0.1.0</Version>
    <Authors>SDA Hymns Contributors</Authors>
    <Company>Romanian Seventh Day Adventist Church</Company>
    <Product>SDA Hymns</Product>
    <Copyright>Copyright © 2025 - AGPL-3.0 License</Copyright>
  </PropertyGroup>
</Project>
```

### Global Usings

Create `src/SDAHymns.Core/GlobalUsings.cs`:

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;
```

## Implementation Steps

### Step 1: Create Solution
```bash
dotnet new sln -n SDAHymns
```

### Step 2: Create Projects
```bash
# Create directories
mkdir -p src/SDAHymns.Core
mkdir -p src/SDAHymns.Desktop
mkdir -p src/SDAHymns.CLI
mkdir -p tests/SDAHymns.Tests

# Create Core library
dotnet new classlib -n SDAHymns.Core -o src/SDAHymns.Core -f net8.0

# Create Desktop app (Avalonia)
cd src/SDAHymns.Desktop
dotnet new avalonia.app -n SDAHymns.Desktop
cd ../..

# Create CLI app
dotnet new console -n SDAHymns.CLI -o src/SDAHymns.CLI -f net8.0

# Create test project
dotnet new xunit -n SDAHymns.Tests -o tests/SDAHymns.Tests -f net8.0
```

### Step 3: Add Projects to Solution
```bash
dotnet sln add src/SDAHymns.Core/SDAHymns.Core.csproj
dotnet sln add src/SDAHymns.Desktop/SDAHymns.Desktop.csproj
dotnet sln add src/SDAHymns.CLI/SDAHymns.CLI.csproj
dotnet sln add tests/SDAHymns.Tests/SDAHymns.Tests.csproj
```

### Step 4: Add Project References
```bash
# Desktop references Core
dotnet add src/SDAHymns.Desktop/SDAHymns.Desktop.csproj reference src/SDAHymns.Core/SDAHymns.Core.csproj

# CLI references Core
dotnet add src/SDAHymns.CLI/SDAHymns.CLI.csproj reference src/SDAHymns.Core/SDAHymns.Core.csproj

# Tests reference Core
dotnet add tests/SDAHymns.Tests/SDAHymns.Tests.csproj reference src/SDAHymns.Core/SDAHymns.Core.csproj
```

### Step 5: Add NuGet Packages
```bash
# Core packages
dotnet add src/SDAHymns.Core package Microsoft.EntityFrameworkCore
dotnet add src/SDAHymns.Core package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/SDAHymns.Core package Microsoft.EntityFrameworkCore.Design
dotnet add src/SDAHymns.Core package NAudio

# Desktop packages (Avalonia template includes most)
# Verify Avalonia packages are present

# CLI packages
dotnet add src/SDAHymns.CLI package CommandLineParser

# Test packages
dotnet add tests/SDAHymns.Tests package FluentAssertions
dotnet add tests/SDAHymns.Tests package Moq
```

### Step 6: Create Directory Structure
```bash
# Core directories
mkdir -p src/SDAHymns.Core/Data/Models
mkdir -p src/SDAHymns.Core/Data/Migrations
mkdir -p src/SDAHymns.Core/Services
mkdir -p src/SDAHymns.Core/Utilities

# Desktop directories
mkdir -p src/SDAHymns.Desktop/Views
mkdir -p src/SDAHymns.Desktop/ViewModels
mkdir -p src/SDAHymns.Desktop/Models
mkdir -p src/SDAHymns.Desktop/Services
mkdir -p src/SDAHymns.Desktop/Assets/Icons
mkdir -p src/SDAHymns.Desktop/Assets/Fonts

# CLI directories
mkdir -p src/SDAHymns.CLI/Commands
mkdir -p src/SDAHymns.CLI/Services

# Test directories
mkdir -p tests/SDAHymns.Tests/Core/Services
mkdir -p tests/SDAHymns.Tests/Core/Data
mkdir -p tests/SDAHymns.Tests/Integration
mkdir -p tests/SDAHymns.Tests/Fixtures

# Resources directories
mkdir -p Resources/audio/{companioni,crestine,exploratori,licurici,tineret}
mkdir -p Resources/config
```

### Step 7: Verify Build
```bash
dotnet restore
dotnet build
dotnet test
```

## Acceptance Criteria

- [ ] Solution builds successfully with `dotnet build`
- [ ] All projects target .NET 8
- [ ] Project references are correctly configured
- [ ] All NuGet packages are installed and restored
- [ ] Directory structure matches specification
- [ ] `Directory.Build.props` is in place with common properties
- [ ] `.gitignore` updated for .NET and project-specific files
- [ ] Desktop app launches with default Avalonia window
- [ ] CLI app runs and displays help text
- [ ] Tests run (even if no tests written yet)
- [ ] Resources directory structure exists

## Testing Verification

```bash
# Build entire solution
dotnet build

# Run tests
dotnet test

# Run desktop app
dotnet run --project src/SDAHymns.Desktop

# Run CLI app
dotnet run --project src/SDAHymns.CLI -- --help

# Verify projects are in solution
dotnet sln list
```

## Notes

- Avalonia template may create additional files (App.axaml, Program.cs) - keep these
- Resources directory is intentionally outside projects for easier resource management
- CLI will initially be standalone; IPC integration comes in later spec
- Consider Git LFS for audio files in production

## Related Specs

- **Next:** 002-data-layer.md (TBD) - EF Core models and context
- **Depends On:** None (foundation spec)

## Status Updates

- **2025-12-03:** Spec created, ready for implementation
