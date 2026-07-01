---
title: "Tutorial: Styling & Theming"
layout: default
parent: Tutorials
nav_order: 3
description: "Apply themes, switch at runtime, customize brushes, and build a branded theme."
---

# Tutorial: Styling & Theming

In this tutorial you will learn how to apply AvalonDock themes, switch between them at runtime, override specific brush resources for branding, and create a fully custom theme class. By the end you'll have an application with a theme selector and your own branded color scheme.

{: .tip }
This tutorial is inspired by the `TestApp` and `MLibTest` sample projects, which demonstrate all built-in themes and runtime switching.

---

## What You'll Build

An application that:
- Starts with a **dark theme** (Arc Dark)
- Has a **theme selector** ComboBox to switch themes at runtime
- Applies **custom brush overrides** for branded accent colors
- Includes a **custom theme class** that can be reused across projects

---

## Prerequisites

```bash
dotnet new wpf -n ThemeDockApp
cd ThemeDockApp
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Themes.Arc
dotnet add package Dirkster.AvalonDock.Themes.VS2013
dotnet add package Dirkster.AvalonDock.Themes.VS2010
dotnet add package Dirkster.AvalonDock.Themes.Expression
dotnet add package Dirkster.AvalonDock.Themes.Metro
dotnet add package Dirkster.AvalonDock.Themes.Aero
```

---

## Part 1: Applying a Built-in Theme

### In XAML (Design-Time)

The simplest way to apply a theme is directly in XAML:

```xml
<avalonDock:DockingManager>
    <avalonDock:DockingManager.Theme>
        <themes:ArcDarkTheme />
    </avalonDock:DockingManager.Theme>
    <!-- layout here -->
</avalonDock:DockingManager>
```

### In Code (Programmatic)

```csharp
using AvalonDock.Themes;

dockManager.Theme = new ArcDarkTheme();
```

### All Available Theme Classes

| Package | Class | Appearance |
|:--------|:------|:-----------|
| `Themes.Arc` | `ArcDarkTheme` | Modern dark with rounded corners |
| `Themes.Arc` | `ArcLightTheme` | Modern light with soft borders |
| `Themes.VS2013` | `Vs2013DarkTheme` | Visual Studio 2013 dark |
| `Themes.VS2013` | `Vs2013LightTheme` | Visual Studio 2013 light |
| `Themes.VS2013` | `Vs2013BlueTheme` | Visual Studio 2013 blue |
| `Themes.VS2010` | `VS2010Theme` | Visual Studio 2010 |
| `Themes.Expression` | `ExpressionDarkTheme` | Expression Blend dark |
| `Themes.Expression` | `ExpressionLightTheme` | Expression Blend light |
| `Themes.Metro` | `MetroTheme` | Flat Metro/WinUI |
| `Themes.Aero` | `AeroTheme` | Classic Windows Aero |
| *(built-in)* | `GenericTheme` | Default Windows look |

---

## Part 2: Runtime Theme Switching

Create an app with a ComboBox that switches themes on-the-fly.

### Step 1: Define Theme Options

**File: `ThemeOption.cs`**

```csharp
using AvalonDock.Themes;

namespace ThemeDockApp;

public class ThemeOption
{
    public string Name { get; init; }
    public Theme Theme { get; init; }

    public override string ToString() => Name;

    public static ThemeOption[] All => new[]
    {
        new ThemeOption { Name = "Arc Dark",        Theme = new ArcDarkTheme() },
        new ThemeOption { Name = "Arc Light",       Theme = new ArcLightTheme() },
        new ThemeOption { Name = "VS2013 Dark",     Theme = new Vs2013DarkTheme() },
        new ThemeOption { Name = "VS2013 Light",    Theme = new Vs2013LightTheme() },
        new ThemeOption { Name = "VS2013 Blue",     Theme = new Vs2013BlueTheme() },
        new ThemeOption { Name = "VS2010",          Theme = new VS2010Theme() },
        new ThemeOption { Name = "Metro",           Theme = new MetroTheme() },
        new ThemeOption { Name = "Aero",            Theme = new AeroTheme() },
        new ThemeOption { Name = "Generic",         Theme = new GenericTheme() },
    };
}
```

### Step 2: Build the XAML

**File: `MainWindow.xaml`**

