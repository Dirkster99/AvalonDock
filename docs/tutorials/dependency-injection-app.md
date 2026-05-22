---
title: "Tutorial: Dependency Injection App"
layout: default
parent: Tutorials
nav_order: 2
description: "Build a modern AvalonDock application using Microsoft.Extensions.DependencyInjection."
---

# Tutorial: Build a Dependency Injection App

In this tutorial you will build a modern AvalonDock application that uses `Microsoft.Extensions.DependencyInjection` for service registration, constructor injection, and toolbox management. This replaces the static singleton pattern with a clean, testable architecture.

{: .tip }
This tutorial is inspired by the `AvalonDockCodeApp` sample project in the repository, which demonstrates the newest DI and MVVM patterns in AvalonDock v5.0.0.

---

## What You'll Build

A code viewer application with:
- **DI-registered toolbox panels** (Explorer, Search, Terminal)
- **Constructor-injected view models** with AvalonDock services
- **`IDockLayoutService`** for programmatic layout management
- **`ToggleDockOptions`** for configuring docking behavior
- A clean `App.xaml.cs` composition root with no singletons

---

## Prerequisites

- .NET 9 or later SDK

```bash
dotnet new wpf -n DiDockApp
cd DiDockApp
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Mvvm
dotnet add package Dirkster.AvalonDock.DependencyInjection
dotnet add package Dirkster.AvalonDock.Themes.Arc
dotnet add package Microsoft.Extensions.Hosting
```

---

## Step 1: Create View Model Base Classes

AvalonDock's MVVM package provides base classes for documents and tool windows.

**File: `ViewModels/DocumentViewModel.cs`**

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiDockApp.ViewModels;

public class DocumentViewModel : INotifyPropertyChanged
{
    private string _title;
    private string _contentId;
    private string _text;

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

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

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

## Step 2: Create Toolbox View Models

Each tool panel is a separate view model registered with the DI container.

**File: `ViewModels/ExplorerToolViewModel.cs`**

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiDockApp.ViewModels;

public class ExplorerToolViewModel : INotifyPropertyChanged
{
    public string Title => "Explorer";
    public string ContentId => "explorer";

    public ObservableCollection<string> Files { get; } = new()
    {
        "Program.cs",
        "MainWindow.xaml",
        "App.xaml",
        "appsettings.json"
    };

    private string _selectedFile;
    public string SelectedFile
    {
        get => _selectedFile;
        set => SetProperty(ref _selectedFile, value);
    }

    /// <summary>
    /// Callback set by MainViewModel to handle file opening.
    /// </summary>
    public Action<string> OnFileOpen { get; set; }

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

**File: `ViewModels/SearchToolViewModel.cs`**

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiDockApp.ViewModels;

public class SearchToolViewModel : INotifyPropertyChanged
{
    public string Title => "Search";
    public string ContentId => "search";

    private string _searchQuery;
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    private string _results = "Enter a search term above.";
    public string Results
    {
        get => _results;
        set => SetProperty(ref _results, value);
    }

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

**File: `ViewModels/TerminalToolViewModel.cs`**

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiDockApp.ViewModels;

public class TerminalToolViewModel : INotifyPropertyChanged
{
    public string Title => "Terminal";
    public string ContentId => "terminal";

    private string _output = "PS> Ready.\n";
    public string Output
    {
        get => _output;
        set => SetProperty(ref _output, value);
    }

    public void AppendLine(string text)
    {
        Output += text + "\n";
    }

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

## Step 3: Create the Main View Model

The `MainViewModel` receives all its dependencies via constructor injection — no singletons, no service locator.

**File: `ViewModels/MainViewModel.cs`**

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DiDockApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private DocumentViewModel _activeDocument;
    private int _docCounter;

    public MainViewModel(
        ExplorerToolViewModel explorer,
        SearchToolViewModel search,
        TerminalToolViewModel terminal)
    {
        Explorer = explorer;
        Search = search;
        Terminal = terminal;

        Tools = new ObservableCollection<object> { Explorer, Search, Terminal };
        Documents = new ObservableCollection<DocumentViewModel>();

        // Wire up file-open callback
        Explorer.OnFileOpen = OpenDocument;

        // Create a welcome document
        OpenDocument("Welcome");

        Terminal.AppendLine("Application initialized via DI.");
    }

    // --- Injected Tools ---
    public ExplorerToolViewModel Explorer { get; }
    public SearchToolViewModel Search { get; }
    public TerminalToolViewModel Terminal { get; }

