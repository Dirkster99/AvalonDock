# AvalonDock

[![CI](https://github.com/Dirkster99/AvalonDock/actions/workflows/ci.yml/badge.svg)](https://github.com/Dirkster99/AvalonDock/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/release/Dirkster99/avalondock.svg)](https://github.com/Dirkster99/avalondock/releases/latest)
[![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.svg)](http://nuget.org/packages/Dirkster.AvalonDock)

![Net48](https://badgen.net/badge/Framework/.NET&nbsp;4.8/blue) ![Net9](https://badgen.net/badge/Framework/.NET&nbsp;9/blue) ![Net10](https://badgen.net/badge/Framework/.NET&nbsp;10/blue)

Support this project with a :star: — report an issue, or even better, place a pull request :mailbox: :blush:

AvalonDock is a WPF Document and Tool Window layout container that is used to arrange documents and tool windows in similar ways to many well known IDEs, such as Visual Studio, Eclipse, JetBrains Rider, and more. **Version 5.0** introduces first-class MVVM support, dependency injection integration, and a modular package architecture.

My projects [Edi](https://dirkster99.github.io/Edi/), [Aehnlich](https://github.com/Dirkster99/Aehnlich), and [many others](https://github.com/search?p=4&q=%22dirkster.avalondock%22&type=Code) (open source and commercial) are powered by this project.

Be sure to check out the [Wiki](https://github.com/Dirkster99/AvalonDock/wiki) and the [documentation site](docs/) for tutorials and API reference.

---

## NuGet Packages

### Core

| Package | Description |
|:--------|:------------|
| [Dirkster.AvalonDock](http://nuget.org/packages/Dirkster.AvalonDock) | Main WPF docking framework package |
| [Dirkster.AvalonDock.Core](http://nuget.org/packages/Dirkster.AvalonDock.Core) | UI-agnostic interfaces and models |
| [Dirkster.AvalonDock.Mvvm](http://nuget.org/packages/Dirkster.AvalonDock.Mvvm) | MVVM base classes (`DockableBase`, `ToolboxBase`, `DockLayoutService`) |
| [Dirkster.AvalonDock.DependencyInjection](http://nuget.org/packages/Dirkster.AvalonDock.DependencyInjection) | `IServiceCollection` extensions for DI registration |

### Serializers

| Package | Description |
|:--------|:------------|
| [Dirkster.AvalonDock.Serializer.Xml](http://nuget.org/packages/Dirkster.AvalonDock.Serializer.Xml) | XML layout serialization |
| [Dirkster.AvalonDock.Serializer.Json](http://nuget.org/packages/Dirkster.AvalonDock.Serializer.Json) | JSON layout serialization (**new in v5**) |

### Themes

| Package | Downloads |
|:--------|:----------|
| [Dirkster.AvalonDock.Themes.Arc](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Arc) | [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Arc.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Arc) **NEW!** |
| [Dirkster.AvalonDock.Themes.Aero](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Aero) | [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Aero.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Aero) |
| [Dirkster.AvalonDock.Themes.Expression](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression) | [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Expression.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Expression) |
| [Dirkster.AvalonDock.Themes.Metro](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro) | [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.Metro.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.Metro) |
| [Dirkster.AvalonDock.Themes.VS2010](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2010) | [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2010.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2010) |
| [Dirkster.AvalonDock.Themes.VS2013](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013) | [![NuGet](https://img.shields.io/nuget/dt/Dirkster.AvalonDock.Themes.VS2013.svg)](http://nuget.org/packages/Dirkster.AvalonDock.Themes.VS2013) |

---

## Quick Start

Install the packages you need:

```bash
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Mvvm
dotnet add package Dirkster.AvalonDock.DependencyInjection
dotnet add package Dirkster.AvalonDock.Themes.Arc
```

---

## Dependency Injection

The `AvalonDock.DependencyInjection` package provides `IServiceCollection` extension methods to wire up the entire docking system through your DI container. This replaces manual `DocumentsSource`/`AnchorablesSource` binding with a clean, service-oriented composition root.

### Extension Methods

| Method | Purpose |
|:-------|:--------|
| `AddToolbox<T>()` | Register a toolbox (anchorable) view model as a singleton |
| `AddToolbox<T>(factory)` | Register a toolbox with a custom factory for constructor parameters |
| `AddDockLayoutService()` | Register `IDockLayoutService` — auto-builds the layout tree from all `IToolbox` instances |
| `AddToggleDockOptions(configure)` | Configure sidebar button size, default dock dimensions, and layout priority |
| `AddAvalonDock<TFactory>()` | Register a custom `IFactory` implementation |
| `AddAvalonDockSerializer<T>()` | Register an `ILayoutSerializer` (XML or JSON) |
| `AddAvalonDockThemeManager<T>()` | Register a theme manager |
| `AddDockingManager(factory)` | Register an `IDockingManager` wrapper |
| `AddAutoHideManager<T>()` | Register an auto-hide manager |
| `AddFloatingWindowService<T>()` | Register a floating window service |
| `AddDragDropHandler<T>()` | Register a custom drag-and-drop handler |

### Composition Root Example

```csharp
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Configure toggle dock options (sidebar behavior)
        services.AddToggleDockOptions(opts =>
        {
            opts.ButtonSize = 28;
            opts.DefaultDockWidth = 280;
            opts.DefaultDockHeight = 220;
            opts.LayoutPriority = nameof(AvalonDock.DockLayoutPriority.BottomFullWidth);
        });

        // Register toolbox view models
        services.AddToolbox<ExplorerToolbox>();
        services.AddToolbox<SearchToolbox>();
        services.AddToolbox<TerminalToolbox>();

        // Register each as IToolbox for auto-discovery
        services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<ExplorerToolbox>());
        services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<SearchToolbox>());
        services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<TerminalToolbox>());

        // Auto-build layout tree from registered toolboxes
        services.AddDockLayoutService();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        (_serviceProvider as IDisposable)?.Dispose();
        base.OnExit(e);
    }
}
```

### Layout Priority Options

The `LayoutPriority` setting controls how panels are arranged when multiple panels are docked:

| Priority | Behavior | Similar To |
|:---------|:---------|:-----------|
| `BottomFullWidth` | Bottom panel spans the full window width | JetBrains Rider |
| `SidesFullHeight` | Side panels span the full window height | VS Code |
| `Default` | No automatic restructuring | Classic AvalonDock |

For a complete walkthrough, see the [Dependency Injection tutorial](docs/tutorials/dependency-injection-app.md).

---

## MVVM

The `AvalonDock.Mvvm` package provides ready-to-use view model base classes built on [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet). These classes implement the core interfaces from `AvalonDock.Core` and handle property change notifications, serialization attributes, and docking behavior out of the box.

### Base Classes

| Class | Purpose |
|:------|:--------|
| `DockableBase` | Base for all dockable view models — provides `Id`, `Title`, `CanClose`, `CanFloat`, `CanPin`, `IsModified`, `DockState`, and more |
| `ToolboxBase` | Base for toolbox (anchorable) view models — adds `Zone`, `IsOpenByDefault`, `ToolTipText`, and `Icon` |
| `DockBase` | Container for multiple dockables — manages `VisibleDockables`, `ActiveDockable`, and navigation |
| `RootDock` | Root of the layout tree — manages floating and pinned dockables |
| `DocumentDock` | Container for document tabs |
| `ToolDock` | Container for tool windows with `Alignment` and `AutoHide` support |
| `Document` / `Tool` | Leaf nodes for document and tool content |

### Creating a Toolbox View Model

```csharp
using AvalonDock.Core;
using AvalonDock.Mvvm;

public class ExplorerToolbox : ToolboxBase
{
    public ExplorerToolbox()
    {
        Id = "Explorer";
        Title = "Explorer";
        Zone = DockZone.LeftTop;        // Sidebar placement zone
        IsOpenByDefault = true;          // Open when app starts
        ToolTipText = "Solution Explorer";
    }
}
```

### Dock Zones

Toolboxes declare their sidebar placement using `DockZone`:

| Zone | Position |
|:-----|:---------|
| `LeftTop` / `LeftBottom` | Left sidebar |
| `RightTop` / `RightBottom` | Right sidebar |
| `BottomLeft` / `BottomRight` | Bottom panel |

### IDockLayoutService

`IDockLayoutService` is the central service for managing documents and toolboxes at runtime:

```csharp
public class MainViewModel
{
    private readonly IDockLayoutService _dockService;

    public MainViewModel(IDockLayoutService dockService)
    {
        _dockService = dockService;
    }

    // Open a new document
    public void OpenFile(string filePath)
    {
        _dockService.OpenOrActivateDocument(
            existing => existing.FilePath == filePath,
            () => new EditorTabViewModel { Title = Path.GetFileName(filePath) });
    }

    // Close a document
    public void CloseFile(IDockable document) => _dockService.CloseDocument(document);

    // Access a registered toolbox by type
    public ExplorerToolbox? Explorer => _dockService.GetAnchorable<ExplorerToolbox>();

    // Iterate all open documents
    public IEnumerable<IDockable> Documents => _dockService.Documents;

    // Bind this to ToggleDockingManager.DockLayout in XAML
    public IRootDock DockLayout => _dockService.Layout;
}
```

### XAML Binding

```xml
<avalonDock:ToggleDockingManager x:Name="dockManager"
    DockLayout="{Binding DockLayout}"
    LayoutItemContainerStyleSelector="{StaticResource PanesStyleSelector}" />
```

---

## Architecture (v5.0)

AvalonDock v5 is organized into modular packages with clear separation of concerns:

```
AvalonDock.Core            UI-agnostic interfaces (IDockable, IDock, IFactory, IToolbox, etc.)
  ├── netstandard2.0       Cross-platform abstractions
  └── net9.0

AvalonDock.Mvvm            CommunityToolkit.Mvvm view models (DockableBase, ToolboxBase, etc.)
  ├── netstandard2.0
  └── net9.0

AvalonDock.DependencyInjection  IServiceCollection extensions
  ├── netstandard2.0
  └── net9.0

AvalonDock                 WPF docking library (DockingManager, ToggleDockingManager)
  ├── net9.0-windows
  ├── net10.0-windows
  └── net48

AvalonDock.Serializer.Xml  XML layout persistence (extracted from core in v5)
AvalonDock.Serializer.Json JSON layout persistence (new in v5)
AvalonDock.Themes.*        Theme packages (Arc, Aero, Expression, Metro, VS2010, VS2013)
```

### Key Interfaces (AvalonDock.Core)

| Interface | Purpose |
|:----------|:--------|
| `IDockable` | Fundamental dockable content unit — title, state, close/pin/float capability |
| `IDock` | Container holding multiple `IDockable` children |
| `IRootDock` | Root of the layout tree — floating and pinned dockables |
| `IDocumentDock` | Document area container |
| `IToolDock` | Tool window container with alignment and auto-hide |
| `IToolbox` | Toolbox with zone-based sidebar placement |
| `IFactory` | Abstract factory for creating and managing layout elements |
| `IDockLayoutService` | High-level document/toolbox management |
| `ILayoutSerializer` | Pluggable layout serialization (XML or JSON) |

---

## Theming

### Arc Theme (NEW!)

Modern theme with compact tabs, rounded corners, and semi-transparent design elements:

```csharp
dockManager.Theme = new ArcDarkTheme();  // Dark mode
dockManager.Theme = new ArcLightTheme(); // Light mode
```

### VS2013 Theme

Classic Visual Studio 2013 look with Dark, Light, and Blue variants:

```csharp
dockManager.Theme = new Vs2013DarkTheme();
dockManager.Theme = new Vs2013LightTheme();
dockManager.Theme = new Vs2013BlueTheme();
```

### Other Themes

- **VS2010** — Visual Studio 2010 style
- **Expression Dark/Light** — Expression Blend inspired
- **Metro** — Modern Metro/WinUI style
- **Aero** — Classic Windows Aero theme

---

## Migrating from v4.x

Version 5.0.0 includes several breaking changes. See the full [Breaking Changes](docs/migration/breaking-changes.md) guide.

### Key Changes

| Change | Impact | Action |
|:-------|:-------|:-------|
| XML serializer moved to `AvalonDock.Serializer.Xml` | High | Install serializer package, update `using` statements |
| Namespace `AvalonDock.Layout.Serialization` → `AvalonDock.Serializer.Xml` | High | Update `using` statements |
| .NET Framework < 4.8 dropped | High | Upgrade target framework |
| .NET Core 3.x / 5 dropped | High | Upgrade target framework |
| `ILayoutEngine` introduced | Low | No action for default behavior |

### Supported Frameworks

- **.NET Framework 4.8**
- **.NET 9.0** (`net9.0-windows`)
- **.NET 10.0** (`net10.0-windows`)

---

## Building from Source

This project requires **Windows** (WPF is Windows-only) and **.NET SDK 9.0.x / 10.0.x**.

```bash
dotnet restore source/AvalonDock.sln
dotnet build source/AvalonDock.sln --configuration Release --no-restore
dotnet test source/AvalonDock.sln --configuration Release --no-restore -m:1
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

---

## Screenshots

The Docking Buttons are [defined in XAML](https://github.com/Dirkster99/AvalonDock/wiki/OverlayWindow), which ensures a good looking image on all resolutions, even 4K or 8K, and enables consistent color theming.

<table width="100%">
   <tr>
      <td>Description</td>
      <td>Dark</td>
      <td>Light</td>
   </tr>
   <tr>
      <td>Dock Document</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockDocument.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockDocument.png" width="400"></td>
   </tr>
   <tr>
      <td>Dock Document</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockDocument_1.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockDocument_1.png" width="400"></td>
   </tr>
   <tr>
      <td>Dock Tool Window</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockToolWindow.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockToolWindow.png" width="400"></td>
   </tr>
   <tr>
      <td>Document</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/Document.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/Document.png" width="400"></td>
   </tr>
   <tr>
      <td>Tool Window</td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/ToolWindow.png" width="400"></td>
      <td><img src="https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/ToolWindow.png" width="400"></td>
   </tr>
</table>

---

## Release History

For detailed release notes and version history, see the [GitHub Releases](https://github.com/Dirkster99/AvalonDock/releases) page.

## More Resources

- [Documentation](docs/)
- [Project Wiki](https://github.com/Dirkster99/AvalonDock/wiki)
- [DI Tutorial](docs/tutorials/dependency-injection-app.md)
- [Breaking Changes (v5.0)](docs/migration/breaking-changes.md)
- [Contributing Guidelines](CONTRIBUTING.md)
- [License (MS-PL)](LICENSE)
