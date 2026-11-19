using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Document lifecycle: rapid tab switching (#7), closing one document among several (#204), closing
	/// and re-adding (#232), a <c>CanClose=False</c> anchorable (#196) and cancelling a close (#184).
	/// Ported from the FlaUI <c>DocumentLifecycleTests</c> fixture.
	/// </summary>
	/// <remarks>
	/// The cancelling cases used to click the tab's close button and answer the modal box. A headless run
	/// cannot click a modal <c>MessageBox</c> at all, so the sample answers its own confirmation through
	/// <c>avd.dialogs.answer</c> and the suite asserts on the outcome, which is what #184 and #204 are
	/// about.
	/// </remarks>
	[Collection("DevFlow")]
	public sealed class DocumentLifecycleTests : PortedTestAppTestBase
	{
		public DocumentLifecycleTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task RapidTabClicking_DoesNotCrash_Issue7()
		{
			using var client = await TryConnectAsync();

			// #7 was a crash from clicking headers faster than the layout settled. Each click is a real
			// mouse click at the tab's header, made back to back with no settle in between.
			for (var round = 0; round < 10; round++)
			{
				await ClickTabAsync(client, "document-tab", "document1");
				await ClickTabAsync(client, "document-tab", "document2");
			}

			var layout = await QueryLayoutAsync(client);
			Assert.Contains("Document 1", DocumentTitles(layout));
			Assert.Contains("Document 2", DocumentTitles(layout));
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task CloseDocument_RemainingDocsAccessible_Issue204()
		{
			using var client = await TryConnectAsync();

			await AnswerAsync(client, "yes");
			await AddDocumentsAsync(client);

			using (JsonDocument.Parse(await client.InvokeAsync("avd.close-content", "Test1")))
			{
			}

			await WaitUntilAsync(
				async () => !DocumentTitles(await QueryLayoutAsync(client)).Contains("Test1"),
				TimeSpan.FromSeconds(5),
				"Closing 'Test1' did not remove it.");

			// #204: after a document closes the manager's state has to stay consistent, so the
			// documents that remain are still reachable.
			var titles = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);
			Assert.Contains("Test2", titles);
		}

		[Fact]
		public async Task CloseAndReAddDocuments_Works_Issue232()
		{
			using var client = await TryConnectAsync();

			await AnswerAsync(client, "yes");
			await AddDocumentsAsync(client);

			Assert.Contains("Test1", DocumentTitles(await QueryLayoutAsync(client)));

			using (JsonDocument.Parse(await client.InvokeAsync("avd.close-content", "Test1")))
			{
			}

			await WaitUntilAsync(
				async () => !DocumentTitles(await QueryLayoutAsync(client)).Contains("Test1"),
				TimeSpan.FromSeconds(5),
				"Closing 'Test1' did not remove it.");

			// The FlaUI case re-found the button and clicked it but never asserted the documents came
			// back, which is the part #232 is about.
			await client.InvokeAsync("avd.document.activate", "document1");
			await client.InvokeAsync("avd.document.add-two");

			await WaitUntilAsync(
				async () =>
				{
					var titles = DocumentTitles(await QueryLayoutAsync(client));
					return titles.Contains("Test1") && titles.Contains("Test2");
				},
				TimeSpan.FromSeconds(5),
				"Re-adding the documents after closing one did not work (Issue #232).");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task CanCloseFalse_CannotCloseWindow_Issue196()
		{
			using var client = await TryConnectAsync();

			// A close attempt may raise the hiding confirmation for other tool windows; answering yes
			// keeps the attempt real, and CanClose=False has to win regardless.
			await AnswerAsync(client, "yes");

			var before = FindContent(await QueryLayoutAsync(client), "anchorables", "WinForms Window");
			Assert.NotNull(before);
			Assert.False(ReadBool(before.Value, "canClose"), "The sample declares 'WinForms Window' with CanClose=False.");

			using var result = JsonDocument.Parse(await client.InvokeAsync("avd.close-content", "WinForms Window"));
			Assert.Contains("cannot be closed", result.RootElement.GetString() ?? string.Empty, StringComparison.Ordinal);

			var after = FindContent(await QueryLayoutAsync(client), "anchorables", "WinForms Window");
			Assert.NotNull(after);
			Assert.True(
				ReadBool(after.Value, "isVisible"),
				$"'WinForms Window' has CanClose=False and must not be closed (Issue #196). State: {after.Value.GetRawText()}");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task CancelDocumentClose_DocumentRemains_Issue184()
		{
			using var client = await TryConnectAsync();

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));

			// Answering "no" is cancelling the close: the FlaUI case pressed RETURN on the box, which
			// selected the default "Yes" and therefore confirmed the close instead of cancelling it.
			await AnswerAsync(client, "no");
			using (JsonDocument.Parse(await client.InvokeAsync("avd.close-content", "Document 1")))
			{
			}

			using var dialogs = JsonDocument.Parse(await client.InvokeAsync("avd.dialogs.query"));
			Assert.Contains(
				dialogs.RootElement.GetProperty("questions").EnumerateArray().Select(q => q.GetString() ?? string.Empty),
				question => question.Contains("close the document", StringComparison.OrdinalIgnoreCase));

			var titles = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);
			await AssertAppAliveAsync(client);
		}

		private static async Task AnswerAsync(DevFlowClient client, string answer)
		{
			using var document = JsonDocument.Parse(await client.InvokeAsync("avd.dialogs.answer", answer));
		}

		private static async Task AddDocumentsAsync(DevFlowClient client)
		{
			await client.InvokeAsync("avd.document.add-two");

			var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
			while (DateTime.UtcNow < deadline)
			{
				using var document = JsonDocument.Parse(await client.InvokeAsync("avd.query.layout"));
				var titles = DocumentTitles(document.RootElement);
				if (titles.Contains("Test1") && titles.Contains("Test2"))
					return;

				await Task.Delay(100, TestContext.Current.CancellationToken);
			}

			throw new Xunit.Sdk.XunitException("The add-documents button did not add Test1 and Test2.");
		}
	}
}
