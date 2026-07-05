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

			await client.WaitForAvalonDockReadyAsync(TestContext.Current.CancellationToken);
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

			if (OperatingSystem.IsMacOS())
				Assert.Contains("native-global", raw);
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
				await client.DragAndAssertOkAsync(new DragRequest
				{
					Global = true,
					FromX = title.CenterX,
					FromY = title.CenterY,
					ToX = manager.CenterX,
					ToY = manager.CenterY + Math.Min(260, manager.Height / 3d),
					Steps = 36
				}, TestContext.Current.CancellationToken);

				var floated = await WaitForLayoutAsync(
					client,
					s => s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat && s.FloatingWindows.Count > 0,
					TestContext.Current.CancellationToken);

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

				var floatingTitle = await WaitForBoundsAsync(
					client,
					"anchorable-title",
					"dragTestTool",
					_ => true,
					TestContext.Current.CancellationToken);
				var documentPane = await client.QueryBoundsAsync("document-pane");
				await client.DragAndAssertOkAsync(new DragRequest
				{
					Global = true,
					FromX = floatingTitle.CenterX,
					FromY = floatingTitle.CenterY,
					ToX = documentPane.CenterX,
					ToY = documentPane.CenterY,
					Steps = 48
				}, TestContext.Current.CancellationToken);

				var docked = await WaitForLayoutAsync(
					client,
					s => !s.Anchorables.Single(a => a.ContentId == "dragTestTool").IsFloat
						&& !s.FloatingWindows.Any(f => f.Contents.Contains("dragTestTool")),
					TestContext.Current.CancellationToken);

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
