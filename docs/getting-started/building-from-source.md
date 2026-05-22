---
title: Building from Source
layout: default
parent: Getting Started
nav_order: 3
description: "Clone and build AvalonDock from source code."
---

# Building from Source

This guide covers how to clone, build, and test AvalonDock locally.

---

## Prerequisites

- **Windows** (WPF is Windows-only)
- **.NET 9 SDK** and/or **.NET 10 SDK**
- **Visual Studio 2022** or later (recommended)

## Clone the Repository

```bash
git clone https://github.com/Dirkster99/AvalonDock.git
cd AvalonDock
```

## Build

```bash
dotnet build source/AvalonDock.sln
```

{: .note }
The build uses `TreatWarningsAsErrors`, so all warnings must be resolved before the build succeeds.

## Run Tests

```bash
dotnet test source/AvalonDock.sln -m:1
```

{: .important }
Tests must be run single-threaded (`-m:1`) because WPF requires STA (Single-Threaded Apartment) mode. Running tests in parallel may cause intermittent failures.

### Test Categories

| Category | Command | Description |
|:---------|:--------|:------------|
| Unit Tests | `dotnet test --filter "Category!=FlaUI" -m:1` | Fast unit tests |
| UI Tests | `dotnet test --filter "Category=FlaUI" --framework net10.0-windows -m:1` | FlaUI-based UI automation tests |

## Solution Structure

```
source/
├── AvalonDock.sln                  # Main solution file
├── Directory.Build.props           # Shared build configuration
├── Components/                     # NuGet package projects
│   ├── AvalonDock/                 # Core docking library
│   ├── AvalonDock.Core/            # UI-agnostic interfaces & models
│   ├── AvalonDock.Mvvm/            # MVVM base classes
│   ├── AvalonDock.DependencyInjection/  # DI extensions
│   ├── AvalonDock.Serializer.Xml/  # XML serializer
│   ├── AvalonDock.Serializer.Json/ # JSON serializer
│   └── AvalonDock.Themes.*/        # Theme packages
├── AutomationTest/                 # FlaUI UI tests
├── TestApp/                        # Test/demo application
├── MVVMTestApp/                    # MVVM demo application
├── MLibTest/                       # MLib integration demo
├── CaliburnDockTestApp/            # Caliburn.Micro demo
├── WinFormsTestApp/                # WinForms interop demo
└── VS2013Test/                     # VS2013 theme demo
```

## Code Quality

The project enforces strict code quality standards:

- **StyleCop.Analyzers** — Consistent code style
- **Microsoft.CodeAnalysis.NetAnalyzers** — .NET best practices
- **SonarAnalyzer.CSharp** — Security and reliability checks
- All warnings are treated as errors in CI

See [CONTRIBUTING.md](https://github.com/Dirkster99/AvalonDock/blob/master/CONTRIBUTING.md) for contribution guidelines.
