---
title: "Tutorial: MVVM IDE Application"
layout: default
parent: Tutorials
nav_order: 1
description: "Build a multi-document IDE with the v5 MVVM + DI architecture using ToolboxBase, Document, IDockLayoutService, and ToggleDockingManager."
---

# Tutorial: Build an MVVM IDE Application (v5)

In this tutorial you will build a Visual Studio Code-style IDE application using AvalonDock v5's first-class MVVM and Dependency Injection architecture. Everything is controlled via view models — no code-behind layout manipulation, no singletons.

{: .tip }
This tutorial follows the exact patterns used in the `AvalonDockCodeApp` sample project, which is the reference implementation for AvalonDock v5.

---

## What You'll Build

A docking IDE with:
- **Sidebar toggle buttons** for tool panels (like VS Code)
- **Explorer panel** — a file tree browser (toolbox, docked left)
- **Search panel** — a search input (toolbox, docked left)
- **Output panel** — log output (toolbox, docked bottom)
- **Document tabs** — file editor tabs, opened from the explorer
- **Everything via DI** — `IDockLayoutService` manages the layout tree, view models are injected
- **Zero code-behind** for layout logic

---

## Prerequisites

- .NET 9 or later SDK
- Windows (WPF is Windows-only)

```bash
dotnet new wpf -n MvvmIdeApp
cd MvvmIdeApp
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Mvvm
dotnet add package Dirkster.AvalonDock.Mvvm.CommunityToolkit
dotnet add package Dirkster.AvalonDock.DependencyInjection
dotnet add package Dirkster.AvalonDock.Themes.Arc
dotnet add package CommunityToolkit.Mvvm
```

---

## The v5 Architecture

Before diving in, here is how the pieces fit together:

```
App.xaml.cs  (Composition Root)
├── AddDockLayoutService(dock => {      ← single entry point for dock configuration
│       dock.ConfigureToggleDock(...)   ← configures sidebar button size, dock sizes
│       dock.AddToolbox<ExplorerToolbox>()  ← registers tool VMs
│       dock.AddToolbox<SearchToolbox>()
│       dock.AddToolbox<OutputToolbox>()
│   })
└── AddSingleton<MainViewModel>()       ← receives IDockLayoutService via DI

MainViewModel
├── DockLayout (IRootDock)              ← bound to ToggleDockingManager.DockLayout
├── OpenFile() → _dockService.OpenOrActivateDocument(...)
└── CloseDocument() → _dockService.CloseDocument(...)

ToggleDockingManager (XAML)
├── DockLayout="{Binding DockLayout}"   ← drives the entire layout from the VM
├── DataTemplates per VM type           ← renders each toolbox/document
└── PanesStyleSelector                  ← binds Title, ContentId, IconSource
```

### Key Base Classes

| Class | Package | Purpose |
|:------|:--------|:--------|
| `ObservableToolboxBase` | `AvalonDock.Mvvm.CommunityToolkit` | Base for tool panel VMs with `[ObservableProperty]` support. Has `Zone`, `Icon`, `IsOpenByDefault`. |
| `ObservableDocument` | `AvalonDock.Mvvm.CommunityToolkit` | Base for document tab VMs with `[ObservableProperty]` support. |
| `ToolboxBase` | `AvalonDock.Mvvm` | Lightweight base for tool panels (no external dependencies). |
| `DockableBase` | `AvalonDock.Mvvm` | Abstract base for all dockables (no external dependencies). |
| `IDockLayoutService` | `AvalonDock.Core` | Manages the layout tree. Open/close documents, get anchorables by type. |
| `IToolbox` | `AvalonDock.Core` | Interface for toolbox VMs. `DockZone` controls placement. |

---

## Step 1: Create Toolbox View Models

Each tool panel inherits from `ObservableToolboxBase` (from `AvalonDock.Mvvm.CommunityToolkit`) and declares its placement via `DockZone`. This enables `[ObservableProperty]` and `[RelayCommand]` source generators.

{: .note }
If you don't need CommunityToolkit.Mvvm source generators, use `ToolboxBase` from `AvalonDock.Mvvm` instead — it has no external dependencies.

