---
title: "Tutorial: Dependency Injection App"
layout: default
parent: Tutorials
nav_order: 2
description: "Build a modern AvalonDock application using the v5 DI architecture with AddDockLayoutService builder pattern and ToggleDockingManager."
---

# Tutorial: Dependency Injection Deep Dive

This tutorial builds on the [MVVM IDE tutorial]({% link tutorials/mvvm-ide.md %}) and provides a detailed walkthrough of every DI extension method, registration pattern, and configuration option available in AvalonDock v5. Use this as a reference when scaling your application.

{: .tip }
This tutorial covers the same DI patterns used in the `AvalonDockCodeApp` sample project — the reference implementation for AvalonDock v5.

---

## What You'll Learn

- The `AddDockLayoutService(configure)` builder pattern and when to use it
- How the `DockLayoutBuilder` auto-builds the layout tree from registered `IToolbox` instances
- How `ConfigureToggleDock()` configures the sidebar and docking behavior
- How to use factory overloads (`AddToolbox<T>(factory)`) for toolboxes that need constructor parameters
- How to register additional AvalonDock services (theme manager, serializer, auto-hide)
- Testing patterns with DI

---

## Prerequisites

```bash
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Mvvm
dotnet add package Dirkster.AvalonDock.DependencyInjection
dotnet add package Dirkster.AvalonDock.Themes.Arc
dotnet add package CommunityToolkit.Mvvm
```

---

## The DI Extension Methods

The `AvalonDock.DependencyInjection` package provides these extension methods on `IServiceCollection`:

| Method | Purpose |
|:-------|:--------|
| `AddDockLayoutService(configure)` | Register `IDockLayoutService` with a builder to configure toolboxes and options |
| `AddDockLayoutService()` | Register `IDockLayoutService` without builder configuration |
| `AddAvalonDock<TFactory>()` | Register a custom `IFactory` implementation |
| `AddAvalonDockSerializer<T>()` | Register a layout serializer |
| `AddAvalonDockThemeManager<T>()` | Register a theme manager |
| `AddDockingManager(factory)` | Register an `IDockingManager` wrapper |
| `AddAutoHideManager<T>()` | Register an auto-hide manager |
| `AddFloatingWindowService<T>()` | Register a floating window service |
| `AddDragDropHandler<T>()` | Register a drag-and-drop handler |

#### DockLayoutBuilder Methods

| Method | Purpose |
|:-------|:--------|
| `ConfigureToggleDock(configure)` | Register `ToggleDockOptions` for sidebar/dock configuration |
| `AddToolbox<T>()` | Register a toolbox VM as singleton |
| `AddToolbox<T>(factory)` | Register a toolbox VM with a custom factory |

---

## Registration Patterns

### Basic Toolbox Registration

For toolboxes with parameterless constructors, use `AddToolbox<T>()` inside the builder:

```csharp
services.AddDockLayoutService(dock =>
{
    dock.AddToolbox<SearchToolbox>();
});
```

This registers `SearchToolbox` as a singleton and also as `IToolbox` so that `DockLayoutService` can discover it.

### Factory-Based Registration

For toolboxes that need constructor parameters (like callbacks):

```csharp
services.AddDockLayoutService(dock =>
{
    dock.AddToolbox<FolderExplorerToolbox>(sp =>
        new FolderExplorerToolbox(filePath => { /* open file callback */ }));
});
```

This is used in `AvalonDockCodeApp` for the `FolderExplorerViewModel` which needs a file-open callback.

### DockLayoutService

```csharp
services.AddDockLayoutService(dock =>
{
    dock.AddToolbox<ExplorerToolbox>();
    dock.AddToolbox<SearchToolbox>();
});
```

This registers `IDockLayoutService` as a singleton. On creation, it:

1. Collects all registered `IToolbox` instances via `IEnumerable<IToolbox>`
2. Builds an `IRootDock` layout tree with toolboxes placed by their `DockZone`
3. Exposes the tree via `Layout` for XAML binding

If `ConfigureToggleDock` is not called, default `ToggleDockOptions` are registered automatically.

### ToggleDockOptions

Configure via `ConfigureToggleDock` inside the builder:

```csharp
services.AddDockLayoutService(dock =>
{
    dock.ConfigureToggleDock(opts =>
    {
        opts.ButtonSize = 28;           // Sidebar toggle button size in pixels
        opts.DefaultDockWidth = 280;    // Default width for side-docked panes
        opts.DefaultDockHeight = 220;   // Default height for bottom-docked panes
        opts.LayoutPriority = nameof(DockLayoutPriority.BottomFullWidth);
        opts.ShowHeaderMinimizeButton = true;
        opts.ShowHeaderOptionsButton = true;
    });
});
```

The `LayoutPriority` controls how the layout restructures when multiple panels are docked:

| Priority | Behavior | Similar To |
|:---------|:---------|:-----------|
| `BottomFullWidth` | Bottom panel spans the full window width | JetBrains Rider |
| `SidesFullHeight` | Side panels span the full window height | VS Code |
| `Default` | No automatic restructuring | Classic AvalonDock |

---

## Complete Composition Root

Here is a full `App.xaml.cs` showing all the pieces together, matching the `AvalonDockCodeApp` pattern:

