---
title: API Reference
layout: default
nav_order: 6
has_children: true
description: "API reference for AvalonDock v5.0.0."
permalink: /api-reference/
---

# API Reference

Reference documentation for AvalonDock's key classes, controls, and interfaces.

---

| Section | Description |
|:--------|:------------|
| [DockingManager]({% link api-reference/docking-manager.md %}) | The root control and central entry point. |
| [Layout Classes]({% link api-reference/layout-classes.md %}) | All layout model classes and interfaces. |
| [Controls]({% link api-reference/controls.md %}) | WPF controls for rendering the docking UI. |
| [Events]({% link api-reference/events.md %}) | Events raised during docking operations. |

---

## Namespaces

| Namespace | Purpose |
|:----------|:--------|
| `AvalonDock` | Root namespace containing `DockingManager` and core types. |
| `AvalonDock.Layout` | Layout model classes: `LayoutRoot`, `LayoutPanel`, `LayoutDocument`, etc. |
| `AvalonDock.Controls` | WPF control implementations for the docking UI. |
| `AvalonDock.Themes` | Theme base classes: `Theme`, `GenericTheme`, `DictionaryTheme`. |
| `AvalonDock.Commands` | Command implementations for docking operations. |
| `AvalonDock.Converters` | WPF value converters used in theme templates. |
| `AvalonDock.Core` | UI-agnostic interfaces and models. |
| `AvalonDock.Core.Serialization` | Serialization contracts and base classes. |
| `AvalonDock.Mvvm` | MVVM base classes and services. |
| `AvalonDock.DependencyInjection` | DI extension methods. |
| `AvalonDock.Serializer.Xml` | XML layout serializer. |
| `AvalonDock.Serializer.Json` | JSON layout serializer. |
