---
title: Floating Windows
layout: default
parent: Concepts
nav_order: 4
description: "How floating windows work in AvalonDock."
---

# Floating Windows

Floating windows allow users to tear off documents or tool windows into independent, freely movable windows. This is one of AvalonDock's most powerful features, enabling flexible multi-monitor workflows.

---

## How Floating Works

Any dockable content — documents or anchorables — can be floated by:

1. **Dragging** a tab away from its pane
2. **Double-clicking** a tab header (depending on configuration)
3. **Programmatically** calling `Float()` on a layout content

When content is floated, AvalonDock creates a floating window that:
- Behaves as an independent window
- Can be moved freely, including to other monitors
- Can be docked back by dragging it over a docking target
- Maintains its content and state

---

## Floating Window Types

| Type | Contains | Created When |
|:-----|:---------|:-------------|
| `LayoutAnchorableFloatingWindow` | Anchorable panes | An anchorable is floated |
| `LayoutDocumentFloatingWindow` | Document panes | A document is floated |

---

## Controlling Float Behavior

### Prevent Floating

Set `CanFloat` to `false` on any content to prevent it from being floated:

```xml
<avalonDock:LayoutAnchorable Title="Fixed Panel"
                              CanFloat="False">
    <!-- This panel cannot be torn off -->
</avalonDock:LayoutAnchorable>
```

### Turn Floating Off Entirely

{: .new }
> New in v5.0.0

`CanFloat` is a decision per piece of content. `DockingManager.AllowFloatingWindows` is the switch for
the whole feature — an application that wants a fixed layout, or one that manages its own windows,
turns it off once:

```xml
<avalonDock:DockingManager AllowFloatingWindows="False" />
```

While it is `False`:

- dragging a tab or a tool window title out of its pane does nothing,
- the **Float** menu entry and `LayoutItem.FloatCommand` report themselves as unavailable, so the
  entry is greyed out rather than silently doing nothing,
- `LayoutContent.Float()` and `DockingManager.CreateFloatingWindow(...)` create no window — the latter
  returns `null`,
- a layout that is loaded with floating windows in it has that content docked back, so restoring a
  layout saved while floating was allowed is not a way around the setting.

Setting it to `False` while floating windows are open docks their content back where it came from.
Turning it on again does not reopen them.

`DockAllFloatingWindows()` performs that dock-back on its own, which is useful for a "reset windows"
menu entry even when floating stays allowed.

### Turn Standalone Windows Off

