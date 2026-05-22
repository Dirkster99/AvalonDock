---
title: "Tutorial: MVVM IDE Application"
layout: default
parent: Tutorials
nav_order: 1
description: "Build a multi-document IDE with MVVM, template selectors, and commands."
---

# Tutorial: Build an MVVM IDE Application

In this tutorial you will build a Visual Studio-like IDE application using the MVVM pattern. By the end, you'll have an app with tabbed documents, a file explorer panel, a properties panel, and an output panel — all driven by view models.

{: .tip }
This tutorial is inspired by the `MVVMTestApp` and `VS2013Test` sample projects in the repository.

---

## What You'll Build

A docking IDE with:
- **Document tabs** that can be opened, closed, and floated
- **Explorer panel** (anchorable) docked to the left
- **Properties panel** (anchorable) docked to the right
- **Output panel** (anchorable) auto-hidden at the bottom
- **Active document tracking** bound to the view model
- **Commands** for File → New, File → Open, and closing documents

---

## Prerequisites

- .NET 9 or later SDK
- A WPF project with `Dirkster.AvalonDock` and `Dirkster.AvalonDock.Themes.VS2013` installed

```bash
dotnet new wpf -n MvvmIdeApp
cd MvvmIdeApp
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Themes.VS2013
```

---

## Step 1: Create the Base View Model

Every dockable panel needs a `Title`, `ContentId`, and property-change notification. Create a base class that all your panel view models will inherit from.

**File: `ViewModels/PaneViewModel.cs`**

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MvvmIdeApp.ViewModels;

public abstract class PaneViewModel : INotifyPropertyChanged
{
    private string _title;
    private string _contentId;
    private bool _isSelected;
    private bool _isActive;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string ContentId
    {
        get => _contentId;
        set => SetProperty(ref _contentId, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public virtual ImageSource IconSource => null;

    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
```

---

## Step 2: Create Document and Tool View Models

Documents are closable content items (like file editors). Tools are persistent panels (like Explorer or Output).

**File: `ViewModels/DocumentViewModel.cs`**

```csharp
namespace MvvmIdeApp.ViewModels;

public class DocumentViewModel : PaneViewModel
{
    private string _text;
    private string _filePath;
    private bool _isDirty;

    public DocumentViewModel(string title)
    {
        Title = title;
        ContentId = System.Guid.NewGuid().ToString();
    }

    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value))
                IsDirty = true;
        }
    }

    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    public bool IsDirty
    {
        get => _isDirty;
        set => SetProperty(ref _isDirty, value);
    }
}
```

**File: `ViewModels/ToolViewModel.cs`**

```csharp
namespace MvvmIdeApp.ViewModels;

public class ToolViewModel : PaneViewModel
{
    public ToolViewModel(string title)
    {
        Title = title;
        ContentId = title.ToLowerInvariant().Replace(" ", "-");
    }
}

// Specialized tool view models
public class ExplorerViewModel : ToolViewModel
{
    public ExplorerViewModel() : base("Explorer") { }
}

public class PropertiesViewModel : ToolViewModel
{
    public PropertiesViewModel() : base("Properties") { }
}

public class OutputViewModel : ToolViewModel
{
    private string _outputText = string.Empty;

    public OutputViewModel() : base("Output") { }

    public string OutputText
    {
        get => _outputText;
        set => SetProperty(ref _outputText, value);
    }

    public void AppendLine(string line)
    {
        OutputText += line + Environment.NewLine;
    }
}
```

---

## Step 3: Create a RelayCommand

You'll need a simple `ICommand` implementation for menu commands.

**File: `RelayCommand.cs`**

```csharp
using System.Windows.Input;

namespace MvvmIdeApp;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter) => _execute(parameter);
}
```

---

## Step 4: Create the Main (Workspace) View Model

This is the heart of the application. It manages the collections of documents and tools, and exposes commands.

**File: `ViewModels/WorkspaceViewModel.cs`**

```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MvvmIdeApp.ViewModels;

public class WorkspaceViewModel : PaneViewModel
{
    private DocumentViewModel _activeDocument;
    private int _documentCounter;

    public WorkspaceViewModel()
    {
        // Initialize tool panels
        Explorer = new ExplorerViewModel();
        Properties = new PropertiesViewModel();
        Output = new OutputViewModel();

        Tools = new ObservableCollection<ToolViewModel>
        {
            Explorer, Properties, Output
        };

        Documents = new ObservableCollection<DocumentViewModel>();

        // Create an initial welcome document
        NewDocument();

        Output.AppendLine("Application started.");
    }

