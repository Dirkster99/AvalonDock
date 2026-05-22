---
title: "Tutorial: Layout Persistence"
layout: default
parent: Tutorials
nav_order: 4
description: "Save and restore docking layouts with XML or JSON, including MVVM integration."
---

# Tutorial: Layout Persistence

In this tutorial you will add layout persistence to an AvalonDock application so that the user's panel arrangement is saved on exit and restored on the next launch. You'll implement both the basic code-behind approach and the MVVM-friendly approach.

{: .tip }
This tutorial is inspired by the `MVVMTestApp` and `TestApp` sample projects, which both demonstrate layout serialization with the `XmlLayoutSerializer`.

---

## What You'll Build

An application that:
- **Saves** the complete docking layout (panel positions, sizes, floating windows) when the window closes
- **Restores** the layout when the application starts
- Properly **reconnects view models** to deserialized layout items (MVVM)
- Provides a **Reset Layout** command to return to the default arrangement
- Supports both **XML and JSON** serialization

---

## Prerequisites

```bash
dotnet add package Dirkster.AvalonDock
dotnet add package Dirkster.AvalonDock.Serializer.Xml
# Or for JSON:
# dotnet add package Dirkster.AvalonDock.Serializer.Json
```

---

## Part 1: Basic Layout Persistence (Code-Behind)

This approach works without MVVM and is the simplest way to get started.

### Step 1: Define a Layout File Path

```csharp
private static string LayoutFilePath => Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "MyApp", "layout.xml");
```

### Step 2: Save on Window Close

```csharp
using AvalonDock.Serializer.Xml;

protected override void OnClosing(CancelEventArgs e)
{
    // Ensure the directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(LayoutFilePath));

    var serializer = new XmlLayoutSerializer(DockManager);
    using (var writer = new StreamWriter(LayoutFilePath))
    {
        serializer.Serialize(writer);
    }

    base.OnClosing(e);
}
```

### Step 3: Restore on Window Load

```csharp
private void DockManager_Loaded(object sender, RoutedEventArgs e)
{
    if (!File.Exists(LayoutFilePath))
        return;

    var serializer = new XmlLayoutSerializer(DockManager);

    // The callback reconnects content to deserialized layout items
    serializer.LayoutSerializationCallback += (s, args) =>
    {
        args.Content = args.Model.ContentId switch
        {
            "explorer"   => FindOrCreateControl("explorer"),
            "properties" => FindOrCreateControl("properties"),
            "output"     => FindOrCreateControl("output"),
            _            => null  // Unknown items are skipped
        };
    };

    using (var reader = new StreamReader(LayoutFilePath))
    {
        serializer.Deserialize(reader);
    }
}
```

{: .important }
The `LayoutSerializationCallback` is called for **every** content item in the saved layout. You must provide the actual UI content (or view model) for each item, matched by `ContentId`. If you set `args.Content = null` or `args.Cancel = true`, that item is removed from the restored layout.

### Step 4: Wire the Loaded Event

```xml
<avalonDock:DockingManager x:Name="DockManager" Loaded="DockManager_Loaded">
```

---

## Part 2: MVVM Layout Persistence

When using MVVM with `DocumentsSource` and `AnchorablesSource`, the serialization callback must return **view models** instead of UI controls.

### Step 1: Add Save/Restore to Your View Model

**File: `ViewModels/WorkspaceViewModel.cs`** (additions)

```csharp
using System.Windows.Input;

public class WorkspaceViewModel
{
    // ... existing code ...

    public ICommand SaveLayoutCommand => new RelayCommand(_ => SaveLayout());
    public ICommand RestoreLayoutCommand => new RelayCommand(_ => RestoreLayout());
    public ICommand ResetLayoutCommand => new RelayCommand(_ => ResetLayout());

    /// <summary>
    /// Called by the view to provide the DockingManager reference for serialization.
    /// </summary>
    public Action<string> OnSaveLayout { get; set; }
    public Action<string> OnRestoreLayout { get; set; }
    public Action OnResetLayout { get; set; }

    private void SaveLayout() => OnSaveLayout?.Invoke(LayoutFilePath);
    private void RestoreLayout() => OnRestoreLayout?.Invoke(LayoutFilePath);
    private void ResetLayout() => OnResetLayout?.Invoke();

    private static string LayoutFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyApp", "layout.xml");
}
```

### Step 2: Implement the Serialization in Code-Behind

Even with MVVM, layout serialization requires a reference to the `DockingManager` control, so the actual serialize/deserialize calls live in the view's code-behind.

**File: `MainWindow.xaml.cs`**

```csharp
using AvalonDock.Serializer.Xml;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnWindowClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is WorkspaceViewModel vm)
        {
            // Wire up layout callbacks
            vm.OnSaveLayout = SaveLayout;
            vm.OnRestoreLayout = path => RestoreLayout(path, vm);
            vm.OnResetLayout = () =>
            {
                // Remove and re-add the DockingManager to reset
                // Or simply delete the layout file and restart
            };

            // Auto-restore on startup
            RestoreLayout(WorkspaceViewModel.LayoutFilePath, vm);
        }
    }

    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        SaveLayout(WorkspaceViewModel.LayoutFilePath);
    }

    private void SaveLayout(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        var serializer = new XmlLayoutSerializer(DockManager);
        using var writer = new StreamWriter(path);
        serializer.Serialize(writer);
    }

    private void RestoreLayout(string path, WorkspaceViewModel vm)
    {
        if (!File.Exists(path)) return;

        var serializer = new XmlLayoutSerializer(DockManager);

        serializer.LayoutSerializationCallback += (sender, args) =>
        {
            // Match by ContentId and return the VIEW MODEL (not a control)
            var contentId = args.Model.ContentId;

            // Try to find among existing tool view models
            var tool = vm.Tools.FirstOrDefault(t => t.ContentId == contentId);
            if (tool != null)
            {
                args.Content = tool;
                return;
            }

            // Try to find among existing documents
            var doc = vm.Documents.FirstOrDefault(d => d.ContentId == contentId);
            if (doc != null)
            {
                args.Content = doc;
                return;
            }

            // Unknown content — skip it
            args.Cancel = true;
        };

        using var reader = new StreamReader(path);
        serializer.Deserialize(reader);
    }
}
```

