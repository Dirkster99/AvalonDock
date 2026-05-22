---
title: Documents & Anchorables
layout: default
parent: Concepts
nav_order: 3
description: "Understanding the two types of dockable content in AvalonDock."
---

# Documents & Anchorables

AvalonDock distinguishes between two types of dockable content: **Documents** and **Anchorables** (tool windows). Understanding the difference is essential for building effective docking layouts.

---

## At a Glance

| Feature | LayoutDocument | LayoutAnchorable |
|:--------|:---------------|:-----------------|
| **Typical use** | File editors, viewers | Tool windows, panels |
| **Closable by default** | ✅ Yes | ❌ No |
| **Hideable by default** | ❌ No | ✅ Yes |
| **Can auto-hide** | ❌ No | ✅ Yes |
| **Can float** | ✅ Yes | ✅ Yes |
| **IDE equivalent** | Editor tabs | Solution Explorer, Properties, Output |

---

## LayoutDocument

A `LayoutDocument` represents a piece of content that the user is actively working on — like a file in an editor. Documents:

- Live in a `LayoutDocumentPane`
- Are **closable** by default (`CanClose = true`)
- Cannot be **hidden** (`CanHide = false`)
- Cannot be **auto-hidden** to a side tab
- Appear as tabs in the document well

```xml
<avalonDock:LayoutDocumentPane>
    <avalonDock:LayoutDocument Title="MainWindow.xaml"
                               ContentId="mainWindow"
                               CanClose="True">
        <TextBox Text="Document content" AcceptsReturn="True" />
    </avalonDock:LayoutDocument>
</avalonDock:LayoutDocumentPane>
```

### Key Properties

| Property | Type | Description |
|:---------|:-----|:------------|
| `Title` | `string` | Display title shown on the tab. |
| `ContentId` | `string` | Unique identifier used for serialization and lookup. |
| `Content` | `object` | The UI content to display. |
| `CanClose` | `bool` | Whether the document can be closed. Default: `true`. |
| `CanFloat` | `bool` | Whether the document can be floated. Default: `true`. |
| `IsActive` | `bool` | Whether this document is currently active/focused. |
| `IsSelected` | `bool` | Whether this document's tab is selected in its pane. |
| `IconSource` | `ImageSource` | Icon displayed on the tab. |
| `ToolTip` | `object` | Tooltip shown when hovering the tab. |

### Events

| Event | Description |
|:------|:------------|
| `Closing` | Raised before the document closes. Can be cancelled. |
| `Closed` | Raised after the document is closed. |

---

## LayoutAnchorable

A `LayoutAnchorable` represents a supporting tool window — like Solution Explorer or the Properties panel. Anchorables:

- Live in a `LayoutAnchorablePane` or auto-hide in a `LayoutAnchorGroup`
- Are **not closable** by default (`CanClose = false`)
- Are **hideable** by default (`CanHide = true`)
- Can be **auto-hidden** to a side tab
- Can be docked to any side of the layout

```xml
<avalonDock:LayoutAnchorablePane>
    <avalonDock:LayoutAnchorable Title="Solution Explorer"
                                  ContentId="solutionExplorer"
                                  CanClose="False"
                                  CanHide="True"
                                  CanAutoHide="True">
        <TreeView>
            <!-- Explorer content -->
        </TreeView>
    </avalonDock:LayoutAnchorable>
</avalonDock:LayoutAnchorablePane>
```

### Key Properties

| Property | Type | Description |
|:---------|:-----|:------------|
| `Title` | `string` | Display title shown on the tab or header. |
| `ContentId` | `string` | Unique identifier for serialization and lookup. |
| `Content` | `object` | The UI content to display. |
| `CanClose` | `bool` | Whether the anchorable can be closed. Default: `false`. |
| `CanHide` | `bool` | Whether the anchorable can be hidden. Default: `true`. |
| `CanAutoHide` | `bool` | Whether auto-hide is available. Default: `true`. |
| `CanFloat` | `bool` | Whether the anchorable can be floated. Default: `true`. |
| `CanDockAsTabbedDocument` | `bool` | Whether it can be docked into the document area. |
| `AutoHideWidth` / `AutoHideHeight` | `double` | Size when in auto-hide mode. |
| `IsActive` | `bool` | Whether this anchorable is currently active. |
| `IsVisible` | `bool` | Whether the anchorable is currently visible (not hidden). |

### Events

| Event | Description |
|:------|:------------|
| `Closing` | Raised before the anchorable closes. Can be cancelled. |
| `Hiding` | Raised before the anchorable hides. Can be cancelled. |
| `IsVisibleChanged` | Raised when visibility changes. |

---

## Show Strategies

When making an anchorable visible programmatically, you can specify where it should appear using `AnchorableShowStrategy`:

```csharp
anchorable.Show();           // Show in its previous location
anchorable.Show(dockManager, AnchorableShowStrategy.Left);   // Dock to left
anchorable.Show(dockManager, AnchorableShowStrategy.Right);  // Dock to right
anchorable.Show(dockManager, AnchorableShowStrategy.Top);    // Dock to top
anchorable.Show(dockManager, AnchorableShowStrategy.Bottom); // Dock to bottom
```

---

## Auto-Hide

Anchorables can be auto-hidden to a side tab. When auto-hidden, they appear as a small tab on the edge of the docking area and expand when hovered or clicked.

```xml
<!-- Define auto-hidden anchorables on the bottom side -->
<avalonDock:LayoutRoot.BottomSide>
    <avalonDock:LayoutAnchorSide>
        <avalonDock:LayoutAnchorGroup>
            <avalonDock:LayoutAnchorable Title="Output"
                                          ContentId="output" />
            <avalonDock:LayoutAnchorable Title="Error List"
                                          ContentId="errorList" />
        </avalonDock:LayoutAnchorGroup>
    </avalonDock:LayoutAnchorSide>
</avalonDock:LayoutRoot.BottomSide>
```

Toggle auto-hide programmatically:

```csharp
anchorable.ToggleAutoHide();
```