    // --- Collections ---

    public ObservableCollection<DocumentViewModel> Documents { get; }
    public ObservableCollection<ToolViewModel> Tools { get; }

    // --- Tool Accessors ---

    public ExplorerViewModel Explorer { get; }
    public PropertiesViewModel Properties { get; }
    public OutputViewModel Output { get; }

    // --- Active Document ---

    public DocumentViewModel ActiveDocument
    {
        get => _activeDocument;
        set => SetProperty(ref _activeDocument, value);
    }

    // --- Commands ---

    public ICommand NewDocumentCommand => new RelayCommand(_ => NewDocument());

    public ICommand CloseDocumentCommand => new RelayCommand(
        _ => CloseDocument(ActiveDocument),
        _ => ActiveDocument != null
    );

    // --- Methods ---

    public void NewDocument()
    {
        _documentCounter++;
        var doc = new DocumentViewModel($"Untitled {_documentCounter}")
        {
            Text = $"// New document {_documentCounter}\n"
        };
        Documents.Add(doc);
        ActiveDocument = doc;
        Output.AppendLine($"Created: {doc.Title}");
    }

    public void CloseDocument(DocumentViewModel doc)
    {
        if (doc == null) return;
        Documents.Remove(doc);
        Output.AppendLine($"Closed: {doc.Title}");
        ActiveDocument = Documents.LastOrDefault();
    }
}
```

---

## Step 5: Create Template Selectors

AvalonDock uses `DataTemplateSelector` to pick the right visual template for each type of content, and a `StyleSelector` to bind layout properties.

**File: `PanesTemplateSelector.cs`**

```csharp
using System.Windows;
using System.Windows.Controls;
using MvvmIdeApp.ViewModels;

namespace MvvmIdeApp;

public class PanesTemplateSelector : DataTemplateSelector
{
    public DataTemplate DocumentTemplate { get; set; }
    public DataTemplate ExplorerTemplate { get; set; }
    public DataTemplate PropertiesTemplate { get; set; }
    public DataTemplate OutputTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            DocumentViewModel => DocumentTemplate,
            ExplorerViewModel => ExplorerTemplate,
            PropertiesViewModel => PropertiesTemplate,
            OutputViewModel => OutputTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
```

**File: `PanesStyleSelector.cs`**

```csharp
using System.Windows;
using System.Windows.Controls;
using MvvmIdeApp.ViewModels;

namespace MvvmIdeApp;

public class PanesStyleSelector : StyleSelector
{
    public Style DocumentStyle { get; set; }
    public Style ToolStyle { get; set; }

