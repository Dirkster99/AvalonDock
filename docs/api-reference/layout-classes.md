---
title: Layout Classes
layout: default
parent: API Reference
nav_order: 2
description: "Reference for AvalonDock's layout model classes."
---

# Layout Classes

The layout model is the backbone of AvalonDock. All classes live in the `AvalonDock.Layout` namespace.

---

## Class Hierarchy

```
ILayoutElement
├── LayoutElement (abstract base)
│   ├── LayoutContent (abstract)
│   │   ├── LayoutDocument
│   │   └── LayoutAnchorable
│   ├── LayoutGroup<T> (abstract)
│   │   ├── LayoutPanel
│   │   ├── LayoutDocumentPane
│   │   ├── LayoutDocumentPaneGroup
│   │   ├── LayoutAnchorablePane
│   │   ├── LayoutAnchorablePaneGroup
│   │   ├── LayoutAnchorSide
│   │   ├── LayoutAnchorGroup
│   │   └── LayoutRoot
│   └── LayoutFloatingWindow (abstract)
│       ├── LayoutAnchorableFloatingWindow
│       └── LayoutDocumentFloatingWindow
```

---

## LayoutRoot

The root of every layout tree.

| Property | Type | Description |
|:---------|:-----|:------------|
| `RootPanel` | `LayoutPanel` | The main panel containing the docking layout. |
| `TopSide` | `LayoutAnchorSide` | Auto-hide side at the top. |
| `BottomSide` | `LayoutAnchorSide` | Auto-hide side at the bottom. |
| `LeftSide` | `LayoutAnchorSide` | Auto-hide side on the left. |
| `RightSide` | `LayoutAnchorSide` | Auto-hide side on the right. |
| `FloatingWindows` | `ObservableCollection<LayoutFloatingWindow>` | Active floating windows. |
| `Hidden` | `ObservableCollection<LayoutAnchorable>` | Hidden anchorables. |
| `ActiveContent` | `LayoutContent` | The currently active content element. |

### Methods

| Method | Description |
|:-------|:------------|
| `Descendents()` | Returns all descendant elements in the layout tree. |

---

## LayoutPanel

A container that arranges children horizontally or vertically. The primary structural element.

| Property | Type | Description |
|:---------|:-----|:------------|
| `Orientation` | `Orientation` | `Horizontal` or `Vertical` arrangement. |
| `Children` | `ObservableCollection<ILayoutPanelElement>` | Child layout elements. |

---

## LayoutContent

Abstract base class for `LayoutDocument` and `LayoutAnchorable`.

| Property | Type | Description |
|:---------|:-----|:------------|
| `Title` | `string` | Display title. |
| `ContentId` | `string` | Unique identifier for serialization. |
| `Content` | `object` | The actual content (UI element or view model). |
| `IsActive` | `bool` | Whether this content is currently active. |
| `IsSelected` | `bool` | Whether this content's tab is selected. |
| `IsLastFocusedDocument` | `bool` | Whether this was the last focused document. |
| `CanFloat` | `bool` | Whether this content can be floated. |
| `IconSource` | `ImageSource` | Icon displayed on the tab. |
| `ToolTip` | `object` | Tab tooltip. |

### Methods

| Method | Description |
|:-------|:------------|
| `Float()` | Float this content into a new floating window. |
| `Dock()` | Dock this content back to its previous position. |
| `DockAsDocument()` | Dock this content as a tabbed document. |
| `Close()` | Close this content. |

---

## LayoutDocument

Represents a document tab. Extends `LayoutContent`.

| Property | Type | Default | Description |
|:---------|:-----|:--------|:------------|
| `CanClose` | `bool` | `true` | Whether the document can be closed. |
| `CanHide` | `bool` | `false` | Documents cannot be hidden. |

---

## LayoutAnchorable

Represents a tool window. Extends `LayoutContent`.

| Property | Type | Default | Description |
|:---------|:-----|:--------|:------------|
| `CanClose` | `bool` | `false` | Whether the anchorable can be closed. |
| `CanHide` | `bool` | `true` | Whether the anchorable can be hidden. |
| `CanAutoHide` | `bool` | `true` | Whether auto-hide is available. |
| `CanDockAsTabbedDocument` | `bool` | `true` | Whether it can dock into the document area. |
| `AutoHideWidth` | `double` | — | Width when in auto-hide mode. |
| `AutoHideHeight` | `double` | — | Height when in auto-hide mode. |
| `AutoHideMinWidth` | `double` | — | Minimum width in auto-hide. |
| `AutoHideMinHeight` | `double` | — | Minimum height in auto-hide. |
| `IsVisible` | `bool` | — | Whether the anchorable is visible. |

### Methods

| Method | Description |
|:-------|:------------|
| `Show()` | Show the anchorable in its previous location. |
| `Hide()` | Hide the anchorable. |
| `ToggleAutoHide()` | Toggle auto-hide state. |

---

## Pane Classes

### LayoutDocumentPane

A tabbed container for `LayoutDocument` items.

| Property | Type | Description |
|:---------|:-----|:------------|
| `Children` | `ObservableCollection<LayoutContent>` | Document children. |
| `SelectedContentIndex` | `int` | Index of the selected tab. |

### LayoutAnchorablePane

A tabbed container for `LayoutAnchorable` items.

| Property | Type | Description |
|:---------|:-----|:------------|
| `Children` | `ObservableCollection<LayoutAnchorable>` | Anchorable children. |
| `DockWidth` | `GridLength` | Width when docked. |
| `DockHeight` | `GridLength` | Height when docked. |
| `DockMinWidth` | `double` | Minimum width. |
| `DockMinHeight` | `double` | Minimum height. |

### LayoutDocumentPaneGroup / LayoutAnchorablePaneGroup

Group multiple panes with an orientation for split views.

| Property | Type | Description |
|:---------|:-----|:------------|
| `Orientation` | `Orientation` | Arrangement direction. |
| `Children` | `ObservableCollection` | Child panes. |

---

## Interfaces

| Interface | Description |
|:----------|:------------|
| `ILayoutElement` | Base for all layout elements. Provides `Parent` property. |
| `ILayoutContainer` | Element that contains children. |
| `ILayoutGroup` | Typed container with `Children` collection. |
| `ILayoutPane` | A pane that holds content items. |
| `ILayoutOrientableGroup` | Group with `Orientation` property. |
| `ILayoutRoot` | The root element of the layout tree. |
| `ILayoutControl` | Bridge between layout model and WPF control. |
| `ILayoutContentSelector` | Selects content within a pane. |
| `IAdjustableSizeLayout` | Element with adjustable `DockWidth`/`DockHeight`. |
| `ILayoutElementWithVisibility` | Element with `IsVisible` property. |
