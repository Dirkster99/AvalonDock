# Agents

Instructions for AI coding agents working on this repository.

## Prerequisites

- **OS:** Windows (required — WPF projects only build on Windows)
- **.NET SDK:** 9.0.x and 10.0.x
- **Solution file:** `source/AvalonDock.sln`

## Build

```powershell
dotnet restore source/AvalonDock.sln
dotnet build source/AvalonDock.sln --configuration Release --no-restore --warnaserror
```

`--warnaserror` is required — all warnings are treated as errors in CI.

## Test

Tests must run with `-m:1` (single-threaded) because multiple test projects target .NET Framework and cannot run in parallel.

### Unit Tests

```powershell
dotnet test source/AvalonDock.sln --configuration Release --no-restore --filter "Category!=FlaUI" -m:1
```

### FlaUI UI Tests

These are UI automation tests that launch actual WPF windows. Run them separately, targeting .NET 10:

```powershell
dotnet test source/AvalonDock.sln --configuration Release --no-restore --filter "Category=FlaUI" --framework net10.0-windows -m:1
```

## Project Structure

- `source/Components/AvalonDock` — Main WPF docking library
- `source/Components/AvalonDock.Core` — UI-agnostic core (interfaces, models, serialization base). Targets `netstandard2.0;net9.0`. Has `GenerateDocumentationFile` enabled — all public members must have XML doc comments.
- `source/Components/AvalonDock.Themes.*` — Theme packages
- `source/Components/AvalonDock.Serializer.Xml` — XML layout serializer
- `source/Components/AvalonDock.Serializer.Json` — JSON layout serializer
- `source/Components/AvalonDock.DependencyInjection` — DI registration extensions
- `source/Components/AvalonDock.Mvvm` — MVVM base classes (DockableBase, ToolboxBase)
- `source/AutomationTest/` — Unit and FlaUI test projects
- `source/TestApp`, `source/MVVMTestApp`, `source/ToggleTestApp` — Sample applications

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
