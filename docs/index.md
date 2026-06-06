---
title: Home
layout: home
nav_order: 1
description: "AvalonDock — A powerful WPF docking layout library for building Visual Studio-like interfaces."
permalink: /
---

# AvalonDock v5.0.0
{: .fs-9 }

A powerful WPF Document and Tool Window layout container for building Visual Studio-like docking interfaces.
{: .fs-6 .fw-300 }

[Get Started]({% link getting-started/index.md %}){: .btn .btn-primary .fs-5 .mb-4 .mb-md-0 .mr-2 }
[View on GitHub](https://github.com/Dirkster99/AvalonDock){: .btn .fs-5 .mb-4 .mb-md-0 }

---

## What is AvalonDock?

AvalonDock is a WPF layout container that allows you to arrange documents and tool windows in ways similar to many well-known IDEs such as Visual Studio, Eclipse, and PhotoShop. It provides a complete docking system with floating windows, tabbed documents, auto-hide panels, and customizable themes.

AvalonDock is used by [many open source and commercial projects](https://github.com/search?p=4&q=%22dirkster.avalondock%22&type=Code), including [Stride](https://github.com/stride3d/stride) (game engine), [Optick](https://github.com/bombomby/optick) (profiler), [RoslynPad](https://github.com/roslynpad/roslynpad) (C# editor), [DaxStudio](https://github.com/DaxStudio/DaxStudio) (DAX query tool), [Macad3D](https://github.com/Macad3D/Macad3D) (3D CAD), and Microsoft's [Profile Explorer](https://github.com/microsoft/profile-explorer).

---

## Key Features

| Feature | Description |
|:--------|:------------|
| **Dockable Panels** | Dock tool windows to any side of the application, or float them freely. |
| **Tabbed Documents** | Organize documents in tabbed groups, split horizontally or vertically. |
| **Auto-Hide** | Minimize tool windows to side tabs, expanding on hover or click. |
| **Floating Windows** | Tear off any panel into an independent floating window. |
| **Layout Serialization** | Save and restore complex layouts via XML or JSON. |
| **MVVM Support** | First-class MVVM integration with view model binding. |
| **Dependency Injection** | Built-in DI support via `Microsoft.Extensions.DependencyInjection`. |
| **6 Built-in Themes** | Arc, VS2013, VS2010, Expression, Metro, and Aero themes included. |
| **Multi-Target** | Supports .NET 9, .NET 10, and .NET Framework 4.8. |

---

## Quick Example

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock">

    <avalonDock:DockingManager>
        <avalonDock:LayoutRoot>
            <avalonDock:LayoutPanel Orientation="Horizontal">
                <avalonDock:LayoutAnchorablePane DockWidth="250">
                    <avalonDock:LayoutAnchorable Title="Solution Explorer"
                                                  ContentId="solutionExplorer">
                        <TextBlock Text="Explorer content here" />
                    </avalonDock:LayoutAnchorable>
                </avalonDock:LayoutAnchorablePane>

                <avalonDock:LayoutDocumentPane>
                    <avalonDock:LayoutDocument Title="Document1.txt"
                                               ContentId="doc1">
                        <TextBox Text="Document content here" />
                    </avalonDock:LayoutDocument>
                </avalonDock:LayoutDocumentPane>
            </avalonDock:LayoutPanel>
        </avalonDock:LayoutRoot>
    </avalonDock:DockingManager>
</Window>
```

---

## NuGet Packages

| Package | Description |
|:--------|:------------|
| [Dirkster.AvalonDock](https://nuget.org/packages/Dirkster.AvalonDock) | Core docking library |
| [Dirkster.AvalonDock.Mvvm](https://nuget.org/packages/Dirkster.AvalonDock.Mvvm) | MVVM base classes (no external dependencies) |
| [Dirkster.AvalonDock.Mvvm.CommunityToolkit](https://nuget.org/packages/Dirkster.AvalonDock.Mvvm.CommunityToolkit) | CommunityToolkit.Mvvm integration |
| [Dirkster.AvalonDock.Themes.Arc](https://nuget.org/packages/Dirkster.AvalonDock.Themes.Arc) | Modern Arc theme (Dark & Light) |
| [Dirkster.AvalonDock.Themes.VS2013](https://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013) | VS2013 theme (Dark, Light, Blue) |
| [Dirkster.AvalonDock.Themes.VS2010](https://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2010) | VS2010 theme |
| [Dirkster.AvalonDock.Themes.Expression](https://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression) | Expression Blend theme |
| [Dirkster.AvalonDock.Themes.Metro](https://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro) | Metro/WinUI theme |
| [Dirkster.AvalonDock.Themes.Aero](https://nuget.org/packages/Dirkster.AvalonDock.Themes.Aero) | Classic Aero theme |

---

## Platform Support

| Framework | Status |
|:----------|:-------|
| .NET 10 | ✅ Supported |
| .NET 9 | ✅ Supported |
| .NET Framework 4.8 | ✅ Supported |

{: .note }
AvalonDock is a WPF library and runs exclusively on Windows.

---

## Upgrading from v4?

If you are migrating from AvalonDock v4.x, see the [Migration Guide]({% link migration/index.md %}) for a complete list of breaking changes and step-by-step upgrade instructions.

---

## License

AvalonDock is dual-licensed under the [MS-PL](https://github.com/Dirkster99/AvalonDock/blob/master/LICENSE) and Apache 2.0 licenses. It is free for both commercial and open source use.
