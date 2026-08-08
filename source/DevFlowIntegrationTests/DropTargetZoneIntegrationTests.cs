using System;
using System.Linq;
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
			NativeInputEnvironment.EnsureNativeDragAvailable();
			await IsolateDesktopForNativeInputAsync();

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			try
			{
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
				const int maxAttempts = 3;
				string lastFailure = null;
				var docked = false;
				for (var attempt = 1; attempt <= maxAttempts && !docked; attempt++)
				{
					await EnsureFloatingClearOfHostAsync(client, mainArea, TestContext.Current.CancellationToken);

					var pathTargetBounds = pathTarget == "anchorable2"
						? await client.QueryBoundsAsync("anchorable-pane", "dragTestTool2")
						: await client.QueryBoundsAsync("document-pane");

					(docked, lastFailure) = await TryDragOntoZoneAsync(client, zoneType, pathTargetBounds, TestContext.Current.CancellationToken);
				}

				Assert.True(docked, $"Failed to dock onto zone '{zoneType}' after {maxAttempts} attempts. LastFailure={lastFailure}");

				var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Single(final.Anchorables, a => a.ContentId == "dragTestTool");
				Assert.False(final.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);

				await AssertDockedPaneRendersSensiblyAsync(client, final);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
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
					await Task.Delay(300, ct);
				}
			}
		}

		/// <summary>Runs one float -> discover -> drag -> drop attempt. Returns (true, null) when the
		/// tool docked onto the zone; (false, reason) for a transient miss the caller should retry.
		/// Hard invariant violations (overlay escaping the DockingManager) still throw.</summary>
		private static async Task<(bool Docked, string Failure)> TryDragOntoZoneAsync(
			DevFlowClient client,
			string zoneType,
			ElementBounds pathTargetBounds,
			CancellationToken ct)
		{
			var floatingTitle = await client.QueryDragHandleAsync("floating-caption", "dragTestTool");
			var dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
			var dragStartY = floatingTitle.CenterY;
			// Hover just inside the host pane's bottom-right corner during discovery: this surfaces the
			// host's compass while staying clear of the drop indicators themselves (they cluster around
			// the pane centre/edges), so the discovery-phase release does NOT land on a target and dock
			// the tool prematurely - it must stay floating for the real drag that follows.
			var discoveryX = pathTargetBounds.Right - 20;
			var discoveryY = pathTargetBounds.Bottom - 20;
			await NativeInputIntegrationTests.AssertSafeFloatingDragStartAsync(client, "dragTestTool", dragStartX, dragStartY, "DropDownControlArea", ct);
			var pressed = false;
			try
			{
				await client.PressAsync(dragStartX, dragStartY, ct);
				pressed = true;
				await Task.Delay(250, ct);
				await client.DragMoveAsync(discoveryX, discoveryY, ct);
				await Task.Delay(500, ct);

				DropTargetInfo target;
				try
				{
					target = await client.WaitForActiveDropTargetAsync(zoneType, ct, TimeSpan.FromSeconds(8));
				}
				catch (TimeoutException)
				{
					var lateTargets = await client.QueryActiveDropTargetsAsync(ct);
					target = DevFlowClient.PickPrimaryDropTarget(lateTargets, zoneType);
				}

				if (target == null)
				{
					await client.ReleaseAsync(discoveryX, discoveryY, ct);
					pressed = false;
					return (false, $"compass never showed drop target '{zoneType}' during discovery");
				}

				await client.ReleaseAsync(discoveryX, discoveryY, ct);
				pressed = false;

				var managerBounds = await client.QueryBoundsAsync("manager");
				await client.InvokeAsync("avd.position-floating", "dragTestTool", managerBounds.CenterX, managerBounds.Y + 80);
				await Task.Delay(300, ct);
				floatingTitle = await client.QueryDragHandleAsync("floating-caption", "dragTestTool");
				dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
				dragStartY = floatingTitle.CenterY;
				await NativeInputIntegrationTests.AssertSafeFloatingDragStartAsync(client, "dragTestTool", dragStartX, dragStartY, "DropDownControlArea", ct);
				await using var dropGesture = await NativeInputIntegrationTests.CliclickHeldDrag.StartAsync(
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
					return (false, $"live drop target '{zoneType}' was not current before release; Target={target.CenterX},{target.CenterY}; Targets={reached.Targets}");
				}

				await dropGesture.ReleaseAsync(ct);

				try
				{
					await WaitForLayoutAsync(
						client,
						s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
							&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
						ct,
						TimeSpan.FromSeconds(6));
					return (true, null);
				}
				catch (TimeoutException)
				{
					var layout = await client.InvokeAsync("avd.query.layout");
					var input = await client.InvokeAsync("avd.input.query");
					return (false,
						$"release over '{zoneType}' did not dock. TargetCenter={target.CenterX},{target.CenterY}; " +
						$"PreReleaseDragState={reached.DragState}; Input={input}; Layout={layout}");
				}
			}
			finally
			{
				if (pressed)
					await client.ReleaseAsync(discoveryX, discoveryY, CancellationToken.None);
			}
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
			NativeInputEnvironment.EnsureNativeDragAvailable();
			await IsolateDesktopForNativeInputAsync();

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var originalXml = await client.InvokeAsync("avd.layout.serialize");
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

				// Capture the preview the user sees for this zone while the tool is floating.
				var previewResult = await client.InvokeAsync("avd.debug-show-overlay", zoneType);
				using (var doc = JsonDocument.Parse(previewResult))
				{
					var preview = doc.RootElement.GetProperty("preview");
					Assert.True(preview.TryGetProperty("previewGeometryBounds", out var geom), $"Preview for '{zoneType}' missing geometry: {previewResult}");
					var previewX = geom.GetProperty("x").GetDouble();
					var previewY = geom.GetProperty("y").GetDouble();
					var previewWidth = geom.GetProperty("width").GetDouble();
					var previewHeight = geom.GetProperty("height").GetDouble();
					Assert.True(previewWidth > 0 && previewHeight > 0,
						$"Preview for '{zoneType}' has no area ({previewWidth}x{previewHeight}): {previewResult}");

					await client.InvokeAsync("avd.debug-hide-overlay");
					var pathTargetBounds = pathTarget == "anchorable2"
						? await client.QueryBoundsAsync("anchorable-pane", "dragTestTool2")
						: await client.QueryBoundsAsync("document-pane");

					const int maxAttempts = 3;
					string lastFailure = null;
					var docked = false;
					for (var attempt = 1; attempt <= maxAttempts && !docked; attempt++)
					{
						await EnsureFloatingClearOfHostAsync(client, mainArea, TestContext.Current.CancellationToken);
						(docked, lastFailure) = await TryDragOntoZoneAsync(client, zoneType, pathTargetBounds, TestContext.Current.CancellationToken);
					}

					Assert.True(docked, $"Failed to dock onto zone '{zoneType}' after {maxAttempts} attempts. LastFailure={lastFailure}");

					var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
					var pane = await QueryHostingPaneAsync(client, final);

					// Same screen position and same size as the preview, within rounding.
					const double tolerance = 2.0;
					Assert.True(Math.Abs(pane.X - previewX) <= tolerance && Math.Abs(pane.Y - previewY) <= tolerance,
						$"Docked pane position does not match preview for '{zoneType}': preview=({previewX},{previewY}) pane={pane}");
					Assert.True(Math.Abs(pane.Width - previewWidth) <= tolerance && Math.Abs(pane.Height - previewHeight) <= tolerance,
						$"Docked pane size does not match preview for '{zoneType}': preview={previewWidth}x{previewHeight} pane={pane.Width}x{pane.Height}");
				}
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
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
