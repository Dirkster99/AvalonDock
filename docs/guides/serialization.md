---
title: Layout Serialization
layout: default
parent: Guides
nav_order: 4
description: "Save and restore docking layouts using XML or JSON serialization."
---

# Layout Serialization

AvalonDock can save and restore complex docking layouts, including panel positions, sizes, floating windows, and auto-hide state. This lets users arrange their workspace once and have it restored on the next launch.

---

## Choose a Serializer

| Format | Package | Class | Best For |
|:-------|:--------|:------|:---------|
| XML | `Dirkster.AvalonDock.Serializer.Xml` | `XmlLayoutSerializer` | Human-readable configs, legacy compatibility |
| JSON | `Dirkster.AvalonDock.Serializer.Json` | `JsonLayoutSerializer` | Modern apps, smaller file sizes |

```bash
# Install one or both
dotnet add package Dirkster.AvalonDock.Serializer.Xml
dotnet add package Dirkster.AvalonDock.Serializer.Json
```

---

## Save a Layout

### XML

```csharp
using AvalonDock.Serializer.Xml;

var serializer = new XmlLayoutSerializer(dockManager);

using (var writer = new StreamWriter("layout.xml"))
{
    serializer.Serialize(writer);
}
```

### JSON

```csharp
using AvalonDock.Serializer.Json;

var serializer = new JsonLayoutSerializer(dockManager);

using (var writer = new StreamWriter("layout.json"))
{
    serializer.Serialize(writer);
}
```

---

## Restore a Layout

### XML

```csharp
var serializer = new XmlLayoutSerializer(dockManager);

// Handle content that needs to be recreated
serializer.LayoutSerializationCallback += (sender, args) =>
{
    // Match serialized content by ContentId
    args.Content = args.Model.ContentId switch
    {
        "explorer"   => new ExplorerControl(),
        "properties" => new PropertiesControl(),
        "output"     => new OutputControl(),
        _            => null  // Skip unknown content
    };
};

using (var reader = new StreamReader("layout.xml"))
{
    serializer.Deserialize(reader);
}
```

### JSON

```csharp
var serializer = new JsonLayoutSerializer(dockManager);

serializer.LayoutSerializationCallback += (sender, args) =>
{
    args.Content = ResolveContent(args.Model.ContentId);
};

using (var reader = new StreamReader("layout.json"))
{
    serializer.Deserialize(reader);
}
```

---

## LayoutSerializationCallback

The `LayoutSerializationCallback` is critical for restoring layouts. When a layout is deserialized, AvalonDock recreates the layout structure but needs you to provide the actual content (UI controls or view models) for each content item.

The callback receives a `LayoutSerializationCallbackEventArgs` with:

| Property | Type | Description |
|:---------|:-----|:------------|
| `Model` | `LayoutContent` | The layout model being deserialized. Use `ContentId` to identify it. |
| `Content` | `object` | Set this to the UI content or view model. Set to `null` to skip. |
| `Cancel` | `bool` | Set to `true` to skip this item entirely. |

---

## MVVM Serialization

When using MVVM with `DocumentsSource` and `AnchorablesSource`, the serialization callback should return view models instead of controls:

```csharp
serializer.LayoutSerializationCallback += (sender, args) =>
{
    // Find or create the view model
    var vm = _viewModels.FirstOrDefault(v => v.ContentId == args.Model.ContentId);

    if (vm != null)
    {
        args.Content = vm;
    }
    else
    {
        args.Cancel = true; // Skip items that no longer exist
    }
};
```

---

## What Gets Serialized

The serializer preserves:

- ✅ Panel positions and orientations
- ✅ Tab order within panes
- ✅ Panel sizes (`DockWidth`, `DockHeight`)
- ✅ Floating window positions and sizes
- ✅ Auto-hide state and side placement
- ✅ Active/selected state
- ✅ `ContentId` for each content item
- ✅ `Title` and other metadata

The serializer does **not** preserve:

- ❌ Actual UI content (restored via callback)
- ❌ View model state (you must persist this separately)
- ❌ Runtime event handlers

---

## Architecture: DTO-Based Serialization

Under the hood, AvalonDock v5 uses a DTO (Data Transfer Object) layer to decouple the WPF layout tree from the serialization format. This enables both XML and JSON serializers to share the same mapping logic, and makes it possible to implement custom serializers.

### Data Flow

```
Save:  LayoutRoot → LayoutDtoMapper.ToDto() → LayoutRootDto → Serializer → File
Load:  File → Serializer → LayoutRootDto → LayoutDtoMapper.FromDto() → LayoutRoot
```

The `ILayoutDtoMapper` interface defines the mapping:

```csharp
public interface ILayoutDtoMapper
{
    LayoutRootDto ToDto(ISerializableLayoutRoot layout);
    ISerializableLayoutRoot FromDto(LayoutRootDto dto);
}
```

### DTO Classes

All DTOs live in `AvalonDock.Core.Serialization.Dto`:

| DTO | Maps To |
|:----|:--------|
| `LayoutRootDto` | Root container (panels, sides, floating windows, hidden items) |
| `LayoutPanelDto` | Panel with orientation and children |
| `LayoutDocumentPaneDto` | Document tab container |
| `LayoutAnchorablePaneDto` | Tool window container |
| `LayoutDocumentPaneGroupDto` | Splitter group for document panes |
| `LayoutAnchorablePaneGroupDto` | Splitter group for tool panes |
| `LayoutDocumentDto` | Individual document |
| `LayoutAnchorableDto` | Individual tool window |
| `LayoutFloatingWindowDto` | Floating window (document or anchorable variant) |
| `LayoutAnchorSideDto` | Auto-hide side |
| `LayoutAnchorGroupDto` | Auto-hide group |

### Custom Serializer

To implement a custom serialization format, extend `LayoutSerializerBase` and override two methods:

```csharp
using AvalonDock.Core.Serialization;
using AvalonDock.Core.Serialization.Dto;

public class YamlLayoutSerializer : LayoutSerializerBase
{
    public YamlLayoutSerializer(IDockingManager manager) : base(manager) { }

    protected override void SerializeCore(Stream stream, LayoutRootDto dto)
    {
        // Serialize the DTO tree to your format
    }

    protected override LayoutRootDto DeserializeCore(Stream stream)
    {
        // Deserialize your format back to a DTO tree
        return dto;
    }
}
```

The base class handles all fixup logic: reconnecting content via `LayoutSerializationCallback`, restoring previous containers, and cleaning up empty panes.

---

## Auto-Save on Exit

A common pattern is to save the layout when the application closes:

```csharp
protected override void OnClosing(CancelEventArgs e)
{
    var serializer = new XmlLayoutSerializer(dockManager);
    var layoutPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyApp", "layout.xml");

    Directory.CreateDirectory(Path.GetDirectoryName(layoutPath));

    using (var writer = new StreamWriter(layoutPath))
    {
        serializer.Serialize(writer);
    }

    base.OnClosing(e);
}

protected override void OnContentRendered(EventArgs e)
{
    var layoutPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyApp", "layout.xml");

    if (File.Exists(layoutPath))
    {
        var serializer = new XmlLayoutSerializer(dockManager);
        serializer.LayoutSerializationCallback += OnLayoutDeserialization;

        using (var reader = new StreamReader(layoutPath))
        {
            serializer.Deserialize(reader);
        }
    }

    base.OnContentRendered(e);
}
```
