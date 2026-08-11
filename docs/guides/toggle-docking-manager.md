---
title: ToggleDockingManager
layout: default
parent: Guides
nav_order: 2
description: "VS Code / Rider-style sidebar toggle docking with ToggleDockingManager."
---

# ToggleDockingManager

`ToggleDockingManager` extends the classic `DockingManager` with a VS Code / Rider-style sidebar UI. Instead of auto-hide sliding panels, it shows **toggle buttons** on the left, right, and bottom edges. Clicking a button docks or hides the associated panel.

{: .tip }
For a full working example, see the `AvalonDockCodeApp` sample project in the repository.

---

## Quick Start

### XAML

```xml
<avalonDock:ToggleDockingManager
    x:Name="dockManager"
    DockLayout="{Binding DockLayout}"
    LayoutItemContainerStyleSelector="{StaticResource PanesStyleSelector}">

    <avalonDock:ToggleDockingManager.Theme>
        <avalonDockThemes:ArcDarkTheme />
    </avalonDock:ToggleDockingManager.Theme>

    <avalonDock:LayoutRoot>
        <avalonDock:LayoutPanel Orientation="Horizontal">
            <avalonDock:LayoutDocumentPaneGroup>
                <avalonDock:LayoutDocumentPane />
            </avalonDock:LayoutDocumentPaneGroup>
        </avalonDock:LayoutPanel>
    </avalonDock:LayoutRoot>
</avalonDock:ToggleDockingManager>
```

### Code-Behind

```csharp
dockManager.ButtonSize = 28;
dockManager.DefaultDockWidth = 280;
dockManager.DefaultDockHeight = 220;
dockManager.LayoutPriority = DockLayoutPriority.BottomFullWidth;
```

---

## Dock Zones

ToggleDockingManager organizes panels into **six zones** instead of four sides:

```
┌────────────┬──────────────────┬────────────┐
│  LeftTop   │                  │  RightTop  │
│  buttons   │                  │  buttons   │
│            │                  │            │
├............│    Document      │............│
│            │      Area        │            │
│ LeftBottom │                  │ RightBottom│
│  buttons   │                  │  buttons   │
├────────────┼──────────────────┼────────────┤
│ BottomLeft │   BottomRight    │            │
│  buttons   │    buttons       │            │
└────────────┴──────────────────┴────────────┘
```

The `DockZone` enum:

| Zone | Location |
|:-----|:---------|
| `LeftTop` | Left sidebar, top section |
| `LeftBottom` | Left sidebar, bottom section |
| `RightTop` | Right sidebar, top section |
| `RightBottom` | Right sidebar, bottom section |
| `BottomLeft` | Bottom panel, left section |
| `BottomRight` | Bottom panel, right section |

---

## Properties

| Property | Type | Default | Description |
|:---------|:-----|:--------|:------------|
| `LayoutPriority` | `DockLayoutPriority` | `BottomFullWidth` | Controls layout restructuring mode |
| `ButtonSize` | `double` | `25.0` | Size of sidebar toggle buttons |
| `DefaultDockWidth` | `double` | `250.0` | Default width for side panels |
| `DefaultDockHeight` | `double` | `200.0` | Default height for bottom panels |
| `ShowHeaderMinimizeButton` | `bool` | `true` | Show minimize button in panel headers |
| `ShowHeaderOptionsButton` | `bool` | `true` | Show three-dot options menu in panel headers |

---

## Layout Priority

Controls how docked panels relate to each other when multiple sides are open.

| Mode | Style | Description |
|:-----|:------|:------------|
| `BottomFullWidth` | Rider | Bottom panels span the full width; sidebars above them |
| `SidesFullHeight` | VS Code | Sidebars span full height; bottom panel constrained |
| `Default` | — | No restructuring; panes stay where inserted |

```csharp
dockManager.LayoutPriority = DockLayoutPriority.BottomFullWidth;
```

---

## MVVM with IToolbox

When using the v5 MVVM approach, implement `IToolbox` on your toolbox view models. The `ToggleDockingManager` automatically discovers registered toolboxes via DI and places them in the correct zones.

```csharp
public class ExplorerToolbox : ToolboxBase
{
    public ExplorerToolbox()
    {
        Id = "Explorer";
        Title = "Explorer";
        Zone = DockZone.LeftTop;
        IsOpenByDefault = true;
        ToolTipText = "Explorer";
        Shortcut = "Ctrl+Shift+E";
        Icon = myExplorerIcon;  // ImageSource, UIElement, or DrawingImage
    }
}
```

### Visibility on startup

`IsOpenByDefault` is the declarative default and seeds `IsOpen`; from then on `IsOpen` is the single
answer to whether a toolbox is showing. Both are read when the manager applies a layout, so a view
model built by a DI container may set `IsOpen` in its constructor — long before the manager exists —
and the toolbox still comes up docked:

```csharp
services.AddDockLayoutService(dock => dock.AddToolbox<ExplorerToolbox>());
// ExplorerToolbox sets IsOpen = true in its constructor: it is showing once the window loads.
```

A zone shows one toolbox at a time, so giving two toolboxes in the same zone `IsOpenByDefault = true`
docks only the last of them; the other is collapsed onto its stripe and reports `IsOpen == false`.
`IsOpen` always reports what the layout actually shows, whether the state was changed by a sidebar
button, a keyboard shortcut, or a sibling toolbox taking the zone.

An anchorable detached into its own window counts as open: setting `IsOpen = false` on it brings that
window forward rather than closing it. Dock it back first if you want it collapsed.