    public override Style SelectStyle(object item, DependencyObject container)
    {
        return item switch
        {
            DocumentViewModel => DocumentStyle,
            ToolViewModel => ToolStyle,
            _ => base.SelectStyle(item, container)
        };
    }
}
```

---

## Step 6: Create an ActiveDocument Converter

The `DockingManager.ActiveContent` property is of type `object`, but your view model tracks a `DocumentViewModel`. A converter bridges this gap.

**File: `ActiveDocumentConverter.cs`**

```csharp
using System.Globalization;
using System.Windows.Data;
using MvvmIdeApp.ViewModels;

namespace MvvmIdeApp;

public class ActiveDocumentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value; // VM → DockingManager: pass through
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // DockingManager → VM: only accept DocumentViewModel
        return value is DocumentViewModel ? value : Binding.DoNothing;
    }
}
```

---

## Step 7: Build the XAML Layout

This is where everything comes together. The XAML wires the `DockingManager` to your view model collections.

**File: `MainWindow.xaml`**

```xml
<Window x:Class="MvvmIdeApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:avalonDockThemes="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.VS2013"
        xmlns:local="clr-namespace:MvvmIdeApp"
        xmlns:vm="clr-namespace:MvvmIdeApp.ViewModels"
        Title="MVVM IDE — AvalonDock Tutorial" Height="600" Width="900">

    <Window.DataContext>
        <vm:WorkspaceViewModel />
    </Window.DataContext>

    <Window.Resources>
        <local:ActiveDocumentConverter x:Key="ActiveDocumentConverter" />
    </Window.Resources>

    <DockPanel>
        <!-- Menu Bar -->
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_New" Command="{Binding NewDocumentCommand}" />
                <MenuItem Header="_Close" Command="{Binding CloseDocumentCommand}" />
            </MenuItem>
        </Menu>

        <!-- Docking Manager -->
        <avalonDock:DockingManager
            DocumentsSource="{Binding Documents}"
            AnchorablesSource="{Binding Tools}"
            ActiveContent="{Binding ActiveDocument, Mode=TwoWay,
                            Converter={StaticResource ActiveDocumentConverter}}">

            <!-- Theme -->
            <avalonDock:DockingManager.Theme>
                <avalonDockThemes:Vs2013LightTheme />
            </avalonDock:DockingManager.Theme>

            <!-- Template Selector: picks the right DataTemplate per content type -->
            <avalonDock:DockingManager.LayoutItemTemplateSelector>
                <local:PanesTemplateSelector>
                    <local:PanesTemplateSelector.DocumentTemplate>
                        <DataTemplate DataType="{x:Type vm:DocumentViewModel}">
                            <TextBox Text="{Binding Text, UpdateSourceTrigger=PropertyChanged}"
                                     AcceptsReturn="True" AcceptsTab="True"
                                     FontFamily="Consolas" FontSize="13"
                                     VerticalScrollBarVisibility="Auto"
                                     HorizontalScrollBarVisibility="Auto" />
                        </DataTemplate>
                    </local:PanesTemplateSelector.DocumentTemplate>

                    <local:PanesTemplateSelector.ExplorerTemplate>
                        <DataTemplate DataType="{x:Type vm:ExplorerViewModel}">
                            <TreeView Margin="4">
                                <TreeViewItem Header="Solution 'MyApp'" IsExpanded="True">
                                    <TreeViewItem Header="MyApp">
                                        <TreeViewItem Header="MainWindow.xaml" />
                                        <TreeViewItem Header="App.xaml" />
                                    </TreeViewItem>
                                </TreeViewItem>
                            </TreeView>
                        </DataTemplate>
                    </local:PanesTemplateSelector.ExplorerTemplate>

                    <local:PanesTemplateSelector.PropertiesTemplate>
                        <DataTemplate DataType="{x:Type vm:PropertiesViewModel}">
                            <StackPanel Margin="8">
                                <TextBlock Text="Properties" FontWeight="Bold" Margin="0,0,0,8" />
                                <TextBlock Text="Select an item to view its properties." 
                                           Foreground="Gray" />
                            </StackPanel>
                        </DataTemplate>
                    </local:PanesTemplateSelector.PropertiesTemplate>

                    <local:PanesTemplateSelector.OutputTemplate>
                        <DataTemplate DataType="{x:Type vm:OutputViewModel}">
                            <TextBox Text="{Binding OutputText, Mode=OneWay}"
                                     IsReadOnly="True" AcceptsReturn="True"
                                     FontFamily="Consolas" FontSize="12"
                                     VerticalScrollBarVisibility="Auto"
                                     Background="#1E1E1E" Foreground="#DCDCDC" />
                        </DataTemplate>
                    </local:PanesTemplateSelector.OutputTemplate>
                </local:PanesTemplateSelector>
            </avalonDock:DockingManager.LayoutItemTemplateSelector>

            <!-- Style Selector: binds layout properties (Title, CanClose, etc.) -->
            <avalonDock:DockingManager.LayoutItemContainerStyleSelector>
                <local:PanesStyleSelector>
                    <local:PanesStyleSelector.DocumentStyle>
                        <Style TargetType="{x:Type avalonDock:LayoutItem}">
                            <Setter Property="Title" Value="{Binding Model.Title}" />
                            <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
                            <Setter Property="CanClose" Value="True" />
                        </Style>
                    </local:PanesStyleSelector.DocumentStyle>

                    <local:PanesStyleSelector.ToolStyle>
                        <Style TargetType="{x:Type avalonDock:LayoutItem}">
                            <Setter Property="Title" Value="{Binding Model.Title}" />
                            <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
                            <Setter Property="CanClose" Value="False" />
                        </Style>
                    </local:PanesStyleSelector.ToolStyle>
                </local:PanesStyleSelector>
            </avalonDock:DockingManager.LayoutItemContainerStyleSelector>

            <!-- Layout Structure -->
            <avalonDock:LayoutRoot>
                <avalonDock:LayoutPanel Orientation="Horizontal">
                    <!-- Left: Explorer -->
                    <avalonDock:LayoutAnchorablePane DockWidth="220" />

                    <!-- Center: Documents -->
                    <avalonDock:LayoutDocumentPane />

                    <!-- Right: Properties -->
                    <avalonDock:LayoutAnchorablePane DockWidth="220" />
                </avalonDock:LayoutPanel>

                <!-- Bottom: auto-hidden Output -->
                <avalonDock:LayoutRoot.BottomSide>
                    <avalonDock:LayoutAnchorSide>
                        <avalonDock:LayoutAnchorGroup>
                            <avalonDock:LayoutAnchorable Title="Output"
                                                          ContentId="output" />
                        </avalonDock:LayoutAnchorGroup>
                    </avalonDock:LayoutAnchorSide>
                </avalonDock:LayoutRoot.BottomSide>
            </avalonDock:LayoutRoot>
        </avalonDock:DockingManager>
    </DockPanel>
