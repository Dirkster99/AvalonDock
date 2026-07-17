---
title: Expression Theme
layout: default
parent: Themes
nav_order: 4
description: "The Expression Blend theme for AvalonDock."
---

# Expression Theme

Inspired by Microsoft Expression Blend with a sleek, designer-focused aesthetic.

---

## Installation

```bash
dotnet add package Dirkster.AvalonDock.Themes.Expression
```

## Usage

### Expression Dark

```csharp
using AvalonDock.Themes;

dockManager.Theme = new ExpressionDarkTheme();
```

### Expression Light

```csharp
dockManager.Theme = new ExpressionLightTheme();
```
