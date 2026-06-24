---
title: Breaking Changes
layout: default
parent: Migration Guide
nav_order: 1
description: "Complete list of breaking changes in AvalonDock v5.0.0."
---

# Breaking Changes in v5.0.0

This page lists all breaking changes between AvalonDock v4.x and v5.0.0, organized by category.

---

## Package Structure

### Serializers Extracted to Separate Packages

**Impact:** High — affects all projects using layout serialization.

The XML layout serializer has been moved from the core `AvalonDock` package to `AvalonDock.Serializer.Xml`. A new JSON serializer is available in `AvalonDock.Serializer.Json`.

| Change | Details |
|:-------|:--------|
| **Removed from** | `Dirkster.AvalonDock` |
| **Moved to** | `Dirkster.AvalonDock.Serializer.Xml` |
| **Old namespace** | `AvalonDock.Layout.Serialization` |
| **New namespace** | `AvalonDock.Serializer.Xml` |
| **Fix** | Install the serializer package and update `using` statements. |

```diff
- using AvalonDock.Layout.Serialization;
+ using AvalonDock.Serializer.Xml;
```

### New Core Package

**Impact:** Low — automatically referenced.

A new `AvalonDock.Core` package contains UI-agnostic interfaces and models. It is automatically referenced by `AvalonDock`, so no explicit installation is needed.

---

## Architecture

### ILayoutEngine Introduction

**Impact:** Low to Medium — only affects custom layout logic.

The layout calculation logic has been formalized behind the `ILayoutEngine` interface.

| Change | Details |
|:-------|:--------|
| **Added** | `ILayoutEngine` interface |
| **Added** | `DefaultLayoutEngine` implementation |
| **Affected** | Custom layout calculations using internal APIs |
| **Fix** | Implement `ILayoutEngine` for custom layout behavior. |

---

## Target Framework Changes

### Dropped Frameworks

**Impact:** High — if targeting dropped frameworks.

| Framework | Status |
|:----------|:-------|
| .NET Framework 4.0 | ❌ **Removed** |
| .NET Framework 4.5.2 | ❌ **Removed** |
| .NET Core 3.0 / 3.1 | ❌ **Removed** |
| .NET 5.0 | ❌ **Removed** |
| .NET 6.0 / 7.0 / 8.0 | ❌ **Not targeted** |

**Supported frameworks in v5.0.0:**
- .NET Framework 4.8
- .NET 9.0 (with `-windows` TFM)
- .NET 10.0 (with `-windows` TFM)

**Fix:** Update your project to target one of the supported frameworks:

```xml
<!-- .NET 9 -->
<TargetFramework>net9.0-windows</TargetFramework>

<!-- .NET 10 -->
<TargetFramework>net10.0-windows</TargetFramework>

<!-- .NET Framework 4.8 -->
<TargetFramework>net48</TargetFramework>

<!-- Multi-target -->
<TargetFrameworks>net10.0-windows;net9.0-windows;net48</TargetFrameworks>
```

---

## New Features (Non-Breaking)

These additions are new in v5.0.0 and do not break existing code:

| Feature | Package | Description |
|:--------|:--------|:------------|
| ToggleDockingManager | `AvalonDock` | VS Code / Rider-style sidebar with toggle buttons. |
| Arc Theme | `AvalonDock.Themes.Arc` | Modern theme with dark/light variants. |
| JSON Serializer | `AvalonDock.Serializer.Json` | JSON-based layout serialization. |
| MVVM Base Classes | `AvalonDock.Mvvm` | `DockableBase`, `ToolboxBase`, `DockLayoutService`, etc. |
| MVVM CommunityToolkit | `AvalonDock.Mvvm.CommunityToolkit` | `ObservableDockableBase`, `ObservableToolboxBase` with source generators. |
| DI Integration | `AvalonDock.DependencyInjection` | `AddAvalonDock()` extension method. |
| Core Abstractions | `AvalonDock.Core` | `IFactory`, `IDockingManager`, `IAutoHideManager`, etc. |
| DTO Serialization | `AvalonDock.Core` | Serialization refactored to DTO layer; custom serializers can extend `LayoutSerializerBase`. |

---

## Behavioral Changes

### Layout Restacking

A bug fix in v5.0.0 corrects the restacking behavior for bottom-docked panels. If your application relied on the previous (incorrect) behavior, you may notice panels appearing in different positions after restacking.

**Fix:** Test your layouts and adjust panel placement if needed.

---

## Summary Table

| Category | Change | Impact | Action |
|:---------|:-------|:-------|:-------|
| Packages | Serializers separated | High | Install serializer package |
| Namespaces | Serializer namespace moved | High | Update `using` statements |
| Architecture | `ILayoutEngine` added | Low | No action for default behavior |
| Frameworks | .NET < 4.8 dropped | High | Upgrade target framework |
| Frameworks | .NET Core 3.x / 5 dropped | High | Upgrade target framework |
| Themes | Arc theme added | None | Optional adoption |
| Serialization | JSON serializer added | None | Optional adoption |
| Behavior | Bottom restack fix | Low | Test and verify layouts |
