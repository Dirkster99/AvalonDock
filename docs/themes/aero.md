---
title: Aero Theme
layout: default
parent: Themes
nav_order: 6
description: "The Windows Aero theme for AvalonDock."
---

# Aero Theme

Classic Windows Aero glass-inspired design. Best suited for applications that need a traditional Windows look and feel.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.Aero
```

## Usage

```csharp
using AvalonDock.Themes;

dockManager.Theme = new AeroTheme();
```

```xml
<avalonDock:DockingManager.Theme>
    <themes:AeroTheme />
</avalonDock:DockingManager.Theme>
```
