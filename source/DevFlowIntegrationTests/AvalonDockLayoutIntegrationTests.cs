using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	[Collection("DevFlow")]
	public sealed class AvalonDockLayoutIntegrationTests : IntegrationTestBase
	{
		[Fact]
		public async Task MacOSNativeViewTrees_IdentifySystemVisualEffectOwners()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return;
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			var mainJson = await client.InvokeAsync("avd.query.macos-view-tree", "main");
			Console.WriteLine($"macOS main native tree: {mainJson}");
			using var main = JsonDocument.Parse(mainJson);
			Assert.True(main.RootElement.GetProperty("windowFound").GetBoolean());
			Assert.NotEmpty(main.RootElement.GetProperty("views").EnumerateArray());

			await client.InvokeAsync("avd.float", "dragTestTool");
			var floatingJson = await client.InvokeAsync("avd.query.macos-view-tree", "floating", "dragTestTool");
			Console.WriteLine($"macOS floating native tree: {floatingJson}");
			using var floating = JsonDocument.Parse(floatingJson);
			Assert.True(floating.RootElement.GetProperty("windowFound").GetBoolean());
			Assert.NotEmpty(floating.RootElement.GetProperty("views").EnumerateArray());

			await client.InvokeAsync("avd.debug-show-overlay", "DockLeft");
			try
			{
				var overlayJson = await client.InvokeAsync("avd.query.macos-view-tree", "overlay");
				Console.WriteLine($"macOS overlay native tree: {overlayJson}");
				using var overlay = JsonDocument.Parse(overlayJson);
				Assert.True(overlay.RootElement.GetProperty("windowFound").GetBoolean());
				Assert.NotEmpty(overlay.RootElement.GetProperty("views").EnumerateArray());
			}
			finally
			{
				await client.InvokeAsync("avd.debug-hide-overlay");
				await client.InvokeAsync("avd.dock", "dragTestTool");
			}
		}

		[Fact]
		public async Task AnchorableFloatAndDock_RepeatedWindowLifecycleRemainsResponsive()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);

			// This deliberately avoids native mouse input and the transparent drag overlay. A failure
			// therefore isolates floating NSWindow creation/destruction from synthetic-input flakes and
			// overlay rendering. It also gives the intermittent macOS CUIThemeFacet/CAPackage crash a
			// deterministic lifecycle workload instead of relying on the much slower zone suite.
			for (var iteration = 0; iteration < 25; iteration++)
			{
				await client.InvokeAsync("avd.float", "dragTestTool");
				var floated = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Contains(floated.Anchorables, a => a.ContentId == "dragTestTool" && a.IsFloat);

				await client.InvokeAsync("avd.dock", "dragTestTool");
				var docked = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Contains(docked.Anchorables, a => a.ContentId == "dragTestTool" && !a.IsFloat);
				Assert.Empty(docked.FloatingWindows);
			}
		}

		[Fact]
		public async Task TransparentOverlay_RepeatedWindowLifecycleRemainsResponsive()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			for (var iteration = 0; iteration < 25; iteration++)
			{
				await client.InvokeAsync("avd.float", "dragTestTool");
				var overlay = await client.InvokeAsync("avd.debug-show-overlay", "DockLeft");
				Assert.Contains("\"shown\":true", overlay);
				await client.InvokeAsync("avd.debug-hide-overlay");
				await client.InvokeAsync("avd.dock", "dragTestTool");
			}

			var final = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
			Assert.Contains(final.Anchorables, a => a.ContentId == "dragTestTool" && !a.IsFloat);
			Assert.Empty(final.FloatingWindows);
		}

		[Fact]
		public async Task QueryLayout_ReturnsTestDocumentsAndAnchorables()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			var snap = await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);

			Assert.Contains(snap.Documents, d => d.ContentId == "dragTestDocument");
			Assert.Contains(snap.Anchorables, a => a.ContentId == "dragTestTool");
		}

		[Fact]
		public async Task AnchorableFloatAndRestore_RoundTripsThroughLayoutModel()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			try
			{
				await client.InvokeAsync("avd.float", "dragTestTool");
				var floated = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Contains(floated.Anchorables, a => a.ContentId == "dragTestTool" && a.IsFloat);
				Assert.NotEmpty(floated.FloatingWindows);

			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}

			var restored = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
			Assert.Contains(restored.Anchorables, a => a.ContentId == "dragTestTool" && !a.IsFloat);
			Assert.Empty(restored.FloatingWindows);
		}

		[Fact]
		public async Task HideAndShowAnchorable_RoundTripsThroughLayoutModel()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			try
			{
				await client.InvokeAsync("avd.hide", "dragTestTool");
				var hidden = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.True(hidden.Anchorables.Single(a => a.ContentId == "dragTestTool").IsHidden);

				await client.InvokeAsync("avd.show", "dragTestTool");
				var shown = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.False(shown.Anchorables.Single(a => a.ContentId == "dragTestTool").IsHidden);
				Assert.True(shown.Anchorables.Single(a => a.ContentId == "dragTestTool").IsVisible);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		[Fact]
		public async Task AddDocuments_CanBeRestoredFromSerializedLayout()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			var before = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
			try
			{
				await client.InvokeAsync("avd.add-documents");
				var afterAdd = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.True(afterAdd.Documents.Count >= before.Documents.Count + 2);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}

			var restored = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
			Assert.Equal(before.Documents.Count, restored.Documents.Count);
		}
	}
}