---

## Part 3: JSON Serialization

The JSON serializer works identically but produces smaller, more portable files.

```bash
dotnet add package Dirkster.AvalonDock.Serializer.Json
```

```csharp
using AvalonDock.Serializer.Json;

// Save
var serializer = new JsonLayoutSerializer(DockManager);
using (var writer = new StreamWriter("layout.json"))
{
    serializer.Serialize(writer);
}

// Restore
var serializer = new JsonLayoutSerializer(DockManager);
serializer.LayoutSerializationCallback += (s, args) => { /* same as XML */ };
using (var reader = new StreamReader("layout.json"))
{
    serializer.Deserialize(reader);
}
```

### XML vs JSON

| Aspect | XML | JSON |
|:-------|:----|:-----|
| **File size** | Larger | Smaller |
| **Human-readable** | Yes (verbose) | Yes (compact) |
| **Legacy support** | Works with older configs | Modern apps only |
| **Performance** | Slightly slower | Slightly faster |
| **Package** | `Serializer.Xml` | `Serializer.Json` |

---

## Part 4: What Gets Serialized (and What Doesn't)

Understanding what is and isn't persisted is critical for a correct implementation.

### ✅ Serialized (Layout State)

| Data | Example |
|:-----|:--------|
| Panel positions | Explorer is docked left, Properties is docked right |
| Panel sizes | Explorer has DockWidth="250" |
| Tab order | Document1 is before Document2 |
| Floating window positions | Properties window is at (400, 200) with size 300×400 |
| Auto-hide state | Output is auto-hidden on the bottom side |
| Active/selected state | Document2 is the active tab |
| ContentId | Each item's unique identifier |
| Title | Each item's display title |
| Panel orientation | Horizontal or vertical splits |

### ❌ Not Serialized (Application State)

| Data | How to Persist |
|:-----|:---------------|
| Document text content | Save separately (e.g., to files or a database) |
| View model property values | Serialize your view models independently |
| Event handlers | Re-attach during deserialization callback |
| Runtime UI state | Rebuild in the callback |
| Undo/redo history | Not applicable to layout |

---

## Part 5: Reset to Default Layout

Users often want a "Reset Layout" option. Here are two approaches:

### Approach A: Embed a Default Layout

Save your default layout as an embedded resource and restore from it:

```csharp
public void ResetLayout()
{
    var assembly = Assembly.GetExecutingAssembly();
    using var stream = assembly.GetManifestResourceStream("MyApp.DefaultLayout.xml");
    if (stream == null) return;

    var serializer = new XmlLayoutSerializer(DockManager);
    serializer.LayoutSerializationCallback += OnLayoutCallback;
    serializer.Deserialize(stream);
}
```

### Approach B: Delete and Restart

The simpler approach — delete the saved layout file so the XAML-defined layout is used on next launch:

```csharp
public void ResetLayout()
{
    if (File.Exists(LayoutFilePath))
        File.Delete(LayoutFilePath);

    MessageBox.Show("Layout will be reset on next restart.");
}
```

---

## Part 6: Error Handling

Layout files can become corrupted or incompatible after application updates. Always wrap deserialization in a try-catch:

```csharp
private void RestoreLayoutSafe(string path)
{
    if (!File.Exists(path)) return;

    try
    {
        var serializer = new XmlLayoutSerializer(DockManager);
        serializer.LayoutSerializationCallback += OnLayoutCallback;

        using var reader = new StreamReader(path);
        serializer.Deserialize(reader);
    }
    catch (Exception ex)
    {
        // Log the error
        System.Diagnostics.Debug.WriteLine($"Failed to restore layout: {ex.Message}");

        // Delete the corrupt file so the default layout is used
        try { File.Delete(path); } catch { }
    }
}
```

{: .warning }
Always handle deserialization errors gracefully. A corrupt layout file should never prevent the application from starting. Delete the file and fall back to the XAML-defined default layout.

---

## How It Works

### Serialization Flow

```
Save:
  DockingManager → XmlLayoutSerializer.Serialize()
  → Walks the LayoutRoot tree
  → Writes each element's position, size, state, ContentId
  → Produces layout.xml

Restore:
  layout.xml → XmlLayoutSerializer.Deserialize()
  → Parses layout tree
  → For each content item: calls LayoutSerializationCallback
  → You provide the content (VM or control) matched by ContentId
  → DockingManager rebuilds the visual tree
```

### The ContentId Contract

The `ContentId` is the key that links serialized layout items to your application content. Follow these rules:

1. **Every dockable item must have a unique `ContentId`** — duplicates cause unpredictable behavior
2. **`ContentId` must be stable** — don't use GUIDs that change every session (for tools). Documents may use GUIDs if you track them separately.
3. **Tools should use descriptive IDs** — e.g., `"explorer"`, `"properties"`, `"output"`
4. **Documents can use file paths** — e.g., `"/path/to/file.txt"` or a database key

---

## Next Steps

- See the [Layout Serialization Guide]({% link guides/serialization.md %}) for a complete API reference
- Combine with [MVVM]({% link tutorials/mvvm-ide.md %}) for a full IDE experience
- Explore the `MVVMTestApp` sample for a working implementation with save/restore
- Explore the `TestApp` sample for layout reload/unload patterns
