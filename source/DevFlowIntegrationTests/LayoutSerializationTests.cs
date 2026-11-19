using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// The Layout menu's save/load round trip: documents (#440), tool-window content (#392), restoring
	/// the saved state after mutations, repeated cycles (#167), content after a restore (#443) and the
	/// standalone window a detached tool window is restored into.
	/// </summary>
	/// <remarks>
	/// TestApp writes <c>AvalonDock_Layout_N.config</c> into its working directory and reading a slot
	/// that was never written kills the process with <c>FileNotFoundException</c>, so every case saves
	/// before it loads. The saved file is the sample's own format, deliberately: this suite is about
	/// what the sample does with it.
	/// </remarks>
	[Collection("DevFlow")]
	public sealed class LayoutSerializationTests : PortedTestAppTestBase
	{
		public LayoutSerializationTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task SaveAndLoadLayout_PreservesDocuments_Issue440()
		{
			using var client = await TryConnectAsync();

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 2"));

			await SaveLayoutAsync(client, "Layout_1");
			await LoadLayoutAsync(client, "Layout_1");

			var titles = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task SaveAndLoadLayout_PreservesToolWindows_Issue392()
		{
			using var client = await TryConnectAsync();

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "anchorables", "Tool Window 1"));
			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "anchorables", "Tool Window 2"));

			await SaveLayoutAsync(client, "Layout_1");
			await LoadLayoutAsync(client, "Layout_1");

			var titles = AnchorableTitles(await QueryLayoutAsync(client));
			Assert.Contains("Tool Window 1", titles);
			Assert.Contains("Tool Window 2", titles);

			// The FlaUI case only checked that the tabs came back. #392/#443 are about the content
			// being there after deserialisation, so a restored tool window is asked what it hosts.
			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.content", "toolWindow1"));
			Assert.True(report.RootElement.GetProperty("viewLoaded").GetBoolean());
			Assert.Contains(
				"This Comtrol demos an Auto-Size FloatingWindow",
				report.RootElement.GetProperty("textBlocks").EnumerateArray().Select(t => t.GetString() ?? string.Empty));
		}

		[Fact]
		public async Task LoadLayout_RestoresOriginalState()
		{
			using var client = await TryConnectAsync();

			await SaveLayoutAsync(client, "Layout_2");

			await client.InvokeAsync("avd.document.activate", "document1");
			await client.InvokeAsync("avd.document.add-two");
			await WaitUntilAsync(
				async () =>
				{
					var layout = await QueryLayoutAsync(client);
					return DocumentTitles(layout).Contains("Test1") && AnchorableTitles(layout).Contains("New Anchorable");
				},
				TimeSpan.FromSeconds(5),
				"The documents and the new anchorable were not added.");

			await LoadLayoutAsync(client, "Layout_2");

			// The FlaUI case only checked that Document 1 survived. The point of loading a saved layout
			// is that what was added afterwards is gone, so that is asserted.
			var restored = await QueryLayoutAsync(client);
			var titles = DocumentTitles(restored);
			Assert.Contains("Document 1", titles);
			Assert.DoesNotContain("Test1", titles);
			Assert.DoesNotContain("Test2", titles);
			Assert.DoesNotContain("New Anchorable", AnchorableTitles(restored));
		}

		[Fact]
		public async Task SaveLoadLayout_DoesNotCrash_Issue167()
		{
			using var client = await TryConnectAsync();

			// #167 was a serialisation error from a document floating window, so repetition is the
			// point: each cycle saves and loads the same slot again.
			for (var cycle = 1; cycle <= 3; cycle++)
			{
				await SaveLayoutAsync(client, "Layout_3", settle: false);
				await LoadLayoutAsync(client, "Layout_3", settle: false);
				await AssertAppAliveAsync(client);
			}

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
		}

		[Fact]
		public async Task AfterLayoutRestore_DocumentsAccessible_Issue443()
		{
			using var client = await TryConnectAsync();

			await SaveLayoutAsync(client, "Layout_4");
			await LoadLayoutAsync(client, "Layout_4");

			// #443: the document has to be usable after the restore, so it is clicked and its content
			// read, not merely found.
			await ClickTabAsync(client, "document-tab", "document1");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "document1",
				TimeSpan.FromSeconds(5),
				"'Document 1' could not be activated after a layout restore (Issue #443).");

			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
			Assert.True(report.RootElement.GetProperty("viewLoaded").GetBoolean());
			Assert.Contains(
				"Document 1 Content",
				report.RootElement.GetProperty("textBoxes").EnumerateArray().Select(t => t.GetString() ?? string.Empty));
		}

		[Fact]
		public async Task SaveAndLoadLayout_RestoresDetachedWindow()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.dialogs.answer", "yes");

			// Detach through the anchorable's own context menu ("Window"), which is the entry the FlaUI
			// case reached through the pane-title drop-down button.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.context.invoke", "Tool Window 1", "Window", "anchorable")))
			{
			}

			await WaitUntilAsync(
				async () => IsDetached(await QueryLayoutAsync(client), "toolWindow1"),
				TimeSpan.FromSeconds(20),
				"'Window' on 'Tool Window 1' did not detach it into a standalone window.",
				async () => $"dialogs={await client.InvokeAsync("avd.dialogs.query")} layout={await client.InvokeAsync("avd.query.layout")}");

			await SaveLayoutAsync(client, "Layout_1", settle: false);

			// A standalone window has to be closed before the restore; without that the restore is
			// adding a second one and nothing is proven.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.close-floating", "toolWindow1")))
			{
			}

			await WaitUntilAsync(
				async () => !IsDetached(await QueryLayoutAsync(client), "toolWindow1"),
				TimeSpan.FromSeconds(10),
				"The detached window should close before the restore.");

			await LoadLayoutAsync(client, "Layout_1");

			await WaitUntilAsync(
				async () => IsDetached(await QueryLayoutAsync(client), "toolWindow1"),
				TimeSpan.FromSeconds(20),
				"'Tool Window 1' was detached when the layout was saved, so restoring it should recreate the standalone window.");

			await AssertAppAliveAsync(client);
		}

		private static bool IsDetached(JsonElement layout, string contentId)
			=> ReadArray(layout, "detachedContentIds")
				.Any(id => string.Equals(id.GetString(), contentId, StringComparison.Ordinal));

		private static async Task SaveLayoutAsync(DevFlowClient client, string slot, bool settle = true)
		{
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", $"Layout/Save/{slot}")))
			{
			}

			if (settle)
				await Task.Delay(300, TestContext.Current.CancellationToken);
		}

		private static async Task LoadLayoutAsync(DevFlowClient client, string slot, bool settle = true)
		{
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", $"Layout/Load/{slot}")))
			{
			}

			// Deserialising rebuilds the docking controls, so the layout is polled until the sample's own
			// documents are back rather than slept through.
			var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(settle ? 5 : 2);
			while (DateTime.UtcNow < deadline)
			{
				using var layout = JsonDocument.Parse(await client.InvokeAsync("avd.query.layout"));
				if (DocumentTitles(layout.RootElement).Contains("Document 1"))
					return;

				await Task.Delay(100, TestContext.Current.CancellationToken);
			}

			throw new Xunit.Sdk.XunitException($"Loading '{slot}' did not bring 'Document 1' back.");
		}
	}
}
