---
title: Dependency Injection
layout: default
parent: Guides
nav_order: 2
description: "Register AvalonDock services with dependency injection."
---

# Dependency Injection

AvalonDock v5.0.0 provides built-in support for `Microsoft.Extensions.DependencyInjection` through the `AvalonDock.DependencyInjection` package.

---

## Install

```bash
dotnet add package Dirkster.AvalonDock.DependencyInjection
```

---

## Register Services

Use the `AddAvalonDock()` extension method on `IServiceCollection`:

```csharp
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddAvalonDock(options =>
{
    // Configure options
});

// Register your view models
services.AddTransient<MainViewModel>();
services.AddTransient<DocumentViewModel>();
services.AddTransient<ExplorerViewModel>();

var provider = services.BuildServiceProvider();
```

---

## Usage with WPF Host

If you're using `Microsoft.Extensions.Hosting` with WPF:

```csharp
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddAvalonDock();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
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

---

## ToggleDockOptions

Configure docking behavior through `ToggleDockOptions`:

```csharp
services.AddAvalonDock(options =>
{
    options.AllowFloatingWindows = true;
    options.AllowAutoHide = true;
});
```

---

## Resolving Services in View Models

With DI configured, your view models can receive AvalonDock services through constructor injection:

```csharp
public class MainViewModel
{
    private readonly IDockLayoutService _layoutService;
    private readonly IThemeManager _themeManager;

    public MainViewModel(
        IDockLayoutService layoutService,
        IThemeManager themeManager)
    {
        _layoutService = layoutService;
        _themeManager = themeManager;
    }
}
```
