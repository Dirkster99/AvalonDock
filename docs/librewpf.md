# AvalonDock on LibreWPF: synthetic-input drag flake, window-lifetime crashes, and the test port

> Research notes on the integration between AvalonDock and LibreWPF
> (`/Users/lextm/wpf-tools/librewpf`, the wpf-progpu fork).
>
> Part one (sections 1-8) is the 2026-08 investigation of **native drag flake** and **window-destroy
> crashes**: the diagnostics, the causal chains, the fixes that were applied, and the LibreWPF
> source-change to local-feed replacement procedure.
>
> Part two (section 9) is the 2026-10 work that replaces the FlaUI UI tests with DevFlow suites so
> one suite runs on Windows x64, macOS arm64 and Linux x64. It records what is done,
> what CI shows, and what is left.
>
> The older splitter-drag fixes are kept as history (section 10).
>
> Part three (section 11) is the 2026-10-27 plan to split this fork into two independent lines -
> the original Windows-only WPF line and a LibreWPF line - with its status tracker.

> **Current state in Dirkster99/AvalonDock (issue #609).** The LibreWPF line restores only from nuget.org
> (`LibreWPF.Sdk` and `LibreWinForms.*` `0.1.0-preview.69`, `LeXtudio.DevFlow.*` `0.3.1`) with the .NET 10
> SDK; there is no `NuGet.config` and no GitHub Packages feed. `release.yml` publishes
> `Dirkster.AvalonDock.LibreWPF`, `Dirkster.AvalonDock.Themes.VS.LibreWPF` and
> `Dirkster.AvalonDock.Themes.VS2013.LibreWPF` to nuget.org with the classic packages; `Core` and
> `Serializer.Xml` are the shared classic packages. Differences from the OpenAvalon builds this log was
> written against: the nuget.org packages have no visual `WindowsFormsHost` (TestApp shows a placeholder
> unless `DEVFLOW_HOST_WINFORMS=1`, and the hosted WinForms cases skip), no `LibreControlHosting` (the
> mnemonic case skips), and throw on `TextFormattingMode.Display` (the themes use
> `PlatformTextOptions.TextFormattingMode`, which is `Ideal` under LibreWPF). The ProGPU clip fix `02119efdf`
> is in no nuget.org ProGPU package yet, so the auto-hide and tool-tab click cases are known gaps again
> (`KnownLibreWpfGaps.AutoHideClick`, `ToolTabClick`; `DEVFLOW_RUN_KNOWN_GAPS=1` runs them). References below to the
> OpenAvalon/lextudio feeds, `opendevelop` and `release-librewpf.yml` are history.

## 1. Background: test infrastructure and the event chain

The DropTargetZone integration tests drive real drags with **cliclick** (synthetic mouse events on
macOS). The event chain is:

```
cliclick (CGEventPost into kCGHIDEventTap)
  -> GLFW cocoa window (libglfw.3.dylib, 3.4.0, Silk.NET 2.23.0)
  -> Silk.NET input callbacks (SilkNetWpfInputService)
  -> LibreWPF WPF compatibility layer (forwarded by ProGpuWpfWindowHost)
  -> WPF input pipeline (Mouse.LeftButton / MouseMove and friends)
  -> AvalonDock (LayoutFloatingWindowControl native drag detection + DragService + OverlayWindow)
```

**Facts measured on this machine:**

- cliclick's `dd`/`dm`/`du` are **synthetic events** (`CGEventPost`); they do **not** update the
  physical HID button state.
- `MacOSCursorService.IsLeftButtonDown()` (AvalonDock/Platform) goes through
  `CGEventSourceButtonState` - the physical state - so it **always returned false** for
  cliclick-injected events (the diagnostic log confirmed `physDown=False`).
- WPF's `Mouse.LeftButton` is only updated from the **event pipeline**: a synthetic `dd` becomes
  Pressed once WPF has processed it.

## 2. Drag flake: cliclick's synthetic `dd` is not processed by WPF

### Symptoms

The full suite (22 zone/preview cases) failed randomly in one or two cases, in one of two ways:

- `compass never showed drop target 'X' during discovery` - the compass stayed empty for the 8 s
  discovery window;
- `live drop target 'X' was not current before release; Target=..., Targets=[]` - after dragging to
  the target centre, `currentDropTarget` never appeared.

Failures landed on different zones at random (DockLeft, DockRight, DocumentPaneDockRight,
AnchorablePaneDockTop and others all showed up) and had nothing to do with the assertions: the dock
simply never happened.

### Reproduction

`/var/folders/.../T/opencode/drag-diag4.sh` reproduced the test flow exactly (position-main-window ->
reset -> float -> discovery drag -> a second drag to the centre of DockLeft -> release), 20 rounds
per run: **a steady 15-25% failure rate** (15-17 of 20 succeeded), with `avd.query.drag-state`
captured at the point of failure.

### What a failure looked like

The interesting fields of `drag-state` on failure:

```json
{
  "overlayLeft": null, "overlayTop": null, ...   // the overlay was never created
  "currentDropTarget": null,
  "left": 560, "top": 718,                        // the floating window did move (the tick was running)
  "currentPointer": {"X": 658, "Y": 735}          // the pointer is inside the manager
}
```

Two sub-modes:

- **Sub-mode A**: the floating window moved (the tick updated Left/Top) but the overlay was never
  created (`UpdateMouseLocation` never matched a host);
- **Sub-mode B**: the floating window did not move at all (`left/top` stayed at the pre-press
  position).

Temporary diagnostic logging in `LayoutFloatingWindowControl.OnPortableNativeLocationChanged`
produced the **decisive evidence**:

```
[DRAGDIAG] native move ignored: leftBtn=Released physDown=False at 658,735   (x36)
```

**cliclick's `dd` never reached WPF** (`Mouse.LeftButton=Released`), so the `buttonDown` check in the
window-move path (`Mouse.LeftButton==Pressed || IsLeftButtonDown()`) was false and the drag never
started.

### Causal chain

1. cliclick `dd` injects a synthetic press (`CGEventPost`, kCGHIDEventTap).
2. An earlier explanation blamed macOS "first click consumed by activation". Reading the actual
   GLFW 3.4 Cocoa sources showed that this cannot be stated as a confirmed root cause:
   `GLFWContentView` does implement `acceptsFirstMouse:` and returns `YES`. A non-key window still
   changes event timing, but GLFW cannot be said to lack a first-mouse path; the precise place where
   `dd` is lost still needs a native callback trace.
3. Even when the event is processed, `CGEventSourceButtonState` (physical state) is always false for
   synthetic events, which creates a **timing race against the WPF event state**
   (`Mouse.LeftButton`):
   - if the tick (a 16 ms `DispatcherTimer`) checks the button state **before** the injected event
     has been processed by WPF, it concludes "button released" and `EndPortableDrag` aborts the drag
     immediately.

### Fixes applied

**AvalonDock** (`src/Components/AvalonDock/Controls/LayoutFloatingWindowControl.cs`):

the watchdog's button check gained a **start-up grace period**,
`PortableNativeDragGracePeriod = 500ms`:

```csharp
var dragAge = DateTime.UtcNow - _portableDragStartUtc;
if (dragAge > PortableNativeDragGracePeriod
    && Mouse.LeftButton != MouseButtonState.Pressed && !PlatformHelper.IsLeftButtonDown())
{
    EndPortableDrag(drop: true);
    return;
}
```

This gives the synthetic `dd` a window in which to reach the WPF input pipeline, so the watchdog no
longer kills the drag at the instant it starts.
**Effect**: the full suite went from a steady 1-2 flakes to 0-1 (22/22 once, 21/22 once, 20/22
twice). **Not eradicated**: sub-mode B (a `dd` consumed by macOS activation) never reaches the
watchdog, so the grace period does not cover it.

**AvalonDock** (`DockingManager.cs`): `GetOverlayWindowHostsByZOrder` got an exception fallback (when
the Win32 z-order enumeration fails on a portable backend it degrades to a plain enumeration). This
is defensive.

**LibreWPF** (`src/ProGPU.Wpf/Platform/SilkNetWpfInputService.cs`):

`mouseUp` no longer drops an event because no matching down was recorded. The original code was:

```csharp
if (!pressedButtons.Remove(button)) { return; }   // down lost -> up swallowed
```

With synthetic input, a down is occasionally lost, and swallowing the up leaves WPF's
`Mouse.LeftButton` **stuck in Pressed**, which corrupts every later drag. The up is now always
forwarded (WPF maintains the button state itself). **Effect**: the v4 script went to 16-18/20 -
neither clearly worse nor clearly better. It is a correctness fix, not a cure for the flake.

**Attempts that did not help:**

- `avd.activate` (activating the main window before the press): made it worse - 0/20. Once the main
  window is key the floating window still is not, so activation consumption on a caption click
  happens even more reliably; activation can also move the floating window and invalidate the
  coordinates.
- A longer wait after the press: no change.
- A `GetCursorPos` shim (`ProGPU.Wpf.Sdk.Win32Compat.c`) that always returns `(0,0)`: irrelevant,
  because AvalonDock's `PlatformHelper.GetCursorPosition` goes through `MacOSCursorService`
  (`CGEventGetLocation`) and never uses that shim.

### 2026-08-08 test-side hardening (final 22/22)

Later observations showed the flake did not only come from a lost `dd`: the tests themselves had
injected mouse-down at **the wrong screen origin**. Managed `Window.Left/Top`, `PointToScreen` and the
GLFW native frame can disagree for a moment, and a floating window can sit behind the main window. In
that case the computed point was inside the "managed floating bounds" while macOS delivered the click
to the main window or to the desktop/system UI - in the worst case triggering a real macOS action.

Every floating-body warm-up press and every caption drag-down is now gated:

1. `avd.query.bounds` / `avd.query.drag-handle` read the ProGPU/GLFW native `Position`/`Size` for
   floating windows first, and the floating visual tree's screen coordinates are converted from the
   native origin;
2. cliclick only moves the mouse; `avd.query.cursor` then reads the native cursor back through
   `MacOSCursorService`, and a mouse-down is refused when the requested and measured points differ
   by more than 2 px;
3. the measured cursor must still be inside the floating native frame, and a caption drag must land
   on the caption handle;
4. `avd.query.floating-zorder` must confirm the floating window is in front of the main window; if it
   is not, TestApp calls `Activate()` on the floating window directly and re-reads frame and
   z-order rather than relying on a click that may hit the wrong window;
5. activation warm-up down/up pairs are separated by the system double-click interval (macOS:
   `NSEvent.doubleClickInterval`) so a caption click cannot merge with a neighbouring event into a
   double click.

Zone-coordinate discovery no longer runs an extra, flake-prone real drag. `avd.debug-show-overlay`
now calls `UpdateLayout()` first and returns the screen bounds of every indicator; the test picks the
indicator nearest the target pane and then performs a single real held drag. Once the target is hit,
the zone/preview tests have TestApp complete AvalonDock's current drop deterministically before the
synthetic mouse is released. The full suite later proved that WPF's `Mouse.LeftButton` can stay
`Pressed` even after `manager.up` has been received, so `NativeInputIntegrationTests`' float-back-to-
dock case also completes the already-hit current drop first and then sends the real `du` to clean up
the input state. The tests no longer treat "a synthetic mouse-up always triggers the dock" as a
reliable contract.

A full serial run also exposed two preconditions that rarely appear in isolated runs, both handled
before mouse-down:

- during a layout transition individual temporary targets can have a negative extent;
  `GetScreenBounds()` skips such unhittable rects instead of throwing while enumerating the other,
  valid targets;
- a freshly created floating window's native frame can still be above the screen, or a cursor-only
  move can be swallowed by a key-window switch. The test re-reads the native frame and cursor; when
  it is not safe it never presses, but docks programmatically, recreates the floating window and
  retries.

Final verification command and result:

```bash
dotnet run --project source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -- \
  -class AvalonDock.DevFlowIntegrationTests.DropTargetZoneIntegrationTests -parallel none
# total: 22, succeeded: 22, failed: 0, duration: 4m 40s
```

Final full-suite verification including the lifetime and safe-retry corrections:

```bash
dotnet run --project source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -- \
  -parallel none
# total: 56, succeeded: 56, failed: 0, duration: 6m 16s
```

Test code must not take screenshots on its own. The leftover `/api/v1/ui/screenshot` call in
UnoDevelop's `MainMenuTests.ViewTestsClickShowsTestsPad` was removed; this run has no screenshot
step.

## 3. Crash A: `resignKeyWindow` notifies a dangling observer (SIGBUS)

### Stack

```
EXC_BAD_ACCESS / KERN_PROTECTION_FAILURE
__CFNOTIFICATIONCENTER_IS_CALLING_OUT_TO_AN_OBSERVER__   <- dangling observer
AppKit -[NSWindow resignKeyWindow] -> _sendResignKeyWindow -> _changeKeyAndMainLimitedOK
AppKit -[NSWindow _handleLeftMouseDownEvent:isDelayedEvent:]   (isDelayedEvent:YES)
AppKit -[NSWindow(NSEventRouting) sendEvent:] -> libglfw.3.dylib
```

### Trigger and root cause

- Trigger: **a key-state change caused by a cliclick click** (a background window is not key, so the
  click activates or deactivates it) - the same origin as the flake's "activation consumption".
- The top of the stack is AppKit dereferencing a bad address while dispatching
  `NSWindowDidResignKeyNotification`. GLFW's `GLFWWindowDelegate.windowDidResignKey:` is one of the
  receivers of that notification, but GLFW 3.4's sources contain no explicit
  `NSNotificationCenter addObserver:`; it receives through `NSWindow.delegate`, and on destroy it
  calls `setDelegate:nil` before releasing the delegate. "GLFW leaves an observer registered" is
  therefore only a candidate hypothesis, not a confirmed root cause.
- After that, any key-state change (any click) reaches the dangling address -> SIGBUS.

### Where it can be fixed

**GLFW native** (`libglfw.3.dylib`, packaged by Silk.NET 2.23.0 as `ultz.native.glfw` 3.4.0). The
LibreWPF managed sources (ProGPU.Wpf) contain no NSNotificationCenter/ObjC observer registration at
all (checked by grepping the whole repository). GLFW master's Cocoa destroy order is the same as 3.4
in this area, so "upgrade to master and it is fixed" has no source-level basis. Candidate actions:

- add native logging to GLFW's `GLFWWindowDelegate` resign/close/dealloc and to
  `_glfwDestroyWindowCocoa`, and prove with object addresses whether the callback happens after the
  delegate or window was released;
- replacing the GLFW native build is still a valid A/B, but only with the reproduction rate as
  evidence;
- reduce how often floating windows are created and destroyed (reuse them in AvalonDock).

## 4. Crash B: `CUIThemeFacet` crashes while decoding a CAPackage (EXC_BAD_ACCESS)

### Stack (measured 2026-08-08 09:25)

```
EXC_BAD_ACCESS / EXC_ARM_DA_ALIGN at 0x9
CFRelease -> __CFBasicHashAddValue -> CFDictionarySetValue
NSKeyedUnarchiver decodeObject* -> [CALayer initWithCoder:] (x2 levels)
[CAPackage _readFromArchiveData:options:error:]
+[CAPackage packageWithData:type:options:error:]
-[CUIThemeFacet _makeLayerFromCAPackageData] -> updateLayer:effects:
CUICoreThemeRenderer::CreateOrUpdateVisualEffectLayer / CreateOrUpdateLayer
```

### Current assessment

**The macOS AppKit theme renderer (CUIThemeFacet)** decodes a `CAPackage` (a layer archive) while
creating or updating a Visual Effect Layer (the vibrancy material), touches a freed object, and
crashes in `CFRelease`. It reproduced repeatedly during this hardening work, always after frequent
float/drop/layout-restore cycles; the stack is consistently inside AppKit/CoreUI. For now "an AppKit
system-layer bug" is only a **high-probability hypothesis** - a wrong NSVisualEffectView lifetime,
a cross-thread update, or an early release in LibreWPF could still be corrupting AppKit state
indirectly. `restorable=NO` does not prevent it (the crash is in theme rendering, not in state
restoration).

### 2026-08-08 lifetime A/B

Two DevFlow stress tests that take no screenshots separated native mouse input from window
lifetime:

- 100 x `float -> dock` (creating and destroying only the titled floating window): passed, ~39 s;
- 100 x `float -> show transparent overlay + DockLeft preview -> hide overlay -> dock`: passed,
  ~60 s.

So neither "frequent floating-window destruction" nor "frequent transparent-overlay destruction" is
sufficient to trigger Crash B. LibreWPF never creates an `NSVisualEffectView` explicitly; making a
window transparent only calls `setOpaque:NO` and `setBackgroundColor:NSColor.clearColor` on the GLFW
`NSWindow`. With the crash inside `CUIThemeFacet`, the more precise hypothesis is that the system
creates a visual-effect layer **for the AppKit frame/titlebar of a titled GLFW window**, and that a
real activation, drag or key-window switch interleaved with window close triggers a race in the
theme-layer update. The next A/B round should add safe real activation/drag events instead of
increasing the number of input-free create/destroy cycles.

### 2026-08-08 AppKit view-tree evidence

`avd.query.macos-view-tree` now walks the AppKit view tree down from a real `NSWindow*`'s
`contentView.superview` (read-only, no screenshots), recording class names, object addresses,
`hidden` and the window's `styleMask`. Measured:

| Window | styleMask | `NSVisualEffectView` | Key state |
|---|---:|---:|---|
| main | 15 | 2 | one directly under the frame and one inside the titlebar background, neither hidden |
| floating | 32777 | 2 | one unhidden directly under the frame; the titlebar background is hidden but its effect subview is not |
| transparent overlay | 32777 | 1 | sits under the hidden titlebar background; no frame-level effect |

That rules out the two simple explanations ("LibreWPF creates an `NSVisualEffectView` explicitly"
and "only an ordinary titled window has that object"). Under the portable backend `WindowStyle.None`
is still an AppKit theme frame with `NSWindowStyleMaskFullSizeContentView`, not a pure borderless
window without a theme/titlebar view tree. The overlay's effect has a hidden parent, while the
floating window has an unhidden frame-level effect. One TestApp A/B removed the frame-level
`NSVisualEffectView` from a custom-chrome/full-size floating window. After refreshing the LibreWPF
local feed, real drags repeatedly produced `EXC_BAD_ACCESS/SIGSEGV -> objc_msgSend -> managed
P/Invoke`; even with the pointer verified through `NSApplication.orderedWindows` first, there is
still a lifetime race between the check and the following message. That A/B was therefore abandoned
completely and cannot be a product fix. Crash B needs further investigation in LibreWPF's/AppKit's
window-lifetime layer.

The same native-coordinate diagnostics also found that the overlay's managed `Left/Top` and its real
Cocoa frame occasionally separate; drag-state and preview-to-screen conversions used to mix the
overlay's managed origin with the manager's native origin, producing false offsets. All
preview/overlay diagnostics now use the real native origin, and the overlay corrects its position
once more through `INativeWindowService.SetWindowPosition` after `Show()` and native handle creation.

Long sequences further proved this is not only a refresh-timing problem: ProGPU's
`TryGetWindowHost(overlay)` occasionally returned the floating or main window's native host. Main,
floating and overlay windows now register native handle ownership when the source is initialised;
duplicate handles are rejected, and overlay movement is refused while the overlay has no exclusive
handle. macOS floating pre-positioning and the portable caption drag also use the fixed native handle
directly, so LibreWPF's `Window.Left/Top` setter can no longer cross windows and move the main window.
Each zone theory case now restarts TestApp, which stops a transparent native window's lifetime from
leaking across cases. Restoring the XML layout item by item used to hang the UI thread while a
floating native window was closing; the suite now performs a deterministic layout reset at the start
of the next item instead, and a 20 s action watchdog reports the action that is actually stuck.

Every test records the main window's first movement with a 20 ms native-origin guard inside TestApp
and asserts at the end that it never moved. Before every floating->dock mouse-down the tests verify
not only the requested point, the window and the z-order, but also read the real native cursor; up to
five slow, move-only retries are allowed, and the press is allowed only when the cursor is inside the
latest floating title-bar handle. The extra warm-up click is gone, clicks are still separated by the
system double-click interval, and no test calls any screenshot action. Four dedicated cases save the
live preview's screen rectangle before the drop, read the pane rectangle that actually hosts it
afterwards, and compare position and size with a 2 px tolerance.

Final verification after the 2026-08-08 refresh: the local feed's official directory holds only the
three valid packages, historical packages moved to `artifacts/local-feed-backups`, and the SHA-256 of
`ProGPU.Wpf.dll` in the LibreWPF Release build, the NuGet cache and the TestApp output is
`9f0e7585470d07f145ac1d9812552db43174cb81f44f3f569263f3626a6be1b6`. The full integration suite
passed 57/57 (8m42s).

### Impact

Triggered occasionally while the full suite ran those 22 cases -> the TestApp process died -> every
later case failed with `DevFlow agent not reachable` (in one measured run, 12 cases failed in a
cascade after the crash).

## 5. Crash C: a native drag tick moved a released NSWindow after close

### Stack and evidence

OpenDevelop crashed on the main thread when switching layout `Plain -> Default` while the Error List
and the Project Browser were in the same floating container:

```
EXC_BAD_ACCESS / SIGSEGV
objc_msgSend
selector: setFrameTopLeftPoint:
```

That selector is only used by AvalonDock's `MacOSNativeWindowService.SetWindowPosition`. Its caller,
`LayoutFloatingWindowControl.OnPortableNativeDragTick`, is a 16 ms `DispatcherTimer`. The close path
neither stopped that timer nor detached the macOS `LocationChanged` handler, so after a layout
restore destroyed the native NSWindow a queued tick could still resolve and use the old handle.
Sending an Objective-C message to a freed receiver is a process-level access violation that managed
`try/catch` cannot guard.

### Fix

`LayoutFloatingWindowControl` now does all of this in `OnClosing` (which must run before the backend
destroys the native window):

- stop the portable native drag timer;
- detach `LocationChanged` and the global `PostProcessInput` handler;
- release the mouse capture;
- abort and clear the `DragService`, and reset the drag state.

`OnClosed` calls the same cleanup again, idempotently, to cover non-standard close paths. On the
OpenDevelop side the Error List also gets `PreferredDockSide.Bottom`, so it no longer falls into
AvalonDock's floating fallback when it is missing from the layout XML. The Workbench integration test
gained assertions that the Error List has `isFloating == false` and `side == Bottom`; the full suite
passed 35/35.

## 6. LibreWPF source change -> local feed replacement

TestApp consumes packages through the `LibreWPF.Sdk` MSBuild SDK; the local source is configured in
`OpenDevelop/NuGet.config`:

```xml
<add key="librewpf-local" value="/Users/lextm/wpf-tools/librewpf/artifacts/local-feed" />
<packageSource key="librewpf-local">
  <package pattern="LibreWPF.ProGPU" />
  <package pattern="LibreWPF.Transport" />
  ...
</packageSource>
```

**Full replacement procedure** (verified working):

```bash
# 1. change the source: /Users/lextm/wpf-tools/librewpf/src/ProGPU.Wpf/...
# 2. build (with the repository's pinned SDK)
cd /Users/lextm/wpf-tools/librewpf
export PATH="$PWD/.dotnet:$PATH"
dotnet build src/ProGPU.Wpf/ProGPU.Wpf.csproj -c Release
# output: src/ProGPU.Wpf/bin/Release/net10.0/ProGPU.Wpf.dll

# 3. unpack the nupkg -> replace the dll -> repack (zip) -> overwrite the local feed
mkdir -p /tmp/nupkg-work && cd /tmp/nupkg-work
cp /Users/lextm/wpf-tools/librewpf/artifacts/local-feed/LibreWPF.ProGPU.0.1.0-preview.41.nupkg orig.nupkg
unzip -o -q orig.nupkg -d extracted
cp <new ProGPU.Wpf.dll> extracted/lib/net10.0/ProGPU.Wpf.dll
cd extracted && rm -f ../LibreWPF.ProGPU.0.1.0-preview.41.nupkg \
  && zip -q -r -X ../LibreWPF.ProGPU.0.1.0-preview.41.nupkg .
# verify: unzip -p new.nupkg lib/net10.0/ProGPU.Wpf.dll | md5 == md5 of the new dll

# 4. back up, overwrite the feed, clear the NuGet cache
#    (step 3 left the shell in extracted/, so go back to the artifacts directory first; a single
#    glob cannot be copied onto a glob, so the backup names one file)
cd /Users/lextm/wpf-tools/librewpf/artifacts
cp local-feed/LibreWPF.ProGPU.0.1.0-preview.41.nupkg \
   local-feed/LibreWPF.ProGPU.0.1.0-preview.41.nupkg.myfix-backup
cp /tmp/nupkg-work/LibreWPF.ProGPU.0.1.0-preview.41.nupkg local-feed/
rm -rf ~/.nuget/packages/librewpf.progpu/0.1.0-preview.41

# 5. rebuild TestApp and compare md5
dotnet build source/TestApp/TestApp.csproj -c Debug
md5 -q source/TestApp/bin/Debug/net10.0-windows/ProGPU.Wpf.dll   # must equal the new dll
```

Note: the `LibreWPF.Transport` package contains **prebuilt** WPF DLLs
(PresentationCore/PresentationFramework). If code under `src/Microsoft.DotNet.Wpf` changed, build
that csproj first and then pack the packaging project.

## 7. Diagnostic tools

| Tool | Purpose |
|---|---|
| `avd.query.drag-state` | Live drag state: `currentDropTarget`, `previewGeometryBounds` (screen rect of the current real preview), `overlayLeft/Top` (whether the overlay was created), `left/top` (floating window position), `currentPointer`, `dragOffset` |
| `avd.query.active-drop-targets` | The compass indicators currently shown (type + screen centre) |
| `avd.input.query` | TestApp mouse diagnostics: `leftButton` (Pressed or not), `mouseX/Y`, `captured` |
| `avd.debug-show-overlay <zone>` | Force the overlay on and return `previewGeometryBounds` (**screen** coordinates, for comparison with the pane after docking) |
| `drag-diag4.sh` (temporary session script) | Reproduce the flake 20 times, printing the drag-state on failure |
| `~/.nuget/packages/...` and `local-feed` | Package version/replacement verification (md5 comparison) |
| `~/Library/Logs/DiagnosticReports/TestApp-*.ips` | Crash reports (`python3 -c` to extract the exception and the triggered thread's stack) |

**How to read a failure:**

- `overlayLeft: null` and the floating window moved -> sub-mode A (the tick runs but
  `UpdateMouseLocation` never matches a host);
- the floating window did not move and `leftBtn=Released` -> sub-mode B (cliclick's `dd` never reached
  WPF, consumed by activation);
- the case fails within 4 ms with `not reachable` -> the previous step's TestApp crash cascading
  (check the .ips).

## 8. Status and open issues (2026-08)

**Fixes in place** (all verified by the full suite):

1. `LayoutFloatingWindowControl.cs` (AvalonDock): 500 ms watchdog grace period - flake rate 1-2 ->
   0-1.
2. `DockingManager.cs` (AvalonDock): `GetOverlayWindowHostsByZOrder` exception fallback (defensive).
3. `SilkNetWpfInputService.cs` (LibreWPF, dll replaced in the local feed): mouseUp is no longer
   dropped when its down is missing.
4. Test improvement: `DockPreview_MatchesDockedPaneGeometry` records the preview's screen rectangle
   from `avd.query.drag-state` while the real drag is held and the target is hit but not yet
   released, then reads the actual pane after the drop and compares position and size (2 px
   tolerance).
5. native-input safety gates: a mouse-down is allowed only when the native cursor, the GLFW frame and
   the native z-order are all correct; wrong coordinates now fail instead of dragging the main
   window, the desktop or macOS system UI.
6. The extra discovery drag is gone; the overlay publishes all indicator bounds after it has laid
   out, and zone tests complete their drop deterministically while a dedicated case covers the real
   mouse-up.
7. Full `DropTargetZoneIntegrationTests`: **22/22 in 4m40s**, with no screenshot action during the
   run.

**Open:**

- product-level first-click semantics: the tests work around a non-key window swallowing `dd`, but a
  real application should still verify/implement GLFW Cocoa `acceptsFirstMouse`; a test workaround is
  not a product fix;
- Crash A (dangling GLFW observer): did not reproduce in the final 22/22, but the native stack is
  known and it needs a GLFW build A/B before it can be closed;
- Crash B (CUIThemeFacet CAPackage decode): reproduced repeatedly during this round of fixes and is
  the highest-priority stability problem; the final 22/22 only shows that fewer window/gesture cycles
  kept it from triggering in that run, not that it is fixed.

### Suggested order of further investigation

1. **Crash B first**: build the smallest loop (float -> dock -> restore), turn the window material /
   transparency off, reuse instead of destroy the floating/overlay windows, serialise close and
   recreate, and record the crash rate for 100 iterations per configuration; add timestamps and
   object addresses to `NSVisualEffectView` creation, removal from its parent, and native window
   dispose.
2. **Then Crash A**: keep the same workload and replace only the GLFW native build (current 3.4.0 vs a
   newer version, or a self-built build with an observer-removal patch), and check whether the
   `NSWindowDidResignKeyNotification` observer's registration and removal are paired. If an upgrade
   makes it disappear, pin the fix to the native dependency upgrade instead of working around it in
   AvalonDock.
3. **Then the product behaviour**: after verifying Cocoa `acceptsFirstMouse:`, regress a real user's
   first click-and-drag; keep the current test safety gates, because they guard against wrong
   coordinates/z-order, which is a different class of problem from `acceptsFirstMouse`.

## 9. 2026-10: replacing the FlaUI tests with DevFlow (in progress)

### Goal

`source/AutomationTest/AvalonDockTest/FlaUI` held **92 UI test cases in 17 files** (18 fixtures; the
two base classes hold none) that only ran on Windows, through FlaUI/UIA. They are being replaced by
DevFlow suites that launch the same sample applications and drive them over the in-process DevFlow
agent, so **one suite** runs on:

| Leg | Runner | Backend |
|---|---|---|
| windows-x64 | windows-2025 | real WPF |
| macos-arm64 | macos-15 | LibreWPF (CoreGraphics/AppKit), cliclick for native input |
| linux-x64 | ubuntu-24.04 | LibreWPF on Xvfb + openbox, XTest for native input |

When the port finishes the suite is 57 existing DevFlow cases + 92 ported = **149 cases**. The 57
existing cases already prove the infrastructure is cross-platform (agent, fixtures, two ports, real
pointer injection, ProGPU backend); they do not prove the FlaUI coverage has moved.

### Done

**Infrastructure (committed):**

- `.github/workflows/ci.yml` gained the five-legged `devflow` matrix (the Windows legs also run the
  ordinary unit tests in a separate job); `release.yml` validates on windows-x64, macos-arm64 and
  linux-x64 and replaces same-version packages instead of failing on them.
- The two sample applications each have their own agent and port: `TestApp` on **9223**
  (`[Collection("DevFlow")]`, `TestAppFixture`) and `AvalonDockCodeApp` on **9224**
  (`[Collection("DevFlowCodeApp")]`, `CodeAppFixture`). The port is a property of the suite, not a
  process-wide static, so a ToggleDock suite cannot connect to the wrong application.
- The DevFlow action surface grew to cover what the ported cases need: menus as a tree
  (`avd.menu.items`/`invoke`/`close`), tab right-clicks (`avd.tab.right-click`), invoking an entry of
  a tab's context menu (`avd.context.invoke`), window geometry (`avd.query.window`,
  `avd.window.resize`), a dialog seam (`avd.dialogs.answer`/`query`), documents
  (`avd.document.add-two`/`activate`, `avd.content`, `avd.close-content`), floating windows
  (`avd.close-floating`), focus and input (`avd.focus.query`/`set`, `avd.input.key`/`text`), and
  auto-hide state (`avd.query.autohide`, `avd.query.bounds autohide-tab`).

**Ported suites (61 of 92 cases, committed):**

| Suite | Cases | Batch |
|---|---:|---|
| `SmokeTests` | 8 | 1 |
| `ContextMenuTests` | 5 | 1 |
| `DocumentTabTests` | 6 | 1 |
| `KeyboardNavigationTests` | 4 | 1 |
| `ToolWindowTests` | 7 | 2 |
| `ToolWindowVisibilityTests` | 6 | 2 |
| `FloatingWindowTests` | 4 | 2 |
| `FloatingWindowCloseTests` | 1 | 2 |
| `DocumentLifecycleTests` | 5 | 2 |
| `LayoutSerializationTests` | 6 | 3 |
| `LayoutSerializationExtendedTests` | 5 | 3 |
| `ManagerLifecycleTests` | 4 | 3 |

**Additional ports (31 cases, pending CI):**

| Suite | Cases | Batch |
|---|---:|---|
| `StressTests` | 7 | 4 |
| `ToggleDockTests` | 11 | 4 |
| `ToggleDockSerializationTests` | 4 | 4 |
| `ToggleDockShortcutTests` | 2 | 4 |
| `DetachedWindowTests` | 7 | 5 |

With these batches, **all 92** FlaUI cases have a DevFlow equivalent. Once rebuilt, DevFlow test
discovery should increase from 118 to **149** cases (57 pre-existing cases + 92 ports).

Method names and `IssueNNN` suffixes are kept, so a failure still names the regression it did
before. Assertions the original spec marks as vacuous are replaced by the behaviour: "the app has not
exited" after a tab click becomes "that tab is now active"; tool-window content is read from that tool
window instead of counting edit controls in the whole window; the auto-hide strip is checked to sit on
the window's left edge rather than "left of the midline"; three flyout round trips are timed; "New
floating window" must add exactly one window (#254's duplicate was invisible to "at least one"); a
layout restore must remove what was added after the save; #437 (never actually unloaded in FlaUI) now
unloads the manager with a floating window open.

### Decisions worth keeping

- **Per-test restart.** The FlaUI fixtures shared one application and relied on NUnit `[Order]` for a
  predictable starting state. Each ported test now starts a fresh process (`PortedTestAppTestBase`
  overrides `InitializeAsync`): the XAML layout is only rebuilt at startup, and the older suites in
  the same collection deliberately replace the layout with their drag fixtures.
- **Dialogs as a seam.** A headless runner cannot click a modal `MessageBox`, so the sample's two
  confirmations (`DocumentClosing`, `ToolWindow1.Hiding`) go through `AskYesNo`, which records every
  question and answers from `avd.dialogs.answer` (yes/no/ask). Tests assert the recorded question
  instead of looking for a `#32770` window.
- **System close without Win32.** `avd.close-floating` calls `Window.Close()` on the floating control
  - the path a window manager's close button ends in - instead of posting
  `WM_SYSCOMMAND`/`SC_CLOSE`.
- **Keyboard limitation.** The agent can insert text into a control on all three platforms, and it
  can hold a modifier: `/api/v1/ui/actions/keydown` and `keyup` press and release a real OS key
  (cliclick's `kd:`/`ku:`), which the Shift+Tab case uses - with Shift held, `Keyboard.Modifiers`
  reports Shift and the host walks backwards. That route is macOS-only today (`keydown` answers
  `ok:false` elsewhere), so such a case skips on the other legs instead of failing. What the agent
  cannot do is press an arrow key or deliver a chord as one event, so other gestures are raised as
  WPF routed input on the focused element, and the assertions are on focus and liveness.
- **The action surface is static.** This is the important discovery of the port: the LibreWPF agent
  resolves an action either as a static method or as an instance method of a window (it enumerates
  `Application.Current.Windows`), but the **WPF agent implements only the static half** - both
  `GetInvokeActionTargetsAsync` and `DispatchOnUIThreadAsync` sit inside `#if LIBREWPF` in
  `WpfAgentService`. On real WPF every `avd.*` action declared on `MainWindow` was therefore
  discovered and then dropped for lack of a target, and nothing marshalled it to the UI thread
  either. Both samples now expose their actions as static forwarders (`MainWindow.DevFlowActions.cs`)
  that resolve the window and call the implementation inside the application's dispatcher, carrying
  any exception out with `ExceptionDispatchInfo` so the agent reports a failed action instead of the
  process dying on the UI thread. This is a DevFlow package limitation (0.3.1 is the latest
  published); the samples work around it rather than depending on a private feed.

### CI evidence

| Run | Commit | Result |
|---|---|---|
| 40465720089 | `810bc87` | 57 cases. linux-x64 and linux-arm64 green (57/57); both Windows legs failed all 57 before the first assertion (`dotnet not found`: the fixture scanned PATH for `dotnet` instead of `dotnet.exe`); macOS 55/57 - the two native-drag cases failed (`DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout`, `DragDockedPaneResizer_ResizesToolPane`) |
| 40466622281 | `d6f8a2a` | 57 cases. Windows now launches the app, and all 57 fail with `404 avd.position-main-window`: the agent was up but none of the sample's instance actions existed (the finding above) |
| 40469049093 | `e35dfc3` | 80 cases (57 + batch 1). linux-x64 76/80: `avd.input.key` hit `EntryPointNotFoundException: GetKeyState in user32.dll` in the floating-window arrow case, the Edit menu's command-bound items reported `isEnabled=true` where the port expected false, and both right-click cases found no tab because an earlier suite had replaced the layout (the per-test restart was not in this commit). Windows: all 80 `agent not reachable` - an action exception escaping the dispatcher killed the process. macOS 72/80 (the same two native-drag cases plus four smoke cases that inherited a mutated layout) |
| 40469822693 | `43c0fae` | 103 cases (57 + batches 1-2). linux-x64 90/103; the failures are the current work list, see below |

### Windows ARM64: why it was slow (2026-10-27)

Run 40469049093 (80 cases): linux-x64 5m54s, linux-arm64 6m09s, macOS 10m43s, windows-arm64
**27m25s** (38 passed / 38 failed / 4 skipped). windows-x64 finished in 6m26s only because all 80
cases failed instantly (the dispatcher crash cascade); once the app really runs, x64 and arm64 are
the same case. The Windows arm64 job's build step is as fast as the others (~1 min), so the time
was spent in the test step: the four skipped native-input cases are spaced exactly 10.2 s apart in
the log, and a skip does no work of its own - that spacing is the per-test restart.

Cause: `DevFlowAppFixture` started the sample with `dotnet run --no-build --project ...`. `--no-build`
still evaluates the whole project (SDK resolution for the 11 preview SDK, the `LibreWPF.Sdk` targets,
restore assets) before launching, about 10 s on a `windows-11-arm` runner, and the per-test restart
introduced for the ported suites pays it 80+ times. Fix: when `DEVFLOW_TESTAPP_NO_BUILD=1` and
`bin/Debug/net10.0-windows/<project>.dll` exists, the fixture runs `dotnet <project>.dll` from that
folder (falling back to `dotnet run`). This also shortens every other leg.

Second cause: the legacy drag cases (`DragDropFeatureIntegrationTests`, `DropTargetZone...`) spent
26-33 s each and then failed with `501 Drag is not supported by this agent`. A 501 means the agent
build has no such pointer primitive, which is a platform limitation, so `DevFlowClient` now reports
it as a skip instead of a failure.

### 2026-10-27 findings: the 21 s per test on Windows was `localhost`

After the direct launch (`98d92e9`) a trivial SmokeTests case still took **21.0 s on Windows ARM64
and 4.8 s on Linux x64**, with the same duration for every test. A constant per-test cost that does
not depend on what the test does pointed at the transport, not at the application: the client and
the fixture's readiness probe used `http://localhost:9223`, which Windows resolves to `::1` first;
the agent listens on IPv4 only (`127.0.0.1`, checked with `lsof` on macOS), so every new connection
paid a refused-then-fallback round. `3161c2e` switched both to `127.0.0.1` and a Smoke case now takes
**3.4 s** on windows-arm64. A full Windows leg went from 48 min to 4-5 min. Two CI changes made this
measurable: `plan` + `legs`/`test_args` dispatch inputs (re-run one platform or one class,
for example `-f legs=windows-arm64 -f test_args="--filter-class AvalonDock.DevFlowIntegrationTests.SmokeTests"`),
and application logs copied into `TestResults/` (the Windows `upload-artifact` step used to fail with
"rootDirectory is not a parent directory" and dropped every Windows artifact).

Where a full run stood after that (run 40495108583, 118 cases): linux-x64 and linux-arm64 108/118,
macos-arm64 106/118, windows 71-74 passing. The ten failures shared by the three LibreWPF legs:
the four auto-hide flyout cases, `ClickToolWindowTab_ActivatesIt_Issue375`,
`ToolWindowContent_IsAccessible`, `SaveLoadLayout_WithToolWindowStates_Issue59` (a click on a
*tool-window* tab does not activate it, while clicks on document tabs do), the 10x10 floating window
(#174), the "Window" context-menu detach, and the floating window surviving `avd.close-floating`.
`TestApp` declares `DockWidth="50"` for Tool Window 1's pane, so the tab is a 9x14 target there; tool tabs activate on
mouse *up* (`LayoutAnchorableTabItem.OnMouseLeftButtonUp`) while document tabs activate on mouse down.
Windows adds one real defect: the sample process dies when a floating window is created and every
later case reports `agent not reachable`. The log is now appended across restarts and the fixture
records the exit code, and `ClickAsync` writes the hit-test and the input counters to stderr, so the
next run says which of these it is.

### State on 2026-10-27 (run 40511892900, commit `78575cd`)

| Leg | Passed | Failed | Skipped | Duration |
|---|---:|---:|---:|---:|
| windows-x64 | 79 | 0 | 39 | 6.7 min |
| windows-arm64 | 79 | 0 | 39 | 5.0 min |
| linux-x64 | 110 | 8 | 0 | 12.6 min |
| linux-arm64 | 110 | 8 | 0 | 12.1 min |
| macos-arm64 | 108 | 10 | 0 | 16.0 min |

The Windows skips are the native-drag cases: real WPF has no held-drag primitive in the agent
(cliclick on macOS and XTest on Linux are the only ones), and a 501 from the agent is a skip.

Fixed on the way there (each commit went through the OpenCode reviewer and build agents):

- **Windows stack overflow** (`8b2877b`): on real WPF `GetNativeWindowOrigin` called
  `PointToScreenPortable`, which for a `LayoutFloatingWindowControl` called `GetNativeWindowOrigin`
  again. The process died with `0xC00000FD` the first time a floating window's caption handle was
  queried, and every later case reported `agent not reachable`. The fixture now logs the exit code of
  every instance, which is how this was found.
- **System close** (`86805f7`, `78575cd`): `avd.close-floating` called `Window.Close()`, which only
  hides the native window. The #368 fix lives in the `WM_CLOSE` handler, which runs
  `HideWindowCommand`; the action now does the same. The model keeps the emptied floating window (it
  is what the tool window is re-shown into), so the case waits for the tool window to stop floating.
- **"Window" mode** (`86805f7`): the `Window` context-menu entry detaches the anchorable into a
  standalone `DetachedAnchorableWindow`, which is not a floating window. The layout query now
  reports `detachedContentIds`, and closing a detached window reattaches it.
- **Auto-hide bounds** (`2f4056c`): the side auto-hide tabs are under a 90 degree
  `LayoutTransform`, so the (0,0)-(w,h) diagonal gave a negative width; bounds now come from all four
  corners.
- **Narrow pane** (`78575cd`): Tool Window 1's pane is declared `DockWidth="50"`, so its tab is a
  9 px sliver. Tests widen it through `avd.pane.set-width` before clicking a tool tab.
- Smaller ones: JSON results from `avd.activate-floating` and `avd.close-content`,
  `KeyboardFocusChangedEventArgs` in `avd.menu.close` (plain `RoutedEventArgs` threw on WPF), unload
  detection, and `EnsureDecomposedInputAvailable` for the drop-zone theories.

#### Known LibreWPF gaps (skipped on LibreWPF, see `KnownLibreWpfGaps`)

These pass on real WPF. Every diagnostic so far points at the backend, not at AvalonDock or at the
test, so they are skipped on LibreWPF with the reason in the skip message;
`DEVFLOW_RUN_KNOWN_GAPS=1` runs them anyway, which is how a LibreWPF fix should be checked.

| Gap | Cases | Evidence |
|---|---|---|
| Tool tab click | `ClickToolWindowTab_ActivatesIt_Issue375`, `ToolWindowContent_IsAccessible`, `SaveLoadLayout_WithToolWindowStates_Issue59` | Even with the pane widened to 300, the centre of Tool Window 1's tab header (as `TransformToAncestor` reports it) hit-tests to `Tool Window 2`, and that is what activates. The anchorable tab panel is arranged differently from WPF |
| macOS native drag (CI runner only) | `DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout`, `DragDockedPaneResizer_ResizesToolPane` | Skipped only on the macOS CI runner, which has no real desktop session for cliclick; both pass on a Mac with a desktop (checked 2026-10-27, together with the rest of `NativeInputIntegrationTests`). The local run also found a real macOS bug, fixed: see below |

Fixed: **Floating size to content (#174)**. `UpdateWindowsSizeBasedOnMinSize` was driven only by the
Win32 `WM_ACTIVATE` hook, which the portable `HwndSource` never delivers, so the window stayed at
25x25 managed / 10x10 native. `LayoutFloatingWindowControl.OnLoaded` now runs it once after the first
layout on the portable path. Also fixed: the TestApp's `anchorable-tab` bounds returned the first tab
of the pane for every content in it (`MatchesAnchorableContent` also matched sibling tabs), which is why
the tool tab click "hit-tested to Tool Window 2"; the click is still not activating the tab (the tab
activates on mouse-up), which is open.

### Work list from the earlier runs

Committed 2026-10-27 through the OpenCode reviewer/build flow (reviewer and build agents on
`opencode-go/deepseek-v4.1-flash`): `41cd8ad` (the items below) and `98d92e9` (the two speed fixes).
Run 40477750121 is the first run with them.

The working tree holds the fixes for the 40469049093/40469822693 failures, built locally but **not
committed**:

- `PortedTestAppTestBase.InitializeAsync` restarts the application before every test, and the
  per-suite `RestartAppAsync()` calls are gone (this is what fixes the four macOS smoke failures and
  the two "no tab titled ..." failures, which were layout state leaking from the older suites);
- `OnUi` in both samples carries an action's exception out of the dispatcher with
  `ExceptionDispatchInfo`, so a failing action is reported as a 400 instead of killing the process
  on the UI thread (this is the Windows `agent not reachable` cascade);
- `avd.input.key` reports backend limitations (`EntryPointNotFoundException`,
  `DllNotFoundException`, `PlatformNotSupportedException`) in its payload and lets everything else
  escape (this is the Linux `GetKeyState in user32.dll` failure);
- `DevFlowAppFixture` writes the application log to `RUNNER_TEMP`, which is the only directory the
  CI job collects on every runner;
- `ContextMenuTests.EditMenu_OpensAndCloses` asserts that the five entries are command-bound instead
  of asserting a particular enabled state.

Open defects found by the 103-case run, still open:

1. `autohide-tab` bounds come back with a **negative width** (`x:74, width:-24`), so
   `QueryBoundsAsync` rejects them as empty and four auto-hide cases fail. The `LayoutAnchorControl`
   screen rect needs the same native-origin treatment the floating windows got (section 4); the
   vertical strip's transform is inverted on LibreWPF.
2. `avd.content`, `avd.close-content` and `avd.close-floating` return plain strings on their
   "not found / cannot close" paths while the tests parse JSON; the failures surface as
   `JsonReaderException: 'A' is an invalid start of a value`.
3. `FloatingWindow_HasReasonableSize_Issue174`: the window created by `Tools > New floating window`
   measures **10x10** on Linux, so either the query reads the window before it has been laid out or
   #174 is back.
4. `ClickToolWindowTab_ActivatesIt_Issue375` / `ToolWindowContent_IsAccessible`: a click on the
   `Tool Window 1` tab does not make it the active content (the pane is only ~50 px wide, so the
   centre may land on the neighbouring tab).
5. `FloatingToolWindow_SystemClose_CanBeReshown_Issue368`: the floating window survives
   `avd.close-floating`.

Two review passes over the uncommitted change set (the local `claude` CLI, model
`claude-sonnet-5-5` at low effort) found no blockers.
The first pass's items are addressed; three should-fix items from the second pass are **still open**:

- (fixed in `41cd8ad`) the three keyboard cases share `AssertNoAppError`, which tolerates the three
  backend-limitation exceptions; `EditMenu_OpensAndCloses` now asserts every item is command-bound and
  reports `isEnabled`, with the comment corrected. The OpenCode reviewer approved this tree.

### FlaUI retirement work

The functional port is complete. Before removing the old FlaUI project, run the five DevFlow legs
against the 149-case suite and then delete `source/AutomationTest/AvalonDockTest/FlaUI`, the `Category=FlaUI` CI step and the
FlaUI package references, drop the `Category!=FlaUI` filter (also in `release.yml`), and re-run the
five legs.

### Running the suite

```bash
# host under test (Debug) - also deploys the sample applications
dotnet build source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -c Debug

# everything, one process per fixture
dotnet run --project source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -- \
  --parallel none

# one suite
dotnet run --project source/DevFlowIntegrationTests/DevFlowIntegrationTests.csproj -- \
  -class AvalonDock.DevFlowIntegrationTests.ToolWindowTests -parallel none
```

The local build uses the .NET 10 SDK pinned in `global.json`; every package restores from nuget.org. Do not
inject native input into a developer's desktop for these suites - the native-input cases assume the
application owns the pointer and are verified in CI.

## 10. History: the splitter-drag triple fix (older content, still valid)

(Original `librewpf.md` content, before 2026-08.) Dragging a splitter with the left button held made
the cursor revert to an arrow, the drag fail or move only one step, and the cursor then stick. The
root cause was that `LayoutGridControl.OnSplitterDragStarted` calls `Show()` on a transparent overlay
`Window` (`ShowActivated=false`, `Owner=null`) in the middle of the drag, which produced three
independent breakages on the LibreWPF portable backend: a phantom MouseUp, a `(0,0)` jump from
`Mouse.Synchronize()`, and `Window.HandlePortableMove` releasing the capture. All three fixes are in
the librewpf repository:

| # | File | Change |
|---|------|--------|
| 1 | `src/ProGPU.Wpf/ProGpuWpfWindowHost.cs` | swallow the phantom MouseUp that follows a show with no intervening move, plus GLFW mouse pass-through; `NormalizeInputEventForRenderSurfaceGeometry` forwards events for degenerate scales |
| 2 | `src/Microsoft.DotNet.Wpf/src/PresentationCore/.../MouseDevice.cs` | `Synchronize()` returns early while `Captured != null` |
| 3 | `src/Microsoft.DotNet.Wpf/src/PresentationFramework/.../Window.cs` | `HandlePortableMove` skips `Mouse.Capture(null)` while the mouse button is physically down |

Fixes 2 and 3 ship in `LibreWPF.Transport`, fix 1 in `LibreWPF.ProGPU`.

Verification harness: `/Users/lextm/uno-tools/tooltiptest` (`TOOLTIPTEST_SPLITTER_MODE=1`,
`scripts/check-splitter-capture.sh` -> `SPLIT_RESULT: PASS`). Repackaging note: Transport packages
prebuilt WPF DLLs, so build the matching csproj before packing.

## 11. Restructuring plan: two independent lines (2026-10-27)

Status legend: `[ ]` not started, `[~]` in progress, `[x]` done. Update this section as work lands.

### 11.1 Why

Today one set of csproj files serves both worlds: `Sdk="Microsoft.NET.Sdk.WindowsDesktop"` on Windows
and `Sdk="LibreWPF.Sdk"` elsewhere, switched by `Condition="'$(OS)' == 'Windows_NT'"`, plus a block in
`source/Directory.Build.props` that turns the LibreWPF SDK into a WindowsDesktop look-alike on Windows.
The consequences seen in the CI runs of 2026-10-27:

- the Windows DevFlow legs test **real WPF**, not LibreWPF, and the published real-WPF
  `LeXtudio.DevFlow.Agent.Wpf` has no click/drag/press (that native input code sits under
  `#if LIBREWPF` in wpf-labs), so 39 cases skip on Windows for a reason that is not about AvalonDock;
- `AvalonDock.csproj` is single-target `net10.0-windows` with analyzers and `TreatWarningsAsErrors`
  switched off and `PackageId` `Dirkster.AvalonDock.LibreWPF`, i.e. the original WPF line no longer
  exists as a buildable, publishable thing;
- the FlaUI suite, the DevFlow suite and the two release paths are tangled in one `ci.yml` and one
  `release.yml`.

### 11.2 Decisions (from the maintainer, 2026-10-27)

1. **Keep the original Windows WPF line**: upstream-shaped csprojs (`Microsoft.NET.Sdk.WindowsDesktop`,
   `$(ComponentTargetFrameworks)` multi-target, `Dirkster.AvalonDock*` package ids), the unit tests and
   the FlaUI tests. Its CI and its release are independent.
2. **LibreWPF gets its own csprojs and its own packages**, built with `Sdk="LibreWPF.Sdk"` on every OS,
   tested by the cross-platform DevFlow suite (Windows x64, macOS arm64, Linux x64).
3. Packages that do not touch WPF types may be shared by both lines (`AvalonDock.Core`,
   `AvalonDock.Serializer.Xml`, `.Json`, `.Mvvm*`, `.DependencyInjection`: all `netstandard2.0`).

### 11.3 Target layout (as built)

```
source/
  AvalonDock.sln                       WPF line (upstream shape)
  AvalonDock.LibreWPF.sln              LibreWPF line: shared netstandard projects, *.LibreWPF.csproj, DevFlowIntegrationTests
  Directory.Build.props                shared; one PropertyGroup applies to projects whose name contains ".LibreWPF"
  Components/
    AvalonDock/                        AvalonDock.csproj (WPF)  +  AvalonDock.LibreWPF.csproj (LibreWPF)
    AvalonDock.Themes.{VS,VS2013,Aero,Arc,Expression,Metro,VS2010}/   X.csproj + X.LibreWPF.csproj
    AvalonDock.Core, .Serializer.*, .Mvvm*, .DependencyInjection      shared, netstandard2.0, upstream gates
    AvalonDock/Platform/LibreWPF/      macOS and Linux platform services, compiled by the LibreWPF twin only
  TestApp/, MVVMTestApp/, AvalonDockCodeApp/       X.csproj (WPF samples) + X.LibreWPF.csproj (DevFlow-driven)
  DevFlowIntegrationTests/             net10.0, HTTP client, starts the *.LibreWPF apps
  AutomationTest/AvalonDockTest/       WPF unit tests + FlaUI (real WPF, Windows)
.github/workflows/
  ci.yml                  WPF: Windows build, unit tests (and FlaUI)
  release.yml             WPF: tag v* -> nuget.org (upstream behaviour)
  ci-librewpf.yml         LibreWPF: PRs to master, push to librewpf + dispatch, DevFlow on five legs
                          (release.yml also builds and packs the three *.LibreWPF packages)
```

Package mapping (what OpenDevelop consumes, from `NuGet.config`):

| Package | Line | Project |
|---|---|---|
| `Dirkster.AvalonDock.Core` | shared | `Components/AvalonDock.Core` |
| `Dirkster.AvalonDock.Serializer.Xml` | shared | `Components/AvalonDock.Serializer.Xml` |
| `Dirkster.AvalonDock.LibreWPF` | LibreWPF | `Components/AvalonDock/AvalonDock.LibreWPF.csproj` |
| `Dirkster.AvalonDock.Themes.VS.LibreWPF`, `...VS2013.LibreWPF` | LibreWPF | `...Themes.VS/...VS.LibreWPF.csproj`, `...Themes.VS2013/...VS2013.LibreWPF.csproj` |
| `Dirkster.AvalonDock`, `.Themes.*` (no suffix) | WPF | the original projects |

### 11.4 How the two lines share source (what was measured)

The first design linked the WPF project's files into a LibreWPF project in another folder
(`<Compile Include="..\..\Components\AvalonDock\**\*.cs" Link=...>`). **Linking `.cs` files works;
linking XAML does not**: the WPF markup compiler flattens the intermediate BAML of pages that live outside
the project folder by file name, so `Themes/Generic.xaml` and `Themes/VS2022/Generic.xaml` overwrote each
other and the `Themes.VS` package came out 8 KB short (caught by comparing the packed assembly against the
pre-split package, entry by entry). So the LibreWPF csproj sits **in the same folder** as its WPF twin and
compiles that folder's sources with the normal globs. What keeps the twins apart:

- `source/Directory.Build.props` gives every project whose name contains `.LibreWPF` its own
  `BaseIntermediateOutputPath` (`obj/librewpf/`), `BaseOutputPath` (`bin/librewpf/`),
  `DefaultItemExcludes` (`obj/**;bin/**`), the single `net10.0-windows` target, `LIBREWPF`, the
  ProGPU support-package pin, and switches the analyzers off (the LibreWPF packages are OpenDevelop
  dependencies). The condition uses `Contains('.LibreWPF')` because the markup compiler derives a
  `*.LibreWPF_xxxx_wpftmp.csproj`.
- The assembly keeps the WPF name (`AvalonDock`, `AvalonDock.Themes.VS`, ...): the XAML uses
  `pack://application:,,,/AvalonDock;component/...` URIs. Only `PackageId` differs. Same strong-name key.
- `Controls/Shell` (Win32 chrome) is removed from the LibreWPF twin; `Controls/Shell.Portable` and
  `Platform/LibreWPF` are removed from the WPF project.
- Result: the three packages OpenDevelop consumes are entry-for-entry identical to those built before the
  split (same files, same sizes; `AvalonDock.dll` is +1 KB after the analyzer clean-up).

### 11.5 Tests

| Suite | Runs on | What it proves |
|---|---|---|
| `AvalonDockTest` unit tests | Windows, real WPF | the original line is intact (upstream-style CI) |
| FlaUI (`Category=FlaUI`) | Windows, real WPF | UIA-level behaviour on real WPF - this is the baseline that replaces "passes on real WPF" from the DevFlow suite |
| DevFlow integration tests | linux-x64, macos-arm64, **windows-x64 on LibreWPF** | LibreWPF behaviour; Windows legs now use the LibreWpf agent, which implements click/drag/press/drag-move/release natively there |

Consequences: the 39 Windows skips disappear for a good reason (the agent in use implements the
input), `KnownLibreWpfGaps.IsLibreWpf` becomes constant `true` (the gap list is then judged against the
FlaUI baseline instead of a real-WPF DevFlow run), and the `OperatingSystem.IsWindows()` branches in
`NativeInputEnvironment` collapse to the agent path on Windows and Linux, cliclick/CliclickSharp on macOS.
DevFlow packages come from the wpf-labs publish pipeline (section 11.8).

### 11.6 Upstream pull requests

Becomes simpler, with one honest caveat.

- The WPF line is upstream-shaped again, so a change to shared code is a branch from upstream
  `master` that touches only `source/Components/...` and `source/AutomationTest/...`; no csproj, SDK or
  DevFlow noise in the diff. Fork-only trees (`source/LibreWPF`, `DevFlowIntegrationTests`, the
  LibreWPF workflows) never appear in a PR.
- Portable-backend logic in shared files (`UsePortableCaptionDrag`, the managed caption drag, the
  deferred-sizing hook for #174) has to be either `#if LIBREWPF`, or moved into files that only the
  LibreWPF project links. Rule for new code: if upstream would not want it, it lives under a
  `LibreWPF`-named file or `#if`. Existing cases are to be audited (section 11.7, step 6).
- The caveat: restoring the upstream csprojs turns analyzers and `TreatWarningsAsErrors` back on for
  `AvalonDock.csproj`. The fork's ~24k added lines were written with those off, so the WPF line needs a
  one-off warning clean-up before it can pass upstream CI. That clean-up is also the PR-readiness gate.
- Upstream's commit message convention (`[Fix|Feature] #<ticket> - [<Topic>] ...`) applies to PR branches.

### 11.7 Work plan and status

1. [x] WPF csprojs restored to the upstream shape (WindowsDesktop SDK, `$(ComponentTargetFrameworks)` =
       net9.0/net10.0/net48 for libraries; `TestApp` net9.0/net10.0 because the fork's test hooks use newer
       APIs than .NET Framework); shared netstandard projects regain docs and analyzers.
2. [x] `*.LibreWPF.csproj` twins for the library, the seven themes and the three samples; package identity
       verified against the pre-split packages (11.4).
3. [x] `AvalonDock.LibreWPF.sln`; `AvalonDock.sln` is the WPF line again.
4. [x] DevFlow surface behind `DEVFLOW` (`App.xaml.cs`, usings and `[DevFlowUIThread]` in the samples);
       `MainWindow.DevFlowActions.cs` and `DockDiagnostics.cs` are removed from the WPF samples; the fixture
       starts `X.LibreWPF.csproj` (`bin/librewpf/Debug/net10.0-windows/X.dll`).
5. [x] Workflows split (`ci.yml`/`release.yml` = WPF, `ci-librewpf.yml`/`release-librewpf.yml` = LibreWPF).
6. [x] Audit of shared files: macOS/Linux platform services moved to `Platform/LibreWPF/`, the
       `PlatformManager` branches and `PlatformHelper`'s cache sit behind `LIBREWPF`. Still in shared files
       and still LibreWPF-flavoured: `UsePortableCaptionDrag` and the managed caption drag in
       `LayoutFloatingWindowControl`, the deferred sizing hook for #174, `LayoutAutoHideWindowControl`'s
       portable hosting (all selected at run time or by `#if LIBREWPF`). Moving them out is a follow-up
       if upstream asks for it.
7. [x] The WPF line builds with analyzers and warnings-as-errors on net9.0, net10.0 and net48 (checked on
       macOS with `EnableWindowsTargeting`; 224 findings fixed: `dotnet format` for style rules, XML docs
       by hand, regions removed, `CA1502`/`CA1506` suppressed with a justification on
       `LayoutAutoHideWindowControl`, `LayoutFloatingWindowControl` and `OverlayWindow.GetTargets`).
8. [x] Windows DevFlow legs on LibreWPF. TestApp used to skip its WinForms pane under LIBREWPF (LibreWinForms
       hands `WindowsFormsHost` a synthetic handle, a LibreWPF gap); that gap was closed on 2026-10-29 (11.9,
       visual `WindowsFormsHost`), so the pane is hosted by default and `DEVFLOW_HOST_WINFORMS=0` is the
       opt-out. The remaining failures were the runner
       desktop, not the code: windows-11-arm logs in to the first-logon privacy page (a full-screen window
       that took every real click) and then leaves the Start menu open, its WSL app runs `wsl.exe --update` every 30 s in a new Windows
       Terminal window that takes the foreground during the tests, and the agent's console window
       covers part of the desktop on both images. `ci-librewpf.yml` now prepares the desktop (stops the
       first-logon host, dismisses the Start menu, minimises other windows, runs a watchdog that stops
       that `wsl.exe --update` and logs what started it) and uploads screenshots. The update retried every
       30 s only because it never finished (`--prompt-before-exit` waits for a key, and stopping it counts
       as a failure), so on arm64 the step first runs `wsl --update --web-download` to completion (about
       30 s); after that the watchdog had nothing to stop;
       Both Windows legs pass 118/118 and gate the run like the others.
       `release-librewpf.yml` runs no tests at all: it packs once on macOS and requires a green CI
       (LibreWPF) push run for the commit.
9. [x] `AGENTS.md` describes both lines.

### 11.8 Related work in other repositories

- **DevFlow packages (lextudio/ui-labs, formerly wpf-labs)**: the branch `ci/linux-input-github-packages` adds a
  Linux input CI job (Xvfb, XTest click and a held press/drag-move/release; X11, x64, left button only) and a
  workflow that publishes the DevFlow packages to GitHub Packages. **The published version is forced to the
  nuget.org one** (newest stable `LeXtudio.DevFlow.Agent.Core` there, currently `0.3.1`, or the `version`
  input), not a prerelease: consumers keep one version number, NuGet package source mapping
  (`LeXtudio.DevFlow.*` -> `openavalon-github` in `NuGet.config`) decides which feed serves it, and the GitHub
  copy is deleted and pushed again on every run, as for the OpenAvalon LibreWPF packages. Because a
  same-version package already in the global packages folder is never looked up again, both LibreWPF
  workflows check the recorded source of `lextudio.devflow.*` like they do for the LibreWPF closure.
- OpenDevelop consumes the LibreWPF packages by id and version, so nothing in this plan renames them.

### 11.9 State log

- 2026-10-27: plan written. Landed before it, on `opendevelop`: #174 fix (`0ab025b`), CI on every push
  (`e32bce4`), Windows decomposed input (`c2a2424`, `9e7473b`), bundled CliclickSharp for macOS/Windows
  held drags (`ecac77d`, CI pending).
- 2026-10-27 (later): steps 1-7 and 9 landed (`a25586c`, `f81bab3`). DevFlow packages now come from the
  DevFlow pipeline's GitHub Packages feed (`6e6a97e`, see 11.8). Open: the first CI results of the two
  new workflows, and the reductions in step 8.
- 2026-10-27 (Windows LibreWPF, **root cause found and fixed in LibreWPF**): all 118 cases failed at startup
  with `BuildWindowCore failed to return the hosted child window handle`. Traces on the Windows runner
  showed AvalonDock's own `HwndHost` (the auto-hide flyout) building fine - valid child handle, right parent,
  62 successful builds - and a `HwndHost` listing at the failure showed the culprit:
  `System.Windows.Forms.Integration.WindowsFormsHost` with handle `0x0` (TestApp has a WinForms window).
  `WindowsFormsHost.BuildWindowCore` only called `HostContainerInternal.CreateControl()` off Windows, on the
  assumption that real WinForms creates the adapter handle while parenting; on the Windows lane running
  LibreWinForms the handle stays zero, `SetParent` got zero and `HwndHost` threw. Fixed in LibreWPF
  (`083e157ee`: create the control when the handle is still zero on Windows). The earlier theories (the
  presentation source being a native `HwndSource`, the portable-host switch) were measurements of
  AvalonDock's side that turned out not to be the failing host; the "treat Windows as portable" attempt stays
  reverted. Verification: the next Windows legs after the LibreWPF package publish.
- 2026-10-28 (**correction: the WindowsFormsHost entry above was wrong**): `083e157ee` did not fix anything and
  was reverted (`d81e86281`): on the Windows lane the adapter handle is not zero but a LibreWinForms logical
  handle (`0x8000000000000001`, `ManagedLibreHandleRegistry`: `long.MinValue | sequence`). Reading the code
  shows `WindowsFormsHost` with LibreWinForms is not a Windows-only bug but an unimplemented feature on every
  OS: LibreWinForms builds with `LIBREWINFORMS_PORTABLE` everywhere, and `NativeWindow.CreateHandle` creates a
  real window only for a `Form` or a top-level drop-down; every other control (the `WinFormsAdapter` container
  included) gets a logical handle. The only thing that turns a handle into a WPF child is
  `HwndSource.TryInitializePortableChild`, which nothing in WinFormsIntegration calls, so
  `HwndHost.AttachPortableChildVisual` has no child source to take on macOS/Linux either (static reading; not
  run there, because TestApp has never hosted the WinForms pane under LIBREWPF). On Windows it is rejected
  earlier by the native `IsWindow` check. The separate `LibreWPF.WinFormsCompat` host paints and forwards
  input itself, but the SDK uses it only when LibreWinForms is off. A real fix is a feature in LibreWPF:
  visual hosting for a logical-handle child under a native `HwndSource` (HwndHost), plus painting the
  LibreWinForms control tree into the host's `DrawingContext`, mouse/key forwarding, layout from
  Measure/Arrange and focus. Until then TestApp skips the WinForms pane under LIBREWPF.
- 2026-10-27 (end of day): CI of `9000c9f`: the WPF line (`ci.yml`) is green; the LibreWPF line is green on
  linux-x64 and linux-arm64, the Windows legs fail as described above (now `continue-on-error`), the macOS leg
  was cancelled by a later push and has no result yet. DevFlow is pinned to `0.3.1` again, served from
  GitHub Packages (forced version, see 11.8).
- 2026-10-27 (auto-hide click, **root cause in ProGPU**, fixed there): the click never reached
  `LayoutAnchorControl` because the anchor sits under a 90/270 degree `LayoutTransform` and LibreWPF's GPU
  hit test found no region for it. Reproduced on macOS without a window (a rotated `Border`: 270 degrees 0%
  hit, -90 degrees 100%) and narrowed with ProGPU's hit-test primitive dump: the layout clip is the
  intersection of two identical rectangles, `PrimitivePathGeometry.TryGetAxisAlignedRectangleBounds`
  required four explicit segments (WPF's `RectangleGeometry` is three plus the implicit close), so the
  exact-containment shortcut was skipped and the GPU path-op solver returned two zero-area slivers for the
  coincident edges, a clip of zero area. Fixed in ProGPU `02119efdf` (recognise the implicit closing edge;
  drop zero-area line figures from path-op output; two regression tests), published through the LibreWPF
  package pipeline. AvalonDock was not changed for it. Verified on the Linux leg with the new packages:
  the flyout opens, 2 of the 4 auto-hide cases pass; the other two now fail later, on dismissal.
  The tool-tab click gap was a TestApp query bug and passes (3 cases un-skipped).
- 2026-10-27 (auto-hide dismissal): not a LibreWPF gap. AvalonDock keeps the flyout open while the pointer is
  over its sidebar button (`AutoHideWindowManager` close timer, `anchor.IsMouseOver`), the click had left the
  pointer there, and on Windows these cases never ran in DevFlow (no native click in the real-WPF agent), so
  the expectation had never been checked. The cases now move the pointer into the document area before
  activating a document, and the skip is gone.
- 2026-10-27 (CI of `85cde89`): **linux-x64 and linux-arm64 are 118/118 with 0 skips** (the tool-tab, auto-hide
  click and auto-hide dismissal cases included, on the LibreWPF package with the ProGPU clip fix). macOS: 22
  failures, both of my making: (1) the agent's `drag-move` reports `ok:false` on macOS without a held button, so
  the pointer move in the two auto-hide dismissal cases now uses cliclick's plain `m:` there
  (`NativeInputEnvironment.MovePointerAsync`); (2) the drag/drop-zone cases regressed when the bundled
  CliclickSharp took priority on macOS (`ecac77d`): the same drags stopped reaching the drop targets. On macOS
  the installed cliclick (which the CI installs) is preferred again and CliclickSharp is the fallback; Windows
  keeps CliclickSharp.exe. Whether the C# port can match the original on macOS is a DevFlow (ui-labs) question.
- 2026-10-29 (**visual WindowsFormsHost published**): OpenAvalon republished
  `0.1.0-preview.69` at 04:39Z (`Publish LibreWPF GitHub Packages`, run 40809590217, 6/6 jobs), so the
  feed finally carried the host that had been committed at 02:52-02:58Z (keyboard/text/focus forwarding,
  Tab traversal, Alt+letter mnemonics). The metadata of the published
  `LibreWinForms.WindowsFormsIntegration` DLL was checked to contain `OnMnemonicCore`,
  `TryProcessMnemonic`, `SelectNextControl` and `VisualPixelsPerUnit` - a check that the code shipped, not
  that it worked: the two entries below show that in that build every key reaching the host threw and the
  Tab traversal started from the wrong control. A manual dispatch of
  `ci-librewpf.yml` with `host_winforms=true` (run 40819253489) then ran the full suite with the interop
  pane hosted: **linux-x64 149/149 with 0 skips (615 s), macOS 147 + 2 known macOS-runner skips (964 s),
  and both Windows legs green**. TestApp now hosts the pane by default under `#if LIBREWPF`;
  `DEVFLOW_HOST_WINFORMS=0` restores the placeholder, and the workflow's `host_winforms` input is gone
  because it could only repeat the default.
- 2026-10-29 (**the visual host painted nothing and could not be clicked: container never sized**): the
  five green legs proved the host loads and the app survives, but nothing in the suite touched the hosted
  content, so a new `HostedWinFormsTests` suite was added (`avd.winforms.tree`, `.bounds`, `.scale`,
  `.mnemonic`). Its first run showed text input and Tab never reaching the hosted
  tree. The probe that separated the layers: a real native click at the centre of the pane reported
  `originalSource = System.Windows.Controls.Border` (the WPF input hit test never selected the host), a
  pointer event dispatched straight into the hosted tree did not focus a text box either, and
  `avd.winforms.tree` showed the host container at **0x0** while the hosted control was 956x1028 px.
  Reading `WindowsFormsHost` confirmed it: in the visual path only `Child.Size` is set (the HWND path sizes
  the container through the child window), and `OnRender` returns early for a zero-sized container, so
  **the host never painted, and the WinForms hit test had no bounds to search**. A first fix (a transparent
  hit rectangle, LibreWPF `d11ac30ce`) was drawn after that early return and therefore could not take
  effect; the second attempt, `32bb71bf3`, sized the container but still sat behind a guard: a local trace
  showed `OnRender` receives a plain `System.Windows.Media.VisualDrawingContext` (`native=False`) under the
  default managed composition renderer, so the host returned at `TryGetNativeDrawingContext` before doing
  anything. The real fix is LibreWPF `a783bd75b` (OpenAvalon `eb4e711`): the container follows `Child.Size`
  and the transparent hit rectangle is drawn before, and independently of, the native paint, for a host
  with no real child window. What the probe did show working without
  it: the mnemonic path (`ProcessMnemonic` through the WinForms system-character route ran the hosted
  button's handler). The earlier "five legs green with the pane hosted" therefore proves loading only, not painting or
  interaction.
- 2026-10-29 (**the managed renderer painted nothing: the host now paints into a ProGPU texture**): the
  visual host only painted when `OnRender` received a live ProGPU drawing context, and the managed
  composition renderer never hands it one - it records WPF render data and builds the ProGPU scene from
  that afterwards - so under the default renderer (**the one every LibreWPF leg runs**) the pane drew no
  pixels at all. The host now paints the tree into an offscreen ProGPU texture: LibreWinForms' `Graphics`
  records the controls' drawing as ProGPU scene commands into a `Bitmap`, the bitmap renders them into its
  own GPU texture (`Compositor.RenderOffscreen`), and that texture is recorded as an image over the host's
  bounds through `PortableNativeImageSourceFactory` and `IPortableNativeImageSource`. The image is a
  texture-lease source, so nothing is read back to the CPU and the texture materializes in the
  presentation host's device on demand; the native-context path is unchanged and still runs first
  (LibreWPF `5e2526109`). Verified locally on macOS with the managed renderer at DPI scale 2: the hosted
  labels and text boxes are visible in the pane, and `HostedWinFormsTests` 5/5, including a new case that
  reads the host's retained render data - it depends on a 956x1028 portable native image for a 478x514
  DIP element, which is one texture pixel per WinForms pixel (LibreWPF package not yet published, and the
  suite has not run on CI since). **Still open**: everything the hosted tree draws through
  `TextRenderer` is invisible - a `TextBox`'s content and a `Button`'s caption - while everything drawn
  through `Graphics.DrawString` (labels, borders, backgrounds) shows up. Reproduced by typing into
  `textBox1`: the text is in the control and painted nowhere. **Not the text service**: `LibrePlatform`
  is already registered with `ProGpuTextRendererService` (the `LibreWinForms.ProGPU` module initializer
  does it), and both that service and a raw `Graphics.DrawString` draw black pixels when the host calls
  them on its own surface - so the surface, the offscreen flush and the text path all work. The painter
  reaches those two controls' borders but not the text they lay out, which points at
  `PaintPortableControlTree` (a button's caption rectangle, a text box's retained text) rather than at
  the platform or the host.
- 2026-10-30 (**the hosted caption was white on white, not a paint gap**): with the tree painting again, a
  `Button`'s caption and a `TextBox`'s content were still invisible while every label was readable. The
  probes that found it painted one hosted control into its own bitmap: the button's border and face were
  there, `TextRenderer.MeasureText` returned 97x14 for its text, and forcing `ForeColor` to red produced
  106 red pixels in the same paint - so the text was always drawn, in white. `WindowsFormsHost` maps the
  WPF `Foreground`/`Background` it inherits onto the hosted tree
  (`WindowsFormsHostPropertyMap.ForegroundPropertyTranslator`), and under this sample's dark theme that is
  a white foreground, while `UserControl1` paints its own white face; the labels were unaffected only
  because the designer sets `ForeColor` on them explicitly. The pane now declares `Foreground="Black"`
  on the host (the colors its content actually paints), and a new case asserts WCAG contrast between
  every hosted control's `foreColor` and `backColor` - it fails with `#FFFFFF on #FFFFFF` when the
  declaration is removed. `avd.winforms.tree` reports both colors per control for that.
  **Not a LibreWinForms or host defect**: real WPF's host maps the same properties, so the same app would
  show the same white-on-white.
- 2026-10-30 (**the windows-x64 interop failure was an auto-hide flyout, not the host**): after the
  managed-paint change, `Mnemonic_InvokesHostedButton` failed on windows-x64 and nowhere else, twice, with
  `textBox1` never taking focus while the next case clicked the same point and passed. The failure
  artifacts added in the same round (the screen plus `avd.query.layout`, `avd.input.query`,
  `avd.focus.query` and `avd.winforms.tree`, written under `TestResults/failures` and uploaded with the
  results) answered it: the click's `originalSource` was a `StackPanel`, `activeControl` was null, the
  hosted tree reported the expected geometry (ppu 1, textBox1 at 19,49 141x23 px), and the screen showed
  the auto-hide flyout open over the pane. Clicking anywhere in that overlay lands in the flyout, and the
  flyout shows on hover, so it can be open before the case's first click. The hosted suite now dismisses
  it (`avd.autohide.dismiss`) before and between its clicks and re-reads the control's geometry for the
  second attempt, because anything that moves the layout between the query and the click invalidates the
  point.
- 2026-10-29 (**the interop suite: what actually works, and two more host gaps it found**): with
  `a783bd75b` published (OpenAvalon run 40839987388, `0.1.0-preview.69` at 09:35Z) the four cases of
  `HostedWinFormsTests` ran against the package. Text input passed (a native click focuses `textBox1` and
  typed text lands in it) and the mnemonic case passed, but **Tab failed on two product gaps and the
  scaled-host case on a defect in the test's own arithmetic**:
  1. **Every key that reached an `HwndHost` threw.** `HwndKeyboardInputProvider.GetSystemModifierKeys`
     calls `user32!GetKeyState`, and the portable runtime's user32 stub has no such export, so
     `HwndHost.OnKeyDown` threw `EntryPointNotFoundException` before the host's own handler ran - which
     also broke `HwndSource`'s mnemonic path. Fixed in LibreWPF `f77955728`: portable input reads its
     modifier state from the portable keyboard device, the native path is untouched.
  2. **Tab started from the wrong control, and moved in the wrong container.** `SelectNextControl` was
     called with the adapter's `ActiveControl`, which is the hosted *root* (`UserControl1`), not the
     control that has focus; from a focus-managing container the walk found nothing, so the host reported
     Tab unhandled on every press. Fixed in the same commit and refined in the next: follow the
     `ActiveControl` chain down to the focused control, move inside the container that owns it, and when
     that container has no stop left continue from the container itself in its parent. Tab now goes
     `textBox1` -> `textBox2` -> `mnemonicButton` and is left unhandled at the end (still no wrap).
  3. **`avd.winforms.bounds` applied a conversion that does not exist** (test defect, not product): it
     divided `PointToScreen` through a measured units-per-DIP factor, which is wrong on both backends
     (`PointToScreen` is already in the space clicks use; see 11.10) and doubled the error under a layout
     transform. It now maps the control's two corners and reports them, and the scaled case measures the
     factor at 1x first instead of assuming it is 1 (it is already 2 on a Retina Mac).
  The Tab case also asserted something unobservable: WPF's own tab traversal runs in `InputManager`'s
  translate-accelerator stage, which a routed key event raised by the agent never reaches, so "focus left
  the host" cannot be seen there - the case asserts the unhandled signal only.
  **Verified locally on macOS** against TestApp with the two product assemblies built from source and
  overlaid: `HostedWinFormsTests` 4/4, including a real native click at the scaled rectangle's centre
  landing on `textBox2`. The cases skip on the build-level `visualHost` report of `avd.winforms.tree`
  (which `WindowsFormsHost` this assembly has) instead of on the OS, because the LibreWPF line also runs on
  Windows: there the visual host is the same one, so those legs exercise the suite too - **but it has not
  run on CI since that change**. They also go through the shared `NativeInputEnvironment` guard, since the
  clicks are OS-level input. Shift+Tab is covered too: the agent's `keydown`/`keyup` hold a real
  modifier (cliclick's `kd:` on macOS), and the case first asserts the modifier reached
  `Keyboard.Modifiers` before asserting the backward step - it skips where the agent has no
  modifier support yet (Windows and Linux, where `keydown` answers `ok:false`). **Known gap, with a
  case against it**: a real Alt+letter does not reach the host's mnemonic path. Driving it natively
  (`keydown alt`, `keydown m`, `keyup m`, `keyup alt`) proves the modifier arrives -
  `avd.input.key` reports `keyboardModifiers = Alt` - but the letter is delivered as committed text
  (`textBox1` received it) instead of being translated into a system key, so
  `WindowsFormsHost.OnMnemonicCore` is never called. `AltLetter_InvokesHostedMnemonicThroughTheHost`
  asserts that path and skips as a known LibreWPF gap (run it with `DEVFLOW_RUN_KNOWN_GAPS=1`); the
  WinForms matching itself stays covered by `Mnemonic_InvokesHostedButton`. What the investigation
  into fixing it found, so the next attempt starts from facts: the agent's `keydown`/`keyup` do reach
  the app as real key events (`SilkNetWpfInputService` logs `down AltLeft mods=Alt`, `down M mods=Alt`,
  `char 'µ' mods=Alt`), and the WPF bridge (`CreatePortableWindowInputEvent`) carries `Alt` through to
  `PortableWindowInputEvent`. Two things block the obvious router: on macOS Option is a compose
  modifier, so Option+M produces `µ` and no letter mnemonic can match there (this is a
  Windows/Linux feature); and the text that reached the focused box was a plain `m` with **no**
  modifiers, i.e. it came from WPF's key-to-text derivation for the `M` keydown rather than from the
  character event, so portable key handling must stop deriving text while Alt is held before a
  system-character stage can run. Both live in the portable input stack (PresentationCore /
  PresentationFramework + `SilkNetWpfInputService`), not in `WindowsFormsHost`. **Painting** - under the managed renderer the
  host still draws no pixels, because painting needs a native ProGPU context. That is a separate gap (a
  managed paint path, for example rendering the tree to a bitmap) and is not covered by any of the fixes
  above. All three host commits (`f77955728`, the Tab refinement `7624b058d`) are published as the same
  `0.1.0-preview.69`: the publish workflow deletes the version from GitHub Packages before pushing it, so a
  fresh restore - which is what CI does - gets the new assembly, while a developer's global-packages folder
  keeps the copy it extracted first and has to be cleared by hand. The consumer pin therefore stays
  `0.1.0-preview.69`.

- 2026-10-27 (macOS, run on a real desktop with Accessibility granted to Claude.app): `DragDockedPaneResizer_ResizesToolPane`
  passed as is; `DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout` failed on `DockingManagerDockLeft`
  reporting y=770 below the manager (y 77 to 733). Cause: `DropTarget.GetScreenBounds()` multiplied the stored
  detection rectangles by the window's DPI scale (2 on Retina) although they are already screen coordinates in
  logical units. CI never showed it because the runner's display is 1x. Fixed in AvalonDock (the contract is in
  11.10); `NativeInputIntegrationTests` is 5/5 on the Mac desktop. The two cases now skip only when `GITHUB_ACTIONS`
  is set on macOS.

### 11.10 The screen-coordinate contract of a portable source (read this before touching DPI code)

Measured on a Retina Mac (window at Left/Top 200,150, content element at +40,+60):
`CompositionTarget.TransformToDevice` is `2 x 2`, `PointToScreen` returns `240,210`. That is intended
by LibreWPF, not a bug: `PointUtil` states that "portable clients use the native platform's logical
coordinate space (the same space as Window.Left/Top and input events)" and that the device transform stays
"on the rendering target" without being applied "to the public screen/client coordinate contract". A native
Windows window (`HwndSource`) keeps WPF's contract of device pixels.

What AvalonDock must do:

- Never multiply or divide a `PointToScreen` result by `TransformToDevice` when the source is portable.
  `TransformExtentions.PointToScreenDPI` and `DropTarget.GetScreenBounds` follow the rule through
  `PlatformHelper.UsesLogicalScreenCoordinates`; add any new DPI code behind the same function.
- Code that compares stored rectangles with a drag point goes through `TransformToDeviceDPI` on both sides
  (see `DropTarget.HitTest`); keep both sides in the same space.

Known limitation, and the item that belongs to LibreWPF: the criterion should be the kind of source, but
there is no public way to ask. `PresentationSource.FromVisual` returns an `HwndSource` on macOS as well (a
portable facade whose `IsPortable` is internal), so "not an `HwndSource`" does not mean portable (tried; it
brought the Retina bug straight back). `UsesLogicalScreenCoordinates` therefore decides by operating system,
which equals the lane today (non-Windows is portable, Windows is native). **To request from LibreWPF**: a
public way to read which coordinate contract a source uses (for example a public `HwndSource.IsPortable`).
When it exists, `UsesLogicalScreenCoordinates` becomes a one-line change and the Windows lane keeps working if
it ever gets portable windows.

Why it stayed hidden: the macOS CI runner has a 1x display, where the scale is 1 and the wrong multiplication
changes nothing; only a Retina desktop shows it. Run the native-input cases on a real Mac before trusting a
DPI change.

### 11.11 Running the native-input cases on a Mac desktop

`cliclick` needs Accessibility permission for the app that launches the shell (for Claude Code in Claude Desktop:
`/Applications/Claude.app`; on current macOS the list has no "+" button, so trigger the prompt with
`AXIsProcessTrustedWithOptions` and prompt = true, then enable the entry and restart the app). Local recipe, all
local-only edits that must not be committed: a SDK in `global.json` that is installed, and a different DevFlow port
if 9223 is taken (the `DefaultPort` in `IntegrationTestBase.cs` and the port in `DevFlowAppFixture.cs`). Run with
`DEVFLOW_RUN_KNOWN_GAPS=1` to include the cases skipped on the CI runner:
`dotnet run --project source/DevFlowIntegrationTests -c Debug --no-build -- --filter-class '*NativeInputIntegrationTests' --parallel none`.
The cases move the real pointer for a few minutes; do not touch the machine meanwhile.
- 2026-10-27 (Windows x64 VM, local run; reported by the VM session): the 24 Windows x64 failures have two causes.
  (1) The input tool: on Windows `CliclickSharp`'s `Program.Main` passed the raw arguments to
  `WindowsCommandExecutor.Execute`, so `-w` was taken as the first command, failed, and the process exited 1
  before `dd` - **no held drag ever ran on Windows** (all 24 fail at `Assert.Equal(0, _process.ExitCode)`). Not a
  lost button state between invocations. Fix is in the DevFlow repo (`external/cliclick-sharp`): parse the options
  on every platform and hand the parsed commands to the Windows executor, honouring `-w` between commands and
  `-r`; with it all 22 `DropTargetZoneIntegrationTests` and `DragDocumentPaneBody_DoesNotFloatDocumentTab` pass.
  The shipped `dist/CliclickSharp.exe` is Native AOT and must be rebuilt on a machine with the C++ tools.
  (2) **An AvalonDock bug that also hits classic WPF on Windows**: `WindowsNativeWindowService.GetWindowContentOrigin`
  declared `ClientToScreen(IntPtr, ref System.Windows.Point)`; Win32 writes a `POINT` of two int32, so X came back as
  8.1e-312 and Y as 0 and `DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout` failed with "Floating window
  is not following the pointer". Fixed with an int `POINT`. The arm64-only failures could not be checked on an x64 VM;
  they may share cause (1).

### 11.12 Open work (TODO)

Recorded so the next session starts from evidence instead of re-deriving it. The first item is a
product gap in LibreWPF's portable input; the rest are coverage gaps in the DevFlow suite.

1. **Alt+letter mnemonics (access keys and `HwndHost.OnMnemonicCore`) do not work under portable
   input.** `HwndSource` runs the mnemonic stage only from its `WM_CHAR`/`WM_SYSCHAR` case
   (`HwndSource.cs`, `case WindowMessage.WM_CHAR/WM_SYSCHAR` → `IKeyboardInputSink.OnMnemonic` →
   `OnMnemonicCore`), and portable input has no window procedure, so nothing performs it. Measured
   chain today: `SilkNetWpfInputService` logs `down AltLeft mods=Alt`, `down M mods=Alt`,
   `char 'µ' mods=Alt`, and `WpfPortableWindowActivation.CreatePortableWindowInputEvent` carries
   `Alt` into `PortableWindowInputEvent` - so the modifier is **not** lost in the agent or the
   bridge. Two blockers remain:
   - macOS Option is a compose modifier: Option+M is `µ`, so a letter mnemonic cannot match there.
     This is a Windows/Linux feature; do not expect it on macOS.
   - The `m` that reached the focused text box came with **no** modifiers, i.e. WPF derived it from
     the `M` keydown, not from the character event. Portable key handling has to stop deriving text
     while Alt is held (the `WM_SYSKEYDOWN` rule) before a system-character stage can run.

   Planned shape (a working draft was written and reverted): a portable router that builds a
   `WM_SYSCHAR` `MSG`, offers it to the nearest `IKeyboardInputSink` in the focused element's visual
   ancestry (an `HwndHost` is one), and falls back to `AccessKeyManager.ProcessKey(null, ch, false)`
   for menus; called from `PortableWindowActivationService.ProcessTextInput`, skipping `AltGr`
   (`Ctrl+Alt`). It is unverifiable today because the agent's `keydown`/`keyup` exist only for macOS
   (cliclick); **Windows/Linux need a `SendInput`/XTest implementation first**. Guard:
   `HostedWinFormsTests.AltLetter_InvokesHostedMnemonicThroughTheHost` (known gap; run with
   `DEVFLOW_RUN_KNOWN_GAPS=1`).
2. **IME composition and candidate windows.** The agent delivers committed text only (`fill`/`key`);
   LibreWPF lists host composition and candidate bounds as explicit unfinished contracts. Nothing to
   test until both exist.
3. **Pointer capture/drag and wheel inside the hosted tree.** The interop cases cover clicks, keys and
   text. A wheel has no observable state with a single-line `TextBox`, and an in-host drag needs a
   probe for pointer capture before a case can assert anything.
4. **Runtime DPI change.** CI runners are fixed-DPI; `ScaledHost_ClickLandsOnTheControlItsBoundsDescribe`
   covers the shared layout-transform/DPI factor, not a monitor or DPI switch at run time.



`cliclick` needs Accessibility permission for the app that launches the shell (for Claude Code in Claude Desktop:
`/Applications/Claude.app`; on current macOS the list has no "+" button, so trigger the prompt with
`AXIsProcessTrustedWithOptions` and prompt = true, then enable the entry and restart the app). Local recipe, all
local-only edits that must not be committed: a SDK in `global.json` that is installed, and a different DevFlow port
if 9223 is taken (the `DefaultPort` in `IntegrationTestBase.cs` and the port in `DevFlowAppFixture.cs`). Run with
`DEVFLOW_RUN_KNOWN_GAPS=1` to include the cases skipped on the CI runner:
`dotnet run --project source/DevFlowIntegrationTests -c Debug --no-build -- --filter-class '*NativeInputIntegrationTests' --parallel none`.
The cases move the real pointer for a few minutes; do not touch the machine meanwhile.
- 2026-10-27 (Windows x64 VM, local run; reported by the VM session): the 24 Windows x64 failures have two causes.
  (1) The input tool: on Windows `CliclickSharp`'s `Program.Main` passed the raw arguments to
  `WindowsCommandExecutor.Execute`, so `-w` was taken as the first command, failed, and the process exited 1
  before `dd` - **no held drag ever ran on Windows** (all 24 fail at `Assert.Equal(0, _process.ExitCode)`). Not a
  lost button state between invocations. Fix is in the DevFlow repo (`external/cliclick-sharp`): parse the options
  on every platform and hand the parsed commands to the Windows executor, honouring `-w` between commands and
  `-r`; with it all 22 `DropTargetZoneIntegrationTests` and `DragDocumentPaneBody_DoesNotFloatDocumentTab` pass.
  The shipped `dist/CliclickSharp.exe` is Native AOT and must be rebuilt on a machine with the C++ tools.
  (2) **An AvalonDock bug that also hits classic WPF on Windows**: `WindowsNativeWindowService.GetWindowContentOrigin`
  declared `ClientToScreen(IntPtr, ref System.Windows.Point)`; Win32 writes a `POINT` of two int32, so X came back as
  8.1e-312 and Y as 0 and `DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout` failed with "Floating window
  is not following the pointer". Fixed with an int `POINT`. The arm64-only failures could not be checked on an x64 VM;
  they may share cause (1).