    // --- Collections ---
    public ObservableCollection<DocumentViewModel> Documents { get; }
    public ObservableCollection<object> Tools { get; }

    // --- Active Document ---
    public DocumentViewModel ActiveDocument
    {
        get => _activeDocument;
        set => SetProperty(ref _activeDocument, value);
    }

    // --- Commands ---
    public ICommand NewDocumentCommand => new RelayCommand(_ => OpenDocument($"Untitled {++_docCounter}"));

    // --- Methods ---
    public void OpenDocument(string name)
    {
        // Check if already open
        var existing = Documents.FirstOrDefault(d => d.Title == name);
        if (existing != null)
        {
            ActiveDocument = existing;
            return;
        }

        var doc = new DocumentViewModel
        {
            Title = name,
            ContentId = Guid.NewGuid().ToString(),
            Text = $"// Content of {name}\n"
        };
        Documents.Add(doc);
        ActiveDocument = doc;
        Terminal.AppendLine($"Opened: {name}");
    }

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

**File: `RelayCommand.cs`**

```csharp
using System.Windows.Input;

namespace DiDockApp;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute;
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

## Step 4: Configure the DI Composition Root

This is where everything gets wired up. The `App.xaml.cs` acts as the composition root.

**File: `App.xaml.cs`**

```csharp
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DiDockApp.ViewModels;

namespace DiDockApp;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register AvalonDock services
                services.AddAvalonDock();

                // Register tool view models as singletons
                // (tools persist for the app lifetime)
                services.AddSingleton<ExplorerToolViewModel>();
                services.AddSingleton<SearchToolViewModel>();
                services.AddSingleton<TerminalToolViewModel>();

                // Register main view model
                services.AddSingleton<MainViewModel>();

                // Register the main window
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
```

{: .important }
Remove the `StartupUri="MainWindow.xaml"` attribute from your `App.xaml` file since you are now creating the window manually via DI.

**File: `App.xaml`** (updated)

```xml
<Application x:Class="DiDockApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources />
</Application>
```

---

## Step 5: Build the XAML Layout

**File: `MainWindow.xaml`**

```xml
<Window x:Class="DiDockApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:themes="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.Arc"
        xmlns:vm="clr-namespace:DiDockApp.ViewModels"
        Title="DI Dock App — AvalonDock Tutorial" Height="600" Width="900">

    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_New Document" Command="{Binding NewDocumentCommand}" />
            </MenuItem>
        </Menu>

        <avalonDock:DockingManager
            DocumentsSource="{Binding Documents}"
            AnchorablesSource="{Binding Tools}">

            <avalonDock:DockingManager.Theme>
                <themes:ArcDarkTheme />
            </avalonDock:DockingManager.Theme>

            <!-- Document template -->
            <avalonDock:DockingManager.DocumentTemplate>
                <DataTemplate DataType="{x:Type vm:DocumentViewModel}">
                    <TextBox Text="{Binding Text, UpdateSourceTrigger=PropertyChanged}"
                             AcceptsReturn="True" FontFamily="Consolas" FontSize="13"
                             VerticalScrollBarVisibility="Auto" />
                </DataTemplate>
            </avalonDock:DockingManager.DocumentTemplate>

            <!-- Anchorable templates -->
            <avalonDock:DockingManager.AnchorableTemplate>
                <DataTemplate>
                    <ContentControl Content="{Binding}">
                        <ContentControl.Resources>
                            <DataTemplate DataType="{x:Type vm:ExplorerToolViewModel}">
                                <ListBox ItemsSource="{Binding Files}"
                                         SelectedItem="{Binding SelectedFile}"
                                         Margin="4" />
                            </DataTemplate>

                            <DataTemplate DataType="{x:Type vm:SearchToolViewModel}">
                                <StackPanel Margin="8">
                                    <TextBox Text="{Binding SearchQuery, UpdateSourceTrigger=PropertyChanged}"
                                             Margin="0,0,0,8" />
                                    <TextBlock Text="{Binding Results}" TextWrapping="Wrap" />
                                </StackPanel>
                            </DataTemplate>

