---
title: Migration Guide
layout: default
nav_order: 7
has_children: true
description: "Migrate from AvalonDock v4.x to v5.0.0."
permalink: /migration/
---

# Migration Guide
{: .label .label-yellow }
v4 → v5

This guide covers all the changes needed to upgrade your application from AvalonDock v4.x to v5.0.0.

---

## Overview

AvalonDock v5.0.0 is a major release that introduces a modular architecture, new packages, and a refactored layout engine. While many APIs remain compatible, there are breaking changes that require attention during migration.

{: .important }
Read through this entire guide before starting your migration. Some changes interact with each other, and understanding the full scope will help you plan the upgrade.

---

## Quick Checklist

- [ ] Update NuGet packages to v5.0.0
- [ ] Install new required packages (serializers are now separate)
- [ ] Update namespace imports if needed
- [ ] Replace deprecated serialization APIs
- [ ] Update theme class references
- [ ] Review `ILayoutEngine` changes if using custom layout logic
- [ ] Test layout serialization roundtrip
- [ ] Test all docking operations (dock, float, auto-hide, close)

---

## Step 1: Update NuGet Packages

### Before (v4.x)

```xml
<PackageReference Include="Dirkster.AvalonDock" Version="4.x.x" />
<PackageReference Include="Dirkster.AvalonDock.Themes.VS2013" Version="4.x.x" />
```

### After (v5.0.0)

```xml
<PackageReference Include="Dirkster.AvalonDock" Version="5.0.0" />
<PackageReference Include="Dirkster.AvalonDock.Themes.VS2013" Version="5.0.0" />

<!-- NEW: Serializers are now separate packages -->
<PackageReference Include="Dirkster.AvalonDock.Serializer.Xml" Version="5.0.0" />

<!-- OPTIONAL: New packages available -->
<PackageReference Include="Dirkster.AvalonDock.Mvvm" Version="5.0.0" />
<PackageReference Include="Dirkster.AvalonDock.DependencyInjection" Version="5.0.0" />
```

---

## Step 2: Package Architecture Changes

v5.0.0 introduces a modular package architecture. The monolithic `AvalonDock` package has been split:

| v4.x | v5.0.0 | Action Required |
|:-----|:-------|:----------------|
| `AvalonDock` (included serializers) | `AvalonDock` (core only) | Install serializer packages separately |
| — | `AvalonDock.Core` (new) | Automatically referenced by `AvalonDock` |
| — | `AvalonDock.Serializer.Xml` (new) | Install if using XML serialization |
| — | `AvalonDock.Serializer.Json` (new) | Install if using JSON serialization |
| — | `AvalonDock.Mvvm` (new) | Optional: MVVM base classes |
| — | `AvalonDock.DependencyInjection` (new) | Optional: DI integration |

---

## Step 3: Serialization Changes

### XML Serialization

The `XmlLayoutSerializer` has moved to a separate package.

**Before (v4.x):**
```csharp
using AvalonDock.Layout.Serialization;

var serializer = new XmlLayoutSerializer(dockManager);
```

**After (v5.0.0):**
```csharp
using AvalonDock.Serializer.Xml;

var serializer = new XmlLayoutSerializer(dockManager);
```

{: .note }
The API remains the same — only the namespace and package have changed.

### JSON Serialization (New)

v5.0.0 adds a new JSON serializer as an alternative:

```csharp
using AvalonDock.Serializer.Json;

var serializer = new JsonLayoutSerializer(dockManager);
```

---

## Step 4: Layout Engine Changes

v5.0.0 introduces the `ILayoutEngine` interface, replacing the previously internal layout calculation logic.

### If You Used Default Behavior

No changes needed. The `DefaultLayoutEngine` provides the same behavior as v4.x.

### If You Had Custom Layout Logic

If you customized layout calculations through internal or reflection-based access, you should now implement `ILayoutEngine`:

```csharp
public class MyLayoutEngine : ILayoutEngine
{
    // Implement the required layout calculation methods
}
```

---

## Step 5: Theme Updates

### New Arc Theme

v5.0.0 introduces the new Arc theme. No migration action needed, but consider switching:

```csharp
// New modern theme
dockManager.Theme = new ArcDarkTheme();
```

### Existing Themes

All existing themes (VS2013, VS2010, Expression, Metro, Aero) remain available and compatible. No changes needed.

---

## Step 6: Target Framework Updates

v5.0.0 updates the supported frameworks:

| v4.x | v5.0.0 |
|:-----|:-------|
| .NET Framework 4.0 | ❌ Dropped |
| .NET Framework 4.5.2 | ❌ Dropped |
| .NET Framework 4.8 | ✅ Supported |
| .NET Core 3.0 | ❌ Dropped |
| .NET 5 | ❌ Dropped |
| .NET 9 | ✅ Supported |
| .NET 10 | ✅ Supported (new) |

{: .warning }
If your project targets .NET Framework versions below 4.8, or .NET Core 3.x / .NET 5, you must update your target framework before upgrading to AvalonDock v5.0.0.

---

## Step 7: Verify

After making all changes:

1. **Build** your solution and fix any compilation errors
2. **Test serialization**: Load a v4.x layout file and verify it deserializes correctly
3. **Test docking operations**: Dock, float, auto-hide, close, and restore panels
4. **Test theme switching**: Verify all themes render correctly
5. **Test MVVM bindings**: If using `DocumentsSource`/`AnchorablesSource`, verify view model binding

---

## Breaking Changes Summary

See the detailed [Breaking Changes]({% link migration/breaking-changes.md %}) page for a complete list.

---

## Getting Help

If you encounter issues during migration:

- Check the [GitHub Issues](https://github.com/Dirkster99/AvalonDock/issues) for known problems
- Review the [GitHub Discussions](https://github.com/Dirkster99/AvalonDock/discussions) for community help
- File a new issue with the `migration` label if you find an undocumented breaking change
