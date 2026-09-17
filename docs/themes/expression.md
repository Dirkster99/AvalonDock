---
title: Expression Theme
layout: default
parent: Themes
nav_order: 4
description: "The Expression Blend theme for AvalonDock."
---

# Expression Theme

Inspired by Microsoft Expression Blend with a sleek, designer-focused aesthetic.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.Expression
```

## Usage

### Expression Dark

```csharp
using AvalonDock.Themes;

dockManager.Theme = new ExpressionDarkTheme();
```

### Expression Light

```csharp
dockManager.Theme = new ExpressionLightTheme();
```

## Docking hints

Both variants draw the drop-target hints shown while a pane is dragged, using the PNG glyphs
shipped in the package (`Images/DockDocument*.png`, `Images/DockAnchorable*.png`). Earlier
releases declared the `OverlayWindow` template without those glyphs, so the hints were invisible
and the centre "dock into" target had no clickable area.