```csharp
using System;
using System.Windows;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MyApp.ViewModels;

namespace MyApp;

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
                opts.LayoutPriority = nameof(DockLayoutPriority.BottomFullWidth);
            });

            // Register toolbox VMs — order determines sidebar button order
            dock.AddToolbox<ExplorerToolbox>();
            dock.AddToolbox<SearchToolbox>();
            dock.AddToolbox<SourceControlToolbox>();
            dock.AddToolbox<ProblemsToolbox>();
            dock.AddToolbox<TerminalToolbox>();
        });

        // MainViewModel receives IDockLayoutService
        services.AddSingleton<MainViewModel>();

        // MainWindow receives MainViewModel + ToggleDockOptions
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        (_serviceProvider as IDisposable)?.Dispose();
        base.OnExit(e);
    }
}
```

---

## How IDockLayoutService Works

The `IDockLayoutService` is the central service that replaces the old `DocumentsSource`/`AnchorablesSource` pattern:

### Opening Documents

```csharp
// Open a new document
_dockService.OpenDocument(new EditorTabViewModel { Title = "new.cs" });

// Open or activate an existing document
_dockService.OpenOrActivateDocument(
    existing => existing.FilePath == filePath,   // Find predicate
    () =>                                         // Factory if not found
    {
        var tab = new EditorTabViewModel();
        tab.LoadFile(filePath);
        return tab;
    });
```

### Closing Documents

```csharp
_dockService.CloseDocument(tab);
```

### Accessing Toolboxes

```csharp
// Get a specific toolbox by type
var explorer = _dockService.GetAnchorable<ExplorerToolbox>();
explorer?.LoadFolder("/some/path");

// Get all open documents
foreach (var doc in _dockService.Documents)
{
    Console.WriteLine(doc.Title);
}
```

### Active Document Tracking

```csharp
var active = _dockService.ActiveDockable;
_dockService.ActiveDockable = someDocument;
```

---

## MainWindow: Applying DockOptions

The `MainWindow` receives both the view model and the dock options through constructor injection:

```csharp
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, ToggleDockOptions? dockOptions = null)
    {
        DataContext = viewModel;
        InitializeComponent();

        if (dockOptions != null)
        {
            dockManager.ButtonSize = dockOptions.ButtonSize;
            dockManager.DefaultDockWidth = dockOptions.DefaultDockWidth;
            dockManager.DefaultDockHeight = dockOptions.DefaultDockHeight;
            dockManager.ShowHeaderMinimizeButton = dockOptions.ShowHeaderMinimizeButton;
            dockManager.ShowHeaderOptionsButton = dockOptions.ShowHeaderOptionsButton;

            if (Enum.TryParse<DockLayoutPriority>(dockOptions.LayoutPriority, out var priority))
            {
                dockManager.LayoutPriority = priority;
            }
        }
    }
}
```

---

## XAML Binding

The XAML binds `DockLayout` — a single property that drives the entire layout:

```xml
<avalonDock:ToggleDockingManager x:Name="dockManager"
    DockLayout="{Binding DockLayout}"
    LayoutItemContainerStyleSelector="{StaticResource PanesStyleSelector}">
    ...
</avalonDock:ToggleDockingManager>
```

Unlike the legacy `DockingManager` which requires separate `DocumentsSource` and `AnchorablesSource` bindings, the `ToggleDockingManager` with `DockLayout` gets everything from the `IRootDock` tree that `DockLayoutService` built.

---

## Disposing Resources

Toolboxes that hold system resources (like the `TerminalViewModel` which spawns a PowerShell process) should implement `IDisposable`:

```csharp
public class TerminalToolbox : ToolboxBase, IDisposable
{
    private Process? _shellProcess;

    public void Dispose()
    {
        if (_shellProcess is { HasExited: false })
            _shellProcess.Kill();
        _shellProcess?.Dispose();
    }
}
```

When the `IServiceProvider` is disposed in `App.OnExit()`, it will call `Dispose()` on all singleton services that implement `IDisposable`.

---

## Testing with DI

The DI pattern makes it easy to test view models in isolation:

```csharp
[Fact]
public void OpenFile_CreatesDocument()
{
    // Arrange: create a mock or real DockLayoutService
    var toolboxes = new IToolbox[] { new ExplorerToolbox(), new OutputToolbox() };
    var layoutService = new DockLayoutService(toolboxes);
    var vm = new MainViewModel(layoutService);

    // Act
    vm.OpenFile("test.cs");

    // Assert
    var doc = layoutService.FindDocument<EditorTabViewModel>(d => d.FilePath == "test.cs");
    Assert.NotNull(doc);
    Assert.Equal("test.cs", doc.Title);
}
```

---

## Next Steps

- Follow the [MVVM IDE tutorial]({% link tutorials/mvvm-ide.md %}) for a complete step-by-step walkthrough
- Add [Layout Persistence]({% link tutorials/layout-persistence.md %}) to save and restore layouts
- Apply [Custom Themes]({% link tutorials/styling-and-theming.md %}) to match your brand
- See the `AvalonDockCodeApp` project in the repository for the complete reference implementation
- Review all [DI extension methods]({% link guides/dependency-injection.md %}) in the reference guide