**File: `ViewModels/ExplorerToolbox.cs`**

```csharp
using System.Collections.ObjectModel;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmIdeApp.ViewModels;

public partial class ExplorerToolbox : ObservableToolboxBase
{
    private Action<string> _openFileCallback = _ => { };

    [ObservableProperty]
    private ObservableCollection<string> _files = new()
    {
        "Program.cs", "MainWindow.xaml", "App.xaml", "README.md"
    };

    public ExplorerToolbox()
    {
        Id = "Explorer";
        Title = "Explorer";
        ToolTipText = "Explorer (Ctrl+Shift+E)";
        Zone = DockZone.LeftTop;     // Docked left, top section
    }

    public void SetOpenFileCallback(Action<string> callback)
        => _openFileCallback = callback;

    public void OpenSelectedFile(string fileName)
        => _openFileCallback(fileName);
}
```

**File: `ViewModels/SearchToolbox.cs`**

```csharp
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmIdeApp.ViewModels;

public partial class SearchToolbox : ObservableToolboxBase
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    public SearchToolbox()
    {
        Id = "Search";
        Title = "Search";
        ToolTipText = "Search (Ctrl+Shift+F)";
        Zone = DockZone.LeftTop;     // Same side as Explorer
    }
}
```

**File: `ViewModels/OutputToolbox.cs`**

```csharp
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmIdeApp.ViewModels;

public partial class OutputToolbox : ObservableToolboxBase
{
    [ObservableProperty]
    private string _outputText = "Ready.\n";

    public OutputToolbox()
    {
        Id = "Output";
        Title = "Output";
        ToolTipText = "Output (Ctrl+Shift+U)";
        Zone = DockZone.BottomLeft;  // Docked at bottom
        IsOpenByDefault = true;      // Open when app starts
    }

    public void AppendLine(string text) => OutputText += text + "\n";
}
```

### DockZone Options

The `DockZone` enum controls where each toolbox appears:

| Zone | Position |
|:-----|:---------|
| `LeftTop` | Left sidebar, upper section |
| `LeftBottom` | Left sidebar, lower section |
| `RightTop` | Right sidebar, upper section |
| `RightBottom` | Right sidebar, lower section |
| `BottomLeft` | Bottom panel, left section |
| `BottomRight` | Bottom panel, right section |

---

## Step 2: Create the Document View Model

Documents inherit from `ObservableDocument` (from `AvalonDock.Mvvm.CommunityToolkit`). Each open file gets its own tab.

**File: `ViewModels/EditorTabViewModel.cs`**

```csharp
using System.IO;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmIdeApp.ViewModels;

public partial class EditorTabViewModel : ObservableDocument
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _toolTip = string.Empty;

    public void LoadFile(string path)
    {
        FilePath = path;
        Title = Path.GetFileName(path);
        Id = path;           // Unique ID for this document
        ToolTip = path;

        try
        {
            Content = File.ReadAllText(path);
            IsModified = false;
        }
        catch (Exception ex)
        {
            Content = $"Error: {ex.Message}";
        }
    }
}
```

---

## Step 3: Create the Main View Model

The `MainViewModel` receives `IDockLayoutService` via constructor injection. It uses the service to open/close documents and access tool panels — no direct layout manipulation needed.

**File: `ViewModels/MainViewModel.cs`**

```csharp
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MvvmIdeApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDockLayoutService _dockService;

    /// <summary>The MVVM layout tree — bind to DockLayout on the DockingManager.</summary>
    public IRootDock DockLayout => _dockService.Layout;

    /// <summary>Typed access to the Explorer toolbox via the layout service.</summary>
    public ExplorerToolbox? Explorer => _dockService.GetAnchorable<ExplorerToolbox>();

    /// <summary>Typed access to the Output toolbox.</summary>
    public OutputToolbox? Output => _dockService.GetAnchorable<OutputToolbox>();

    public MainViewModel(IDockLayoutService dockService)
    {
        _dockService = dockService;

        // Wire up file-open callback from Explorer
        Explorer?.SetOpenFileCallback(OpenFile);
        Output?.AppendLine("Application initialized via DI.");
    }

    /// <summary>Opens a file as a document tab, or activates it if already open.</summary>
    public void OpenFile(string filePath)
    {
        _dockService.OpenOrActivateDocument(
            existing => existing.FilePath == filePath,  // find existing
            () =>                                        // or create new
            {
                var tab = new EditorTabViewModel();
                tab.LoadFile(filePath);
                return tab;
            });

        Output?.AppendLine($"Opened: {filePath}");
    }

    [RelayCommand]
    private void CloseDocument(EditorTabViewModel? tab)
    {
        if (tab != null)
        {
            _dockService.CloseDocument(tab);
            Output?.AppendLine($"Closed: {tab.Title}");
        }
    }
}
```

