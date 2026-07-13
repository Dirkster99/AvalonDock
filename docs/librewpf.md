# AvalonDock on LibreWPF: splitter-drag bug (spurious deactivation kills the drag)

## Symptom

Hovering the mouse over a pane splitter shows the resize cursor correctly. But pressing the left
button to drag:

- the cursor immediately **reverts** to the arrow,
- the **drag does nothing** (panes don't resize),
- and afterwards the resize cursor is **stuck everywhere** over the main window.

## Root cause (confirmed by a controlled A/B experiment, not by theory)

The splitter (`AvalonDock.Controls.LayoutGridResizerControl`) is a `Thumb` with `Cursor = SizeWE`.
On mouse-down it captures the mouse and raises `DragStarted`; AvalonDock's
`LayoutGridControl.OnSplitterDragStarted` → `ShowResizerOverlayWindow()` immediately shows a
**separate transparent top-level `Window`** (the resize "ghost"), with `ShowActivated = false` and
`Owner = null` (`LayoutGridControl.cs`, `ShowResizerOverlayWindow`).

On the LibreWPF portable backend (Silk/GLFW), **showing any new native window steals real OS focus**
— there is no `WS_EX_NOACTIVATE` equivalent, and `ShowActivated = false` is not honored at the
native layer. So the main window fires `Deactivated` the instant the ghost `Window.Show()` runs.

LibreWPF's `WpfPortableWindowActivation` handler (`librewpf/src/ProGPU.Wpf/WpfPortableWindowActivation.cs`,
`case WpfWindowEventKind.Deactivated:` ~line 1061) suppresses this spurious deactivation **only when
one of this process's own popups is open** (`WpfPortablePopupActivation.HasAnyOpenPopup`).
AvalonDock's resizer ghost is a plain `Window`, **not** a `Popup`, so the suppression doesn't apply
— the deactivation is **PROPAGATED** (`DispatchPortableActivationHooks(isActive:false)` +
`TrySetWindowActivationStateForHostEvent(isActive:false)`), which tears down the in-progress
captured mouse interaction in the main window mid-drag.

### The evidence (tooltiptest splitter harness, A/B)

Reproduced with the isolated harness (no AvalonDock — see below). Every failing attempt logs the
same sequence, ~11ms apart, with **zero mouse movement in between**:

```text
WIN PreviewMouseDown pos=242,101 src=Thumb captured=null
SPLIT DragStarted #1 captured=Thumb isThumb=True
WIN PreviewMouseUp   pos=242,101 src=Thumb captured=Thumb   ← phantom up, same position, no move
SPLIT LostMouseCapture #1 nowCaptured=null
SPLIT DragCompleted  #1 canceled=False totalH=-483.0
SPLIT afterOverlayShow captured=null overlayAlive=False     ← already torn down inside Show()
```

The phantom `MouseUp` arrives *inside* the `overlay.Show()` call (its timestamp falls between
`DragStarted` and `afterOverlayShow`) — i.e. `Show()` pumps/deactivates and the drag is killed
before the user can move at all.

**Control run with `TOOLTIPTEST_SPLITTER_NO_OVERLAY=1` (skip showing the overlay window):** the drag
works perfectly — 164 `WIN PreviewMouseMove(LDown)` events, `SPLIT DragDelta` firing continuously
(#1→#160), and `MouseUp` only on the real release. **Showing the overlay window is the entire
cause.**

### Hypotheses this experiment *eliminated*

- **Not** managed `Mouse.Captured` loss: a synthetic probe (capture → `Window.Show()` → re-check)
  shows capture is retained across `Show()`/`Close()` (`RESULT: PASS ... probeB_heldAfterShow=True`).
- **Not** cross-window input routing: `PROGPU_WPF_TRACE_INPUT=1` reported **0** cross-window input
  drops during the failing drags. (The main window's per-window input filter
  `ProGpuWpfWindowHost.OnPlatformInputReceived` *does* drop input tagged to other windows with no
  `Mouse.Captured` escape hatch, and that trace is still wired as a general diagnostic — but it is
  not what breaks the splitter drag.)

### Secondary observation

`Thumb.DragCompleted`'s `HorizontalChange` is garbage on LibreWPF (~-480 regardless of actual
movement), even in the working control run. AvalonDock computes its resize delta from the ghost
canvas position, not from `e.HorizontalChange`, so this doesn't affect AvalonDock — but it's a real
LibreWPF `Thumb` bug worth noting separately.

## Instrumentation (kept in-tree)

- **App-side isolation harness** — `/Users/lextm/uno-tools/tooltiptest`, `TOOLTIPTEST_SPLITTER_MODE=1`:
  a minimal Grid + `Thumb` splitter that mimics `LayoutGridResizerControl` and shows the same kind of
  separate transparent overlay `Window` on `DragStarted`, stripping AvalonDock away. Logs the full
  lifecycle (`SPLIT DragStarted/DragDelta/DragCompleted/LostMouseCapture`, `WIN PreviewMouse*`) to
  `/tmp/tooltiptest_debug.log`, plus a synthetic capture probe. `TOOLTIPTEST_SPLITTER_NO_OVERLAY=1`
  is the A/B control. Driver: `scripts/check-splitter-capture.sh`.
- **LibreWPF-side** — `PROGPU_WPF_TRACE_INPUT=1` now also traces cross-window input *drops*
  (`ProGpuWpfWindowHost.OnPlatformInputReceived`). Not the cause of this bug, but a useful general
  input diagnostic. The relevant activation trace for *this* bug is `TraceMenuCaptureDismissal`
  ("Deactivated PROPAGATED (no popup)").

## Reproduce (needs a real mouse gesture)

Synthetic/DevFlow drags won't reproduce it — the trigger is a real OS focus-steal when the native
overlay window appears.

```bash
cd /Users/lextm/uno-tools/tooltiptest
TOOLTIPTEST_SPLITTER_MODE=1 dotnet run -c Debug          # drag the gray splitter → fails
TOOLTIPTEST_SPLITTER_MODE=1 TOOLTIPTEST_SPLITTER_NO_OVERLAY=1 dotnet run -c Debug   # drag → works
grep -E "SPLIT |WIN " /tmp/tooltiptest_debug.log
```

## Fix directions (not yet implemented — this doc is the diagnosis)

- **LibreWPF (proper fix):** broaden the spurious-deactivation suppression so a `Deactivated` caused
  by *this process's own* newly-shown non-activating (`ShowActivated=false`) window is suppressed the
  same way an open popup's is — the `case WpfWindowEventKind.Deactivated:` handler currently checks
  only `HasAnyOpenPopup`; it should also consult the "non-activating owned window" registry
  (`s_nonActivatingOwnedActivations`, which already exists but is gated on `Owner != null` and isn't
  consulted here). Even better: honor `ShowActivated=false` at the native layer so such a window
  never steals focus at all. This fixes any plain-overlay-window-during-mouse-interaction scenario,
  not just AvalonDock. (Note: merely setting a non-null `Owner` on AvalonDock's overlay does **not**
  fix it — the deactivation handler is popup-only and ignores the owned-window registry.)
- **AvalonDock (workaround):** replace the separate top-level ghost `Window` with an in-window
  `Adorner`/overlay in `ShowResizerOverlayWindow`/`HideResizerOverlayWindow`, so no second native
  window is ever shown and the main window never deactivates mid-drag.

The LibreWPF fix is preferred — the "show a transient top-level window during a mouse interaction"
pattern is common and this spurious-deactivation tear-down will keep surfacing elsewhere.
