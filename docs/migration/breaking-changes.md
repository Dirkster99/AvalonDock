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
| Standalone Windows | `AvalonDock` | `DetachAnchorableToWindow` moves a tool window into an ordinary top level window with its own taskbar entry ("Window" view mode). Survives layout serialization. |
| Toolbox Shortcuts | `AvalonDock` | `IToolbox.Shortcut` registers a key binding that toggles the toolbox and shows up in its tooltip. |
| Arc Theme | `AvalonDock.Themes.Arc` | Modern theme with dark/light variants. |
| VS Themes | `AvalonDock.Themes.VS` | `.vstheme` based Visual Studio themes with VS2015, VS2022 and VS2026 variants, plus loading of custom theme files. |
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

### Unresolved Items Are Dropped on Deserialization

**Impact:** Low — affects applications that read `LayoutRoot.Hidden` after restoring a layout.

When a stored layout contains an item whose content cannot be resolved — a tool window the
application no longer offers, or one the `LayoutSerializationCallback` supplies no content for —
that item is now removed from the layout entirely.

In v4 an unresolved *anchorable* was hidden instead: it stayed in `LayoutRoot.Hidden` with the pane
and index it was stored at, and was written back into every layout saved afterwards. Unresolved
*documents* were already removed, so anchorables were the exception rather than the rule. The v4
source described both cases as "skip this".

**Fix:** Nothing to do in most applications. If yours attaches content after the layout was restored
— a plugin that loads late, or a tool window created on first use that expects to find itself in the
hidden list — request the old behavior explicitly:

```csharp
var serializer = new XmlLayoutSerializer(dockManager)
{
    UnresolvedContentHandling = UnresolvedContentHandling.Hide
};
```

Prefer deciding per item where you can. Only the application can tell a tool window that will never
come back from one whose plugin simply has not loaded yet, and parking only the latter keeps the
stored layout free of entries nothing will ever claim:

```csharp
serializer.LayoutSerializationCallback += (s, args) =>
{
    var content = FindContent(args.Model.ContentId);
    if (content != null)
    {
        args.Content = content;
        return;
    }

    // A plugin we know about but have not loaded yet: keep the entry so the tool window can
    // return to its stored position once the plugin supplies its content.
    if (_knownPluginContentIds.Contains(args.Model.ContentId))
        args.UnresolvedContentHandling = UnresolvedContentHandling.Hide;
};
```

Restoring a parked item once its content arrives:

```csharp
var parked = dockManager.Layout.Hidden.FirstOrDefault(a => a.ContentId == contentId);
if (parked != null)
{
    parked.Content = pluginViewModel;
    parked.Show();
}
```

`Show()` puts the anchorable back into the pane and at the index it was stored at. If that pane is no
longer part of the layout — a restored layout rebuilds the docked area from scratch — it docks to an
existing tool window pane instead, creating one if the layout has none, so the call never silently
does nothing. Under `ToggleDockingManager` use `RestoreHiddenAnchorable`, which places the tool
window on the sidebar of its zone.

Note that `args.Cancel = true` always drops the item, under either setting.

### ToolTip Is No Longer Stored in the Layout

**Impact:** Low — affects applications that set `LayoutContent.ToolTip` and never set it again.

A tool tip belongs to the content, not to the layout. `LayoutItem` pushes it down from the view every
time the view is attached, and it is routinely a control or a binding, neither of which a layout file
can describe. v4 wrote the text form of it into the file and read it back; v5 leaves it out.

**Fix:** Set the tool tip where you set the rest of the content — in the view, or on the view model
the `LayoutSerializationCallback` supplies. A `ToolTip` attribute in a file written by v4 is ignored
on read, so existing layout files still load.

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
| Behavior | Unresolved layout items dropped | Low | Set `UnresolvedContentHandling.Hide` if you relied on `LayoutRoot.Hidden` |
| Behavior | `ToolTip` no longer stored in the layout | Low | Set it in the view or on the view model |
