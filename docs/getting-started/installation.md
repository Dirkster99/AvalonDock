---
title: Installation
layout: default
parent: Getting Started
nav_order: 1
description: "Install AvalonDock via NuGet and add it to your WPF project."
---

# Installation

AvalonDock is distributed as a set of NuGet packages. The core package contains the docking framework, and theme packages are installed separately.

---

## Install via NuGet

### .NET CLI

```bash
dotnet add package Dirkster.AvalonDock
```

### Package Manager Console

```powershell
Install-Package Dirkster.AvalonDock
```

### PackageReference (csproj)

```xml
<PackageReference Include="Dirkster.AvalonDock" Version="5.0.0" />
```

---

## Install a Theme

AvalonDock ships with a default generic theme, but you'll likely want one of the polished built-in themes. Install the theme package for your preferred look:

```bash
# Modern Arc theme (recommended)
dotnet add package Dirkster.AvalonDock.Themes.Arc

# Visual Studio 2013 theme
dotnet add package Dirkster.AvalonDock.Themes.VS2013

# Other themes
dotnet add package Dirkster.AvalonDock.Themes.VS2010
dotnet add package Dirkster.AvalonDock.Themes.Expression
dotnet add package Dirkster.AvalonDock.Themes.Metro
dotnet add package Dirkster.AvalonDock.Themes.Aero
```

---

## Optional Packages

For advanced scenarios, AvalonDock provides additional packages:

| Package | Install Command | Purpose |
|:--------|:----------------|:--------|
| Serializer (XML) | `dotnet add package Dirkster.AvalonDock.Serializer.Xml` | Save/restore layouts in XML format |
| Serializer (JSON) | `dotnet add package Dirkster.AvalonDock.Serializer.Json` | Save/restore layouts in JSON format |
| MVVM | `dotnet add package Dirkster.AvalonDock.Mvvm` | MVVM base classes and services (no external dependencies) |
| MVVM + CommunityToolkit | `dotnet add package Dirkster.AvalonDock.Mvvm.CommunityToolkit` | `ObservableObject`-based classes with `[ObservableProperty]` support |
| DI | `dotnet add package Dirkster.AvalonDock.DependencyInjection` | Dependency injection integration |

---

## Verify Installation

After installing, add the AvalonDock namespace to your XAML:

```xml
<Window xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock">
    <avalonDock:DockingManager>
        <avalonDock:LayoutRoot>
            <avalonDock:LayoutPanel>
                <avalonDock:LayoutDocumentPane />
            </avalonDock:LayoutPanel>
        </avalonDock:LayoutRoot>
    </avalonDock:DockingManager>
</Window>
```

If the project builds without errors, you're ready to go. Head to the [Quick Start]({% link getting-started/quick-start.md %}) guide to build your first layout.

---

## Target Framework Compatibility

| Framework | Supported |
|:----------|:----------|
| .NET 10 (`net10.0-windows`) | ✅ |
| .NET 9 (`net9.0-windows`) | ✅ |
| .NET Framework 4.8 (`net48`) | ✅ |

{: .note }
Make sure your project targets one of the supported frameworks. AvalonDock requires the `-windows` TFM suffix for .NET 9+ projects.
