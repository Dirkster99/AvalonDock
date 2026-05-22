---
title: Controls
layout: default
parent: API Reference
nav_order: 3
description: "Reference for AvalonDock's WPF controls."
---

# Controls

AvalonDock provides a rich set of WPF controls in the `AvalonDock.Controls` namespace. These controls are the visual representation of the layout model.

---

## Primary Controls

### LayoutItem

Base class for items displayed in the docking manager. Used with `LayoutItemContainerStyle` for MVVM binding.

| Property | Type | Description |
|:---------|:-----|:------------|
| `Title` | `string` | Display title (bindable). |
| `ContentId` | `string` | Unique identifier (bindable). |
| `CanClose` | `bool` | Whether the item can be closed (bindable). |
| `CanFloat` | `bool` | Whether the item can be floated (bindable). |
| `IconSource` | `ImageSource` | Tab icon (bindable). |
| `CloseCommand` | `ICommand` | Command executed when the item is closed. |
| `FloatCommand` | `ICommand` | Command executed when the item is floated. |
| `DockAsDocumentCommand` | `ICommand` | Command to dock as a tabbed document. |
| `MoveToNextTabGroupCommand` | `ICommand` | Command to move to next tab group. |
| `MoveToPreviousTabGroupCommand` | `ICommand` | Command to move to previous tab group. |

### LayoutAnchorableItem

Extends `LayoutItem` with anchorable-specific properties.

| Property | Type | Description |
|:---------|:-----|:------------|
| `CanHide` | `bool` | Whether the anchorable can be hidden. |
| `HideCommand` | `ICommand` | Command to hide the anchorable. |
| `AutoHideCommand` | `ICommand` | Command to toggle auto-hide. |

### LayoutDocumentItem

Extends `LayoutItem` for documents. Inherits all `LayoutItem` properties.

---

## Container Controls

| Control | Description |
|:--------|:------------|
| `LayoutAnchorableControl` | Renders the content of a `LayoutAnchorable`. |
| `LayoutDocumentControl` | Renders the content of a `LayoutDocument`. |
| `LayoutAnchorablePaneControl` | Renders a `LayoutAnchorablePane` with tabs. |
| `LayoutDocumentPaneControl` | Renders a `LayoutDocumentPane` with document tabs. |
| `LayoutAnchorablePaneGroupControl` | Renders a group of anchorable panes. |
| `LayoutDocumentPaneGroupControl` | Renders a group of document panes. |
| `LayoutPanelControl` | Renders a `LayoutPanel` with splitters between children. |
| `LayoutAutoHideWindowControl` | The popup window that appears when hovering an auto-hide tab. |

---

## Floating Window Controls

| Control | Description |
|:--------|:------------|
| `LayoutFloatingWindowControl` | Abstract base for floating windows. |
| `LayoutAnchorableFloatingWindowControl` | Floating window for anchorable content. |
| `LayoutDocumentFloatingWindowControl` | Floating window for document content. |

---

## Tab Controls

| Control | Description |
|:--------|:------------|
| `TabControlEx` | Extended tab control with optimized layout switching. |
| `LayoutDocumentTabItem` | Individual document tab. |
| `LayoutAnchorableTabItem` | Individual anchorable tab. |
| `AnchorablePaneTabPanel` | Tab strip for anchorable panes. |
| `DocumentPaneTabPanel` | Tab strip for document panes. |

---

## Drag & Drop Controls

| Control | Description |
|:--------|:------------|
| `OverlayWindow` | Transparent overlay showing docking indicators during drag. |
| `OverlayArea` | Area within the overlay window for drop detection. |
| `AnchorablePaneDropTarget` | Drop target for anchorable panes. |
| `DocumentPaneDropTarget` | Drop target for document panes. |
| `DockingManagerDropTarget` | Drop target at the edge of the docking manager. |
| `DropArea` | Defines a region where content can be dropped. |

---

## Utility Controls

| Control | Description |
|:--------|:------------|
| `DropDownButton` | Button with a dropdown menu (used in tab overflow). |
| `MenuItemEx` | Extended menu item with icon support. |
| `LayoutGridResizerControl` | The resizer/splitter between docked panels. |
| `NavigatorWindow` | Ctrl+Tab navigation window for switching between items. |
