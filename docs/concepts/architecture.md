---
title: Architecture
layout: default
parent: Concepts
nav_order: 1
description: "High-level architecture of AvalonDock v5.0.0."
---

# Architecture

AvalonDock v5.0.0 is built on a layered architecture that separates concerns between the layout model, visual controls, and extensibility points.

---

## Component Layers

```
┌─────────────────────────────────────────────────────┐
│                   Your Application                  │
├─────────────────────────────────────────────────────┤
│   AvalonDock.Mvvm.CommunityToolkit  (optional)      │
│   (ObservableObject-based classes)                  │
├─────────────────────────────────────────────────────┤
│   AvalonDock.Mvvm          │  AvalonDock.DI          │
│   (MVVM base classes)      │  (DI extensions)        │
├─────────────────────────────────────────────────────┤
│                    AvalonDock                        │
│   ┌─────────────┐  ┌──────────┐  ┌──────────────┐  │
│   │  Controls    │  │  Themes  │  │  Serializers │  │
│   │  (WPF UI)   │  │  (XAML)  │  │  (XML/JSON)  │  │
│   └──────┬──────┘  └────┬─────┘  └──────┬───────┘  │
│          │              │               │           │
│   ┌──────┴──────────────┴───────────────┴───────┐   │
│   │              Layout Model                    │   │
│   │   (LayoutRoot, LayoutPanel, LayoutContent)   │   │
│   └──────────────────┬──────────────────────────┘   │
├──────────────────────┼──────────────────────────────┤
│              AvalonDock.Core                        │
│   (Interfaces, Models, Base Classes)                │
└─────────────────────────────────────────────────────┘
```

---

## Package Architecture

AvalonDock v5.0.0 is distributed as multiple NuGet packages with clear dependency boundaries:

### Core Packages

| Package | Purpose |
|:--------|:--------|
| `AvalonDock.Core` | UI-agnostic interfaces, models, and serialization contracts. No WPF dependency. |
| `AvalonDock` | The main WPF docking library. Contains `DockingManager`, all controls, layout classes, and the default theme. Depends on `AvalonDock.Core`. |

### Extension Packages

| Package | Purpose |
|:--------|:--------|
| `AvalonDock.Mvvm` | MVVM base classes: `DockableBase`, `ToolboxBase`, `DockLayoutService`, `DockViewModels`, and `Factory`. No external dependencies. |
| `AvalonDock.Mvvm.CommunityToolkit` | `ObservableObject`-based equivalents (`ObservableDockableBase`, `ObservableToolboxBase`, `ObservableDocument`, `ObservableTool`) with `[ObservableProperty]` and `[RelayCommand]` source generator support. Depends on `CommunityToolkit.Mvvm`. |
| `AvalonDock.DependencyInjection` | `IServiceCollection` extension methods for registering AvalonDock services. |
| `AvalonDock.Serializer.Xml` | XML-based layout serialization via `XmlLayoutSerializer`. |
| `AvalonDock.Serializer.Json` | JSON-based layout serialization via `JsonLayoutSerializer`. |

### Theme Packages

Each theme is a separate NuGet package so you only ship what you use:

| Package | Variants |
|:--------|:---------|
| `AvalonDock.Themes.Arc` | `ArcDarkTheme`, `ArcLightTheme` |
| `AvalonDock.Themes.VS2013` | `Vs2013DarkTheme`, `Vs2013LightTheme`, `Vs2013BlueTheme` |
| `AvalonDock.Themes.VS2010` | Single theme |
| `AvalonDock.Themes.Expression` | Dark and Light variants |
| `AvalonDock.Themes.Metro` | Single theme |
| `AvalonDock.Themes.Aero` | Single theme |

---

## Key Design Principles

### 1. Layout Model Drives the UI

AvalonDock uses a **model-first** architecture. The layout is represented as a tree of layout objects (`LayoutRoot`, `LayoutPanel`, `LayoutDocumentPane`, etc.). WPF controls are generated from this model, not the other way around. This separation enables:

- Layout serialization without UI dependency
- Programmatic layout manipulation
- MVVM-friendly design

### 2. Pluggable Layout Engine

The `ILayoutEngine` interface allows customization of how panels are measured, arranged, and resized. The `DefaultLayoutEngine` provides standard behavior, but you can implement your own for specialized layouts.

### 3. Theme Isolation

Themes are separate assemblies that provide XAML resource dictionaries. They can be swapped at runtime via the `DockingManager.Theme` property without any code changes.

### 4. Event-Driven Extensibility

AvalonDock exposes a rich set of events for every docking operation:

- `AnchorableClosing` / `AnchorableClosed`
- `AnchorableHiding` / `AnchorableHidden`
- `DocumentClosing` / `DocumentClosed`
- `ContentDocked` / `ContentFloating`
- `LayoutFloatingWindowControlClosed`

These events use cancellable patterns where appropriate, letting you intercept and prevent operations.

---

## The DockingManager

`DockingManager` is the root control and the central entry point for AvalonDock. It:

- Hosts the entire layout tree via its `Layout` property (of type `LayoutRoot`)
- Manages floating windows
- Handles drag-and-drop docking operations
- Applies themes
- Provides MVVM bindings for documents and anchorables

```xml
<avalonDock:DockingManager x:Name="dockManager">
    <avalonDock:DockingManager.Theme>
        <themes:ArcDarkTheme />
    </avalonDock:DockingManager.Theme>

    <avalonDock:LayoutRoot>
        <!-- Layout tree here -->
    </avalonDock:LayoutRoot>
</avalonDock:DockingManager>
```

In code-behind:
```csharp
// Access the layout
LayoutRoot layout = dockManager.Layout;

// Change theme at runtime
dockManager.Theme = new ArcLightTheme();

// Find a specific anchorable
var explorer = layout.Descendents()
    .OfType<LayoutAnchorable>()
    .FirstOrDefault(a => a.ContentId == "explorer");
```