`DockingManager.AllowDetachedWindows` does the same for the standalone "Window" view mode described
[below](#standalone-windows-window-view-mode):

```xml
<avalonDock:DockingManager AllowDetachedWindows="False" />
```

While it is `False`, `DetachAnchorableToWindow` does nothing, `DetachToWindowCommand` is unavailable
so the **View Mode → Window** entry is disabled, and a layout that was saved with a detached tool
window is restored with that window docked. Setting it to `False` returns anchorables that are
already in standalone windows.

The two switches are independent: floating windows can be allowed while standalone windows are not,
and the other way round.

### MVVM and Dependency Injection

Both switches are also on the MVVM layout, so an application that builds its layout from view models
never has to reach into the view:

```csharp
dockService.Layout.AllowFloatingWindows = false;
dockService.Layout.AllowDetachedWindows = false;
```

`IRootDock` carries them and the `DockingManager` follows the layout it is bound to through
`DockLayout`, including later changes.

{: .warning }
> The layout wins. Binding `DockLayout` applies the layout's values to the manager, so a
> `DockingManager` that sets `AllowFloatingWindows="False"` in XAML *and* binds a `DockLayout` whose
> root dock leaves it at the default has floating switched back on. Set the switches in one place —
> either on the manager or on the layout, not both.

With `AvalonDock.DependencyInjection` they are part of the registration, and are applied to the layout
for you:

```csharp
services.AddDockLayoutService(dock =>
{
    dock.AddToolbox<ExplorerViewModel>();
    dock.ConfigureDocking(o =>
    {
        o.AllowFloatingWindows = false;
        o.AllowDetachedWindows = false;
    });
});
```

`ToggleDockOptions` derives from `DockingOptions`, so an application configuring the toggle docking
manager sets them on that same options object via `ConfigureToggleDock`.

### Float Programmatically

```csharp
// Float an anchorable
var anchorable = layout.Descendents()
    .OfType<LayoutAnchorable>()
    .First(a => a.ContentId == "properties");

anchorable.Float();
```

---

## Docking Targets

When a user drags a floating window over the main docking area, **docking indicators** appear showing valid drop targets:

- **Center** — Tab into the target pane
- **Top / Bottom / Left / Right** — Split and dock to that side
- **Edge indicators** — Dock to the edge of the entire docking area

The docking indicators are [defined in XAML](https://github.com/Dirkster99/AvalonDock/wiki/OverlayWindow), ensuring crisp rendering on all resolutions including 4K and 8K displays.

### Overlay Window Lifetime

The indicators live in an `OverlayWindow`: a borderless, transparent window that every drop target
host — the `DockingManager` itself and each floating window — puts over its own area while a drag is
in progress.

Each host keeps one overlay window and shows and hides it again for every drag, rather than creating
a new one per drag. A drag ends through a window message of the window being dragged, and that
message does not always arrive — moving a window between monitors with different DPI scaling can end
the modal move loop of Windows without one. Recreating the overlay window per drag turned every such
drag into an empty window that stayed on screen for the rest of the session, so the count grew with
every float. Reusing one window per host, and clearing the overlay windows of all hosts whenever a
drag starts or ends, removes that possibility: an overlay window is only ever destroyed together with
the host it belongs to.

---

## Events

| Event | Description |
|:------|:------------|
| `ContentFloating` | Raised on `DockingManager` when content starts floating. |
| `ContentDocked` | Raised on `DockingManager` when floating content is docked. |
| `LayoutFloatingWindowControlClosed` | Raised when a floating window is closed. |

```csharp
dockManager.ContentFloating += (sender, args) =>
{
    // React to content being floated
    Console.WriteLine($"Floating: {args.Content.Title}");
};

dockManager.ContentDocked += (sender, args) =>
{
    // React to content being docked
    Console.WriteLine($"Docked: {args.Content.Title}");
};
```

---

## Multi-Monitor Support

Floating windows are standard WPF windows and fully support multi-monitor setups. Users can:

- Drag floating windows to any monitor
- Snap floating windows using Windows snap features
- Arrange multiple floating windows side by side

Layout serialization preserves floating window positions, so the user's multi-monitor arrangement is restored when the layout is loaded.

---

## Standalone Windows ("Window" View Mode)

{: .new }
> New in v5.0.0

A floating window is owned by the main window: it stays on top of it, has no taskbar entry, and takes
part in drag docking. That is the right behaviour for moving a tool window around, but not for
parking one on a second monitor and leaving it there.

`DetachAnchorableToWindow` moves the content of an anchorable into an ordinary top level window
instead — the equivalent of the "Window" view mode that IDEs offer for their tool windows. It has the
operating system chrome, owns a taskbar entry, minimizes and restores independently of the main
window, and may be moved behind it.

```csharp
dockManager.DetachAnchorableToWindow(anchorable);   // move it out
dockManager.ReattachAnchorable(anchorable);         // bring it back
dockManager.ReattachAllDetachedAnchorables();       // bring all of them back
```

Users reach the same thing through **View Mode → Window** in the options menu of the anchorable, or
through the `DetachToWindowCommand` of `LayoutAnchorableItem`. Closing the standalone window returns
the content to the layout.

| Member | Type | Description |
|:-------|:-----|:------------|
| `DetachAnchorableToWindow(anchorable)` | `void` | Moves the content into a standalone window. |
| `ReattachAnchorable(anchorable)` | `void` | Closes the window and returns the content to the layout. |
| `ReattachAllDetachedAnchorables()` | `void` | Returns every detached anchorable. |
| `IsDetached(anchorable)` | `bool` | Whether that anchorable is currently in a standalone window. |
| `DetachedAnchorables` | `IEnumerable<LayoutAnchorable>` | The anchorables currently in standalone windows. |
| `AllowDetachedWindows` | `bool` | Whether the mode is available at all. See [Turn Standalone Windows Off](#turn-standalone-windows-off). |

### What Happens to the Layout

Only the presenter holding the content moves, because a WPF element can have one parent. Where the
anchorable itself goes is decided by the manager: the default hides it, so `Show()` puts it back in
the pane and index it came from. `ToggleDockingManager` collapses it onto its side stripe instead, so
the toggle button stays available and clicking it brings the standalone window forward.

### Serialization

`LayoutAnchorable.IsDetached` takes part in layout serialization, so a saved layout remembers which
tool windows were in standalone windows, and restoring it recreates them at their stored position and
size. A position that no longer lands on any screen — a layout saved on a multi-monitor machine and
restored on a single monitor — is discarded in favour of centring the window, so it can never open
where the user cannot reach it.

### Lifetime

A standalone window has no owner, so it would otherwise keep the process alive under
`ShutdownMode.OnLastWindowClose`. AvalonDock hooks the host window and closes the detached windows
when it goes away. Replacing `DockingManager.Layout` also closes them, because their anchorables are
about to leave the manager.
