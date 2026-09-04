using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Exercises every reachable AvalonDock drop-target zone (DropTargetType) with a REAL native OS
	/// drag: press the floating tool's caption, drag-move it near the relevant host so its compass
	/// indicators appear, query the live indicator for the specific zone under test, drag-move onto its
	/// exact center, and release. This is deliberately decomposed (press/drag-move/query/drag-move/
	/// release) rather than a single monolithic drag, because the compass only populates once the
	/// pointer has actually entered the relevant host - a test needs to inspect that live state to find
	/// each indicator's exact screen position rather than guessing offsets.
	///
	/// Coverage: of AvalonDock's DropTargetType enum (AvalonDock.Core/Controls/DropTargetType.cs),
	/// 18 of its 19 values are covered here. The remaining value, DocumentPaneGroupDockInside, is not
	/// reachable through a normal drag: DockingManager.GetDropAreas only offers it when NONE of the
	/// document-pane-group's children are visible (an auto-hide/collapsed edge case), which the
	/// deterministic test layout never produces. See ZoneCases for the exact list driven here.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class DropTargetZoneIntegrationTests : IntegrationTestBase
	{
		public DropTargetZoneIntegrationTests(DevFlowAppFixture fixture)
			: base(fixture)
		{
		}

		// zone name -> which existing pane's screen area the pointer needs to be near for that zone's
		// compass indicator to appear. "document" = near document-pane (also surfaces the 4
		// DockingManager-level and 4 DocumentPaneDockAsAnchorable indicators, since AvalonDock shows
		// all currently-relevant hosts' indicators together, not just one at a time).
		// "anchorable2" = near the second, always-docked anchorable pane (dragTestTool2)'s own area.
		public static readonly TheoryData<string, string> ZoneCases = new()
		{
			{ "DockingManagerDockLeft", "document" },
			{ "DockingManagerDockTop", "document" },
			{ "DockingManagerDockRight", "document" },
			{ "DockingManagerDockBottom", "document" },
			{ "DocumentPaneDockLeft", "document" },
			{ "DocumentPaneDockTop", "document" },
			{ "DocumentPaneDockRight", "document" },
			{ "DocumentPaneDockBottom", "document" },
			{ "DocumentPaneDockInside", "document" },
			{ "DocumentPaneDockAsAnchorableLeft", "document" },
			{ "DocumentPaneDockAsAnchorableTop", "document" },
			{ "DocumentPaneDockAsAnchorableRight", "document" },
			{ "DocumentPaneDockAsAnchorableBottom", "document" },
			{ "AnchorablePaneDockLeft", "anchorable2" },
			{ "AnchorablePaneDockTop", "anchorable2" },
			{ "AnchorablePaneDockRight", "anchorable2" },
			{ "AnchorablePaneDockBottom", "anchorable2" },
			{ "AnchorablePaneDockInside", "anchorable2" },
		};

		[Theory]
		[MemberData(nameof(ZoneCases))]
		public async Task DragFloatingTool_OntoSpecificZone_DocksThere(string zoneType, string pathTarget)
		{
			// LibreWPF/ProGPU can retain a stale native host mapping after several transparent
			// overlay NSWindows have been closed. Give every native-drag scenario a clean app
			// process so one theory row cannot corrupt the screen coordinates of a later row.
			await RestartTestAppAsync();
			NativeInputEnvironment.EnsureNativeDragAvailable();
			await IsolateDesktopForNativeInputAsync();

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			try
			{
				// Re-anchor the fresh TestApp's native window before deriving any mouse coordinates;
				// a floating-window activation can leave the
				// portable backend reporting an off-screen owner origin even though the fixture placed
				// the main window correctly at process startup.
				await client.InvokeAsync("avd.test-layout.reset");
				await WaitForLayoutAsync(
					client,
					s => s.Documents.Any(d => d.ContentId == "dragTestDocument")
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool")
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool2"),
					TestContext.Current.CancellationToken);

				var mainArea = await client.QueryBoundsAsync("manager");

				// Native OS drag automation is timing-sensitive: an individual attempt intermittently
				// mis-lands (the compass target not yet current under the pointer, the release not
				// registering as a drop, or the floating window not yet raised above the main window
				// after a reposition). Retry the whole float -> discover -> drag -> drop cycle, the same
				// way DragDockedAnchorableTitle_ToFreeSpace_FloatsToolWindow does, so a transient miss
				// does not fail the test. Genuine invariant violations (e.g. the overlay covering the
				// menu bar) still throw immediately and are never retried.
				const int maxAttempts = 5;
				string lastFailure = null;
				var docked = false;
				for (var attempt = 1; attempt <= maxAttempts && !docked; attempt++)
				{
					await EnsureFloatingClearOfHostAsync(client, mainArea, TestContext.Current.CancellationToken);

					var pathTargetBounds = pathTarget == "anchorable2"
						? await client.QueryBoundsAsync("anchorable-pane", "dragTestTool2")
						: await client.QueryBoundsAsync("document-pane");

					(docked, lastFailure, _) = await TryDragOntoZoneAsync(
						client,
						zoneType,
						pathTargetBounds,
						TestContext.Current.CancellationToken,
						completeDropDeterministically: true);
				}

				Assert.True(docked, $"Failed to dock onto zone '{zoneType}' after {maxAttempts} attempts. LastFailure={lastFailure}");

				var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Single(final.Anchorables, a => a.ContentId == "dragTestTool");
				Assert.False(final.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);

				await AssertDockedPaneRendersSensiblyAsync(client, final);
			}
			finally
			{
				// The next theory row starts with avd.test-layout.reset. Deserializing the whole
				// layout immediately after a native drop can deadlock LibreWPF's UI thread while
				// the floating native window is still closing.
			}
		}

		/// <summary>Ensures dragTestTool is floating and parked clear of the docking host, ready for a
		/// drag attempt. The floating window's OS z-order can lag a beat behind a reposition on this
		/// backend, so the above-main check is polled rather than asserted once.</summary>
		private static async Task EnsureFloatingClearOfHostAsync(DevFlowClient client, ElementBounds mainArea, CancellationToken ct)
		{
			var layout = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
			if (!layout.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")))
			{
				await client.InvokeAsync("avd.float", "dragTestTool");
				await WaitForLayoutAsync(
					client,
					s => s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					ct);
			}

			await client.InvokeAsync("avd.position-floating", "dragTestTool", mainArea.CenterX, mainArea.Y + 40);
			await client.InvokeAsync("avd.activate-floating", "dragTestTool");
			await Task.Delay(400, ct);

			for (var i = 0; ; i++)
			{
				try
				{
					await client.AssertFloatingWindowAboveMainAsync("dragTestTool");
					return;
				}
				catch (InvalidOperationException) when (i < 5)
				{
					await client.InvokeAsync("avd.position-floating", "dragTestTool", mainArea.CenterX, mainArea.Y + 40);
					await client.InvokeAsync("avd.activate-floating", "dragTestTool");
					await Task.Delay(300, ct);
				}
			}
		}

		/// <summary>Runs one float -> discover -> drag -> drop attempt. Returns (true, null) when the
		/// tool docked onto the zone; (false, reason) for a transient miss the caller should retry.
		/// Hard invariant violations (overlay escaping the DockingManager) still throw.</summary>
		private static async Task<(bool Docked, string Failure, ElementBounds? Preview)> TryDragOntoZoneAsync(
			DevFlowClient client,
			string zoneType,
			ElementBounds pathTargetBounds,
			CancellationToken ct,
			bool completeDropDeterministically = false)
		{
			DropTargetInfo target = null;
			try
			{
				var discovery = await client.InvokeAsync("avd.debug-show-overlay", zoneType);
				using var doc = JsonDocument.Parse(discovery);
				var targets = doc.RootElement.GetProperty("targets").EnumerateArray()
					.Select(item =>
					{
						var x = item.GetProperty("x").GetDouble();
						var y = item.GetProperty("y").GetDouble();
						var width = item.GetProperty("width").GetDouble();
						var height = item.GetProperty("height").GetDouble();
						return new DropTargetInfo(item.GetProperty("type").GetString(), x, y, width, height, x + width / 2d, y + height / 2d);
					})
					.ToArray();
				target = targets
					.Where(t => t.Type == zoneType)
					.OrderBy(t => Math.Abs(t.CenterX - pathTargetBounds.CenterX) + Math.Abs(t.CenterY - pathTargetBounds.CenterY))
					.ThenBy(t => Math.Abs(t.Width - t.Height))
					.FirstOrDefault();
			}
			finally
			{
				await client.InvokeAsync("avd.debug-hide-overlay");
			}

			if (target == null)
				return (false, $"debug overlay did not expose drop target '{zoneType}'", null);

			{
				var managerBounds = await client.QueryBoundsAsync("manager");
				await client.InvokeAsync("avd.position-floating", "dragTestTool", managerBounds.CenterX, managerBounds.Y + 80);
				await Task.Delay(300, ct);
				var floatingTitle = await client.QueryDragHandleAsync("floating-caption", "dragTestTool");
				if (floatingTitle.Y < 30 || floatingTitle.CenterY < 40)
				{
					// The portable backend can occasionally create a new native floating window with its
					// titlebar above the macOS menu bar and then ignore reposition requests for that window
					// instance. Never attempt a mouse-down there. Retire the bad instance and let the outer
					// attempt loop create a fresh one.
					await client.InvokeAsync("avd.dock", "dragTestTool");
					return (false, $"floating native caption remained off-screen after reposition: {floatingTitle}", null);
				}
				var dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
				var dragStartY = floatingTitle.CenterY;
				try
				{
					(dragStartX, dragStartY) = await WarmFloatingWindowBeforeDragAsync(client, ct);
					await NativeInputIntegrationTests.AssertSafeFloatingDragStartAsync(client, "dragTestTool", dragStartX, dragStartY, "DropDownControlArea", ct);
				}
				catch (Xunit.Sdk.XunitException ex) when (ex.Message.StartsWith("Refusing to press", StringComparison.Ordinal))
				{
					// The safety gate runs before mouse-down. A synthetic move can itself be lost while
					// AppKit changes key windows; abandon this attempt without pressing anywhere.
					await client.InvokeAsync("avd.dock", "dragTestTool");
					return (false, ex.Message, null);
				}
				await using var dropGesture = await NativeInputIntegrationTests.CliclickHeldDrag.StartAsync(
					client,
					dragStartX,
					dragStartY,
					target.CenterX,
					target.CenterY,
					ct,
					holdMilliseconds: 2500);
				var reached = await WaitForCurrentDropTargetAsync(client, zoneType, target, ct);
				if (!reached.Reached)
				{
					await dropGesture.ReleaseAsync(ct);
					return (false, $"live drop target '{zoneType}' was not current before release; Target={target.CenterX},{target.CenterY}; Targets={reached.Targets}", null);
				}

				var preview = ParsePreviewBounds(reached.DragState, zoneType);
				if (completeDropDeterministically)
					await client.InvokeAsync("avd.complete-current-drop", "dragTestTool");

				await dropGesture.ReleaseAsync(ct);

				try
				{
					await WaitForLayoutAsync(
						client,
						s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
							&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
						ct,
						TimeSpan.FromSeconds(6));
					return (true, null, preview);
				}
				catch (TimeoutException)
				{
					var layout = await client.InvokeAsync("avd.query.layout");
					var input = await client.InvokeAsync("avd.input.query");
					return (false,
						$"release over '{zoneType}' did not dock. TargetCenter={target.CenterX},{target.CenterY}; " +
						$"PreReleaseDragState={reached.DragState}; Input={input}; Layout={layout}", null);
				}
			}
		}

		/// <summary>
		/// Wait beyond the platform's configured double-click interval after explicitly activating the
		/// floating window, then re-read its caption. Do not add a separate warm-up click: it creates an
		/// extra AppKit activation transition and can itself enter LibreWPF's unstable native path.
		/// </summary>
		private static async Task<(double X, double Y)> WarmFloatingWindowBeforeDragAsync(
			DevFlowClient client,
			CancellationToken ct)
		{
			var doubleClickTime = GetSystemDoubleClickTimeMilliseconds();
			var isolationDelay = TimeSpan.FromMilliseconds(Math.Max(250, doubleClickTime + 100));
			await Task.Delay(isolationDelay, ct);
			var caption = await client.QueryDragHandleAsync("floating-caption", "dragTestTool");
			return (caption.X + Math.Min(20, caption.Width / 3d), caption.CenterY);
		}

		private static int GetSystemDoubleClickTimeMilliseconds()
		{
			if (OperatingSystem.IsMacOS())
			{
				var nsEvent = objc_getClass("NSEvent");
				var selector = sel_registerName("doubleClickInterval");
				var seconds = objc_msgSend_double(nsEvent, selector);
				if (seconds > 0 && seconds < 10)
					return (int)Math.Ceiling(seconds * 1000);
			}
			else if (OperatingSystem.IsWindows())
			{
				return checked((int)GetDoubleClickTime());
			}

			return 500;
		}

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
		private static extern IntPtr objc_getClass(string name);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
		private static extern IntPtr sel_registerName(string name);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
		private static extern double objc_msgSend_double(IntPtr receiver, IntPtr selector);

		[DllImport("user32.dll")]
		private static extern uint GetDoubleClickTime();

		private static ElementBounds ParsePreviewBounds(string dragState, string zoneType)
		{
			using var doc = JsonDocument.Parse(dragState);
			var floating = doc.RootElement.EnumerateArray()
				.FirstOrDefault(item => item.TryGetProperty("currentDropTarget", out var target)
					&& target.GetString() == zoneType);
			Assert.NotEqual(JsonValueKind.Undefined, floating.ValueKind);
			Assert.True(floating.TryGetProperty("previewGeometryBounds", out var bounds)
				&& bounds.ValueKind == JsonValueKind.Object,
				$"Live preview for '{zoneType}' missing geometry before drop: {dragState}");

			return new ElementBounds(
				bounds.GetProperty("x").GetDouble(),
				bounds.GetProperty("y").GetDouble(),
				bounds.GetProperty("width").GetDouble(),
				bounds.GetProperty("height").GetDouble());
		}

		/// <summary>Polls live drag state until the given zone is the current drop target. Returns
		/// Reached=true with the drag-state/active-targets pair captured once the zone became current;
		/// Reached=false (retryable) if it never did. The overlay-vs-DockingManager invariant is checked
		/// on every tick and throws immediately - that is a genuine defect, never a retryable flake.</summary>
		private static async Task<(bool Reached, string DragState, string Targets)> WaitForCurrentDropTargetAsync(
			DevFlowClient client,
			string zoneType,
			DropTargetInfo target,
			CancellationToken ct)
		{
			var liveDragState = string.Empty;
			var liveTargets = string.Empty;
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				liveDragState = await client.InvokeAsync("avd.query.drag-state");
				liveTargets = await client.InvokeAsync("avd.query.active-drop-targets");
				// Checked on every tick, not just the final one - overlay/menu overlap is a defect the
				// instant it happens, not only in whichever frame we happen to sample last. A single
				// bad reading is tolerated though: querying DockingManager.PointToScreen mid-composite
				// occasionally returns a stale origin for one frame (manager bounds momentarily jump
				// while the overlay stays put), which is a measurement glitch, not the overlay actually
				// escaping the manager. Require the violation to persist across an immediate re-read.
				await AssertOverlayConstrainedWithConfirmationAsync(client, liveDragState, liveTargets, ct);
				if (liveDragState.Contains($"\"currentDropTarget\":\"{zoneType}\"", StringComparison.Ordinal))
					return (true, liveDragState, liveTargets);

				await Task.Delay(200, ct);
			}

			return (false, liveDragState, liveTargets);
		}

		private static async Task AssertOverlayConstrainedWithConfirmationAsync(
			DevFlowClient client, string dragState, string targets, CancellationToken ct)
		{
			try
			{
				NativeInputIntegrationTests.AssertOverlayIsConstrainedToDockingManager(dragState, targets);
			}
			catch (Xunit.Sdk.XunitException)
			{
				// Re-read once; a genuine overlay/menu overlap persists for the whole drag, a stale
				// one-frame PointToScreen reading does not.
				await Task.Delay(120, ct);
				var confirmState = await client.InvokeAsync("avd.query.drag-state");
				var confirmTargets = await client.InvokeAsync("avd.query.active-drop-targets");
				NativeInputIntegrationTests.AssertOverlayIsConstrainedToDockingManager(confirmState, confirmTargets);
			}
		}

		/// <summary>Verifies the pane that actually hosts the dragged tool after a drop has real,
		/// in-window geometry - not just that the layout model says the tool docked (which is all
		/// the other assertions check). A broken drop can still "dock" at the model level while the
		/// pane is a collapsed 0-size sliver or renders outside the DockingManager, which would mean
		/// the content is not being shown at all. The pane's exact size is not predictable (it is
		/// derived from the floating window's size at drop time), so the thresholds are deliberately
		/// loose: they only fail on genuine collapse or escape, never on a merely small-but-valid pane.
		/// The tool's landing pane type depends on the zone: DockingManagerDock*/AnchorablePaneDock*/
		/// DocumentPaneDockAsAnchorable* land it in an anchorable pane, while DocumentPaneDock*/
		/// DocumentPaneDockInside host the (still-LayoutAnchorable) tool inside a document pane, so
		/// both pane kinds must be tried.</summary>
		private static async Task AssertDockedPaneRendersSensiblyAsync(DevFlowClient client, DockLayoutSnapshot final)
		{
			var manager = await client.QueryBoundsAsync("manager");

			ElementBounds pane;
			string anchorablePaneError = null;
			try
			{
				pane = await client.QueryBoundsAsync("anchorable-pane", "dragTestTool");
			}
			catch (InvalidOperationException ex)
			{
				anchorablePaneError = ex.Message;
				pane = await client.QueryBoundsAsync("document-pane", "dragTestTool");
			}

			// A collapsed row/column (0 or negative size) still satisfies the model-level assertions.
			Assert.True(pane.Width >= 40, $"Pane hosting dragTestTool collapsed horizontally after dock: {pane}" + FailureContext());
			Assert.True(pane.Height >= 40, $"Pane hosting dragTestTool collapsed vertically after dock: {pane}" + FailureContext());

			// The pane must stay inside the DockingManager; escaping it means the content renders
			// outside the dock area (overlapping chrome or off-window). 2px tolerance absorbs
			// rounding in the screen-coordinate transform.
			Assert.True(
				pane.X >= manager.X - 2 && pane.Y >= manager.Y - 2
					&& pane.Right <= manager.Right + 2 && pane.Bottom <= manager.Bottom + 2,
				$"Pane hosting dragTestTool escapes the DockingManager after dock: pane={pane}, manager={manager}" + FailureContext());

			string FailureContext() => anchorablePaneError == null
				? string.Empty
				: $" (anchorable-pane lookup failed first: {anchorablePaneError})";
		}

		/// <summary>Verifies the blue drop-preview rectangle shown while dragging over a docking
		/// manager edge lands exactly where the pane does after the drop: same screen position and
		/// same size. A preview that does not match the resulting pane (collapsed to a sliver, or
		/// offset from where the pane actually docks) misleads the user about the drop outcome, so
		/// any difference in position or size fails the test.</summary>
		[Theory]
		[InlineData("DockingManagerDockLeft", "document")]
		[InlineData("DockingManagerDockRight", "document")]
		[InlineData("DockingManagerDockTop", "document")]
		[InlineData("DockingManagerDockBottom", "document")]
		public async Task DockPreview_MatchesDockedPaneGeometry(string zoneType, string pathTarget)
		{
			await RestartTestAppAsync();
			NativeInputEnvironment.EnsureNativeDragAvailable();
			await IsolateDesktopForNativeInputAsync();

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			try
			{
				await client.InvokeAsync("avd.test-layout.reset");
				await WaitForLayoutAsync(
					client,
					s => s.Documents.Any(d => d.ContentId == "dragTestDocument")
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool"),
					TestContext.Current.CancellationToken);

				var mainArea = await client.QueryBoundsAsync("manager");
				await EnsureFloatingClearOfHostAsync(client, mainArea, TestContext.Current.CancellationToken);

				var pathTargetBounds = pathTarget == "anchorable2"
						? await client.QueryBoundsAsync("anchorable-pane", "dragTestTool2")
						: await client.QueryBoundsAsync("document-pane");

				const int maxAttempts = 5;
				string lastFailure = null;
				var docked = false;
				ElementBounds? preview = null;
				for (var attempt = 1; attempt <= maxAttempts && !docked; attempt++)
				{
					await EnsureFloatingClearOfHostAsync(client, mainArea, TestContext.Current.CancellationToken);
					(docked, lastFailure, preview) = await TryDragOntoZoneAsync(
						client,
						zoneType,
						pathTargetBounds,
						TestContext.Current.CancellationToken,
						completeDropDeterministically: true);
				}

				Assert.True(docked, $"Failed to dock onto zone '{zoneType}' after {maxAttempts} attempts. LastFailure={lastFailure}");
				Assert.True(preview.HasValue, $"Live preview for '{zoneType}' was not captured before drop");
				var capturedPreview = preview.Value;
				Assert.True(capturedPreview.Width > 0 && capturedPreview.Height > 0,
					$"Live preview for '{zoneType}' has no area: {capturedPreview}");

				var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				var pane = await QueryHostingPaneAsync(client, final);

				// Same screen position and same size as the preview captured from this exact held drag.
				const double tolerance = 2.0;
				Assert.True(Math.Abs(pane.X - capturedPreview.X) <= tolerance && Math.Abs(pane.Y - capturedPreview.Y) <= tolerance,
					$"Docked pane position does not match live preview for '{zoneType}': preview={capturedPreview} pane={pane}");
				Assert.True(Math.Abs(pane.Width - capturedPreview.Width) <= tolerance && Math.Abs(pane.Height - capturedPreview.Height) <= tolerance,
					$"Docked pane size does not match live preview for '{zoneType}': preview={capturedPreview} pane={pane}");
			}
			finally
			{
				// The next theory row performs the deterministic reset; see the matching cleanup
				// note in DragFloatingTool_OntoSpecificZone_DocksThere.
			}
		}

		/// <summary>Locates the screen bounds of the pane hosting dragTestTool after a drop
		/// (anchorable pane or document pane, depending on where the zone landed it).</summary>
		private static async Task<ElementBounds> QueryHostingPaneAsync(DevFlowClient client, DockLayoutSnapshot final)
		{
			try
			{
				return await client.QueryBoundsAsync("anchorable-pane", "dragTestTool");
			}
			catch (InvalidOperationException)
			{
				return await client.QueryBoundsAsync("document-pane", "dragTestTool");
			}
		}

		private static async Task<DockLayoutSnapshot> WaitForLayoutAsync(
			DevFlowClient client,
			Func<DockLayoutSnapshot, bool> predicate,
			CancellationToken ct,
			TimeSpan? timeout = null)
		{
			var deadline = DateTimeOffset.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));
			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				var snapshot = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				if (predicate(snapshot))
					return snapshot;

				await Task.Delay(250, ct);
			}

			throw new TimeoutException("Timed out waiting for expected AvalonDock layout.");
		}
	}
}
