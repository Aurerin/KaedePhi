# AGENTS.md - KaedePhi Project Guide

## Project Overview

**KaedePhi** (formerly PhiFanmadeTool) is a .NET toolset for parsing, converting, rendering, and manipulating Phigros fan-made chart formats. It supports multiple chart formats including RePhiEdit, PhiEdit, PhiFans, PhiChain, Phigros official v3, and its own native format (KaedePhi/KPC).

- **License**: GNU LGPL 2.1
- **Version**: 0.4.1 (defined in `Directory.Build.props`)
- **Repository**: https://github.com/NuanRMxi-Lazy-Team/KaedePhi

## Tech Stack

- **Language**: C# (.NET)
- **Target Frameworks**:
  - `KaedePhi.Core`: `netstandard2.1`, `net8.0`, `net10.0`
  - `KaedePhi.Tool`: `net8.0`, `net10.0`
  - `KaedePhi.Tool.Cli`: `net8.0`, `net10.0`
  - `KaedePhi.Tool.Localization`: `net8.0`, `net10.0`
- **Platform**: x64 only
- **Configurations**: `Debug`, `Release`, `PreRelease`
- **IDE**: JetBrains Rider (`.idea/` directory present)

## Key Dependencies

| Package | Project | Purpose |
|---------|---------|---------|
| `Newtonsoft.Json` 13.0.4 | Core | JSON serialization/deserialization |
| `JetBrains.Annotations` 2025.2.4 | Core, Tool | Code analysis annotations |
| `SkiaSharp` 3.119.2 | Tool | Chart rendering (image export) |
| `Spectre.Console.Cli` 0.55.0 | CLI | Command-line argument parsing |
| `YamlDotNet` 17.1.0 | CLI | YAML config file parsing |

## Solution Structure

```
KaedePhi.sln
├── KaedePhi.Core/           # Core library - chart format models & serialization
│   ├── KaedePhi/            # Native KPC format (primary/internal format)
│   ├── RePhiEdit/           # RePhiEdit format models & JSON converters
│   ├── PhiEdit/             # PhiEdit format models
│   ├── PhiFans/             # PhiFans format models
│   ├── PhiChain/v6/         # PhiChain v6 format models
│   ├── Phigros/v3/          # Official Phigros v3 format models
│   ├── Common/              # Shared types (Beat)
│   └── Utils/               # Utilities (Easings, Bezier, JsonConverters)
│
├── KaedePhi.Tool/           # Tool library - converters, processors, renderers
│   ├── Converter/           # Format converters (IChartConverter interface)
│   │   ├── IChartConverter.cs     # Core converter interface
│   │   ├── ChartPipeline.cs       # Conversion pipeline
│   │   ├── RePhiEdit/             # RPE <-> KPC conversion
│   │   ├── PhiEdit/               # PhiEdit <-> KPC conversion
│   │   ├── Phigros/v3/            # Phigros v3 <-> KPC conversion
│   │   └── KaedePhi/              # KPC converters (KpcToPe, KpcToRpe)
│   ├── Event/               # Event operations (Cut, Merge, Fit, Compress)
│   ├── JudgeLines/          # Judge line operations (Father unbind)
│   ├── Layer/               # Layer operations (merge/processing)
│   ├── Render/              # Chart rendering (SkiaSharp-based)
│   ├── Common/              # Shared types (ChartType, Unit, ILoggable, CoordinateProfile)
│   └── RePhiEdit/           # RPE-specific converters
│
├── KaedePhi.Tool.Cli/       # CLI executable
│   ├── Program.cs           # Entry point (Spectre.Console.Cli)
│   ├── Commands/            # CLI commands (Convert, Render, Unbind, Cut, Fit, etc.)
│   ├── Model/               # Config models per command
│   ├── Infrastructure/      # Services (ChartService, WorkspaceService, ConsoleWriter)
│   └── Settings/            # Command settings
│
└── KaedePhi.Tool.Localization/  # i18n resource strings
    ├── Strings.resx              # Default (zh-CN) strings
    ├── Strings.en-US.resx        # English strings
    ├── Strings.zh-hant.resx      # Traditional Chinese strings
    ├── CliLocalizationString.resx        # CLI-specific default strings
    ├── CliLocalizationString.en-us.resx  # CLI-specific English strings
    └── CliLocalizationString.zh-hant.resx # CLI-specific Traditional Chinese strings
```

