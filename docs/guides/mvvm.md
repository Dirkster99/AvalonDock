---
title: MVVM Integration
layout: default
parent: Guides
nav_order: 1
description: "How to use AvalonDock with the MVVM pattern."
---

# MVVM Integration

AvalonDock v5 provides first-class support for the MVVM pattern through the `AvalonDock.Mvvm` and `AvalonDock.Core` packages. This guide covers both the **v5 recommended approach** (using `IDockLayoutService` + DI) and the **classic approach** (using `DocumentsSource`/`AnchorablesSource`).

{: .tip }
For a full step-by-step tutorial, see [Tutorial: MVVM IDE Application]({% link tutorials/mvvm-ide.md %}). The `AvalonDockCodeApp` sample project in the repository is the reference implementation.

---

## Install

```bash
dotnet add package Dirkster.AvalonDock.Mvvm
dotnet add package Dirkster.AvalonDock.DependencyInjection
dotnet add package CommunityToolkit.Mvvm
```

---

## v5 Recommended: IDockLayoutService + ToolboxBase

In v5, the layout tree is built automatically from registered `IToolbox` instances. Your `MainViewModel` receives `IDockLayoutService` via DI and uses it for all layout operations.

### Base Classes

| Class | Use For | Key Properties |
|:------|:--------|:---------------|
| `ToolboxBase` | Tool panels (Explorer, Output, etc.) | `Zone`, `Icon`, `IsOpenByDefault`, `ToolTipText` |
| `Document` | Document tabs (file editors) | `Title`, `Id`, `IsModified` |
| `DockableBase` | Abstract base for both | `CanClose`, `CanFloat`, `CanDrag`, `IsActive` |

### Define a Toolbox

```csharp
using AvalonDock.Core;
using AvalonDock.Mvvm;

public class ExplorerToolbox : ToolboxBase
{
    public ExplorerToolbox()
    {
        Id = "Explorer";
        Title = "Explorer";
        Zone = DockZone.LeftTop;      // Controls placement
        ToolTipText = "Explorer (Ctrl+Shift+E)";
    }
}
```

### Define a Document

```csharp
using AvalonDock.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class EditorTab : Document
{
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private string _filePath = string.Empty;

    public void LoadFile(string path)
    {
        FilePath = path;
        Title = System.IO.Path.GetFileName(path);
        Id = path;
        Content = System.IO.File.ReadAllText(path);
    }
}
```

### Use IDockLayoutService

```csharp
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class MainViewModel : ObservableObject
{
    private readonly IDockLayoutService _dockService;

    // Bind to ToggleDockingManager.DockLayout
    public IRootDock DockLayout => _dockService.Layout;

    // Typed access to toolboxes
    public ExplorerToolbox? Explorer => _dockService.GetAnchorable<ExplorerToolbox>();

    public MainViewModel(IDockLayoutService dockService)
    {
        _dockService = dockService;
    }

    public void OpenFile(string path)
    {
        _dockService.OpenOrActivateDocument(
            existing => existing.FilePath == path,
            () => { var tab = new EditorTab(); tab.LoadFile(path); return tab; });
    }
}
```

### Bind in XAML (ToggleDockingManager)

```xml
<avalonDock:ToggleDockingManager
    DockLayout="{Binding DockLayout}"
    LayoutItemContainerStyleSelector="{StaticResource PanesStyleSelector}">
    ...
</avalonDock:ToggleDockingManager>
```

---

## Classic Approach: DocumentsSource + AnchorablesSource

The classic approach uses `ObservableCollection` bindings — still fully supported in v5, and useful when you don't need the sidebar toggle UI.

### Define View Models

```csharp
using AvalonDock.Mvvm;

public class DocumentViewModel : DockableBase
{
    private string _text;

    public DocumentViewModel(string title)
    {
        Title = title;
        Id = Guid.NewGuid().ToString();
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
}

public class ToolViewModel : DockableBase
{
    public ToolViewModel(string title)
    {
        Title = title;
        Id = title.ToLowerInvariant().Replace(" ", "-");
        CanClose = false;
    }
}
```

### Bind in XAML (DockingManager)

```xml
<avalonDock:DockingManager DocumentsSource="{Binding Documents}"
                           AnchorablesSource="{Binding Tools}"
                           ActiveContent="{Binding ActiveDocument, Mode=TwoWay}">

    <avalonDock:DockingManager.DocumentTemplate>
        <DataTemplate DataType="{x:Type local:DocumentViewModel}">
            <TextBox Text="{Binding Text}" AcceptsReturn="True" />
        </DataTemplate>
    </avalonDock:DockingManager.DocumentTemplate>

    <avalonDock:LayoutRoot>
        <avalonDock:LayoutPanel Orientation="Horizontal">
            <avalonDock:LayoutAnchorablePane DockWidth="200" />
            <avalonDock:LayoutDocumentPane />
        </avalonDock:LayoutPanel>
    </avalonDock:LayoutRoot>
</avalonDock:DockingManager>
```

---

## LayoutItemContainerStyleSelector

Binds layout-level properties (`Title`, `ContentId`, `CanClose`) from your view model to the layout container. Use a `StyleSelector` to differentiate between toolboxes and documents:

```csharp
public class PanesStyleSelector : StyleSelector
{
    public Style? ToolboxStyle { get; set; }
    public Style? DocumentStyle { get; set; }

    public override Style? SelectStyle(object item, DependencyObject container)
    {
        if (item is IToolbox) return ToolboxStyle;
        if (item is EditorTab) return DocumentStyle;
        return base.SelectStyle(item, container);
    }
}
```

```xml
<local:PanesStyleSelector x:Key="PanesStyleSelector">
    <local:PanesStyleSelector.ToolboxStyle>
        <Style TargetType="{x:Type avalonDockControls:LayoutAnchorableItem}">
            <Setter Property="Title" Value="{Binding Model.Title}" />
            <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
        </Style>
    </local:PanesStyleSelector.ToolboxStyle>
    <local:PanesStyleSelector.DocumentStyle>
        <Style TargetType="{x:Type avalonDockControls:LayoutDocumentItem}">
            <Setter Property="Title" Value="{Binding Model.Title}" />
            <Setter Property="ToolTip" Value="{Binding Model.ToolTip}" />
            <Setter Property="IconSource" Value="{Binding Model.IconSource}" />
            <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
        </Style>
    </local:PanesStyleSelector.DocumentStyle>
</local:PanesStyleSelector>
```

---

## LayoutItemTemplateSelector

For different content types, use implicit `DataTemplate` declarations (matched by `DataType`) or a `DataTemplateSelector`:

```csharp
public class DockTemplateSelector : DataTemplateSelector
{
    public DataTemplate DocumentTemplate { get; set; }
    public DataTemplate ExplorerTemplate { get; set; }
    public DataTemplate PropertiesTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            DocumentViewModel => DocumentTemplate,
            ExplorerToolbox => ExplorerTemplate,
            PropertiesToolbox => PropertiesTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
```

---

## IDockLayoutService API Reference

| Member | Description |
|:-------|:------------|
| `Layout` | The `IRootDock` tree — bind to `DockLayout` |
| `ActiveDockable` | Get/set the currently active dockable |
| `Documents` | All currently open documents |
| `Anchorables` | All registered toolboxes |
| `OpenDocument(doc)` | Add a document and make it active |
| `CloseDocument(doc)` | Remove a document from the layout |
| `OpenOrActivateDocument<T>(predicate, factory)` | Find or create a document |
| `FindDocument<T>(predicate)` | Find an open document by predicate |
| `GetAnchorable<T>()` | Get a registered toolbox by type |
