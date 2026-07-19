using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
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
				{
					return;
				}

			// Don't depend on the app's pristine default layout (document1/toolWindow1): other tests
			// in this run mutate the live layout via avd.test-layout.reset, and restoring it back to
			// the exact pre-test state isn't guaranteed to fully re-materialize the original content
			// (AvalonDock's XML layout serializer round-trips STRUCTURE, not content instances). Reset
			// to the deterministic test layout instead, matching every other native-input test here.
				await client.InvokeAsync("avd.test-layout.reset");
				await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
				var manager = await client.QueryBoundsAsync("manager");
				await AssertSafeDragStartAsync(client, manager.CenterX, manager.CenterY, "DockingManager", TestContext.Current.CancellationToken);
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
					var title = await QueryRequiredDragHandleAsync(
						client,
						"docked-anchorable",
						"dragTestTool",
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
							await client.InvokeAsync("avd.input.reset");
							await AssertSafeDragHandleStartAsync(client, title, title.CenterX, title.CenterY, TestContext.Current.CancellationToken);
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
						catch (TimeoutException ex)
						{
							var layout = await client.InvokeAsync("avd.query.layout");
							var input = await client.InvokeAsync("avd.input.query");
							var surfaces = await client.InvokeAsync("avd.query.anchorable-drag-surfaces");
							var platform = await client.InvokeAsync("avd.query.platform");
							var startHit = await client.InvokeAsync("avd.hit-test", title.CenterX, title.CenterY);
							var endHit = await client.InvokeAsync("avd.hit-test", manager.CenterX, manager.CenterY + Math.Min(260, manager.Height / 3d));
							throw new Xunit.Sdk.XunitException(
								$"Docked anchorable title drag did not float the tool window. " +
								$"Start={title}; Manager={manager}; Layout={layout}; Input={input}; Platform={platform}; Surfaces={surfaces}; StartHit={startHit}; EndHit={endHit}",
								ex);
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
						var resizer = await client.QueryDragHandleAsync("anchorable-resizer", "dragTestTool");
					await AssertSafeDragStartAsync(client, resizer.CenterX, resizer.CenterY, "LayoutGridResizerControl", TestContext.Current.CancellationToken);
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
		public async Task DragDocumentPaneBody_DoesNotFloatDocumentTab()
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

					var documentPane = await client.QueryDragHandleAsync("document-body");
				var documentTab = await client.QueryBoundsAsync("document-tab", "dragTestDocument");
				Assert.True(documentPane.CenterY > documentTab.Bottom + 20, $"Document pane body is not below its tab. Pane={documentPane}; Tab={documentTab}");
				var bodyDragY = Math.Max(documentPane.CenterY, documentTab.Bottom + 80);
				await AssertSafeDragStartAsync(client, documentPane.CenterX, bodyDragY, "LayoutDocumentPaneControl", TestContext.Current.CancellationToken);

				await client.InvokeAsync("avd.input.reset");
				await using var gesture = await CliclickHeldDrag.StartAsync(
					documentPane.CenterX,
					bodyDragY,
					documentPane.CenterX + 180,
					bodyDragY,
					TestContext.Current.CancellationToken);
				await gesture.ReleaseAsync(TestContext.Current.CancellationToken);

				var after = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.False(after.Documents.Single(d => d.ContentId == "dragTestDocument").IsFloat);
				Assert.DoesNotContain(after.FloatingWindows, f => f.Contents.Contains("dragTestDocument"));
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
					mainArea.CenterX,
					mainArea.Y + 40);
				await Task.Delay(500, TestContext.Current.CancellationToken);
				await client.AssertFloatingWindowAboveMainAsync("dragTestTool");

				var documentPane = await client.QueryBoundsAsync("document-pane");

					var floatingTitle = await client.QueryDragHandleAsync("floating-caption", "dragTestTool");
						var dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
						var dragStartY = floatingTitle.CenterY;
						await AssertSafeFloatingDragStartAsync(client, "dragTestTool", dragStartX, dragStartY, "DropDownControlArea", TestContext.Current.CancellationToken);

					await using var discoveryGesture = await CliclickHeldDrag.StartAsync(
					dragStartX,
					dragStartY,
					documentPane.Right - 20,
					documentPane.Bottom - 20,
					TestContext.Current.CancellationToken);
				var insideTarget = await client.WaitForActiveDropTargetAsync(
					"DocumentPaneDockInside",
					TestContext.Current.CancellationToken,
					TimeSpan.FromSeconds(4));
					// The compass overlay is now on screen for the first time. Verify here - before the
					// drop-to-dock gesture even begins - that it is constrained to the DockingManager and
					// is not sitting too high over the main menu. The overlay-too-high regression shows up
					// the instant the overlay appears (i.e. during this discovery drag), so checking it
					// only during the later drop gesture would miss it.
					var discoveryDragState = await client.InvokeAsync("avd.query.drag-state");
					AssertOverlayIsConstrainedToDockingManager(discoveryDragState, await client.InvokeAsync("avd.query.active-drop-targets"));
					AssertFloatingWindowIsFollowingPointer(discoveryDragState);
				await discoveryGesture.ReleaseAsync(TestContext.Current.CancellationToken);

				var afterDiscovery = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Contains(afterDiscovery.FloatingWindows, f => f.Contents.Contains("dragTestTool"));

				await client.InvokeAsync("avd.position-floating", "dragTestTool", mainArea.CenterX, mainArea.Y + 40);
				await Task.Delay(400, TestContext.Current.CancellationToken);
					floatingTitle = await client.QueryDragHandleAsync("floating-caption", "dragTestTool");
						dragStartX = floatingTitle.X + Math.Min(20, floatingTitle.Width / 3d);
						dragStartY = floatingTitle.CenterY;
						await AssertSafeFloatingDragStartAsync(client, "dragTestTool", dragStartX, dragStartY, "DropDownControlArea", TestContext.Current.CancellationToken);

					await using var gesture = await CliclickHeldDrag.StartAsync(
					dragStartX,
					dragStartY,
					insideTarget.CenterX,
					insideTarget.CenterY,
					TestContext.Current.CancellationToken);
				var liveDragState = "[]";
				var hitDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
				do
				{
					await Task.Delay(200, TestContext.Current.CancellationToken);
					liveDragState = await client.InvokeAsync("avd.query.drag-state");
					// Checked on every tick, not just the final one - overlay/menu overlap is a defect
					// the instant it happens, not only in whichever frame we happen to sample last.
					AssertOverlayIsConstrainedToDockingManager(liveDragState, await client.InvokeAsync("avd.query.active-drop-targets"));
				}
				while (!liveDragState.Contains("\"currentDropTarget\":\"DocumentPaneDockInside\"", StringComparison.Ordinal)
					&& DateTime.UtcNow < hitDeadline);
				var liveTargets = await client.InvokeAsync("avd.query.active-drop-targets");
				if (!liveDragState.Contains("\"currentDropTarget\":\"DocumentPaneDockInside\"", StringComparison.Ordinal))
				{
					var inputState = await client.InvokeAsync("avd.input.query");
					var floatingHandle = await client.InvokeAsync("avd.query.drag-handle", "floating-caption", "dragTestTool");
					throw new Xunit.Sdk.XunitException(
						$"Floating drag did not reach DocumentPaneDockInside before release. " +
						$"Start={dragStartX},{dragStartY}; Target={insideTarget}; DragState={liveDragState}; " +
						$"Targets={liveTargets}; Input={inputState}; FloatingHandle={floatingHandle}");
				}
					AssertFloatingWindowIsFollowingPointer(liveDragState);

				var compassOutlinesVisible = liveTargets != "[]";
				await gesture.ReleaseAsync(TestContext.Current.CancellationToken);
				if (!compassOutlinesVisible)
				{
					var inputState = await client.InvokeAsync("avd.input.query");
					throw new Xunit.Sdk.XunitException(
						$"Compass targets were absent while the real cliclick drag was held. DragState={liveDragState}; Targets={liveTargets}; Input={inputState}");
				}

				DockLayoutSnapshot docked;
				try
				{
					docked = await WaitForLayoutAsync(
							client,
							s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
								&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
							TestContext.Current.CancellationToken,
							timeout: TimeSpan.FromSeconds(6));
				}
				catch (TimeoutException ex)
				{
					var afterLayout = await client.InvokeAsync("avd.query.layout");
					var afterDragState = await client.InvokeAsync("avd.query.drag-state");
					var afterTargets = await client.InvokeAsync("avd.query.active-drop-targets");
					var inputState = await client.InvokeAsync("avd.input.query");
					throw new Xunit.Sdk.XunitException(
						$"Floating drag release did not dock the tool window. " +
						$"ReleaseTarget={insideTarget}; PreReleaseDragState={liveDragState}; PreReleaseTargets={liveTargets}; " +
						$"AfterLayout={afterLayout}; AfterDragState={afterDragState}; AfterTargets={afterTargets}; Input={inputState}",
						ex);
				}

				Assert.NotNull(docked);
				Assert.False(docked.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		internal sealed class CliclickHeldDrag : IAsyncDisposable
		{
			private readonly Process _process;
			private bool _released;

			private CliclickHeldDrag(Process process)
			{
				_process = process;
			}

			public static Task<CliclickHeldDrag> StartAsync(
				double fromX, double fromY, double toX, double toY, CancellationToken ct, int holdMilliseconds = 10000)
			{
				var fromXi = (int)Math.Round(fromX);
				var fromYi = (int)Math.Round(fromY);
				var toXi = (int)Math.Round(toX);
				var toYi = (int)Math.Round(toY);
				var arguments = new List<string> { "-w", "40", $"m:{fromXi},{fromYi}", "w:250", $"dd:{fromXi},{fromYi}" };
				for (var step = 1; step <= 24; step++)
				{
					var progress = step / 24d;
					arguments.Add($"dm:{Math.Round(fromX + (toX - fromX) * progress)},{Math.Round(fromY + (toY - fromY) * progress)}");
				}
				arguments.Add($"w:{Math.Max(250, holdMilliseconds)}");
				arguments.Add($"du:{toXi},{toYi}");

				var startInfo = new ProcessStartInfo { FileName = NativeInputEnvironment.CliclickPath, UseShellExecute = false };
				foreach (var argument in arguments)
					startInfo.ArgumentList.Add(argument);
				var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start cliclick drag gesture.");
				return Task.FromResult(new CliclickHeldDrag(process));
			}

			public async Task ReleaseAsync(CancellationToken ct)
			{
				if (_released)
					return;
				_released = true;
				await _process.WaitForExitAsync(ct);
				Assert.Equal(0, _process.ExitCode);
			}

			public async ValueTask DisposeAsync()
			{
				await ReleaseAsync(CancellationToken.None);
				_process.Dispose();
			}
		}

		/// <summary>Checks, at a single key moment during a held drag, that the dragged window's title
		/// bar is actually under the live OS cursor - this is the direct, cheap check for the regression
		/// this repo has repeatedly hit: the floating window staying at its original position instead of
		/// following the drag at all. Call this at the moments that matter (drag-to-float discovery,
		/// just before drop-to-dock release), not on every polling tick - a window that's merely a frame
		/// or two behind the cursor is not the bug being guarded against here.</summary>
		internal static void AssertFloatingWindowIsFollowingPointer(string liveDragState)
		{
			using var dragDoc = JsonDocument.Parse(liveDragState);
			var dragRoot = dragDoc.RootElement.EnumerateArray().FirstOrDefault();
			Assert.Equal(JsonValueKind.Object, dragRoot.ValueKind);
			var pointer = dragRoot.GetProperty("currentPointer");
			var offset = dragRoot.GetProperty("dragOffset");
			var expectedLeft = pointer.GetProperty("X").GetDouble() - offset.GetProperty("X").GetDouble();
			var expectedTop = pointer.GetProperty("Y").GetDouble() - offset.GetProperty("Y").GetDouble();
			var left = dragRoot.GetProperty("left").GetDouble();
			var top = dragRoot.GetProperty("top").GetDouble();

			const double margin = 24;
			Assert.True(
				Math.Abs(left - expectedLeft) <= margin && Math.Abs(top - expectedTop) <= margin,
				$"Floating window is not following the pointer. Expected left/top near " +
					$"{expectedLeft},{expectedTop}; actual={left},{top}; DragState={liveDragState}");
		}

			internal static async Task AssertSafeDragStartAsync(
				DevFlowClient client,
				double screenX,
				double screenY,
				string expectedAncestorTypeFragment,
				CancellationToken ct)
			{
				var manager = await client.QueryBoundsAsync("manager").ConfigureAwait(false);
				var menu = await client.QueryBoundsAsync("menu").ConfigureAwait(false);
				Assert.True(
					manager.Contains(screenX, screenY),
					$"Refusing to start a real mouse drag outside DockingManager. Point={screenX},{screenY}; Manager={manager}");
				Assert.False(
					menu.Contains(screenX, screenY),
					$"Refusing to start a real mouse drag on the main menu. Point={screenX},{screenY}; Menu={menu}; Manager={manager}");

				await AssertDragStartHitAsync(client, screenX, screenY, expectedAncestorTypeFragment, ct).ConfigureAwait(false);
			}

			internal static async Task AssertSafeDragHandleStartAsync(
				DevFlowClient client,
				ElementBounds handle,
				double screenX,
				double screenY,
				CancellationToken ct)
			{
				var manager = await client.QueryBoundsAsync("manager").ConfigureAwait(false);
				var menu = await client.QueryBoundsAsync("menu").ConfigureAwait(false);
				Assert.True(
					manager.Contains(screenX, screenY),
					$"Refusing to start a real mouse drag outside DockingManager. Point={screenX},{screenY}; Manager={manager}; Handle={handle}");
				Assert.False(
					menu.Contains(screenX, screenY),
					$"Refusing to start a real mouse drag on the main menu. Point={screenX},{screenY}; Menu={menu}; Manager={manager}; Handle={handle}");
				Assert.True(
					handle.Contains(screenX, screenY),
					$"Refusing to start a real mouse drag outside the queried drag handle. Point={screenX},{screenY}; Handle={handle}; Manager={manager}; Menu={menu}");
				Assert.True(
					handle.Y >= manager.Y - 0.5 && handle.Bottom <= manager.Bottom + 0.5,
					$"Refusing to use a drag handle outside DockingManager vertical bounds. Handle={handle}; Manager={manager}; Menu={menu}");
				Assert.True(
					handle.Y >= menu.Bottom - 0.5,
					$"Refusing to use a drag handle that overlaps the main menu. Handle={handle}; Menu={menu}; Manager={manager}");
				_ = await client.InvokeAsync("avd.hit-test", screenX, screenY).ConfigureAwait(false);
				await AssertNativeMouseMoveLandsInsideHandleAsync(client, handle, screenX, screenY, ct).ConfigureAwait(false);
			}

			private static async Task AssertNativeMouseMoveLandsInsideHandleAsync(
				DevFlowClient client,
				ElementBounds handle,
				double screenX,
				double screenY,
				CancellationToken ct)
			{
				var platformBefore = await client.InvokeAsync("avd.query.platform").ConfigureAwait(false);
				await client.InvokeAsync("avd.input.reset").ConfigureAwait(false);
				await RunCliclickMoveAsync(screenX, screenY, ct).ConfigureAwait(false);
				await Task.Delay(150, ct).ConfigureAwait(false);

				var input = await client.InvokeAsync("avd.input.query").ConfigureAwait(false);
				using var doc = JsonDocument.Parse(input);
				var root = doc.RootElement;
				var mouseX = root.GetProperty("mouseX").GetDouble();
				var mouseY = root.GetProperty("mouseY").GetDouble();
				var manager = await client.QueryBoundsAsync("manager").ConfigureAwait(false);
				var actualScreenX = manager.X + mouseX;
				var actualScreenY = manager.Y + mouseY;
				if (handle.Contains(actualScreenX, actualScreenY))
					return;

				var hit = await client.InvokeAsync("avd.hit-test", screenX, screenY).ConfigureAwait(false);
				var platformAfter = await client.InvokeAsync("avd.query.platform").ConfigureAwait(false);
				throw new Xunit.Sdk.XunitException(
					$"Refusing to start a real mouse drag because the OS mouse did not land on the requested AvalonDock handle. " +
					$"Requested={screenX},{screenY}; WpfMouse={mouseX},{mouseY}; ActualScreenFromWpf={actualScreenX},{actualScreenY}; " +
					$"Handle={handle}; Manager={manager}; Input={input}; Hit={hit}; PlatformBefore={platformBefore}; PlatformAfter={platformAfter}");
			}

			private static async Task RunCliclickMoveAsync(double screenX, double screenY, CancellationToken ct)
			{
				var startInfo = new ProcessStartInfo { FileName = NativeInputEnvironment.CliclickPath, UseShellExecute = false };
				startInfo.ArgumentList.Add($"m:{(int)Math.Round(screenX)},{(int)Math.Round(screenY)}");
				using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start cliclick move.");
				await process.WaitForExitAsync(ct).ConfigureAwait(false);
				Assert.Equal(0, process.ExitCode);
			}

		internal static async Task AssertSafeFloatingDragStartAsync(
			DevFlowClient client,
			string contentId,
			double screenX,
			double screenY,
			string expectedAncestorTypeFragment,
			CancellationToken ct)
		{
			var window = await client.QueryBoundsAsync("floating-window", contentId).ConfigureAwait(false);
			var handle = await client.QueryDragHandleAsync("floating-caption", contentId).ConfigureAwait(false);
			var menu = await client.QueryBoundsAsync("menu").ConfigureAwait(false);
			Assert.True(
				window.Contains(screenX, screenY),
				$"Refusing to start a real mouse drag outside the floating window. Point={screenX},{screenY}; Window={window}; ContentId={contentId}");
			Assert.True(
				handle.Contains(screenX, screenY),
				$"Refusing to start a real mouse drag outside the floating caption handle. Point={screenX},{screenY}; Handle={handle}; Window={window}; ContentId={contentId}");
			Assert.False(
				menu.Contains(screenX, screenY),
				$"Refusing to start a real mouse drag on the main menu. Point={screenX},{screenY}; Menu={menu}; Window={window}; Handle={handle}");
		}

		private static async Task AssertDragStartHitAsync(
			DevFlowClient client,
			double screenX,
			double screenY,
			string expectedAncestorTypeFragment,
			CancellationToken ct)
		{
			var hitJson = await client.InvokeAsync("avd.hit-test", screenX, screenY).ConfigureAwait(false);
			using var doc = JsonDocument.Parse(hitJson);
			var root = doc.RootElement;
			Assert.True(root.TryGetProperty("ancestors", out var ancestors) && ancestors.ValueKind == JsonValueKind.Array,
				$"Hit-test payload missing ancestors for real mouse drag start. HitTest={hitJson}");
				var ancestorTypes = ancestors.EnumerateArray().Select(a => a.GetString()).ToArray();
				if (ancestorTypes.Any(a => a?.Contains(expectedAncestorTypeFragment, StringComparison.Ordinal) == true))
					return;

				var managerJson = await client.InvokeAsync("avd.query.bounds", "manager").ConfigureAwait(false);
				var menuJson = await client.InvokeAsync("avd.query.bounds", "menu").ConfigureAwait(false);
				throw new Xunit.Sdk.XunitException(
					$"Refusing to start a real mouse drag from a point that does not hit the expected AvalonDock element. " +
					$"Point={screenX},{screenY}; ExpectedAncestor={expectedAncestorTypeFragment}; " +
					$"HitTest={hitJson}; Manager={managerJson}; Menu={menuJson}");
			}

		private static (double X, double Y) ReadCurrentPointer(string liveDragState)
		{
			using var dragDoc = JsonDocument.Parse(liveDragState);
			var dragRoot = dragDoc.RootElement.EnumerateArray().FirstOrDefault();
			Assert.Equal(JsonValueKind.Object, dragRoot.ValueKind);
			var pointer = dragRoot.GetProperty("currentPointer");
			return (pointer.GetProperty("X").GetDouble(), pointer.GetProperty("Y").GetDouble());
		}

		/// <summary>Checks the overlay-vs-DockingManager invariant against a single drag-state snapshot.
		/// Call this on EVERY poll tick while a drag is in progress (not just at one key moment) - unlike
		/// the pointer-follow check, a transient bad frame here (overlay briefly covering the menu bar,
		/// say) is itself the defect, so it must be caught whenever it occurs, not only in whatever frame
		/// happens to be sampled last. No-ops when no floating window/overlay is present yet in this
		/// snapshot (drag not started, or between hosts) so callers can call it unconditionally.</summary>
		internal static void AssertOverlayIsConstrainedToDockingManager(string liveDragState, string liveTargets)
		{
			using var dragDoc = JsonDocument.Parse(liveDragState);
			var dragRoot = dragDoc.RootElement.EnumerateArray().FirstOrDefault();
			if (dragRoot.ValueKind != JsonValueKind.Object)
				return;
			if (dragRoot.GetProperty("overlayLeft").ValueKind == JsonValueKind.Null)
				return;

			var overlay = new ElementBounds(
				dragRoot.GetProperty("overlayLeft").GetDouble(),
				dragRoot.GetProperty("overlayTop").GetDouble(),
				dragRoot.GetProperty("overlayWidth").GetDouble(),
				dragRoot.GetProperty("overlayHeight").GetDouble());
			var manager = ReadBounds(dragRoot.GetProperty("managerBounds"));
			var menu = ReadBounds(dragRoot.GetProperty("menuBounds"));

			Assert.True(dragRoot.GetProperty("overlayAllowsTransparency").GetBoolean(), liveDragState);
			Assert.Equal("#00FFFFFF", dragRoot.GetProperty("overlayBackground").GetString());

			// The overlay must coincide with the DockingManager area exactly (not merely fit inside a
			// larger window) - it should cover the manager fully and never bleed into other main-window
			// chrome such as the menu bar.
			const double tolerance = 0.5;
			Assert.True(Math.Abs(overlay.X - manager.X) <= tolerance, $"Overlay left edge does not coincide with DockingManager. Overlay={overlay}; Manager={manager}");
			Assert.True(Math.Abs(overlay.Y - manager.Y) <= tolerance, $"Overlay top edge does not coincide with DockingManager. Overlay={overlay}; Manager={manager}");
			Assert.True(Math.Abs(overlay.Width - manager.Width) <= tolerance, $"Overlay width does not coincide with DockingManager. Overlay={overlay}; Manager={manager}");
			Assert.True(Math.Abs(overlay.Height - manager.Height) <= tolerance, $"Overlay height does not coincide with DockingManager. Overlay={overlay}; Manager={manager}");
			Assert.True(overlay.Y >= menu.Bottom - tolerance, $"Overlay intersects the main menu. Overlay={overlay}; Menu={menu}");

			using var targetsDoc = JsonDocument.Parse(liveTargets);
			foreach (var target in targetsDoc.RootElement.EnumerateArray())
			{
				var targetBounds = new ElementBounds(
					target.GetProperty("x").GetDouble(),
					target.GetProperty("y").GetDouble(),
					target.GetProperty("width").GetDouble(),
					target.GetProperty("height").GetDouble());
				Assert.True(
					targetBounds.X >= manager.X - 0.5
						&& targetBounds.Y >= manager.Y - 0.5
						&& targetBounds.Right <= manager.Right + 0.5
						&& targetBounds.Bottom <= manager.Bottom + 0.5,
					$"Drop target '{target.GetProperty("type").GetString()}' extends outside DockingManager. Target={targetBounds}; Manager={manager}");
			}
		}

		private static ElementBounds ReadBounds(JsonElement element)
		{
			return new ElementBounds(
				element.GetProperty("x").GetDouble(),
				element.GetProperty("y").GetDouble(),
				element.GetProperty("width").GetDouble(),
				element.GetProperty("height").GetDouble());
		}

		internal static async Task FailCompassMissingAsync(
			DevFlowClient client,
			string dropTargetType,
			double hitTestX,
			double hitTestY,
			TimeoutException ex)
		{
			var activeTargets = await client.InvokeAsync("avd.query.active-drop-targets");
			var dragState = await client.InvokeAsync("avd.query.drag-state");
			var inputState = await client.InvokeAsync("avd.input.query");
			var managerBounds = await client.InvokeAsync("avd.query.bounds", "manager");
			var documentBounds = await client.InvokeAsync("avd.query.bounds", "document-pane");
			var hitTest = await client.InvokeAsync("avd.hit-test", hitTestX, hitTestY);
			throw new Xunit.Sdk.XunitException(
				$"AvalonDock compass overlay did not show drop target '{dropTargetType}' while dragging. " +
				$"ActiveTargets={activeTargets}; DragState={dragState}; Input={inputState}; " +
				$"ManagerBounds={managerBounds}; DocumentBounds={documentBounds}; HitTest={hitTest}",
				ex);
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
			string lastBoundsJson = null;
			Exception lastException = null;
			while (DateTimeOffset.UtcNow < deadline)
			{
				ct.ThrowIfCancellationRequested();
				try
				{
					lastBoundsJson = await client.InvokeAsync("avd.query.bounds", target, contentId).ConfigureAwait(false);
					var bounds = await client.QueryBoundsAsync(target, contentId);
					if (predicate(bounds))
						return bounds;
				}
				catch (InvalidOperationException ex)
				{
					lastException = ex;
				}

				await Task.Delay(250, ct);
			}

			var layoutJson = await client.InvokeAsync("avd.query.layout").ConfigureAwait(false);
			var tabsJson = await client.InvokeAsync("avd.query.tabs").ConfigureAwait(false);
				throw new TimeoutException(
					$"Timed out waiting for expected bounds for '{target}'/'{contentId}'. " +
					$"LastBounds={lastBoundsJson}; LastError={lastException?.Message}; Layout={layoutJson}; Tabs={tabsJson}");
			}

			private static async Task<ElementBounds> QueryRequiredDragHandleAsync(
				DevFlowClient client,
				string target,
				string contentId,
				CancellationToken ct)
			{
				try
				{
					return await client.QueryDragHandleAsync(target, contentId).ConfigureAwait(false);
				}
				catch (InvalidOperationException ex)
				{
					var rawHandle = await client.InvokeAsync("avd.query.drag-handle", target, contentId).ConfigureAwait(false);
					var tabs = await client.InvokeAsync("avd.query.tabs").ConfigureAwait(false);
					var surfaces = await client.InvokeAsync("avd.query.anchorable-drag-surfaces").ConfigureAwait(false);
					var layout = await client.InvokeAsync("avd.query.layout").ConfigureAwait(false);
					var pane = await client.InvokeAsync("avd.query.bounds", "anchorable-pane", contentId).ConfigureAwait(false);
					throw new Xunit.Sdk.XunitException(
						$"AvalonDock drag handle '{target}'/'{contentId}' was not available before a real mouse drag. " +
						$"Handle={rawHandle}; Pane={pane}; Tabs={tabs}; Surfaces={surfaces}; Layout={layout}",
						ex);
				}
			}
		}
	}