### IDockLayoutService API

The service manages all layout operations from your view model:

| Method | Description |
|:-------|:------------|
| `Layout` | The `IRootDock` tree — bind to `DockLayout` in XAML |
| `OpenDocument(doc)` | Add a document and make it active |
| `CloseDocument(doc)` | Remove a document from the layout |
| `OpenOrActivateDocument(predicate, factory)` | Find existing or create new document |
| `FindDocument<T>(predicate)` | Find an open document by predicate |
| `GetAnchorable<T>()` | Get a registered toolbox by type |
| `Documents` | All currently open documents |
| `Anchorables` | All registered toolboxes |

---

## Step 4: Configure the DI Composition Root

The `App.xaml.cs` is the composition root. This is where toolboxes are registered, the layout service is configured, and everything is wired together.

**File: `App.xaml.cs`**

```csharp
using System.Windows;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MvvmIdeApp.ViewModels;

namespace MvvmIdeApp;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Configure dock layout: toggle dock options + toolboxes in one call
        services.AddDockLayoutService(dock =>
        {
            dock.ConfigureToggleDock(opts =>
            {
                opts.ButtonSize = 28;
                opts.DefaultDockWidth = 280;
                opts.DefaultDockHeight = 220;
            });

            // Register toolbox VMs — order determines sidebar button order
            dock.AddToolbox<ExplorerToolbox>();
            dock.AddToolbox<SearchToolbox>();
            dock.AddToolbox<OutputToolbox>();
        });

        // Main view model — receives IDockLayoutService
        services.AddSingleton<MainViewModel>();

        // Main window — receives MainViewModel + ToggleDockOptions
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        (_serviceProvider as IDisposable)?.Dispose();
        base.OnExit(e);
    }
}
```

{: .important }
Remove `StartupUri="MainWindow.xaml"` from `App.xaml` — the window is now created via DI.

**File: `App.xaml`**

```xml
<Application x:Class="MvvmIdeApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             ShutdownMode="OnMainWindowClose">
    <Application.Resources />
</Application>
```

---

## Step 5: Create the Style Selector

The style selector binds layout properties (`Title`, `ContentId`, `IconSource`) from view models to layout items. It differentiates between toolboxes and documents.

**File: `PanesStyleSelector.cs`**

```csharp
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Core;
using MvvmIdeApp.ViewModels;

namespace MvvmIdeApp;

public class PanesStyleSelector : StyleSelector
{
    public Style? ToolboxStyle { get; set; }
    public Style? DocumentStyle { get; set; }

    public override Style? SelectStyle(object item, DependencyObject container)
    {
        if (item is IToolbox)
            return ToolboxStyle;

        if (item is EditorTabViewModel)
            return DocumentStyle;

        return base.SelectStyle(item, container);
    }
}
```

---

## Step 6: Build the XAML Layout

The XAML uses `ToggleDockingManager` (the v5 sidebar-based docking control) and binds its `DockLayout` property to the view model.

**File: `MainWindow.xaml`**

