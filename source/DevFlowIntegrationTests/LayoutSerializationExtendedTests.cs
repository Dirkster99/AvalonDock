using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Layout save/load in the states that used to break it: pane sizes (#118), a saved document that
	/// has since been closed (#38), documents across a restore (#83), the auto-hide side (#111) and an
	/// active tool window (#59). Ported from the FlaUI <c>LayoutSerializationExtendedTests</c> fixture.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class LayoutSerializationExtendedTests : PortedTestAppTestBase
	{
		public LayoutSerializationExtendedTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task SaveLoadLayout_PreservesPaneRatio_Issue118()
		{
			using var client = await TryConnectAsync();

			var before = await client.QueryBoundsAsync("anchorable-pane", "toolWindow1");

			await SaveLayoutAsync(client, "Layout_3");
			await LoadLayoutAsync(client, "Layout_3");

			var after = await client.QueryBoundsAsync("anchorable-pane", "toolWindow1");

			// The name says "ratio" and the FlaUI case only proved the panes still existed. The pane the
			// samples declares has a fixed DockWidth, so the round trip has to keep its size.
			Assert.True(
				Math.Abs(after.Width - before.Width) <= 2,
				$"The tool pane changed width across a layout round trip. Before={before}; After={after}");
			Assert.True(
				Math.Abs(after.Height - before.Height) <= 2,
				$"The tool pane changed height across a layout round trip. Before={before}; After={after}");

			var titles = AnchorableTitles(await QueryLayoutAsync(client));
			Assert.Contains("Tool Window 1", titles);
			Assert.Contains("WinForms Window", titles);
		}

		[Fact]
		public async Task LoadLayoutAfterClosingDocument_DoesNotCrash_Issue38()
		{
			using var client = await TryConnectAsync();

			// The saved file lists Document 1 and Document 2 only.
			await SaveLayoutAsync(client, "Layout_3");

			await client.InvokeAsync("avd.dialogs.answer", "yes");
			await client.InvokeAsync("avd.document.activate", "document1");
			await client.InvokeAsync("avd.document.add-two");
			await WaitUntilAsync(
				async () => DocumentTitles(await QueryLayoutAsync(client)).Contains("Test1"),
				TimeSpan.FromSeconds(5),
				"The documents were not added.");

			using (JsonDocument.Parse(await client.InvokeAsync("avd.close-content", "Test1")))
			{
			}

			await WaitUntilAsync(
				async () => !DocumentTitles(await QueryLayoutAsync(client)).Contains("Test1"),
				TimeSpan.FromSeconds(5),
				"'Test1' did not close.");

			// #38: the layout on disk still knows about the document that no longer exists.
			await LoadLayoutAsync(client, "Layout_3");

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task LayoutRestore_BringsBackDocuments_Issue83()
		{
			using var client = await TryConnectAsync();

			await SaveLayoutAsync(client, "Layout_4");

			var before = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", before);
			Assert.Contains("Document 2", before);

			await LoadLayoutAsync(client, "Layout_4");

			var after = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", after);
			Assert.Contains("Document 2", after);
		}

		[Fact]
		public async Task SaveLoadLayout_WithAutoHidePanels_Issue111()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.AutoHideClick);
			using var client = await TryConnectAsync();

			var anchorable = FindContent(await QueryLayoutAsync(client), "anchorables", "AutoHide1 Content");
			Assert.NotNull(anchorable);

			// Open and dismiss the flyout first: #111 is about serialising the left side after the
			// auto-hide window has been shown at least once.
			var tab = await client.QueryBoundsAsync("autohide-tab", ReadString(anchorable.Value, "contentId"));
			await client.ClickAsync(tab.CenterX, tab.CenterY, TestContext.Current.CancellationToken);
			await WaitUntilAsync(
				async () =>
				{
					using var state = JsonDocument.Parse(await client.InvokeAsync("avd.query.autohide", "AutoHide1 Content"));
					return state.RootElement.GetProperty("requestedIsOpen").GetBoolean();
				},
				TimeSpan.FromSeconds(5),
				"The auto-hide flyout did not open.");
			await client.InvokeAsync("avd.document.activate", "document1");

			await SaveLayoutAsync(client, "Layout_4");
			await LoadLayoutAsync(client, "Layout_4");

			var restored = FindContent(await QueryLayoutAsync(client), "anchorables", "AutoHide1 Content");
			Assert.NotNull(restored);
			Assert.True(ReadBool(restored.Value, "isAutoHidden"), $"'{restored.Value.GetRawText()}' should still be auto-hidden.");
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task SaveLoadLayout_WithToolWindowStates_Issue59()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.ToolTabClick);
			using var client = await TryConnectAsync();

			// A tool window is activated before the save: #59 was an InvalidOperationException during
			// deserialisation when an anchorable was selected.
			await ClickTabAsync(client, "anchorable-tab", "toolWindow1");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "toolWindow1",
				TimeSpan.FromSeconds(5),
				"'Tool Window 1' did not become the active content.");

			await SaveLayoutAsync(client, "Layout_4");

			await client.InvokeAsync("avd.document.activate", "document1");
			await LoadLayoutAsync(client, "Layout_4");

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "anchorables", "Tool Window 1"));
			await AssertAppAliveAsync(client);
		}

		private static async Task SaveLayoutAsync(DevFlowClient client, string slot)
		{
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", $"Layout/Save/{slot}")))
			{
			}

			await Task.Delay(300, TestContext.Current.CancellationToken);
		}

		private static async Task LoadLayoutAsync(DevFlowClient client, string slot)
		{
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", $"Layout/Load/{slot}")))
			{
			}

			var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
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
