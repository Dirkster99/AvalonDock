---
title: Events
layout: default
parent: API Reference
nav_order: 4
description: "Reference for events raised during docking operations."
---

# Events

AvalonDock uses an event-driven model to notify your application of docking operations. Many events follow a cancellable `Closing`/`Closed` pattern.

---

## DockingManager Events

### Document Lifecycle

| Event | EventArgs | Cancellable | Description |
|:------|:----------|:------------|:------------|
| `DocumentClosing` | `DocumentClosingEventArgs` | ✅ | Raised before a document is closed. Set `Cancel = true` to prevent. |
| `DocumentClosed` | `DocumentClosedEventArgs` | — | Raised after a document has been closed. |

```csharp
dockManager.DocumentClosing += (sender, args) =>
{
    // Prompt to save changes
    if (HasUnsavedChanges(args.Document))
    {
        var result = MessageBox.Show("Save changes?", "Confirm",
            MessageBoxButton.YesNoCancel);

        if (result == MessageBoxResult.Cancel)
            args.Cancel = true;
        else if (result == MessageBoxResult.Yes)
            Save(args.Document);
    }
};
```

### Anchorable Lifecycle

| Event | EventArgs | Cancellable | Description |
|:------|:----------|:------------|:------------|
| `AnchorableClosing` | `AnchorableClosingEventArgs` | ✅ | Before an anchorable closes. |
| `AnchorableClosed` | `AnchorableClosedEventArgs` | — | After an anchorable is closed. |
| `AnchorableHiding` | `AnchorableHidingEventArgs` | ✅ | Before an anchorable hides. |
| `AnchorableHidden` | `AnchorableHiddenEventArgs` | — | After an anchorable is hidden. |

```csharp
dockManager.AnchorableHiding += (sender, args) =>
{
    // Prevent hiding of critical panels
    if (args.Anchorable.ContentId == "errorList")
    {
        args.Cancel = true;
        MessageBox.Show("The Error List cannot be hidden.");
    }
};
```

### Docking Operations

| Event | EventArgs | Description |
|:------|:----------|:------------|
| `ContentDocked` | `ContentDockedEventArgs` | Content has been docked from a floating state. |
| `ContentFloating` | `ContentFloatingEventArgs` | Content has been floated from a docked state. |
| `LayoutFloatingWindowControlClosed` | `LayoutFloatingWindowControlClosedEventArgs` | A floating window control was closed. |

### Layout Changes

| Event | EventArgs | Description |
|:------|:----------|:------------|
| `ActiveContentChanged` | `EventArgs` | The active content has changed. |
| `LayoutChanged` | `EventArgs` | The layout structure has been modified. |
| `LayoutChanging` | `EventArgs` | The layout structure is about to be modified. |

```csharp
dockManager.ActiveContentChanged += (sender, args) =>
{
    var active = dockManager.ActiveContent;
    StatusBar.Text = $"Active: {active?.GetType().Name ?? "None"}";
};
```

---

## LayoutContent Events

These events are available on individual `LayoutDocument` and `LayoutAnchorable` instances:

| Event | Description |
|:------|:------------|
| `Closing` | Before this content closes. Cancellable. |
| `Closed` | After this content is closed. |
| `IsActiveChanged` | When the `IsActive` property changes. |
| `IsSelectedChanged` | When the `IsSelected` property changes. |

---

## EventArgs Reference

### DocumentClosingEventArgs

| Property | Type | Description |
|:---------|:-----|:------------|
| `Document` | `LayoutDocument` | The document being closed. |
| `Cancel` | `bool` | Set to `true` to prevent closing. |

### AnchorableClosingEventArgs

| Property | Type | Description |
|:---------|:-----|:------------|
| `Anchorable` | `LayoutAnchorable` | The anchorable being closed. |
| `Cancel` | `bool` | Set to `true` to prevent closing. |

### AnchorableHidingEventArgs

| Property | Type | Description |
|:---------|:-----|:------------|
| `Anchorable` | `LayoutAnchorable` | The anchorable being hidden. |
| `Cancel` | `bool` | Set to `true` to prevent hiding. |

### ContentDockedEventArgs

| Property | Type | Description |
|:---------|:-----|:------------|
| `Content` | `LayoutContent` | The content that was docked. |

### ContentFloatingEventArgs

| Property | Type | Description |
|:---------|:-----|:------------|
| `Content` | `LayoutContent` | The content that started floating. |

---

## Common Patterns

### Save-on-Close

```csharp
dockManager.DocumentClosing += async (sender, args) =>
{
    if (args.Document.Content is IDocument doc && doc.IsDirty)
    {
        args.Cancel = true; // Prevent immediate close

        var result = await ShowSaveDialogAsync(doc);
        if (result != SaveResult.Cancel)
        {
            if (result == SaveResult.Save)
                await doc.SaveAsync();

            args.Document.Close(); // Close after handling
        }
    }
};
```

### Track Active Document

```csharp
dockManager.ActiveContentChanged += (sender, args) =>
{
    if (dockManager.ActiveContent is MyDocumentViewModel doc)
    {
        Title = $"MyApp - {doc.FileName}";
    }
};
```
