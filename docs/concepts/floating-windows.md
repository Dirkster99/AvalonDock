---
title: Floating Windows
layout: default
parent: Concepts
nav_order: 4
description: "How floating windows work in AvalonDock."
---

# Floating Windows

Floating windows allow users to tear off documents or tool windows into independent, freely movable windows. This is one of AvalonDock's most powerful features, enabling flexible multi-monitor workflows.

---

## How Floating Works

Any dockable content — documents or anchorables — can be floated by:

1. **Dragging** a tab away from its pane
2. **Double-clicking** a tab header (depending on configuration)
3. **Programmatically** calling `Float()` on a layout content

When content is floated, AvalonDock creates a floating window that:
- Behaves as an independent window
- Can be moved freely, including to other monitors
- Can be docked back by dragging it over a docking target
- Maintains its content and state

---

## Floating Window Types

| Type | Contains | Created When |
|:-----|:---------|:-------------|
| `LayoutAnchorableFloatingWindow` | Anchorable panes | An anchorable is floated |
| `LayoutDocumentFloatingWindow` | Document panes | A document is floated |

---

## Controlling Float Behavior

### Prevent Floating

Set `CanFloat` to `false` on any content to prevent it from being floated:

```xml
<avalonDock:LayoutAnchorable Title="Fixed Panel"
                              CanFloat="False">
    <!-- This panel cannot be torn off -->
</avalonDock:LayoutAnchorable>
```

### Float Programmatically

```csharp
// Float an anchorable
var anchorable = layout.Descendents()
    .OfType<LayoutAnchorable>()
    .First(a => a.ContentId == "properties");

anchorable.Float();
```

---

## Docking Targets

When a user drags a floating window over the main docking area, **docking indicators** appear showing valid drop targets:

- **Center** — Tab into the target pane
- **Top / Bottom / Left / Right** — Split and dock to that side
- **Edge indicators** — Dock to the edge of the entire docking area

The docking indicators are [defined in XAML](https://github.com/Dirkster99/AvalonDock/wiki/OverlayWindow), ensuring crisp rendering on all resolutions including 4K and 8K displays.

---

## Events

| Event | Description |
|:------|:------------|
| `ContentFloating` | Raised on `DockingManager` when content starts floating. |
| `ContentDocked` | Raised on `DockingManager` when floating content is docked. |
| `LayoutFloatingWindowControlClosed` | Raised when a floating window is closed. |

```csharp
dockManager.ContentFloating += (sender, args) =>
{
    // React to content being floated
    Console.WriteLine($"Floating: {args.Content.Title}");
};

dockManager.ContentDocked += (sender, args) =>
{
    // React to content being docked
    Console.WriteLine($"Docked: {args.Content.Title}");
};
```

---

## Multi-Monitor Support

Floating windows are standard WPF windows and fully support multi-monitor setups. Users can:

- Drag floating windows to any monitor
- Snap floating windows using Windows snap features
- Arrange multiple floating windows side by side

Layout serialization preserves floating window positions, so the user's multi-monitor arrangement is restored when the layout is loaded.