</Window>
```

---

## Step 8: Wire Up the Code-Behind

The code-behind is minimal — just default initialization.

**File: `MainWindow.xaml.cs`**

```csharp
using System.Windows;

namespace MvvmIdeApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

---

## How It Works

### The MVVM Data Flow

```
WorkspaceViewModel
├── Documents (ObservableCollection<DocumentViewModel>)
│   └── bound to → DockingManager.DocumentsSource
├── Tools (ObservableCollection<ToolViewModel>)
│   └── bound to → DockingManager.AnchorablesSource
└── ActiveDocument
    └── bound to → DockingManager.ActiveContent (via converter)
```

### Key Concepts

1. **`DocumentsSource` and `AnchorablesSource`** — These are the two main binding points. AvalonDock creates layout items automatically for each view model in these collections.

2. **`LayoutItemTemplateSelector`** — Chooses the visual template (the content area) based on the view model type. Each type of panel gets its own `DataTemplate`.

3. **`LayoutItemContainerStyleSelector`** — Binds layout-level properties like `Title`, `ContentId`, and `CanClose` from the view model to the layout container. This controls the tab header and docking behavior.

4. **`ActiveDocumentConverter`** — The `ActiveContent` property accepts any object, but your view model only wants to track documents. The converter filters out non-document activations (e.g., when a tool window is focused).

5. **Layout structure** — The `LayoutRoot` in XAML defines the *initial* layout. Once the user rearranges panels, the runtime layout takes over. Empty `LayoutAnchorablePane` elements are filled by AvalonDock based on the order of items in `AnchorablesSource`.

---

## Common Patterns

### Controlling Where Tools Appear

By default, AvalonDock places anchorables in the first available `LayoutAnchorablePane`. To control placement, implement `ILayoutUpdateStrategy`:

```csharp
public class LayoutInitializer : ILayoutUpdateStrategy
{
    public bool BeforeInsertAnchorable(
        LayoutRoot layout, LayoutAnchorable anchorableToShow,
        ILayoutContainer destinationContainer)
    {
        var pane = anchorableToShow.Content switch
        {
            ExplorerViewModel => layout.Descendents()
                .OfType<LayoutAnchorablePane>().FirstOrDefault(),
            PropertiesViewModel => layout.Descendents()
                .OfType<LayoutAnchorablePane>().LastOrDefault(),
            _ => null
        };

        if (pane != null)
        {
            pane.Children.Add(anchorableToShow);
            return true;
        }
        return false;
    }

    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorable) { }
    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument doc,
        ILayoutContainer container) => false;
    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument doc) { }
}
```

Then set it in XAML:

```xml
<avalonDock:DockingManager.LayoutUpdateStrategy>
    <local:LayoutInitializer />
</avalonDock:DockingManager.LayoutUpdateStrategy>
```

### Handling Document Close

To react when the user closes a document via the tab's X button, handle the `DocumentClosing` event or use `LayoutItemContainerStyle` with a close command:

```xml
<Style TargetType="{x:Type avalonDock:LayoutItem}">
    <Setter Property="CloseCommand" Value="{Binding Model.CloseCommand}" />
</Style>
```

### Dirty Indicator in Tab Title

Show an asterisk when a document has unsaved changes:

```csharp
public string DisplayTitle => IsDirty ? $"{Title} *" : Title;
```

Bind the layout title to `DisplayTitle` instead of `Title`, and raise `PropertyChanged` for `DisplayTitle` whenever `IsDirty` changes.

---

## Next Steps

- Add [Layout Serialization]({% link tutorials/layout-persistence.md %}) to save and restore the user's panel arrangement
- Add [Dependency Injection]({% link tutorials/dependency-injection-app.md %}) to replace the singleton pattern with constructor injection
- Apply a [Custom Theme]({% link tutorials/styling-and-theming.md %}) to match your application's branding
- See the `MVVMTestApp` and `VS2013Test` projects in the repository for production-ready examples