```xml
<Window x:Class="MvvmIdeApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:avalonDockControls="clr-namespace:AvalonDock.Controls;assembly=AvalonDock"
        xmlns:themes="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.Arc"
        xmlns:local="clr-namespace:MvvmIdeApp"
        xmlns:vm="clr-namespace:MvvmIdeApp.ViewModels"
        Title="MVVM IDE — AvalonDock v5 Tutorial"
        Width="1000" Height="700"
        Background="#252729">

    <Window.Resources>
        <!-- DataTemplate: how to render each toolbox VM's content area -->
        <DataTemplate DataType="{x:Type vm:ExplorerToolbox}">
            <ListBox ItemsSource="{Binding Files}" Background="#252526"
                     Foreground="#CCCCCC" BorderThickness="0" Margin="4" />
        </DataTemplate>

        <DataTemplate DataType="{x:Type vm:SearchToolbox}">
            <Border Background="#252526" Padding="12">
                <TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                         Background="#3C3C3C" Foreground="#CCCCCC"
                         BorderThickness="0" Padding="6,4" />
            </Border>
        </DataTemplate>

        <DataTemplate DataType="{x:Type vm:OutputToolbox}">
            <TextBox Text="{Binding OutputText, Mode=OneWay}"
                     IsReadOnly="True" AcceptsReturn="True"
                     FontFamily="Consolas" FontSize="12"
                     VerticalScrollBarVisibility="Auto"
                     Background="#1E1E1E" Foreground="#DCDCDC" />
        </DataTemplate>

        <!-- DataTemplate: how to render document content -->
        <DataTemplate DataType="{x:Type vm:EditorTabViewModel}">
            <TextBox Text="{Binding Content, UpdateSourceTrigger=PropertyChanged}"
                     AcceptsReturn="True" AcceptsTab="True"
                     FontFamily="Consolas" FontSize="13"
                     VerticalScrollBarVisibility="Auto"
                     Background="#1E1E1E" Foreground="#DCDCDC" />
        </DataTemplate>

        <!-- LayoutItem style selector -->
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
                    <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
                </Style>
            </local:PanesStyleSelector.DocumentStyle>
        </local:PanesStyleSelector>
    </Window.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <!-- Menu -->
        <Menu Grid.Row="0" Background="#252729" Foreground="#CCCCCC" Padding="4">
            <MenuItem Header="_File" Foreground="#CCCCCC">
                <MenuItem Header="E_xit" Click="OnExit" />
            </MenuItem>
        </Menu>

        <!-- ToggleDockingManager — the v5 sidebar-based docking control -->
        <avalonDock:ToggleDockingManager x:Name="dockManager" Grid.Row="1"
            Background="#252729" BorderThickness="0"
            DockLayout="{Binding DockLayout}"
            LayoutItemContainerStyleSelector="{StaticResource PanesStyleSelector}">

            <avalonDock:ToggleDockingManager.Theme>
                <themes:ArcDarkTheme />
            </avalonDock:ToggleDockingManager.Theme>

            <!-- Initial document area (toolboxes are auto-placed by DockLayoutService) -->
            <avalonDock:LayoutRoot>
                <avalonDock:LayoutPanel Orientation="Horizontal">
                    <avalonDock:LayoutDocumentPaneGroup>
                        <avalonDock:LayoutDocumentPane>
                            <avalonDock:LayoutDocument Title="Welcome" ContentId="welcome">
                                <Border Background="#1E1E1E" Padding="40">
                                    <StackPanel VerticalAlignment="Center"
                                                HorizontalAlignment="Center">
                                        <TextBlock Text="MVVM IDE" FontSize="28"
                                                   FontWeight="Light" Foreground="#CCCCCC"
                                                   HorizontalAlignment="Center" />
                                        <TextBlock Text="AvalonDock v5 Tutorial" FontSize="14"
                                                   Foreground="#808080"
                                                   HorizontalAlignment="Center" Margin="0,8,0,24" />
                                        <TextBlock Foreground="#808080" FontSize="13"
                                                   TextAlignment="Center" LineHeight="24">
                                            <Run Text="Click sidebar icons to toggle panels" />
                                            <LineBreak />
                                            <Run Text="Powered by AvalonDock.Mvvm + DependencyInjection" />
                                        </TextBlock>
                                    </StackPanel>
                                </Border>
                            </avalonDock:LayoutDocument>
                        </avalonDock:LayoutDocumentPane>
                    </avalonDock:LayoutDocumentPaneGroup>
                </avalonDock:LayoutPanel>
            </avalonDock:LayoutRoot>
        </avalonDock:ToggleDockingManager>
    </Grid>
</Window>
```

