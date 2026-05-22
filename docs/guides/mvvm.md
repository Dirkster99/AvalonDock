---
title: MVVM Integration
layout: default
parent: Guides
nav_order: 1
description: "How to use AvalonDock with the MVVM pattern."
---

# MVVM Integration

AvalonDock provides first-class support for the MVVM (Model-View-ViewModel) pattern. This guide shows how to bind documents and anchorables to view models.

---

## Overview

In an MVVM setup, instead of defining `LayoutDocument` and `LayoutAnchorable` elements directly in XAML, you:

1. Define **view model collections** for documents and anchorables
2. Bind them to the `DockingManager` via `DocumentsSource` and `AnchorablesSource`
3. Use **DataTemplates** to define the visual representation
4. Use the `AvalonDock.Mvvm` package for base classes and services

---

## Install the MVVM Package

```bash
dotnet add package Dirkster.AvalonDock.Mvvm
```

---

## Step 1: Define View Models

```csharp
using AvalonDock.Mvvm;

// Base class for all dockable content
public class DocumentViewModel : DockableBase
{
    private string _text;

    public DocumentViewModel(string title)
    {
        Title = title;
        ContentId = Guid.NewGuid().ToString();
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
        ContentId = title.ToLowerInvariant().Replace(" ", "-");
        CanClose = false;
    }
}
```

## Step 2: Create the Main View Model

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DocumentViewModel> Documents { get; }
        = new ObservableCollection<DocumentViewModel>();

    public ObservableCollection<ToolViewModel> Tools { get; }
        = new ObservableCollection<ToolViewModel>();

    private DockableBase _activeDocument;
    public DockableBase ActiveDocument
    {
        get => _activeDocument;
        set { _activeDocument = value; OnPropertyChanged(); }
    }

    public MainViewModel()
    {
        // Add initial documents
        Documents.Add(new DocumentViewModel("Welcome") { Text = "Hello!" });

        // Add tool windows
        Tools.Add(new ToolViewModel("Explorer"));
        Tools.Add(new ToolViewModel("Properties"));
        Tools.Add(new ToolViewModel("Output"));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

## Step 3: Bind in XAML

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:local="clr-namespace:MyApp">

    <Window.DataContext>
        <local:MainViewModel />
    </Window.DataContext>

    <avalonDock:DockingManager DocumentsSource="{Binding Documents}"
                               AnchorablesSource="{Binding Tools}"
                               ActiveContent="{Binding ActiveDocument, Mode=TwoWay}">

        <!-- Template for documents -->
        <avalonDock:DockingManager.DocumentTemplate>
            <DataTemplate DataType="{x:Type local:DocumentViewModel}">
                <TextBox Text="{Binding Text}" AcceptsReturn="True" />
            </DataTemplate>
        </avalonDock:DockingManager.DocumentTemplate>

        <!-- Template for tool windows -->
        <avalonDock:DockingManager.AnchorableTemplate>
            <DataTemplate DataType="{x:Type local:ToolViewModel}">
                <TextBlock Text="{Binding Title}" Margin="8" />
            </DataTemplate>
        </avalonDock:DockingManager.AnchorableTemplate>

        <avalonDock:LayoutRoot>
            <avalonDock:LayoutPanel Orientation="Horizontal">
                <avalonDock:LayoutAnchorablePane DockWidth="200" />
                <avalonDock:LayoutDocumentPane />
            </avalonDock:LayoutPanel>
        </avalonDock:LayoutRoot>
    </avalonDock:DockingManager>
</Window>
```

---

## LayoutItemContainerStyle

To bind layout properties (like `Title`, `ContentId`, `CanClose`) from your view model to the layout item, use `LayoutItemContainerStyle`:

```xml
<avalonDock:DockingManager.LayoutItemContainerStyle>
    <Style TargetType="{x:Type avalonDock:LayoutItem}">
        <Setter Property="Title" Value="{Binding Model.Title}" />
        <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
        <Setter Property="CanClose" Value="{Binding Model.CanClose}" />
        <Setter Property="IconSource" Value="{Binding Model.IconSource}" />
    </Style>
</avalonDock:DockingManager.LayoutItemContainerStyle>
```

---

## LayoutItemTemplateSelector

For different content types, use a `DataTemplateSelector`:

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
            ExplorerViewModel => ExplorerTemplate,
            PropertiesViewModel => PropertiesTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
```

```xml
<avalonDock:DockingManager.LayoutItemTemplateSelector>
    <local:DockTemplateSelector
        DocumentTemplate="{StaticResource DocumentDataTemplate}"
        ExplorerTemplate="{StaticResource ExplorerDataTemplate}"
        PropertiesTemplate="{StaticResource PropertiesDataTemplate}" />
</avalonDock:DockingManager.LayoutItemTemplateSelector>
```

---

## DockLayoutService

The `AvalonDock.Mvvm` package includes `DockLayoutService` for managing layouts from your view model:

```csharp
using AvalonDock.Mvvm;

public class MainViewModel
{
    private readonly IDockLayoutService _layoutService;

    public MainViewModel(IDockLayoutService layoutService)
    {
        _layoutService = layoutService;
    }

    public void SaveLayout()
    {
        _layoutService.SaveLayout("my-layout");
    }

    public void RestoreLayout()
    {
        _layoutService.RestoreLayout("my-layout");
    }
}
```
