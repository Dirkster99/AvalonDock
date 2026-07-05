using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	public sealed class AvalonDockLayoutIntegrationTests : IntegrationTestBase
	{
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