```xml
<Window x:Class="ThemeDockApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:avalonDock="https://github.com/Dirkster99/AvalonDock"
        xmlns:themes="clr-namespace:AvalonDock.Themes;assembly=AvalonDock.Themes.Arc"
        Title="Theme Switcher — AvalonDock Tutorial" Height="550" Width="800">

    <DockPanel>
        <!-- Toolbar with theme selector -->
        <ToolBar DockPanel.Dock="Top">
            <TextBlock Text="Theme:" VerticalAlignment="Center" Margin="4,0" />
            <ComboBox x:Name="ThemeSelector" Width="160"
                      SelectionChanged="ThemeSelector_SelectionChanged" />
        </ToolBar>

        <!-- Docking Manager -->
        <avalonDock:DockingManager x:Name="DockManager">
            <avalonDock:DockingManager.Theme>
                <themes:ArcDarkTheme />
            </avalonDock:DockingManager.Theme>

            <avalonDock:LayoutRoot>
                <avalonDock:LayoutPanel Orientation="Horizontal">
                    <avalonDock:LayoutAnchorablePane DockWidth="200">
                        <avalonDock:LayoutAnchorable Title="Explorer" CanClose="False">
                            <TreeView>
                                <TreeViewItem Header="Project" IsExpanded="True">
                                    <TreeViewItem Header="MainWindow.xaml" />
                                    <TreeViewItem Header="App.xaml" />
                                    <TreeViewItem Header="Styles/" />
                                </TreeViewItem>
                            </TreeView>
                        </avalonDock:LayoutAnchorable>
                    </avalonDock:LayoutAnchorablePane>

                    <avalonDock:LayoutDocumentPane>
                        <avalonDock:LayoutDocument Title="Welcome.md">
                            <TextBox Text="# Welcome&#10;&#10;Try switching themes using the dropdown above."
                                     AcceptsReturn="True" FontFamily="Consolas"
                                     VerticalScrollBarVisibility="Auto" />
                        </avalonDock:LayoutDocument>
                    </avalonDock:LayoutDocumentPane>

                    <avalonDock:LayoutAnchorablePane DockWidth="200">
                        <avalonDock:LayoutAnchorable Title="Properties" CanClose="False">
                            <StackPanel Margin="8">
                                <TextBlock Text="Properties Panel" FontWeight="Bold" />
                                <TextBlock Text="Theme changes apply instantly to all docking controls."
                                           TextWrapping="Wrap" Margin="0,8,0,0" />
                            </StackPanel>
                        </avalonDock:LayoutAnchorable>
                    </avalonDock:LayoutAnchorablePane>
                </avalonDock:LayoutPanel>
            </avalonDock:LayoutRoot>
        </avalonDock:DockingManager>
    </DockPanel>
</Window>
```

### Step 3: Wire Up the Theme Switcher

**File: `MainWindow.xaml.cs`**

```csharp
using System.Windows;
using System.Windows.Controls;

namespace ThemeDockApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Populate the theme selector
        ThemeSelector.ItemsSource = ThemeOption.All;
        ThemeSelector.SelectedIndex = 0; // Arc Dark
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeSelector.SelectedItem is ThemeOption option)
        {
            DockManager.Theme = option.Theme;
        }
    }
}
```

Run the app and try switching themes — all docking controls update instantly.

---

## Part 3: Customizing Theme Brushes

AvalonDock themes are built on WPF resource dictionaries. You can override specific brushes to brand the docking chrome with your own colors without building a full custom theme.

### Step 1: Create a Brush Override Dictionary

**File: `Themes/BrandOverrides.xaml`** (add as a Resource in your project)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!--
        Override specific AvalonDock brushes.
        
        To discover brush keys, examine the built-in theme XAML files in the
        AvalonDock source code under source/Components/AvalonDock.Themes.*/
        
        Common brush categories:
        - Tab backgrounds and foregrounds
        - Pane header backgrounds
        - Active/selected tab highlights
        - Border brushes
        - Auto-hide tab brushes
        - Floating window backgrounds
        - Grip/drag indicator colors
    -->

</ResourceDictionary>
```

### Step 2: Merge Into App Resources

```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/BrandOverrides.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

{: .tip }
The easiest way to find the exact brush resource keys is to examine the source of the built-in theme you are basing your customization on. Look in `source/Components/AvalonDock.Themes.Arc/` (or whichever theme) for the resource dictionary XAML files.

---

## Part 4: Creating a Custom Theme Class

For maximum control and reusability, create a standalone theme class.

### Step 1: Create the Resource Dictionary

**File: `Themes/CorporateTheme.xaml`**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Start from an existing theme as a base -->
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/DarkBrushs.xaml" />
        <ResourceDictionary Source="/AvalonDock.Themes.VS2013;component/DarkTheme.xaml" />
    </ResourceDictionary.MergedDictionaries>

    <!-- Override brushes for corporate branding -->
    <!-- Add your overrides here after examining the base theme's keys -->