## Architecture Patterns

### Core Data Flow

All chart formats flow through the **KPC (KaedePhi Chart)** intermediate representation:

```
[External Format] → IChartConverter.ToKpc() → [Kpc.Chart] → IChartConverter.FromKpc() → [External Format]
```

### Key Interfaces

- **`IChartConverter<T, TInOptions, TOutOptions>`** (`KaedePhi.Tool/Converter/IChartConverter.cs`): Defines `ToKpc()` and `FromKpc()` for format conversion
- **`IEventCutter`**, **`IEventMerger`**, **`IEventFit`**, **`IEventCompressor`**: Event manipulation operations
- **`IJudgeLineUnbinder`**: Judge line parent-child unbinding
- **`ILayerProcessor`**: Layer merge/processing
- **`IChartRenderExporter`**: Chart rendering to image
- **`ILoggable`** / **`LoggableBase`**: Logging support via `LogSubscription`

### Naming Convention

- `Kpc` prefix = KaedePhi Chart (internal format) types and tools
- `Rpe` = RePhiEdit
- `Pe` = PhiEdit

## Build & Run

```bash
# Build entire solution
dotnet build KaedePhi.sln

# Build specific project
dotnet build KaedePhi.Core/KaedePhi.Core.csproj

# Run CLI
dotnet run --project KaedePhi.Tool.Cli -- [command] [options]

# CLI help
dotnet run --project KaedePhi.Tool.Cli -- --help
```

## CLI Commands

| Command | Alias | Description |
|---------|-------|-------------|
| `version` | `ver` | Show version |
| `convert` | | Convert between chart formats |
| `unbind-father` | `unbind` | Unbind father judge lines (RPE) |
| `layer-merge` | | Merge event layers (RPE) |
| `cut` | `cut-event`, `cut-all` | Cut events |
| `fit` | `fit-event` | Fit events |
| `render-event` | `render` | Render chart events to image |
| `load` | | Load chart into workspace |
| `save` | | Save workspace chart |
| `workspace list` | | List workspace contents |
| `workspace clear` | | Clear workspace |

## Supported Chart Formats

| Format | Namespace | Status |
|--------|-----------|--------|
| **KaedePhi (KPC)** | `KaedePhi.Core.KaedePhi` | Primary internal format |
| **RePhiEdit** | `KaedePhi.Core.RePhiEdit` | Full support (serialize/deserialize) |
| **PhiEdit** | `KaedePhi.Core.PhiEdit` | Full support (serialize/deserialize) |
| **Phigros v3** | `KaedePhi.Core.Phigros.v3` | Deserialization supported |
| **PhiFans** | `KaedePhi.Core.PhiFans` | Models defined, WIP |
| **PhiChain v6** | `KaedePhi.Core.PhiChain.v6` | Models defined, WIP |

## Code Conventions

- **Language version**: C# 9 (Core), latest (Tool/CLI)
- **Implicit usings**: Disabled in Core (`ImplicitUsings=disable`), enabled in Tool/CLI
- **Nullable**: Enabled in Tool/CLI projects
- **Namespace style**: File-scoped namespaces in newer code, block namespaces in Core
- **JSON**: Uses `Newtonsoft.Json` with custom `JsonConverter` implementations per format
- **Logging**: Via `ILoggable`/`LoggableBase` with `LogSubscription` delegates
- **Localization**: All user-facing strings go through `Strings` or `CliLocalizationString` resource classes

## CI/CD

GitHub Actions workflows in `.github/workflows/`:
- `auto-release-core.yml` - Auto-release Core NuGet package
- `auto-release-tool.yml` - Auto-release Tool NuGet package
- `auto-release-cli.yml` - Auto-release CLI
- `auto-nightly-cli.yml` - Nightly CLI builds

## Important Notes

- **Version 0.4.1** introduced major architecture rewrite; some APIs are broken or renamed/deprecated
- **No test project** currently exists in the solution
- `KaedePhi.Core` targets `netstandard2.1` for Unity compatibility (`build_unity_package.ps1` present)
- The `Kpc/` directory under Core is empty; the KPC format types live in `KaedePhi.Core.KaedePhi` namespace
- All chart coordinate systems use normalized coordinates (-1 to 1)
