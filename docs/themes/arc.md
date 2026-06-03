---
title: Arc Theme
layout: default
parent: Themes
nav_order: 1
description: "The modern Arc theme for AvalonDock."
---

# Arc Theme
{: .label .label-green }
New in v5.0.0

A modern theme with compact tabs, rounded corners, and semi-transparent design elements. Inspired by contemporary IDEs with minimal spacing and clean aesthetics.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.Arc
```

## Variants

### Arc Dark

```csharp
using AvalonDock.Themes;

dockManager.Theme = new ArcDarkTheme();
```

```xml
<avalonDock:DockingManager.Theme>
    <themes:ArcDarkTheme />
</avalonDock:DockingManager.Theme>
```

A dark theme with subtle contrasts, perfect for long coding sessions and modern dark-mode applications.

### Arc Light

```csharp
dockManager.Theme = new ArcLightTheme();
```

```xml
<avalonDock:DockingManager.Theme>
    <themes:ArcLightTheme />
</avalonDock:DockingManager.Theme>
```

A clean light theme with soft borders and comfortable contrast.

---

## Theme Brushes

You can use the Arc theme brushes independently in your resource dictionaries:

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Dark mode brushes -->
    <ResourceDictionary Source="/AvalonDock.Themes.Arc;component/DarkBrushs.xaml" />

    <!-- Or light mode brushes -->
    <ResourceDictionary Source="/AvalonDock.Themes.Arc;component/LightBrushs.xaml" />
</ResourceDictionary.MergedDictionaries>
```

---

## Screenshots

| Dark | Light |
|:-----|:------|
| ![Arc Dark - Dock Document](https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Dark/DockDocument.png) | ![Arc Light - Dock Document](https://raw.githubusercontent.com/Dirkster99/Docu/master/AvalonDock/VS2013/AD_MLib/Light/DockDocument.png) |
