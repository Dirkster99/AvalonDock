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
| `LayoutPriority` | `DockLayoutPriority` | `Default` | Controls layout restructuring mode |
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
        ToolTipText = "Explorer (Ctrl+Shift+E)";
        Icon = myExplorerIcon;  // ImageSource, UIElement, or DrawingImage
    }
}
```

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

Buttons display vertically rotated text by default. When an icon is set, the icon is shown alongside the title. Button visual states:

| State | Appearance |
|:------|:-----------|
| Default | Light gray text (#C5C5C5) |
| Hover | Light gray background |
| Checked (docked) | Medium gray background |
| Focused (active panel) | Blue background (#007ACC) with white text |

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
