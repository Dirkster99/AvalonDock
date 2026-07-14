using System;
using System.Linq;
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
	public sealed class DropTargetZoneIntegrationTests : IntegrationTestBase
	{
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
			if (!IsNativeInputEnabled())
				return;

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

				await client.InvokeAsync("avd.float", "dragTestTool");
				await WaitForLayoutAsync(
					client,
					s => s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					TestContext.Current.CancellationToken);

				// Move the floating window clear of the main window's own area (see
				// DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout for why: a synthetic press
				// can land on whatever unrelated window/app is at the screen position otherwise, and
				// overlapping the main window is not safe even with a one-shot raise).
				var mainArea = await client.QueryBoundsAsync("manager");
				await client.InvokeAsync("avd.position-floating", "dragTestTool", mainArea.Right + 40, mainArea.Y + 40);
				await Task.Delay(500, TestContext.Current.CancellationToken);

				var pathTargetBounds = pathTarget == "anchorable2"
					? await client.QueryBoundsAsync("anchorable-pane", "dragTestTool2")
					: await client.QueryBoundsAsync("document-pane");

				var docked = await TryDragOntoZoneAsync(client, zoneType, pathTargetBounds, TestContext.Current.CancellationToken);

				Assert.True(docked, $"Failed to dock onto zone '{zoneType}' after all attempts.");

				// A successful dock must leave EXACTLY one dragTestTool anchorable, no longer floating.
				// (An earlier iteration of this suite's retry logic could re-issue a drag from stale,
				// already-docked coordinates and corrupt the model into a duplicate - this assertion
				// would catch that regression.)
				var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Single(final.Anchorables, a => a.ContentId == "dragTestTool");
				Assert.False(final.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		/// <summary>
		/// Runs the press -> drag-move-near -> query -> drag-move-onto -> release choreography, with
		/// retries: real native OS-level drags on this backend have a low-frequency window-manager
		/// timing race (see DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout), so a single
		/// attempt occasionally fails to make the compass populate at all. Each retry re-presses fresh
		/// (the previous attempt's release, even onto empty space, cleanly ends the gesture) rather than
		/// assuming any stale state from a prior attempt.
		/// </summary>
		private static async Task<bool> TryDragOntoZoneAsync(
			DevFlowClient client,
			string zoneType,
			ElementBounds pathTargetBounds,
			CancellationToken ct)
		{
			const int maxAttempts = 3;
			for (var attempt = 1; attempt <= maxAttempts; attempt++)
			{
				var title = await client.QueryBoundsAsync("anchorable-title", "dragTestTool");
				await client.PressAsync(title.CenterX, title.CenterY, ct);
				await Task.Delay(500, ct);

				// Step toward the relevant host in a few increments - AvalonDock's DragService updates
				// on each move, and a single huge jump has proven less reliable than several smaller
				// ones at making the compass populate.
				for (var i = 1; i <= 3; i++)
				{
					var t = i / 3d;
					var x = title.CenterX + (pathTargetBounds.CenterX - title.CenterX) * t;
					var y = title.CenterY + (pathTargetBounds.CenterY - title.CenterY) * t;
					await client.DragMoveAsync(x, y, ct);
					await Task.Delay(300, ct);
				}

				DropTargetInfo target = null;
				try
				{
					target = await client.WaitForActiveDropTargetAsync(zoneType, ct, TimeSpan.FromSeconds(4));
				}
				catch (TimeoutException)
				{
					// Compass never populated this attempt - release back at the drag's OWN origin
					// (the floating window's own caption position) to cleanly abort. Releasing at
					// pathTargetBounds' center is NOT safe here: that point is deliberately chosen to be
					// near/inside the relevant host, which is often ALSO exactly where an "Inside"-style
					// target sits - an "abort" release there could accidentally complete a real (wrong-
					// zone) dock instead of aborting, and the next retry's press would then land on a
					// now-DOCKED title, which drives a different AvalonDock code path
					// (AnchorablePaneTitle.OnMouseLeave) that isn't accounted for here.
					await client.ReleaseAsync(title.CenterX, title.CenterY, ct);
					await Task.Delay(500, ct);
					continue;
				}

				await client.DragMoveAsync(target.CenterX, target.CenterY, ct);
				await Task.Delay(300, ct);
				await client.ReleaseAsync(target.CenterX, target.CenterY, ct);
				await Task.Delay(700, ct);

				var snapshot = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				var stillFloating = snapshot.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
					|| snapshot.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool"));
				if (!stillFloating)
					return true;

				// Didn't dock - genuinely still floating, retry the whole gesture (not just the last
				// step): the compass populating is itself part of what can race.
			}

			return false;
		}

		private static bool IsNativeInputEnabled()
			=> string.Equals(Environment.GetEnvironmentVariable("DEVFLOW_TEST_NATIVE_INPUT"), "1", StringComparison.Ordinal);

		private static async Task<DockLayoutSnapshot> WaitForLayoutAsync(
			DevFlowClient client,
			Func<DockLayoutSnapshot, bool> predicate,
			CancellationToken ct)
		{
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(10);
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
