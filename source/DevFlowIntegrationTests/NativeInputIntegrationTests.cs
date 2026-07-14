using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	public sealed class NativeInputIntegrationTests : IntegrationTestBase
	{
		[Fact]
		public async Task GlobalDragEndpoint_UsesNativeMouseInjectionOnMacOS()
		{
			if (!IsNativeSmokeEnabled())
				return;

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			// Don't depend on the app's pristine default layout (document1/toolWindow1): other tests
			// in this run mutate the live layout via avd.test-layout.reset, and restoring it back to
			// the exact pre-test state isn't guaranteed to fully re-materialize the original content
			// (AvalonDock's XML layout serializer round-trips STRUCTURE, not content instances). Reset
			// to the deterministic test layout instead, matching every other native-input test here.
			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var manager = await client.QueryBoundsAsync("manager");
			var result = await client.DragAsync(new DragRequest
			{
				Global = true,
				FromX = manager.CenterX,
				FromY = manager.CenterY,
				ToX = manager.CenterX + 1,
				ToY = manager.CenterY + 1,
				Steps = 1
			}, TestContext.Current.CancellationToken);

			var raw = result.GetRawText();
			Assert.DoesNotContain("only supported on Windows", raw);

			// The agent prefers real OS-level mouse events via the `cliclick` CLI when it's installed
			// (drives the genuine native input path); it falls back to the original Quartz-based
			// native-global injection otherwise. Either is a valid "native mouse injection on macOS".
			if (OperatingSystem.IsMacOS())
				Assert.True(raw.Contains("native-global") || raw.Contains("cliclick"),
					$"Expected a native mouse injection mode (native-global or cliclick), got: {raw}");
		}

		[Fact]
		public async Task DragDockedAnchorableTitle_ToFreeSpace_FloatsToolWindow()
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
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool"),
					TestContext.Current.CancellationToken);
				var title = await WaitForBoundsAsync(
					client,
					"anchorable-title",
					"dragTestTool",
					_ => true,
					TestContext.Current.CancellationToken);
				var manager = await client.QueryBoundsAsync("manager");

				// See DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout for why this retries:
				// native OS-level drags on this backend have a real, low-frequency window-manager
				// timing flake (a click can occasionally land a beat before/after the target window is
				// actually ready), and only retrying once the CURRENT state is confirmed still stale
				// (not just that our poll raced) is safe against re-issuing a drag from wrong/stale
				// coordinates.
				DockLayoutSnapshot floated = null;
				const int maxAttempts = 3;
				for (var attempt = 1; attempt <= maxAttempts && floated == null; attempt++)
				{
					await client.DragAndAssertOkAsync(new DragRequest
					{
						Global = true,
						FromX = title.CenterX,
						FromY = title.CenterY,
						ToX = manager.CenterX,
						ToY = manager.CenterY + Math.Min(260, manager.Height / 3d),
						Steps = 36
					}, TestContext.Current.CancellationToken);

					try
					{
						floated = await WaitForLayoutAsync(
							client,
							s => s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat && s.FloatingWindows.Count > 0,
							TestContext.Current.CancellationToken,
							timeout: TimeSpan.FromSeconds(6));
					}
					catch (TimeoutException) when (attempt < maxAttempts)
					{
						var current = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
						var alreadyFloated = current.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
							&& current.FloatingWindows.Count > 0;
						if (alreadyFloated)
						{
							floated = current;
						}
						// else: genuinely still docked - the title is still at the same coordinates
						// (a docked drag that didn't budge leaves the pane where it was), so retry.
					}
				}

				Assert.NotNull(floated);
				Assert.Contains("dragTestTool", floated.FloatingWindows.SelectMany(f => f.Contents));
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		[Fact]
		public async Task DragDockedPaneResizer_ResizesToolPane()
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
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool"),
					TestContext.Current.CancellationToken);
				var before = await WaitForBoundsAsync(
					client,
					"anchorable-pane",
					"dragTestTool",
					_ => true,
					TestContext.Current.CancellationToken);
				var resizer = await WaitForBoundsAsync(
					client,
					"anchorable-resizer",
					"dragTestTool",
					_ => true,
					TestContext.Current.CancellationToken);
				await client.DragAndAssertOkAsync(new DragRequest
				{
					Global = true,
					FromX = resizer.CenterX,
					FromY = resizer.CenterY,
					ToX = resizer.CenterX + 90,
					ToY = resizer.CenterY,
					Steps = 30
				}, TestContext.Current.CancellationToken);

				var after = await WaitForBoundsAsync(
					client,
					"anchorable-pane",
					"dragTestTool",
					b => b.Width > before.Width + 30,
					TestContext.Current.CancellationToken);

				Assert.True(after.Width > before.Width + 30, $"Expected tool pane width to grow from {before} to {after}.");
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		[Fact]
		public async Task DragFloatingToolWindow_ToDocumentPane_DocksBackIntoLayout()
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
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool"),
					TestContext.Current.CancellationToken);

				await client.InvokeAsync("avd.float", "dragTestTool");
				await WaitForLayoutAsync(
					client,
					s => s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					TestContext.Current.CancellationToken);

				// A newly floated tool opens as its own top-level OS window, by default flush against
				// the screen origin - exactly where other apps' windows (terminal, IDE) commonly sit. A
				// synthetic click there can land on whatever else is frontmost at that screen position
				// instead of this window. Move it clear of the MAIN window's own area entirely (not
				// just elsewhere on screen, but non-overlapping with it) and don't call avd.activate
				// afterwards (that raises the MAIN window and undoes the floating window's own one-shot
				// raise done by avd.position-floating). Overlapping the two windows was tried and is
				// NOT safe: even with the floating window raised once, a synthetic press in the
				// overlapping region can still land on the main window underneath (its own drop-zone
				// compass, a separate OverlayWindow, is anchored to the main window and evidently wins
				// the overlap there).
				var mainArea = await client.QueryBoundsAsync("manager");
				await client.InvokeAsync(
					"avd.position-floating",
					"dragTestTool",
					mainArea.Right + 40,
					mainArea.Y + 40);

				// The WPF-side Left/Top/PointToScreen values above update the instant the action
				// returns, but the native OS window they describe settles into that new position and
				// raised state asynchronously. A synthetic click issued immediately can land before the
				// native window has actually finished moving/raising. Give it a moment.
				await Task.Delay(500, TestContext.Current.CancellationToken);

				var floatingTitle = await WaitForBoundsAsync(
					client,
					"anchorable-title",
					"dragTestTool",
					_ => true,
					TestContext.Current.CancellationToken);
				var documentPane = await client.QueryBoundsAsync("document-pane");

				// The drag itself is a real OS-level window-manager race that setup alone doesn't fully
				// eliminate: the floated window's native raise can occasionally lose to something else
				// at the click point by the time the synthetic press actually lands, and the drop then
				// silently no-ops (DevFlow still reports ok=true - a click landed, just not on the right
				// window/at the right moment).
				//
				// Retrying is safe ONLY if we are certain the previous attempt genuinely left the tool
				// floating - not merely that OUR poll happened to time out while the drop had actually
				// already landed. Re-issuing a drag from stale (now-docked) title coordinates hits a
				// DIFFERENT AvalonDock code path (AnchorablePaneTitle.OnMouseLeave ->
				// StartDraggingFloatingWindowForPane, the "drag a DOCKED title back out" gesture) and
				// previously corrupted the model into a duplicate dragTestTool anchorable. So after a
				// timeout, explicitly re-check the CURRENT layout before deciding whether to retry: if
				// it's actually docked already (a pure observation race), accept that snapshot; only
				// retry the drag if it is still genuinely floating.
				DockLayoutSnapshot docked = null;
				const int maxAttempts = 3;
				for (var attempt = 1; attempt <= maxAttempts && docked == null; attempt++)
				{
					await client.DragAndAssertOkAsync(new DragRequest
					{
						Global = true,
						FromX = floatingTitle.CenterX,
						FromY = floatingTitle.CenterY,
						ToX = documentPane.CenterX,
						ToY = documentPane.CenterY,
						Steps = 48
					}, TestContext.Current.CancellationToken);

					try
					{
						docked = await WaitForLayoutAsync(
							client,
							s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
								&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
							TestContext.Current.CancellationToken,
							timeout: TimeSpan.FromSeconds(6));
					}
					catch (TimeoutException)
					{
						var current = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
						var stillFloating = current.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
							|| current.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool"));
						if (!stillFloating)
						{
							// Actually docked - our poll just lost the race. Don't retry the drag.
							docked = current;
						}
						else if (attempt >= maxAttempts)
						{
							throw;
						}
						// else: genuinely still floating and attempts remain - loop and retry the drag
						// from the SAME (still valid, still-floating) title coordinates.
					}
				}

				Assert.NotNull(docked);
				Assert.False(docked.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		private static bool IsNativeInputEnabled()
			=> string.Equals(Environment.GetEnvironmentVariable("DEVFLOW_TEST_NATIVE_INPUT"), "1", StringComparison.Ordinal);

		private static bool IsNativeSmokeEnabled()
			=> string.Equals(Environment.GetEnvironmentVariable("DEVFLOW_TEST_NATIVE_SMOKE"), "1", StringComparison.Ordinal);

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

			throw new TimeoutException("Timed out waiting for expected AvalonDock layout after native drag.");
		}

		private static async Task<ElementBounds> WaitForBoundsAsync(
			DevFlowClient client,
			string target,
			string contentId,
			Func<ElementBounds, bool> predicate,
			CancellationToken ct)
		{
			var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(10);
			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				try
				{
					var bounds = await client.QueryBoundsAsync(target, contentId);
					if (predicate(bounds))
						return bounds;
				}
				catch (InvalidOperationException)
				{
				}

				await Task.Delay(250, ct);
			}

			throw new TimeoutException($"Timed out waiting for expected bounds for '{target}'/'{contentId}'.");
		}
	}
}