</ResourceDictionary>
```

### Step 2: Create the Theme Class

**File: `Themes/CorporateTheme.cs`**

```csharp
using System;
using AvalonDock.Themes;

namespace ThemeDockApp.Themes;

public class CorporateTheme : Theme
{
    public override Uri GetResourceUri()
    {
        return new Uri(
            "/ThemeDockApp;component/Themes/CorporateTheme.xaml",
            UriKind.Relative);
    }
}
```

### Step 3: Use the Custom Theme

In code:

```csharp
dockManager.Theme = new CorporateTheme();
```

In XAML:

```xml
<avalonDock:DockingManager.Theme>
    <local:CorporateTheme />
</avalonDock:DockingManager.Theme>
```

You can also add it to the theme selector alongside built-in themes:

```csharp
new ThemeOption { Name = "Corporate", Theme = new CorporateTheme() }
```

---

## Part 5: DictionaryTheme for Quick Overrides

If you don't need a reusable theme class, `DictionaryTheme` lets you wrap any resource dictionary as a theme:

```csharp
var overrides = new ResourceDictionary
{
    Source = new Uri("pack://application:,,,/ThemeDockApp;component/Themes/BrandOverrides.xaml")
};

dockManager.Theme = new DictionaryTheme(overrides);
```

This is useful for quick experiments or for loading theme resources from external files at runtime.

---

## Part 6: Application-Wide Theming

AvalonDock themes only style the docking controls (tabs, headers, grips, borders). Standard WPF controls like `Button`, `TextBox`, and `ComboBox` inside your panels are **not** affected by AvalonDock themes.

For a consistent look, combine AvalonDock theming with a full WPF theming library:

### Option A: MahApps.Metro

```bash
dotnet add package MahApps.Metro
```

```xml
<mah:MetroWindow x:Class="MyApp.MainWindow"
                 xmlns:mah="http://metro.mahapps.com/winfx/xaml/controls"
                 ...>
    <avalonDock:DockingManager>
        <avalonDock:DockingManager.Theme>
            <themes:Vs2013DarkTheme />
        </avalonDock:DockingManager.Theme>
        ...
    </avalonDock:DockingManager>
</mah:MetroWindow>
```

### Option B: MLib

[MLib](https://github.com/Dirkster99/MLib) is a lightweight theming companion designed specifically for AvalonDock. The `MLibTest` sample project in the repository demonstrates this integration.

### Matching Colors

When using both an AvalonDock theme and an application theme, ensure the background and accent colors are compatible. For example, pair `Vs2013DarkTheme` with a dark application theme, and `ArcLightTheme` with a light application theme.

---

## How It Works

### Theme Architecture

```
DockingManager
│
├── Theme property (Theme object)
│   └── GetResourceUri() → returns URI to resource dictionary
│
└── On theme change:
    1. Removes previous theme's ResourceDictionary
    2. Loads new theme's ResourceDictionary
    3. All templated controls re-evaluate their styles
    4. UI updates immediately — no restart needed
```

### What Themes Control

| Element | Themed? |
|:--------|:--------|
| Document tab headers | ✅ |
| Anchorable tab headers | ✅ |
| Pane title bars | ✅ |
| Auto-hide tabs | ✅ |
| Floating window chrome | ✅ |
| Docking indicators (overlay) | ✅ |
| Drag grip indicators | ✅ |
| Panel borders and separators | ✅ |
| Your content inside panels | ❌ (use app theming) |
| Standard WPF controls | ❌ (use app theming) |

---

## Troubleshooting

### Theme Not Applying

- Ensure the theme NuGet package is installed and referenced
- Verify the `xmlns` namespace is correct for the theme assembly
- Check that the `Theme` property is being set (not just the resource dictionary)

### Custom Theme Resources Not Loading

- Verify the resource dictionary's **Build Action** is set to `Page`
- Check the `pack://` URI matches your assembly name exactly
- Ensure merged dictionaries reference valid paths

### Colors Look Wrong with Application Theme

- Match dark AvalonDock themes with dark application themes
- Match light AvalonDock themes with light application themes
- Override conflicting brushes in a `BrandOverrides.xaml` dictionary

---

## Next Steps

- See the [Custom Themes Guide]({% link guides/custom-themes.md %}) for a reference of all customization points
- See the [Theming Guide]({% link guides/theming.md %}) for a complete list of themes and their variants
- Explore the `TestApp` sample for all 11 theme variants with runtime switching
- Explore the `MLibTest` sample for MLib + AvalonDock unified theming