---

## Step 7: Wire Up the Code-Behind

The code-behind receives the view model and dock options via constructor injection. Layout options are applied here — the only code-behind needed.

**File: `MainWindow.xaml.cs`**

```csharp
using System.Windows;
using AvalonDock.DependencyInjection;
using MvvmIdeApp.ViewModels;

namespace MvvmIdeApp;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, ToggleDockOptions? dockOptions = null)
    {
        DataContext = viewModel;
        InitializeComponent();

        // Apply DI-configured dock options
        if (dockOptions != null)
        {
            dockManager.ButtonSize = dockOptions.ButtonSize;
            dockManager.DefaultDockWidth = dockOptions.DefaultDockWidth;
            dockManager.DefaultDockHeight = dockOptions.DefaultDockHeight;
        }
    }

    private void OnExit(object sender, RoutedEventArgs e) => Close();
}
```

---

## How It Works

### The v5 Data Flow

```
DI Container
├── ExplorerToolbox (IToolbox, Zone=LeftTop)
├── SearchToolbox   (IToolbox, Zone=LeftTop)
├── OutputToolbox   (IToolbox, Zone=BottomLeft)
│
└── DockLayoutService
    ├── Collects all IToolbox instances
    ├── Auto-builds IRootDock layout tree
    └── Exposes: Layout, OpenDocument(), CloseDocument(), GetAnchorable<T>()

MainViewModel
├── DockLayout → bound to ToggleDockingManager.DockLayout
├── OpenFile() → _dockService.OpenOrActivateDocument(...)
└── Explorer  → _dockService.GetAnchorable<ExplorerToolbox>()

ToggleDockingManager (XAML)
├── Reads DockLayout to know what panels exist and where
├── Creates sidebar toggle buttons from toolbox icons
├── Uses DataTemplates to render each VM's content
└── Uses PanesStyleSelector to bind Title/ContentId
```

### What Makes v5 Different

| v4 / Legacy Pattern | v5 Pattern |
|:---------------------|:-----------|
| Static `Workspace.This` singleton | `IDockLayoutService` via constructor injection |
| Manual `DocumentsSource` / `AnchorablesSource` binding | `DockLayout` property binds the entire layout tree |
| `ILayoutUpdateStrategy` to control placement | `DockZone` enum on each `ToolboxBase` |
| `ActiveDocumentConverter` for active tracking | `IDockLayoutService.ActiveDockable` |
| Manual `ObservableCollection` management | `OpenOrActivateDocument()` / `CloseDocument()` |
| `DockingManager` with manual layout | `ToggleDockingManager` with sidebar toggles |

---

## Adding a New Toolbox

To add a new tool panel in the v5 architecture:

1. **Create the VM** — inherit from `ObservableToolboxBase` (or `ToolboxBase`), set `Zone`:
   ```csharp
   public class ProblemsToolbox : ObservableToolboxBase
   {
       public ProblemsToolbox()
       {
           Id = "Problems";
           Title = "Problems";
           Zone = DockZone.BottomLeft;
       }
   }
   ```

2. **Register in DI** — add one line inside the `AddDockLayoutService` builder:
   ```csharp
   dock.AddToolbox<ProblemsToolbox>();
   ```

3. **Add a DataTemplate** in XAML:
   ```xml
   <DataTemplate DataType="{x:Type vm:ProblemsToolbox}">
       <TextBlock Text="No problems detected" Foreground="#808080" Margin="12" />
   </DataTemplate>
   ```

That's it — `DockLayoutService` automatically places it in the layout based on its `DockZone`.

---

## Next Steps

- Add [Layout Persistence]({% link tutorials/layout-persistence.md %}) to save and restore the user's panel arrangement
- Apply a [Custom Theme]({% link tutorials/styling-and-theming.md %}) to match your application's branding
- See the `AvalonDockCodeApp` project in the repository for the full reference implementation with a terminal, file icons, and syntax highlighting
- Review the [MVVM Guide]({% link guides/mvvm.md %}) for additional patterns (template selectors, style selectors)
- Review the [DI Guide]({% link guides/dependency-injection.md %}) for all available DI extension methods
