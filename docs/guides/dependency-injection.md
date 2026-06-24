---
title: Dependency Injection
layout: default
parent: Guides
nav_order: 2
description: "Register AvalonDock services with dependency injection."
---

# Dependency Injection

AvalonDock v5 provides built-in support for `Microsoft.Extensions.DependencyInjection` through the `AvalonDock.DependencyInjection` package.

{: .tip }
For a full walkthrough, see [Tutorial: Dependency Injection Deep Dive]({% link tutorials/dependency-injection-app.md %}). The `AvalonDockCodeApp` sample project demonstrates all DI patterns.

---

## Install

```bash
dotnet add package Dirkster.AvalonDock.DependencyInjection
```

---

## Extension Methods Reference

### Core Registration

| Method | Purpose |
|:-------|:--------|
| `AddDockLayoutService(configure)` | Register `IDockLayoutService` with a builder to configure toolboxes and options |
| `AddDockLayoutService()` | Register `IDockLayoutService` without builder configuration |

#### DockLayoutBuilder Methods

Used inside the `AddDockLayoutService(dock => { ... })` builder:

| Method | Purpose |
|:-------|:--------|
| `ConfigureToggleDock(configure)` | Configure sidebar button size, dock dimensions, layout priority |
| `AddToolbox<T>()` | Register a toolbox VM as singleton |
| `AddToolbox<T>(factory)` | Register a toolbox VM with a custom factory |

### Additional Services

| Method | Purpose |
|:-------|:--------|
| `AddAvalonDock<TFactory>()` | Register a custom `IFactory` |
| `AddAvalonDockSerializer<T>()` | Register a layout serializer |
| `AddAvalonDockThemeManager<T>()` | Register a theme manager |
| `AddDockingManager(factory)` | Register an `IDockingManager` wrapper |
| `AddAutoHideManager<T>()` | Register an auto-hide manager |
| `AddFloatingWindowService<T>()` | Register a floating window service |
| `AddDragDropHandler<T>()` | Register a drag-and-drop handler |
| `AddDockLayoutService<T>()` | Register a custom `IDockLayoutService` implementation |

---

## Recommended Setup (v5 Pattern)

This matches the pattern used in the `AvalonDockCodeApp` sample:

```csharp
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

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

    // Register toolboxes — order determines sidebar button order
    dock.AddToolbox<ExplorerToolbox>();
    dock.AddToolbox<SearchToolbox>();
    dock.AddToolbox<OutputToolbox>();
});

// Application view models and windows
services.AddSingleton<MainViewModel>();
services.AddSingleton<MainWindow>();

var provider = services.BuildServiceProvider();
```

---

## ToggleDockOptions

Configure the sidebar and docking behavior via `ConfigureToggleDock` inside the builder:

```csharp
services.AddDockLayoutService(dock =>
{
    dock.ConfigureToggleDock(opts =>
    {
        opts.ButtonSize = 28;                // Sidebar button size (px)
        opts.DefaultDockWidth = 280;         // Default width for side panes
        opts.DefaultDockHeight = 220;        // Default height for bottom panes
        opts.LayoutPriority = "BottomFullWidth";  // Layout restructuring mode
        opts.ShowHeaderMinimizeButton = true; // Show minimize in pane headers
        opts.ShowHeaderOptionsButton = true;  // Show options (⋯) in pane headers
    });
});
```

If `ConfigureToggleDock` is not called, default `ToggleDockOptions` are registered automatically.

### Layout Priority Modes

| Mode | Behavior | Similar To |
|:-----|:---------|:-----------|
| `BottomFullWidth` | Bottom panel spans full width | JetBrains Rider |
| `SidesFullHeight` | Side panels span full height | VS Code |
| `Default` | No automatic restructuring | Classic AvalonDock |

---

## Factory Registration

For toolboxes that need constructor parameters, use the factory overload inside the builder:

```csharp
services.AddDockLayoutService(dock =>
{
    dock.AddToolbox<FolderExplorerToolbox>(sp =>
        new FolderExplorerToolbox(filePath => { /* callback */ }));
});
```

---

## WPF App Integration

```csharp
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

    protected override void OnExit(ExitEventArgs e)
    {
        (_serviceProvider as IDisposable)?.Dispose();
        base.OnExit(e);
    }
}
```

{: .important }
Remove `StartupUri="MainWindow.xaml"` from `App.xaml` when using DI — the window is created via `GetRequiredService<MainWindow>()`.

---

## Applying Options in MainWindow

The `MainWindow` receives the view model and dock options via constructor injection:

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
                dockManager.LayoutPriority = priority;
        }
    }
}
```

---

## Resolving Services in View Models

```csharp
public class MainViewModel
{
    private readonly IDockLayoutService _dockService;

    public MainViewModel(IDockLayoutService dockService)
    {
        _dockService = dockService;
    }

    // Typed access to any registered toolbox
    public ExplorerToolbox? Explorer => _dockService.GetAnchorable<ExplorerToolbox>();
}
```
