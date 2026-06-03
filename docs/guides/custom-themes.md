---
title: Custom Themes
layout: default
parent: Guides
nav_order: 5
description: "Create custom themes for AvalonDock."
---

# Custom Themes

AvalonDock's theme system is based on WPF resource dictionaries, making it straightforward to create custom themes that match your application's branding.

---

## Approach 1: DictionaryTheme (Quickest)

The simplest way to create a custom theme is to modify an existing theme's brushes in a resource dictionary and wrap it with `DictionaryTheme`:

### Step 1: Create a Resource Dictionary

Create a XAML resource dictionary with your custom brushes:

```xml
<!-- Themes/MyCustomTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Merge the base theme as a starting point -->
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/AvalonDock.Themes.Arc;component/DarkBrushs.xaml" />
    </ResourceDictionary.MergedDictionaries>

    <!-- Override specific brushes -->
    <!-- Add your custom brush overrides here -->

</ResourceDictionary>
```

### Step 2: Apply in Code

```csharp
var resourceDict = new ResourceDictionary
{
    Source = new Uri("pack://application:,,,/MyApp;component/Themes/MyCustomTheme.xaml")
};

dockManager.Theme = new DictionaryTheme(resourceDict);
```

---

## Approach 2: Custom Theme Class

For a more structured approach, create a class that inherits from `Theme`:

### Step 1: Create the Theme Class

```csharp
using AvalonDock.Themes;
using System;
using System.Windows;

public class MyCustomTheme : Theme
{
    public override Uri GetResourceUri()
    {
        return new Uri(
            "/MyApp;component/Themes/MyCustomDockTheme.xaml",
            UriKind.Relative);
    }
}
```

### Step 2: Create the Resource Dictionary

Create the full theme resource dictionary at `Themes/MyCustomDockTheme.xaml`. Start by copying the XAML from one of the built-in themes and modifying it.

### Step 3: Apply the Theme

```csharp
dockManager.Theme = new MyCustomTheme();
```

Or in XAML:

```xml
<avalonDock:DockingManager.Theme>
    <local:MyCustomTheme />
</avalonDock:DockingManager.Theme>
```

---

## Key Brush Resources

When customizing themes, these are the most commonly overridden brush resources:

| Resource Key | Controls |
|:-------------|:---------|
| Tab backgrounds | Document and anchorable tab colors |
| Tab foregrounds | Tab text colors |
| Pane headers | Tool window title bar backgrounds |
| Grip colors | Drag grip indicators |
| Border brushes | Panel and tab borders |
| Selection highlights | Active/selected tab highlights |
| Auto-hide tab brushes | Side tab colors |
| Floating window backgrounds | Floating window chrome |

{: .tip }
The easiest way to discover all available brush keys is to examine the XAML source of the [built-in themes](https://github.com/Dirkster99/AvalonDock/tree/master/source/Components) in the repository.

---

## Combining with Application Themes

AvalonDock themes only style the docking controls. For a consistent application look, combine your AvalonDock theme with a full WPF theming library:

- **[MahApps.Metro](https://github.com/MahApps/MahApps.Metro)** — Modern Metro-style theming
- **[MLib](https://github.com/Dirkster99/MLib)** — Lightweight theming companion for AvalonDock
- **[MUI](https://github.com/firstfloorsoftware/mui)** — Modern UI framework

These libraries will theme standard WPF controls (buttons, textboxes, etc.) while AvalonDock handles the docking chrome.
