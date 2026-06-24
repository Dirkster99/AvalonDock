---
title: Tutorials
layout: default
nav_order: 3
has_children: true
description: "Step-by-step tutorials for building real-world applications with AvalonDock."
permalink: /tutorials/
---

# Tutorials

Hands-on, step-by-step tutorials that walk you through building real applications with AvalonDock. Each tutorial builds a complete, working example from scratch.

{: .note }
These tutorials assume you have already completed the [Quick Start]({% link getting-started/quick-start.md %}) guide and are familiar with WPF basics.

---

## Choose a Tutorial

| Tutorial | What You'll Build | Key Topics |
|:---------|:------------------|:-----------|
| [MVVM IDE Application]({% link tutorials/mvvm-ide.md %}) | A VS Code-style IDE with sidebar toggles | ToolboxBase, Document, IDockLayoutService, ToggleDockingManager, DockZone, DI |
| [Dependency Injection Deep Dive]({% link tutorials/dependency-injection-app.md %}) | DI patterns and APIs in depth | AddDockLayoutService builder, DockLayoutBuilder, ConfigureToggleDock, factory registration, testing |
| [Styling & Theming]({% link tutorials/styling-and-theming.md %}) | A theme-switchable app with custom branding | Runtime theme switching, brush overrides, custom theme classes, application-wide theming |
| [Layout Persistence]({% link tutorials/layout-persistence.md %}) | An app that remembers its window layout | XML/JSON serialization, MVVM serialization callbacks, auto-save, layout reset |

---

## How the Tutorials Are Organized

Each tutorial follows the same structure:

1. **What you'll build** — A preview of the finished result
2. **Prerequisites** — What you need before starting
3. **Step-by-step instructions** — Incremental steps with full code listings
4. **How it works** — Explanation of the key concepts
5. **Next steps** — Where to go from here

---

## Sample Projects

These tutorials are inspired by the sample projects included in the AvalonDock repository under `source/`:

| Project | Pattern | Description |
|:--------|:--------|:------------|
| `MVVMTestApp` | MVVM + Singleton | Classic MVVM with a static Workspace singleton |
| `AvalonDockCodeApp` | MVVM + DI | Modern approach using `Microsoft.Extensions.DependencyInjection` |
| `VS2013Test` | MVVM + Themed IDE | Production-like IDE simulator with multiple tool panels |
| `CaliburnDockTestApp` | Caliburn.Micro | Integration with the Caliburn.Micro MVVM framework |
| `MLibTest` | MLib Theming | Unified theming with the MLib theming library |
| `TestApp` | Code-behind | Comprehensive feature showcase (all themes, WinForms interop) |
| `WinFormsTestApp` | WinForms Host | Hosting AvalonDock in a WinForms application |

You can run any sample project from the solution:

```bash
cd source
dotnet run --project MVVMTestApp
```
