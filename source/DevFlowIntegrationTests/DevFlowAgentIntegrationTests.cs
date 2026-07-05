using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	public sealed class DevFlowAgentIntegrationTests : IntegrationTestBase
	{
		[Fact]
		public async Task AgentStatus_ReportsExpectedWpfCapabilities()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var status = await client.GetStatusAsync(TestContext.Current.CancellationToken);

			Assert.Equal("wpf", status.GetProperty("framework").GetString());
			Assert.True(status.GetProperty("capabilities").GetProperty("drag").GetBoolean());
			Assert.True(status.GetProperty("capabilities").GetProperty("multiWindow").GetBoolean());
		}

		[Fact]
		public async Task InvokeActions_ExposeAvalonDockInstanceActions()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			var actions = await client.ListActionsAsync(TestContext.Current.CancellationToken);

			Assert.Contains("avd.query.layout", actions);
			Assert.Contains("avd.layout.serialize", actions);
			Assert.Contains("avd.layout.restore", actions);
			Assert.Contains("avd.float", actions);
			Assert.Contains("avd.dock", actions);
		}

		[Fact]
		public async Task QueryElements_ReturnsStableAvalonDockChromeBounds()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);

			var documentTab = await client.WaitForStableElementBoundsAsync("LayoutDocumentTabItem", TestContext.Current.CancellationToken);
			Assert.True(documentTab.Width > 20, $"Expected document tab width > 20, got {documentTab}.");
			Assert.True(documentTab.Height > 10, $"Expected document tab height > 10, got {documentTab}.");

			var resizers = await client.QueryElementsAsync(
				"LayoutGridResizerControl",
				maxResults: 10,
				maxDepth: 96,
				TestContext.Current.CancellationToken);

			Assert.Contains(resizers, e =>
			{
				if (!e.TryGetProperty("bounds", out var bounds) || bounds.ValueKind != System.Text.Json.JsonValueKind.Object)
					return false;
				var b = DevFlowTree.Bounds(e);
				return b.Width > 0 && b.Height > 0;
			});
		}
	}
}
