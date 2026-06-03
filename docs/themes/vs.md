---
title: VS Theme
layout: default
parent: Themes
nav_order: 2
description: "The Visual Studio theme for AvalonDock."
---

# VS Theme (.vstheme)

Classic Visual Studio styling with three color variants based on .vstheme files. A well-tested, polished theme suitable for professional IDE-like applications.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.VS
```

## API

### C#

```csharp
dockManager.Theme = new VsTheme("vs2015dark.vstheme");
```

Dark color scheme matching Visual Studio 2013's dark mode.

### WPF/XAML

```xaml
 <DockingManager Theme="{vs:VsTheme Path=vs2015blue.vstheme}">
```
---
