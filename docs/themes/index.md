---
title: Themes
layout: default
nav_order: 5
has_children: true
description: "Browse the built-in AvalonDock themes."
permalink: /themes/
---

# Themes

AvalonDock ships with six polished theme packages. Each theme is a separate NuGet package, so you only include what you need.

---

## Theme Gallery

| Theme                                         | Variants | Style                            |
|:----------------------------------------------|:---------|:---------------------------------|
| [Arc]({{ site.baseurl }}{% link themes/arc.md %})               | Dark, Light | Modern, compact, rounded corners |
| [VS]({{ site.baseurl }}{% link themes/vs.md %})                 | Dark, Light, Blue | .vstheme based Visual Studio |
| [VS2013]({{ site.baseurl }}{% link themes/vs2013.md %})         | Dark, Light, Blue | Classic Visual Studio 2013       |
| [VS2010]({{ site.baseurl }}{% link themes/vs2010.md %})         | Single | Visual Studio 2010               |
| [Expression]({{ site.baseurl }}{% link themes/expression.md %}) | Dark, Light | Expression Blend inspired        |
| [Metro]({{ site.baseurl }}{% link themes/metro.md %})           | Single | Flat Metro / WinUI               |
| [Aero]({{ site.baseurl }}{% link themes/aero.md %})             | Single | Classic Windows Aero             |

---

## Applying a Theme

All themes follow the same pattern:

```bash
# Install the theme package
dotnet add package Dirkster.AvalonDock.Themes.Arc
```

```csharp
// Apply in code
dockManager.Theme = new ArcDarkTheme();
```

```xml
<!-- Or in XAML -->
<avalonDock:DockingManager.Theme>
    <themes:ArcDarkTheme />
</avalonDock:DockingManager.Theme>
```

Themes can be switched at runtime — see the [Theming Guide]({{ site.baseurl }}{% link guides/theming.md %}) for details.
