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
			var floatingTitle = await client.QueryBoundsAsync("anchorable-title", "dragTestTool");
			var dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
			var dragStartY = floatingTitle.CenterY;
			var discoveryX = pathTargetBounds.Right - 20;
			var discoveryY = pathTargetBounds.Bottom - 20;
			await using var discoveryGesture = await NativeInputIntegrationTests.CliclickHeldDrag.StartAsync(
				dragStartX,
				dragStartY,
				discoveryX,
				discoveryY,
				ct);
			await Task.Delay(1000, ct);

			DropTargetInfo target;
			try
			{
				target = await client.WaitForActiveDropTargetAsync(zoneType, ct, TimeSpan.FromSeconds(4));
				await NativeInputIntegrationTests.AssertFloatingWindowIsUnderPointerAsync(
					client, "dragTestTool", await client.InvokeAsync("avd.query.drag-state"), ct);
			}
			catch (TimeoutException ex)
			{
				await discoveryGesture.ReleaseAsync(ct);
					await NativeInputIntegrationTests.FailCompassMissingAsync(
						client,
						zoneType,
						discoveryX,
						discoveryY,
						ex);
					throw;
				}

			await discoveryGesture.ReleaseAsync(ct);

			var afterDiscovery = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
			if (!afterDiscovery.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")))
			{
				await client.InvokeAsync("avd.float", "dragTestTool");
				await WaitForLayoutAsync(client, s => s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")), ct);
			}

			var managerBounds = await client.QueryBoundsAsync("manager");
			await client.InvokeAsync("avd.position-floating", "dragTestTool", managerBounds.Right + 40, managerBounds.Y + 40);
			await Task.Delay(400, ct);
			floatingTitle = await client.QueryBoundsAsync("anchorable-title", "dragTestTool");
			dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
			dragStartY = floatingTitle.CenterY;

			await using var dropGesture = await NativeInputIntegrationTests.CliclickHeldDrag.StartAsync(
				dragStartX,
				dragStartY,
				target.CenterX,
				target.CenterY,
				ct);
			var (preReleaseDragState, preReleaseTargets) = await WaitForCurrentDropTargetAsync(client, zoneType, target, ct);
			await NativeInputIntegrationTests.AssertFloatingWindowIsUnderPointerAsync(client, "dragTestTool", preReleaseDragState, ct);
			await dropGesture.ReleaseAsync(ct);

			try
			{
				await WaitForLayoutAsync(
					client,
					s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
						&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					ct,
					TimeSpan.FromSeconds(6));
				return true;
			}
			catch (TimeoutException)
			{
				var layout = await client.InvokeAsync("avd.query.layout");
				var input = await client.InvokeAsync("avd.input.query");
				System.Diagnostics.Debug.WriteLine(
					$"Zone={zoneType}; TargetCenter={target.CenterX},{target.CenterY}; PreReleaseDragState={preReleaseDragState}; " +
					$"PreReleaseTargets={preReleaseTargets}; Input={input}; Layout={layout}");
				return false;
			}
		}

		/// <summary>Polls live drag state until the given zone is the current drop target. Returns the
		/// drag-state/active-targets pair captured once the zone became current, for the caller to
		/// additionally verify overlay placement and window-under-pointer before releasing.</summary>
		private static async Task<(string DragState, string Targets)> WaitForCurrentDropTargetAsync(
			DevFlowClient client,
			string zoneType,
			DropTargetInfo target,
			CancellationToken ct)
		{
			var liveDragState = string.Empty;
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				liveDragState = await client.InvokeAsync("avd.query.drag-state");
				var liveTargets = await client.InvokeAsync("avd.query.active-drop-targets");
				// Checked on every tick, not just the final one - overlay/menu overlap is a defect the
				// instant it happens, not only in whichever frame we happen to sample last.
				NativeInputIntegrationTests.AssertOverlayIsConstrainedToDockingManager(liveDragState, liveTargets);
				if (liveDragState.Contains($"\"currentDropTarget\":\"{zoneType}\"", StringComparison.Ordinal))
					return (liveDragState, liveTargets);

				await Task.Delay(200, ct);
			}

			var timedOutTargets = await client.InvokeAsync("avd.query.active-drop-targets");
			throw new Xunit.Sdk.XunitException(
				$"Expected live drop target '{zoneType}' before releasing real mouse drag, but it was not current. " +
				$"Target center={target.CenterX},{target.CenterY}. DragState={liveDragState}; Targets={timedOutTargets}");
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