### Keyboard Shortcuts

Setting `IToolbox.Shortcut` to a WPF gesture string registers a `KeyBinding` on the host window that
toggles the toolbox, exactly as clicking its sidebar button would. The gesture is also appended to
the button's tooltip, so there is no need to write it into `ToolTipText` yourself:

```csharp
Shortcut = "Ctrl+Shift+E";   // tooltip becomes "Explorer (Ctrl+Shift+E)"
```

An unparsable gesture is ignored rather than throwing, and the bindings are rebuilt whenever the set
of registered toolboxes changes.

Register toolboxes with DI:

```csharp
services.AddDockLayoutService(dock =>
{
    dock.AddToolbox<ExplorerToolbox>();
    dock.AddToolbox<OutputToolbox>();
    dock.AddToolbox<SearchToolbox>();
});
```

See [MVVM Integration]({% link guides/mvvm.md %}) for the full pattern.

---

## Toggle Button Customization

### Icons and Tooltips

Use the `ToggleDock` attached properties to customize button appearance:

```xml
xmlns:avalonDockControls="clr-namespace:AvalonDock.Controls;assembly=AvalonDock"

<avalonDock:LayoutAnchorable
    Title="Explorer"
    avalonDockControls:ToggleDock.Icon="{StaticResource ExplorerIcon}"
    avalonDockControls:ToggleDock.ToolTip="File Explorer (Ctrl+Shift+E)">
    <!-- Panel content -->
</avalonDock:LayoutAnchorable>
```

| Attached Property | Type | Description |
|:------------------|:-----|:------------|
| `ToggleDock.Icon` | `object` | Icon displayed on the button (ImageSource, UIElement, or DrawingImage) |
| `ToggleDock.ToolTip` | `object` | Custom tooltip (overrides `Title`) |
| `ToggleDock.IconTemplate` | `DataTemplate` | Template for rendering the icon content |

When using MVVM, set these via the `IToolbox.Icon` and `IToolbox.ToolTipText` properties on your view model.

### Button Appearance

A button shows **either** an icon **or** its title, never both:

- **With an icon** (`ToggleDock.Icon`, `IToolbox.Icon`, or `LayoutAnchorable.IconSource`) the button is a square of `ButtonSize` showing the icon upright.
- **Without an icon** the title is rendered vertically (rotated 90°) through the docking manager's `AnchorableHeaderTemplate` — the same `DataTemplate` the classic auto-hide tabs use, so an icon-less sidebar matches the rest of the theme. Overriding `AnchorableHeaderTemplate` changes both.

Only the bar-width axis is pinned to `ButtonSize`; a title-only button grows along the bar so long titles are not clipped, with `ButtonSize` acting as its minimum. Note that a very long title therefore produces a very tall button.

Button visual states are supplied by the active theme, mapped onto the brushes that theme already uses for its auto-hide tabs:

| State | Appearance |
|:------|:-----------|
| Default | Theme auto-hide tab text color |
| Hover | Theme auto-hide tab hover background, plus a 2px accent stroke under the title |
| Checked (docked) | Theme auto-hide tab background with a 1px tab border |
| Focused (active panel) | Theme accent background with white text |

Because the title is rotated 90°, the hover stroke sits on the button's right edge — visually beneath the text, the same relationship the auto-hide tab's accent border has to its own label. Title-only buttons also carry 4px of padding at each end so the text is not flush against the button edges; icon buttons reset that to keep their square.

When no AvalonDock theme is applied, the fallback style in `generic.xaml` is used instead (light gray text on transparent, `#007ACC` focus). The foreground is exposed as `ToggleDockButton.ForegroundBrushKey` so the sidebar and the "hidden panels" button can be recolored from a single resource.

---

## Context Menu

Right-clicking a toggle button or using the header options menu provides:

- **Hide** — Hides the panel
- **Move To** — Relocate to any of the six zones
- **View Mode** — Switch between Float, Docked, and Hidden

---

## Drag and Drop

Toggle buttons support drag-and-drop to move panels between zones. Dragging a button displays a visual overlay showing the six drop zones with labels and highlights.

---

## Programmatic API

```csharp
// Toggle a panel's docked/hidden state
dockManager.ToggleAnchorable(anchorable, DockZone.LeftTop);

// Move a panel to a different zone
dockManager.MoveAnchorableToZone(anchorable, DockZone.RightTop);

// Restore a hidden panel
dockManager.RestoreHiddenAnchorable(anchorable);

// Remove a panel's button from all bars
dockManager.RemoveButtonFromAllBars(anchorable);
```

### Toggling Entire Sides

Use `SideToggleManager` to toggle all panels on a side at once (remembers previously open panels):

```csharp
var sideToggle = new SideToggleManager(dockLayoutService);

// Toggle all left panels
sideToggle.Toggle(ToolboxSide.Left);

// Toggle bottom panels
sideToggle.Toggle(ToolboxSide.Bottom);
```

---

## Differences from Classic DockingManager

| Aspect | DockingManager | ToggleDockingManager |
|:-------|:---------------|:---------------------|
| Side panels | Auto-hide sliding panels | Toggle button bars |
| Zones | 4 (Left, Right, Top, Bottom) | 6 (LeftTop/Bottom, RightTop/Bottom, BottomLeft/Right) |
| Layout engine | `DefaultLayoutEngine` | `ToggleLayoutEngine` |
| MVVM integration | `DocumentsSource`/`AnchorablesSource` | `IToolbox` with auto-discovery via DI |
| Drag target | Pane-level overlay | Zone-level overlay |
