---
title: VS Theme
layout: default
parent: Themes
nav_order: 2
description: "The Visual Studio theme for AvalonDock."
---

# VS Theme (.vstheme)

Visual Studio styling based on `.vstheme` files. Includes built-in VS2015 and VS2022 variants and supports loading custom `.vstheme` or `.vstheme.gz` (gzip-compressed) files.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.VS
```

## Built-in Variants

### VS2022

```csharp
dockManager.Theme = new VS2022DarkTheme();
dockManager.Theme = new VS2022LightTheme();
dockManager.Theme = new VS2022BlueTheme();
```

### VS2015

```csharp
dockManager.Theme = new VS2015DarkTheme();
dockManager.Theme = new VS2015LightTheme();
dockManager.Theme = new VS2015BlueTheme();
```

## Loading Custom .vstheme Files

### From a File Path

```csharp
dockManager.Theme = new VsTheme("path/to/custom.vstheme");
```

### From a Stream

```csharp
using var stream = File.OpenRead("custom.vstheme");
dockManager.Theme = new VsTheme(stream);
```

The `VsTheme` constructor auto-detects gzip compression (via magic bytes `0x1F 0x8B`), so both plain `.vstheme` and `.vstheme.gz` files work with the stream constructor.

### From Gzip-Compressed Bytes

For embedded resources stored as `.vstheme.gz`, load the raw bytes directly:

```csharp
byte[] gzipBytes = VsThemeResourceLoader.Load(
    Assembly.GetExecutingAssembly(),
    "MyApp.Resources.mytheme.vstheme.gz");

dockManager.Theme = new VsTheme(gzipBytes);
```

### From a Pre-Parsed Palette

```csharp
var palette = VsThemeParser.ParseFile("custom.vstheme");
dockManager.Theme = new VsTheme(palette);
```

## XAML Markup Extension

```xml
<DockingManager Theme="{vs:VsTheme Path=vs2015blue.vstheme}" />
```

The `Path` can be absolute or relative to the application directory.

## Parsing API

`VsThemeParser` extracts a `VsThemeColorPalette` from `.vstheme` files (the `Environment` category):

| Method | Description |
|:-------|:------------|
| `VsThemeParser.Parse(Stream)` | Parse from a stream (auto-detects gzip) |
| `VsThemeParser.ParseFile(string)` | Parse from a file path |
| `VsThemeParser.ParseGZip(byte[])` | Parse from gzip-compressed bytes |

The returned `VsThemeColorPalette` provides lookup methods:

```csharp
var palette = VsThemeParser.ParseFile("mytheme.vstheme");

Color? bg = palette.GetBackground("ToolWindowBackground");
Color fg = palette.GetForegroundOrDefault("ToolWindowText", Colors.White);
bool has = palette.Contains("AccentMedium");
```

---
