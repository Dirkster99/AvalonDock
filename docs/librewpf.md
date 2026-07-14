# AvalonDock on LibreWPF: splitter-drag bug (transient overlay window kills the captured drag)

## Symptom

Hovering the mouse over a pane splitter shows the resize cursor correctly. But pressing the left
button to drag:

- the cursor immediately **reverts** to the arrow,
- the **drag does nothing** (panes don't resize) — or resizes by a single step then stops,
- and afterwards the resize cursor is **stuck everywhere** over the main window.

## Root cause

The splitter (`AvalonDock.Controls.LayoutGridResizerControl`) is a `Thumb` with `Cursor = SizeWE`.
On mouse-down it captures the mouse and raises `DragStarted`; AvalonDock's
`LayoutGridControl.OnSplitterDragStarted` → `ShowResizerOverlayWindow()` immediately shows a
**separate transparent top-level `Window`** (the resize "ghost"), with `ShowActivated = false` and
`Owner = null` (`LayoutGridControl.cs`).

On the LibreWPF portable backend (Silk/GLFW) there is no `WS_EX_NOACTIVATE` equivalent, so **showing
that overlay window mid-drag disturbs the main window** (real OS focus-steal + a transient OS
reposition of the owner). That single `Window.Show()` produces **three distinct effects**, each of
which independently damages the in-progress captured Thumb drag. All three are now fixed in LibreWPF:

1. **Phantom `MouseUp` on show.** GLFW delivers a spurious button-up to the main window when the
   overlay appears. *Fix (pre-existing):* `ProGpuWpfWindowHost.OnPlatformInputReceived` swallows a
   post-window-show `MouseUp` that arrives with no intervening move (see
   `NoteWindowShownForSpuriousUpGuard`), plus GLFW mouse-passthrough on windows shown during a press.

2. **`(0,0)` teleport from `Mouse.Synchronize()`.** The overlay `Window.Show()` flips
   `CriticalActiveSource` to the overlay's source. A hit-test-invalidated / mouse-over reevaluation
   then calls `MouseDevice.Synchronize()`, whose position comes from `GetClientPosition()`; against
   the flipped source it **collapses to `(0,0)`** and is delivered to the captured Thumb as a
   teleport to the window corner → a huge bogus `DragDelta` that snaps the pane to the edge.
   *Fix:* `MouseDevice.Synchronize()` returns early when `Captured != null` — during a captured drag,
   routing is fixed to the captured element and driven by real move events; a synthesized re-hittest
   move is unnecessary and, on this backend, unreliable. (Win32 WPF reads the true OS cursor position
   via `GetCursorPos`, so its synchronize-during-capture is harmless; ours is not.)

3. **Capture released by `Window.HandlePortableMove`.** Showing the overlay makes the OS transiently
   reposition the main window. `Window.HandlePortableMove` releases mouse capture on any move
   (`Mouse.Capture(null)`) as its popup-dismiss-on-owner-move mechanism, excluding only WPF popups.
   The AvalonDock ghost is a plain `Window`, not a popup, so the Thumb's capture was released → the
   drag ended after a single move. *Fix:* also skip the capture release while a mouse button is
   physically held (`Mouse.LeftButton/MiddleButton/RightButton == Pressed`) — a window move with a
   button down is an in-progress captured drag, never a popup-dismissing move.

The `(0,0)` teleport was the "线跑到奇怪的地方" symptom; the capture release was the "只拖了一下就停"
symptom. Effects 2 and 3 were the residual failures after the phantom-up/passthrough work.

## The three fixes (all in the `librewpf` repo)

| # | File | Change |
|---|------|--------|
| 1 | `src/ProGPU.Wpf/ProGpuWpfWindowHost.cs` | swallow phantom post-show `MouseUp` + GLFW mouse-passthrough (pre-existing); plus a defensive guard in `NormalizeInputEventForRenderSurfaceGeometry` that passes the event through unchanged when the render-surface scale is degenerate (0/NaN) instead of collapsing coordinates to `(0,0)` |
| 2 | `src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Input/MouseDevice.cs` | `Synchronize()` returns early when `Captured != null` |
| 3 | `src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Window.cs` | `HandlePortableMove` skips `Mouse.Capture(null)` while a mouse button is held |

Fixes 2 and 3 ship in `LibreWPF.Transport`; fix 1 in `LibreWPF.ProGPU`. Repack recipe:
`OpenDevelop/doc/technotes/librewpf.md`. Note: the Transport package packs **pre-built** WPF DLLs, so
`dotnet build` `PresentationCore.csproj` / `PresentationFramework.csproj` *before* `dotnet pack`-ing
the packaging project, or the DLL inside the nupkg is stale.

## Docking round-trip harnesses (tooltiptest)

`tooltiptest` has three AvalonDock-on-LibreWPF drag harnesses, each a `TOOLTIPTEST_*` mode driven by a
DevFlow script that asserts a `*_RESULT:` line:

| Mode / script | Scenario | Status |
|---|---|---|
| `TOOLTIPTEST_SPLITTER_MODE` — `scripts/check-splitter-capture.sh` | splitter Thumb resize drag | **PASS** (fixes above) |
| `TOOLTIPTEST_AVALONDOCK_FLOAT_MODE` — `scripts/check-float-drag.sh` | tear a docked tool pane out into a floating window | **PASS** |
| `TOOLTIPTEST_AVALONDOCK_DOCK_MODE` — `scripts/check-dock-drop.sh` | drag a floating window over the manager and drop on a drop target to re-dock | **FAIL — drag engine ported; blocked on cross-window coordinate scale** |

### Re-dock: drag engine ported, blocked on cross-window coordinate scale

Upstream the floating-window drag engine is **entirely Win32**: `LayoutFloatingWindowControl.AttachDrag`
sends `WM_NCLBUTTONDOWN`/`HT_CAPTION` to enter OS move-mode, `FilterMessage` drives
`DragService.UpdateMouseLocation` from `WM_MOVING` and calls `DragService.Drop` on `WM_EXITSIZEMOVE`,
and `WindowChrome` handles caption hit-testing. On LibreWPF the `HwndSource` is a **shim** (it *is*
reported as an `HwndSource`, so a null check does not detect the portable case) that never pumps those
`WM_*` messages, and `WindowChrome` caption dragging is inert — so none of it engages.

**Ported (this repo, `LayoutFloatingWindowControl`):** a managed caption drag, gated on
`UsePortableCaptionDrag` (`!OSPlatform.Windows`). On caption mouse-down (an element not marked
`WindowChrome.IsHitTestVisibleInChrome`) it captures the mouse; each `OnMouseMove` repositions the
window to follow `PointToScreen(e.GetPosition(this))` and feeds `DragService.UpdateMouseLocation`;
mouse-up calls `DragService.Drop` (lost-capture aborts). This **works**: the window follows the
pointer, `DragService` finds the overlay host, the `OverlayWindow` shows, and drop *areas* are
detected (`GetDropAreas` → `DragEnter`).

**Remaining blocker — a LibreWPF cross-window coordinate/scale inconsistency.** The drop never
completes because the drag point and the drop-indicator detection rects are in mismatched screen
spaces:
- the **main window** reports `PointToScreen(0,0)` and `CompositionTarget.TransformToDevice.M11` in
  **logical** units (`M11 == 1`; the `OverlayWindow` is placed at logical `100,100`), but
- the **floating / overlay windows'** `GetScreenArea` / `PointToScreen` come out in **device px (2×)**
  (a `DocumentPaneDockInside` indicator reports a detection rect at `~1271,951` = 2× its logical
  `~635,475`), and the overlay window also renders offset from its requested `Left/Top` (the same
  overlay-placement issue seen with the splitter's ghost line).

So `DropTarget.HitTest` compares a point in one space against rects in another and never intersects
(`curTarget` stays null → `Drop` returns `handled=false` → the pane stays floating). This is not an
AvalonDock defect — the engine is internally consistent — it is LibreWPF handing different windows
different DPI scales and mis-placing transient top-level windows. The fix belongs in LibreWPF
(uniform per-window DPI scale + correct portable `Window.Left/Top` placement); once screen coordinates
are consistent across windows, `check-dock-drop.sh` should pass with no AvalonDock change.

## Reproduce / verify (automated)

The isolation harness lives in `/Users/lextm/uno-tools/tooltiptest` (`TOOLTIPTEST_SPLITTER_MODE=1`): a
minimal Grid + `Thumb` splitter that shows the same kind of transparent overlay `Window` on
`DragStarted`, with AvalonDock referenced directly (there is also `TOOLTIPTEST_AVALONDOCK_FLOAT_MODE=1`
for the tear-out-float scenario). `scripts/check-splitter-capture.sh` drives a real DevFlow drag
against the `SplitterProbeThumb` and asserts `SPLIT_RESULT: PASS` (24 continuous `DragDelta`s, capture
held until the real release, pane width changes by the drag distance).

```bash
cd /Users/lextm/uno-tools/tooltiptest
bash scripts/check-splitter-capture.sh   # -> SPLIT_RESULT: PASS
```

### Why the earlier "phantom-MouseUp-on-show" A/B (`TOOLTIPTEST_SPLITTER_NO_OVERLAY=1`) was incomplete

That control proved the overlay `Window.Show()` was the trigger, and the phantom `MouseUp` was one
real effect. But swallowing the phantom up alone left effects 2 and 3, which only surface once the
drag is actually allowed to proceed past the first sample — hence the "can drag but jitters / line
jumps to a weird place / stops after one step" reports. Instrumenting `MouseDevice.Synchronize`,
`ChangeMouseCapture`, and the input paths (native `OnPlatformInputReceived` vs. synthetic
`PortableWindowActivationService.ProcessInput`) pinned effects 2 and 3.

### Instrumentation kept in-tree

- **LibreWPF:** `PROGPU_WPF_TRACE_INPUT=1` traces native/wpf input in `ProGpuWpfWindowHost`; the
  activation trace for focus effects is `TraceMenuCaptureDismissal` / `TraceWindowMoveCapture`.
- **App-side:** the tooltiptest splitter harness logs the full Thumb lifecycle + `WIN PreviewMouse*`
  to `/tmp/tooltiptest_debug.log`, plus a static capture probe (`CAPTURE_PROBE:`).
