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

### VS2026

Visual Studio 2026 replaced the XML `.vstheme` format with a JSON-based override
format (see [Make Visual Studio look the way you want](https://devblogs.microsoft.com/visualstudio/make-visual-studio-look-the-way-you-want/)).
The built-in VS2026 variants apply the Fluent accent and the new tokens that color
the window/tool headers and tab strips independently from the rest of the shell:

```csharp
dockManager.Theme = new VS2026DarkTheme();
dockManager.Theme = new VS2026LightTheme();
dockManager.Theme = new VS2026BlueTheme();
```

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

## Loading Custom VS2026 JSON Files

VS2026 stores theme customizations as JSON. The files are *override only* — they
list only the color tokens that were changed — and are shared by dropping them into
`%LOCALAPPDATA%\Microsoft\VisualStudio\18.0_xxxxxxxx\ColorThemes`, where the file
name matches the theme being overridden (e.g. `dark.json` overrides Dark).

A VS2026 JSON file is a flat array of `{ "Name", "Category", "Background" }` entries
with 8-digit `AARRGGBB` color values:

```json
[
  { "Name": "EnvironmentHeader", "Category": "5af241b7-...", "Background": "FFF5CC84" },
  { "Name": "EnvironmentTab",    "Category": "5af241b7-...", "Background": "FFF5CC84" },
  { "Name": "EnvironmentBackground", "Category": "5af241b7-...", "Background": "FFCCD5F0" }
]
```

Use `VsTheme.FromJson` to load one. Because the file is override-only, pass a base
palette to layer the overrides on top of a full theme:

```csharp
// Use a built-in VS theme palette as the base, then apply the JSON overrides.
using var baseStream = File.OpenRead("vs2022dark.vstheme");
var basePalette = VsThemeParser.Parse(baseStream);

dockManager.Theme = VsTheme.FromJson("cool-breeze.json", basePalette);
```

If no base palette is supplied, only the tokens present in the JSON are used and all
other colors fall back to the resource builder's defaults. The JSON can also be loaded
from a stream via `VsTheme.FromJson(Stream, ...)`.

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

`VsJsonThemeParser` extracts the same `VsThemeColorPalette` from VS2026 JSON files:

| Method | Description |
|:-------|:------------|
| `VsJsonThemeParser.Parse(string)` | Parse from a JSON string |
| `VsJsonThemeParser.Parse(Stream)` | Parse from a stream |
| `VsJsonThemeParser.ParseFile(string)` | Parse from a file path |

Because VS2026 JSON files are override-only, combine them with a base palette using
`basePalette.Merge(overridePalette)` before building a theme.

The returned `VsThemeColorPalette` provides lookup methods:

```csharp
var palette = VsThemeParser.ParseFile("mytheme.vstheme");

Color? bg = palette.GetBackground("ToolWindowBackground");
Color fg = palette.GetForegroundOrDefault("ToolWindowText", Colors.White);
bool has = palette.Contains("AccentMedium");
```

---
