# Agents

Instructions for AI coding agents working on this repository.

## Two lines in one repository

The fork keeps two independent lines (see `docs/librewpf.md`, section 11):

| | WPF line | LibreWPF line |
|---|---|---|
| Projects | `source/Components/*/X.csproj`, `TestApp`, `MVVMTestApp`, `AvalonDockCodeApp`, `AutomationTest` | `*.LibreWPF.csproj`, next to the WPF twin |
| SDK | `Microsoft.NET.Sdk.WindowsDesktop` (upstream shape, multi-target, analyzers on) | `LibreWPF.Sdk` on every OS, `LIBREWPF` and `DEVFLOW` defined by the project |
| Solution | `source/AvalonDock.sln` | `source/AvalonDock.LibreWPF.sln` |
| Tests | NUnit unit tests + FlaUI (Windows, real WPF) | DevFlow integration tests (Windows, macOS, Linux) |
| CI / release | `ci.yml`, `release.yml` (tag `v*`, nuget.org) | `ci-librewpf.yml`; released by `release.yml` with the WPF packages |
| Packages | `Dirkster.AvalonDock*` | `Dirkster.AvalonDock*.LibreWPF` |

`AvalonDock.Core`, the serializers, MVVM and DI projects are `netstandard2.0` and shared by both lines.
Source files are shared too: a LibreWPF project compiles the sources of its own folder, so a change to
`source/Components/AvalonDock/**` lands in both lines. Code that only makes sense on the portable
backend goes under `Platform/LibreWPF/` or behind `#if LIBREWPF`; the DevFlow surface of the samples
sits behind `#if DEVFLOW`. Keep both lines building.

## Prerequisites

- **WPF line:** Windows to run it; it also *compiles* on macOS/Linux (`EnableWindowsTargeting`).
- **LibreWPF line:** any OS; `LibreWPF.Sdk` and its packages restore from nuget.org.
- **.NET SDK:** the one pinned in `global.json` (10.0).

## Build

```powershell
# WPF line
dotnet restore source/AvalonDock.sln
dotnet build source/AvalonDock.sln --configuration Release --no-restore
# LibreWPF line
dotnet build source/AvalonDock.LibreWPF.sln --configuration Debug
```

All warnings are errors in the WPF line (`source/Directory.Build.props`); the LibreWPF projects opt out
because the analyzers are dev-time gates of the WPF line, which shares the sources.

## Test

Tests must run with `-m:1` (single-threaded) because multiple test projects target .NET Framework and cannot run in parallel.

```powershell
# WPF line: unit tests, then the FlaUI UI tests (Windows only)
dotnet test source/AvalonDock.sln --configuration Release --no-restore --filter "Category!=FlaUI" -m:1
dotnet test source/AvalonDock.sln --configuration Release --no-restore --filter "Category=FlaUI" --framework net10.0-windows -m:1
```

LibreWPF line: build `source/AvalonDock.LibreWPF.sln` (Debug), then run
`source/DevFlowIntegrationTests` (see `docs/librewpf.md`, "Running the suite").

## Project Structure

- `source/Components/AvalonDock` - main docking library (`AvalonDock.csproj` and `AvalonDock.LibreWPF.csproj`)
- `source/Components/AvalonDock.Core` - UI-agnostic core (interfaces, models, serialization base). Targets `netstandard2.0;net9.0`. Has `GenerateDocumentationFile` enabled - all public members must have XML doc comments.
- `source/Components/AvalonDock.Themes.*` - theme packages (each with a `.LibreWPF` twin)
- `source/Components/AvalonDock.Serializer.Xml`, `.Serializer.Json`, `.DependencyInjection`, `.Mvvm*` - shared
- `source/AutomationTest/` - unit and FlaUI test projects (WPF line)
- `source/DevFlowIntegrationTests/` - cross-platform integration tests (LibreWPF line)
- `source/TestApp`, `source/MVVMTestApp` - classic sample applications
- `source/AvalonDockCodeApp` - ToggleDockingManager sample, driven by the FlaUI toggle-dock tests

## Code Style

- StyleCop Analyzers are enabled for non-test projects (configured in `source/Directory.Build.props`)
- `TreatWarningsAsErrors` is enabled globally
- Follow existing conventions: `#nullable disable` in legacy code, XML doc comments required in `AvalonDock.Core`

## Commit Messages

```
[Fix|Feature] #<TicketID> - [<Topic>] <What was done>
```

Examples:
- `[Feature] #1234 - [Serializer] Unified XmlLayoutSerializer with ILayoutSerializer`
- `[Fix] #5678 - [UI] Fixed toggle button styling in Arc theme`
