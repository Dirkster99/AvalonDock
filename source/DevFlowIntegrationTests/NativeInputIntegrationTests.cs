using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	[Collection("DevFlow")]
	public sealed class NativeInputIntegrationTests : IntegrationTestBase
	{
		public NativeInputIntegrationTests(DevFlowAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task GlobalDragEndpoint_UsesNativeMouseInjectionOnMacOS()
		{
			NativeInputEnvironment.EnsureNativeDragAvailable();
			await IsolateDesktopForNativeInputAsync();

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

			// The agent prefers cliclick (a real CGEventPost-backed native input tool) when it's
			// installed; it falls back to macos-native (direct CGEventPost) or the original
			// Quartz-based native-global injection otherwise. All three are genuine native OS mouse
			// injection on macOS.
			if (OperatingSystem.IsMacOS())
				Assert.True(raw.Contains("cliclick") || raw.Contains("macos-native") || raw.Contains("native-global"),
					$"Expected a native mouse injection mode (cliclick, macos-native, or native-global), got: {raw}");
		}

		[Fact]
		public async Task DragDockedAnchorableTitle_ToFreeSpace_FloatsToolWindow()
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
						&& s.Anchorables.Any(a => a.ContentId == "dragTestTool"),
					TestContext.Current.CancellationToken);

				await client.InvokeAsync("avd.float", "dragTestTool");
				await WaitForLayoutAsync(
					client,
					s => s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					TestContext.Current.CancellationToken);

				var mainArea = await client.QueryBoundsAsync("manager");
				await client.InvokeAsync(
					"avd.position-floating",
					"dragTestTool",
					mainArea.Right + 40,
					mainArea.Y + 40);
				await Task.Delay(500, TestContext.Current.CancellationToken);
				await client.AssertFloatingWindowAboveMainAsync("dragTestTool");

				var documentPane = await client.QueryBoundsAsync("document-pane");

				var floatingTitle = await WaitForBoundsAsync(
						client,
						"anchorable-title",
						"dragTestTool",
						_ => true,
						TestContext.Current.CancellationToken);
				var dragStartX = floatingTitle.X + Math.Min(60, floatingTitle.Width / 4);
				var dragStartY = floatingTitle.CenterY;

				using var gesture = StartCliclickDragWithInspectionPause(
					dragStartX,
					dragStartY,
					documentPane.CenterX,
					documentPane.CenterY);

				DropTargetInfo compassTarget;
					try
					{
						compassTarget = await client.WaitForActiveDropTargetAsync(
							"DocumentPaneDockInside",
							TestContext.Current.CancellationToken,
							TimeSpan.FromSeconds(4));
					}
					catch (TimeoutException ex)
					{
						var activeTargets = await client.InvokeAsync("avd.query.active-drop-targets");
						var dragState = await client.InvokeAsync("avd.query.drag-state");
						var inputState = await client.InvokeAsync("avd.input.query");
						var managerBounds = await client.InvokeAsync("avd.query.bounds", "manager");
						var documentBounds = await client.InvokeAsync("avd.query.bounds", "document-pane");
						var hitTest = await client.InvokeAsync(
							"avd.hit-test",
							documentPane.CenterX,
							documentPane.CenterY);
						throw new Xunit.Sdk.XunitException(
							"AvalonDock compass overlay did not show the DocumentPaneDockInside target while dragging over the document pane. " +
							$"ActiveTargets={activeTargets}; DragState={dragState}; Input={inputState}; ManagerBounds={managerBounds}; " +
							$"DocumentBounds={documentBounds}; HitTest={hitTest}",
							ex);
					}

				Assert.True(
					Math.Abs(compassTarget.CenterX - documentPane.CenterX) < documentPane.Width / 4 &&
					Math.Abs(compassTarget.CenterY - documentPane.CenterY) < documentPane.Height / 4,
					$"DocumentPaneDockInside target is unexpectedly far from the document pane center: target={compassTarget}, pane={documentPane}");

				await gesture.WaitForExitAsync(TestContext.Current.CancellationToken);
				Assert.Equal(0, gesture.ExitCode);

				var docked = await WaitForLayoutAsync(
							client,
							s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
								&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
							TestContext.Current.CancellationToken,
							timeout: TimeSpan.FromSeconds(6));

				Assert.NotNull(docked);
				Assert.False(docked.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		private static Process StartCliclickDragWithInspectionPause(
			double fromX, double fromY, double toX, double toY)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = NativeInputEnvironment.CliclickPath,
				UseShellExecute = false,
			};
			startInfo.ArgumentList.Add("-w");
			startInfo.ArgumentList.Add("40");
			startInfo.ArgumentList.Add($"m:{Math.Round(fromX)},{Math.Round(fromY)}");
			startInfo.ArgumentList.Add($"dd:{Math.Round(fromX)},{Math.Round(fromY)}");
			for (var step = 1; step <= 24; step++)
			{
				var progress = step / 24d;
				startInfo.ArgumentList.Add(
					$"dm:{Math.Round(fromX + (toX - fromX) * progress)},{Math.Round(fromY + (toY - fromY) * progress)}");
			}
			startInfo.ArgumentList.Add("w:5000");
			startInfo.ArgumentList.Add($"du:{Math.Round(toX)},{Math.Round(toY)}");
			return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start cliclick drag gesture.");
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
