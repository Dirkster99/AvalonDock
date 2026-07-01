---
title: Theming
layout: default
parent: Guides
nav_order: 3
description: "Apply and switch AvalonDock themes at runtime."
---

# Theming

AvalonDock v5.0.0 ships with six built-in themes that can be applied and switched at runtime with a single line of code.

---

## Applying a Theme

### In XAML

```xml
<avalonDock:DockingManager>
    <avalonDock:DockingManager.Theme>
        <themes:ArcDarkTheme />
    </avalonDock:DockingManager.Theme>
    ...
</avalonDock:DockingManager>
```

### In Code

```csharp
using AvalonDock.Themes;

dockManager.Theme = new ArcDarkTheme();
```

---

## Available Themes

### Arc Theme *(New in v5.0.0)*

Modern theme with compact tabs, rounded corners, and semi-transparent elements. Inspired by contemporary IDEs.

```bash
dotnet add package Dirkster.AvalonDock.Themes.Arc
```

| Variant | Class | Look |
|:--------|:------|:-----|
| Dark | `ArcDarkTheme` | Dark background with subtle contrasts |
| Light | `ArcLightTheme` | Clean white background with soft borders |

### VS2013 Theme

Classic Visual Studio 2013 look with three color variants.

```bash
dotnet add package Dirkster.AvalonDock.Themes.VS2013
```

| Variant | Class |
|:--------|:------|
| Dark | `Vs2013DarkTheme` |
| Light | `Vs2013LightTheme` |
| Blue | `Vs2013BlueTheme` |

### VS2010 Theme

Visual Studio 2010 styling.

```bash
dotnet add package Dirkster.AvalonDock.Themes.VS2010
```

### Expression Theme

Expression Blend inspired styling with dark and light modes.

```bash
dotnet add package Dirkster.AvalonDock.Themes.Expression
```

### Metro Theme

Modern Metro / WinUI style with clean lines and flat design.

```bash
dotnet add package Dirkster.AvalonDock.Themes.Metro
```

### Aero Theme

Classic Windows Aero glass-inspired look.

```bash
dotnet add package Dirkster.AvalonDock.Themes.Aero
```

---

## Switching Themes at Runtime

Themes can be changed dynamically. The DockingManager will update all controls immediately:

```csharp
// Theme selector
public void ApplyTheme(string themeName)
{
    dockManager.Theme = themeName switch
    {
        "Arc Dark"    => new ArcDarkTheme(),
        "Arc Light"   => new ArcLightTheme(),
        "VS2013 Dark" => new Vs2013DarkTheme(),
        "VS2013 Light"=> new Vs2013LightTheme(),
        "VS2013 Blue" => new Vs2013BlueTheme(),
        "VS2010"      => new VS2010Theme(),
        "Metro"       => new MetroTheme(),
        "Aero"        => new AeroTheme(),
        _             => new GenericTheme()
    };
}
```

---

## Using Theme Brush Resources

You can import theme brushes into your own resource dictionaries to keep your application visually consistent:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="/AvalonDock.Themes.Arc;component/DarkBrushs.xaml" />
</ResourceDictionary.MergedDictionaries>
```

{: .note }
Theme brush resources only cover AvalonDock controls. For full application theming, combine with a WPF theming library like [MahApps.Metro](https://github.com/MahApps/MahApps.Metro), [MLib](https://github.com/Dirkster99/MLib), or [MUI](https://github.com/firstfloorsoftware/mui).

---

## DictionaryTheme

If you have a custom resource dictionary with AvalonDock styles, you can wrap it in a `DictionaryTheme`:

```csharp
var resourceDict = new ResourceDictionary
{
    Source = new Uri("pack://application:,,,/MyApp;component/Themes/CustomDockTheme.xaml")
};

dockManager.Theme = new DictionaryTheme(resourceDict);
```

See the [Custom Themes]({% link guides/custom-themes.md %}) guide for more details on creating your own theme from scratch.
