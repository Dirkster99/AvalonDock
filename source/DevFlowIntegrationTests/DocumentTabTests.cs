using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Document tab behaviour (#511 mouse-down NRE, #244 right-click closing a tab, #493 new documents,
	/// #240 content after activation), ported from the FlaUI <c>DocumentTabTests</c> fixture.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class DocumentTabTests : PortedTestAppTestBase
	{
		public DocumentTabTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task ClickDocumentTab_ActivatesDocument_Issue511()
		{
			using var client = await TryConnectAsync();

			// The regression is in the tab's mouse-down handler, so the tab is clicked for real rather
			// than activated through the model.
			await ClickTabAsync(client, "document-tab", "document2");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "document2",
				TimeSpan.FromSeconds(5),
				"Clicking the 'Document 2' tab did not make it the active document.");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task SwitchBetweenDocumentTabs_Works()
		{
			using var client = await TryConnectAsync();

			await ClickTabAsync(client, "document-tab", "document2");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "document2",
				TimeSpan.FromSeconds(5),
				"The second document did not become active.");

			await ClickTabAsync(client, "document-tab", "document1");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "document1",
				TimeSpan.FromSeconds(5),
				"The first document did not become active again.");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task RightClickDocumentTab_DoesNotClose_Issue244()
		{
			using var client = await TryConnectAsync();

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));

			using (JsonDocument.Parse(await client.InvokeAsync("avd.tab.right-click", "Document 1", "document")))
			{
			}

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task AddDocumentsViaButton_AppearsAsTabs_Issue493()
		{
			using var client = await TryConnectAsync();

			using var content = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
			Assert.Contains(
				"Click to add 2 documents",
				content.RootElement.GetProperty("buttons").EnumerateArray().Select(b => b.GetString()));

			await client.InvokeAsync("avd.document.add-two");

			// The tabs appear as the pane's children, after a layout pass: poll instead of sleeping.
			await WaitUntilAsync(
				async () =>
				{
					var titles = DocumentTitles(await QueryLayoutAsync(client));
					return titles.Contains("Test1") && titles.Contains("Test2");
				},
				TimeSpan.FromSeconds(5),
				"Documents added from the button did not appear as tabs (Issue #493).");
		}

		[Fact]
		public async Task InitialDocumentTabs_ArePresent()
		{
			using var client = await TryConnectAsync();

			var titles = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);
		}

		[Fact]
		public async Task ActivateDocument_ContentIsAccessible_Issue240()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.document.activate", "document1");

			// Scoped to the document, which is what the FlaUI case's "first button in the window" was
			// standing in for: the content has to arrive, with the controls it declares.
			using var content = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
			Assert.True(content.RootElement.GetProperty("isActive").GetBoolean());
			Assert.True(content.RootElement.GetProperty("isVisible").GetBoolean());
			Assert.True(content.RootElement.GetProperty("viewLoaded").GetBoolean());
			Assert.Contains(
				"Document 1 Content",
				content.RootElement.GetProperty("textBoxes").EnumerateArray().Select(t => t.GetString()));
		}
	}
}
