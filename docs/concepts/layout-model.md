---
title: Layout Model
layout: default
parent: Concepts
nav_order: 2
description: "Understanding the tree-based layout model in AvalonDock."
---

# Layout Model

AvalonDock organizes its docking layout as a **tree of layout objects**. Understanding this tree is key to building and manipulating complex layouts.

---

## The Layout Tree

Every AvalonDock layout is rooted in a `LayoutRoot` object. The tree structure looks like this:

```
LayoutRoot
├── LayoutPanel (Orientation: Horizontal)
│   ├── LayoutAnchorablePane
│   │   └── LayoutAnchorable ("Explorer")
│   ├── LayoutDocumentPaneGroup
│   │   ├── LayoutDocumentPane
│   │   │   ├── LayoutDocument ("File1.cs")
│   │   │   └── LayoutDocument ("File2.cs")
│   │   └── LayoutDocumentPane
│   │       └── LayoutDocument ("File3.cs")
│   └── LayoutAnchorablePane
│       └── LayoutAnchorable ("Properties")
├── BottomSide: LayoutAnchorSide
│   └── LayoutAnchorGroup
│       └── LayoutAnchorable ("Output")  [auto-hidden]
├── TopSide: LayoutAnchorSide
├── LeftSide: LayoutAnchorSide
└── RightSide: LayoutAnchorSide
```

---

## Layout Classes

### Container Elements

These elements **contain** other layout elements:

| Class | Purpose |
|:------|:--------|
| `LayoutRoot` | Root of the entire layout tree. Has a `RootPanel` and four `LayoutAnchorSide`s. |
| `LayoutPanel` | A panel that arranges children horizontally or vertically. The primary structural element. |
| `LayoutDocumentPane` | A tabbed container for `LayoutDocument` items. |
| `LayoutDocumentPaneGroup` | Groups multiple `LayoutDocumentPane`s with an orientation for split views. |
| `LayoutAnchorablePane` | A tabbed container for `LayoutAnchorable` items. |
| `LayoutAnchorablePaneGroup` | Groups multiple `LayoutAnchorablePane`s with an orientation. |
| `LayoutAnchorSide` | Represents one side (Top, Bottom, Left, Right) for auto-hidden anchorables. |
| `LayoutAnchorGroup` | Groups auto-hidden anchorables on an anchor side. |

### Content Elements

These represent actual **content** that users interact with:

| Class | Purpose |
|:------|:--------|
| `LayoutDocument` | A document tab — closable by default, not hideable. |
| `LayoutAnchorable` | A tool window — hideable by default, not closable. |

### Floating Elements

| Class | Purpose |
|:------|:--------|
| `LayoutAnchorableFloatingWindow` | A floating window containing anchorable panes. |
| `LayoutDocumentFloatingWindow` | A floating window containing document panes. |

---

## Layout Properties

### Orientation

`LayoutPanel` and pane groups support `Orientation`:

```xml
<!-- Stack children left to right -->
<avalonDock:LayoutPanel Orientation="Horizontal">
    ...
</avalonDock:LayoutPanel>

<!-- Stack children top to bottom -->
<avalonDock:LayoutPanel Orientation="Vertical">
    ...
</avalonDock:LayoutPanel>
```

### Size Control

Use `DockWidth` and `DockHeight` to control initial sizes:

```xml
<avalonDock:LayoutAnchorablePane DockWidth="250">
    ...
</avalonDock:LayoutAnchorablePane>

<avalonDock:LayoutPanel Orientation="Vertical">
    <avalonDock:LayoutDocumentPane DockHeight="*" />
    <avalonDock:LayoutAnchorablePane DockHeight="200" />
</avalonDock:LayoutPanel>
```

Sizes can be:
- **Fixed pixels**: `DockWidth="250"`
- **Star (proportional)**: `DockWidth="2*"` — takes twice the space of a `1*` sibling
- **Auto**: `DockWidth="Auto"` — sizes to content

---

## Navigating the Layout Tree

### From Code

```csharp
LayoutRoot layout = dockManager.Layout;

// Find all documents
var documents = layout.Descendents()
    .OfType<LayoutDocument>()
    .ToList();

// Find a specific anchorable by ContentId
var explorer = layout.Descendents()
    .OfType<LayoutAnchorable>()
    .FirstOrDefault(a => a.ContentId == "explorer");

// Iterate children of a panel
foreach (var child in layout.RootPanel.Children)
{
    // Process each child
}
```

### Key Layout Interfaces

| Interface | Description |
|:----------|:------------|
| `ILayoutElement` | Base interface for all layout elements. |
| `ILayoutContainer` | An element that contains children. |
| `ILayoutGroup` | A container with typed children. |
| `ILayoutPane` | A pane that can hold content. |
| `ILayoutOrientableGroup` | A group with an `Orientation` property. |
| `ILayoutRoot` | The root of the layout tree. |

---

## Programmatic Layout Manipulation

You can build layouts entirely in code:

```csharp
var layout = new LayoutRoot();
var panel = new LayoutPanel { Orientation = Orientation.Horizontal };

// Left tool panel
var toolPane = new LayoutAnchorablePane();
toolPane.Children.Add(new LayoutAnchorable
{
    Title = "Explorer",
    ContentId = "explorer"
});

// Center document area
var docPane = new LayoutDocumentPane();
docPane.Children.Add(new LayoutDocument
{
    Title = "Welcome",
    ContentId = "welcome"
});

panel.Children.Add(toolPane);
panel.Children.Add(docPane);
layout.RootPanel = panel;

dockManager.Layout = layout;
```
