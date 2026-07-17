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
			NativeInputEnvironment.EnsureDecomposedInputAvailable();
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

				await client.InvokeAsync("avd.float", "dragTestTool");
				await WaitForLayoutAsync(
					client,
					s => s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					TestContext.Current.CancellationToken);

				var mainArea = await client.QueryBoundsAsync("manager");
				await client.InvokeAsync("avd.position-floating", "dragTestTool", mainArea.Right + 40, mainArea.Y + 40);
				await Task.Delay(500, TestContext.Current.CancellationToken);
				await client.AssertFloatingWindowAboveMainAsync("dragTestTool");

				var pathTargetBounds = pathTarget == "anchorable2"
					? await client.QueryBoundsAsync("anchorable-pane", "dragTestTool2")
					: await client.QueryBoundsAsync("document-pane");

				var docked = await TryDragOntoZoneAsync(client, zoneType, pathTargetBounds, TestContext.Current.CancellationToken);

				Assert.True(docked, $"Failed to dock onto zone '{zoneType}' after all attempts.");

				var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Single(final.Anchorables, a => a.ContentId == "dragTestTool");
				Assert.False(final.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		private static async Task<bool> TryDragOntoZoneAsync(
			DevFlowClient client,
			string zoneType,
			ElementBounds pathTargetBounds,
			CancellationToken ct)
		{
			const int maxAttempts = 3;
			TimeoutException lastCompassFailure = null;
			for (var attempt = 1; attempt <= maxAttempts; attempt++)
			{
				var title = await client.QueryBoundsAsync("anchorable-title", "dragTestTool");
				await client.PressAsync(title.CenterX, title.CenterY, ct);
				await Task.Delay(500, ct);

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
				catch (TimeoutException ex)
				{
					lastCompassFailure = ex;
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
			}

			if (lastCompassFailure != null)
			{
				throw new Xunit.Sdk.XunitException(
					$"AvalonDock compass overlay did not show drop target '{zoneType}' while dragging over its host.",
					lastCompassFailure);
			}

			return false;
		}

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
