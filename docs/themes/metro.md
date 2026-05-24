---
title: Metro Theme
layout: default
parent: Themes
nav_order: 5
description: "The Metro theme for AvalonDock."
---

# Metro Theme

A modern, flat design inspired by the Metro / WinUI design language with clean lines and minimal chrome.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.Metro
```

## Usage

```csharp
using AvalonDock.Themes;

dockManager.Theme = new MetroTheme();
```

```xml
<avalonDock:DockingManager.Theme>
    <themes:MetroTheme />
</avalonDock:DockingManager.Theme>
```
