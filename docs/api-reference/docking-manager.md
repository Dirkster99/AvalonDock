---
title: DockingManager
layout: default
parent: API Reference
nav_order: 1
description: "API reference for the DockingManager control."
---

# DockingManager

`AvalonDock.DockingManager` is the root control of AvalonDock. It hosts the entire docking layout, manages floating windows, handles drag-and-drop operations, and applies themes.

---

## Inheritance

```
System.Windows.Controls.Control
  └── DockingManager
```

**Implements:** `IOverlayWindowHost`, `ISerializableDockingManager`, `IDockingManager`

---

## Key Properties

### Layout

| Property | Type | Description |
|:---------|:-----|:------------|
| `Layout` | `LayoutRoot` | The root of the layout tree. Set this to define or replace the entire layout. |
| `DockLayout` | `IRootDock` | The MVVM layout model. When set, the manager keeps this ViewModel tree in sync with the internal layout. Leave it `null` for classic (v4.x) mode. |
| `Theme` | `Theme` | The active theme. Change at runtime to switch themes. |
| `ActiveContent` | `object` | The currently active content (document or anchorable). Bindable. |
| `LayoutEngine` | `ILayoutEngine` | The engine performing layout tree operations. Overridden by `ToggleDockingManager`. |

### Data Binding (MVVM)

| Property | Type | Description |
|:---------|:-----|:------------|
| `DocumentsSource` | `IEnumerable` | Bind to a collection of document view models. |
| `AnchorablesSource` | `IEnumerable` | Bind to a collection of anchorable view models. |
| `LayoutItemTemplate` | `DataTemplate` | Default template for rendering the content of documents and anchorables. |
| `LayoutItemTemplateSelector` | `DataTemplateSelector` | Select templates based on content type. |
| `DocumentHeaderTemplate` | `DataTemplate` | Template for the tab header of a document. |
| `DocumentTitleTemplate` | `DataTemplate` | Template for the title of a document. |
| `AnchorableHeaderTemplate` | `DataTemplate` | Template for the header of an anchorable. |
| `AnchorableTitleTemplate` | `DataTemplate` | Template for the title of an anchorable. |
| `LayoutItemContainerStyle` | `Style` | Style applied to `LayoutItem` containers (target type: `LayoutItem`). |
| `LayoutItemContainerStyleSelector` | `StyleSelector` | Select container styles based on content type. |

### Behavior

| Property | Type | Description |
|:---------|:-----|:------------|
| `AllowMixedOrientation` | `bool` | Allow panels with mixed horizontal/vertical orientation. |
| `ShowSystemMenu` | `bool` | Show system menu on floating windows. |
| `AllowDrop` | `bool` | Enable drag-and-drop docking (inherited from WPF). |
| `AutoHideDelay` | `int` | Delay in milliseconds before an auto-hidden tool window slides shut. |
| `SupportsAutoHideFlyout` | `bool` | Whether auto-hidden anchorables are shown in the classic slide-out flyout. `false` for `ToggleDockingManager`, which presents them through its sidebar buttons. |

### Layout Update Strategy

| Property | Type | Description |
|:---------|:-----|:------------|
| `LayoutUpdateStrategy` | `ILayoutUpdateStrategy` | Custom strategy for handling layout updates during drag operations. |

---

## Key Methods

| Method | Returns | Description |
|:-------|:--------|:------------|
| `GetFloatingWindows()` | `IEnumerable<LayoutFloatingWindowControl>` | Get all active floating windows. |
| `DetachAnchorableToWindow(anchorable)` | `void` | Move the content of an anchorable into a standalone top level window. See [Floating Windows]({% link concepts/floating-windows.md %}). |
| `ReattachAnchorable(anchorable)` | `void` | Close that window and return the content to the layout. |
| `ReattachAllDetachedAnchorables()` | `void` | Return every detached anchorable to the layout. |
| `IsDetached(anchorable)` | `bool` | Whether the anchorable is currently hosted by a standalone window. |
| `DetachedAnchorables` | `IEnumerable<LayoutAnchorable>` | The anchorables currently hosted by standalone windows. |

---

## Events

### Document Events

| Event | Args | Description |
|:------|:-----|:------------|
| `DocumentClosing` | `DocumentClosingEventArgs` | Before a document closes. Cancellable. |
| `DocumentClosed` | `DocumentClosedEventArgs` | After a document is closed. |

### Anchorable Events

| Event | Args | Description |
|:------|:-----|:------------|
| `AnchorableClosing` | `AnchorableClosingEventArgs` | Before an anchorable closes. Cancellable. |
| `AnchorableClosed` | `AnchorableClosedEventArgs` | After an anchorable is closed. |
| `AnchorableHiding` | `AnchorableHidingEventArgs` | Before an anchorable hides. Cancellable. |
| `AnchorableHidden` | `AnchorableHiddenEventArgs` | After an anchorable is hidden. |

### Docking Events

| Event | Args | Description |
|:------|:-----|:------------|
| `ContentFloating` | `ContentFloatingEventArgs` | Before docked content starts floating. Cancellable. |
| `ContentFloated` | `ContentFloatedEventArgs` | After content has been floated. |
| `ContentDocking` | `ContentDockingEventArgs` | Before floating content is docked back. Cancellable. |
| `ContentDocked` | `ContentDockedEventArgs` | After content has been docked. |
| `LayoutFloatingWindowControlCreated` | `LayoutFloatingWindowControlCreatedEventArgs` | When a floating window control is created. |
| `LayoutFloatingWindowControlClosed` | `LayoutFloatingWindowControlClosedEventArgs` | When a floating window control is closed. |

### Layout Events

| Event | Args | Description |
|:------|:-----|:------------|
| `ActiveContentChanged` | `EventArgs` | When the active content changes. |
| `LayoutChanged` | `EventArgs` | When the layout structure changes. |
| `LayoutChanging` | `EventArgs` | Before the layout structure changes. |

---

## XAML Example

```xml
<avalonDock:DockingManager
    x:Name="dockManager"
    DocumentsSource="{Binding Documents}"
    AnchorablesSource="{Binding Tools}"
    ActiveContent="{Binding ActiveContent, Mode=TwoWay}"
    DocumentClosing="OnDocumentClosing"
    AnchorableHiding="OnAnchorableHiding">

    <avalonDock:DockingManager.Theme>
        <themes:ArcDarkTheme />
    </avalonDock:DockingManager.Theme>

    <avalonDock:DockingManager.DocumentTemplate>
        <DataTemplate>
            <ContentPresenter Content="{Binding}" />
        </DataTemplate>
    </avalonDock:DockingManager.DocumentTemplate>

    <avalonDock:LayoutRoot>
        <avalonDock:LayoutPanel Orientation="Horizontal">
            <avalonDock:LayoutAnchorablePane DockWidth="250" />
            <avalonDock:LayoutDocumentPane />
        </avalonDock:LayoutPanel>
    </avalonDock:LayoutRoot>
</avalonDock:DockingManager>
```

---

## Code Example

```csharp
// Create and configure a DockingManager
var dockManager = new DockingManager();

// Set theme
dockManager.Theme = new ArcDarkTheme();

// Handle document closing
dockManager.DocumentClosing += (sender, args) =>
{
    if (args.Document.Content is IModified modified && modified.HasChanges)
    {
        var result = MessageBox.Show(
            "Save changes?", "Confirm",
            MessageBoxButton.YesNoCancel);

        if (result == MessageBoxResult.Cancel)
            args.Cancel = true;
    }
};

// Set the layout
dockManager.Layout = new LayoutRoot
{
    RootPanel = new LayoutPanel(new LayoutDocumentPane())
};
```