                            <DataTemplate DataType="{x:Type vm:TerminalToolViewModel}">
                                <TextBox Text="{Binding Output, Mode=OneWay}"
                                         IsReadOnly="True" AcceptsReturn="True"
                                         FontFamily="Consolas" FontSize="12"
                                         VerticalScrollBarVisibility="Auto"
                                         Background="#1E1E1E" Foreground="#DCDCDC" />
                            </DataTemplate>
                        </ContentControl.Resources>
                    </ContentControl>
                </DataTemplate>
            </avalonDock:DockingManager.AnchorableTemplate>

            <!-- Style for binding Title and ContentId -->
            <avalonDock:DockingManager.LayoutItemContainerStyle>
                <Style TargetType="{x:Type avalonDock:LayoutItem}">
                    <Setter Property="Title" Value="{Binding Model.Title}" />
                    <Setter Property="ContentId" Value="{Binding Model.ContentId}" />
                </Style>
            </avalonDock:DockingManager.LayoutItemContainerStyle>

            <!-- Initial layout -->
            <avalonDock:LayoutRoot>
                <avalonDock:LayoutPanel Orientation="Horizontal">
                    <avalonDock:LayoutAnchorablePane DockWidth="200" />
                    <avalonDock:LayoutDocumentPane />
                </avalonDock:LayoutPanel>

                <avalonDock:LayoutRoot.BottomSide>
                    <avalonDock:LayoutAnchorSide>
                        <avalonDock:LayoutAnchorGroup>
                            <avalonDock:LayoutAnchorable Title="Terminal"
                                                          ContentId="terminal" />
                        </avalonDock:LayoutAnchorGroup>
                    </avalonDock:LayoutAnchorSide>
                </avalonDock:LayoutRoot.BottomSide>
            </avalonDock:LayoutRoot>
        </avalonDock:DockingManager>
    </DockPanel>
</Window>
```

---

## How It Works

### The DI Container Graph

```
IHost
└── ServiceProvider
    ├── MainWindow (singleton)
    │   └── DataContext = MainViewModel
    ├── MainViewModel (singleton)
    │   ├── ExplorerToolViewModel (injected)
    │   ├── SearchToolViewModel (injected)
    │   └── TerminalToolViewModel (injected)
    └── AvalonDock services (via AddAvalonDock())
```

### Key Concepts

1. **`AddAvalonDock()`** — Registers AvalonDock's internal services with the DI container. This is the entry point for DI integration.

2. **Tool registration as singletons** — Tool panels typically live for the entire application lifetime, so they are registered as singletons. Documents can be transient since they are created and destroyed dynamically.

3. **Constructor injection** — The `MainViewModel` receives its dependencies through the constructor, not via a service locator or static instance. This makes it easy to test and replace dependencies.

4. **Composition root in `App.xaml.cs`** — All service registration happens in one place. The rest of the application is unaware of the DI container.

5. **No `StartupUri`** — When using DI, you create the window manually via `GetRequiredService<MainWindow>()` and call `Show()`. Remove the `StartupUri` from `App.xaml`.

### Why DI Over Singletons?

| Aspect | Singleton Pattern | DI Pattern |
|:-------|:------------------|:-----------|
| **Testability** | Hard to mock static instances | Easy to inject mocks |
| **Coupling** | View models know about the singleton | View models only know interfaces |
| **Lifetime** | Global state, hard to reset | Container manages lifetimes |
| **Scalability** | Complex as app grows | Clean separation of concerns |

---

## Adding More Tools

To add a new tool panel:

1. **Create the view model:**
   ```csharp
   public class ProblemsToolViewModel : INotifyPropertyChanged
   {
       public string Title => "Problems";
       public string ContentId => "problems";
       // ... properties and logic
   }
   ```

2. **Register it in `App.xaml.cs`:**
   ```csharp
   services.AddSingleton<ProblemsToolViewModel>();
   ```

3. **Inject it into `MainViewModel`:**
   ```csharp
   public MainViewModel(
       ExplorerToolViewModel explorer,
       SearchToolViewModel search,
       TerminalToolViewModel terminal,
       ProblemsToolViewModel problems)  // <-- add here
   ```

4. **Add a `DataTemplate` in XAML** for the new view model type.

---

## Next Steps

- Add [Layout Persistence]({% link tutorials/layout-persistence.md %}) to save the user's panel arrangement
- Apply [Custom Styling]({% link tutorials/styling-and-theming.md %}) to match your brand
- See the `AvalonDockCodeApp` project in the repository for a complete DI example with `ToggleDockingManager`
- Review the [DI Guide]({% link guides/dependency-injection.md %}) for `ToggleDockOptions` configuration details
