using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Showing and hiding tool windows: the Tools menu (#19), the auto-hide flyout and its side (#62,
	/// #9), flyout responsiveness (#10) and dynamically added tools (#382). Ported from the FlaUI
	/// <c>ToolWindowVisibilityTests</c> fixture.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class ToolWindowVisibilityTests : PortedTestAppTestBase
	{
		public ToolWindowVisibilityTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task HideAndShowToolWindow_ViaMenu_Issue19()
		{
			using var client = await TryConnectAsync();

			// Hiding asks "are you sure you want to hide this tool?", which a headless run answers
			// itself; the question is still recorded, so the lifecycle can be asserted.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.dialogs.answer", "yes")))
			{
			}

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "anchorables", "Tool Window 1"));

			await client.InvokeAsync("avd.hide", "toolWindow1");
			await WaitUntilAsync(
				async () => !IsVisible(await QueryLayoutAsync(client), "anchorables", "Tool Window 1"),
				TimeSpan.FromSeconds(5),
				"Hiding 'Tool Window 1' did not remove it from the docked layout.");

			// #19: the Tools menu has to bring a hidden tool window back. The header is "Tool Window1"
			// (no space) while the tab is "Tool Window 1".
			await client.InvokeAsync("avd.menu.invoke", "Tools/Tool Window1");
			await WaitUntilAsync(
				async () => IsVisible(await QueryLayoutAsync(client), "anchorables", "Tool Window 1"),
				TimeSpan.FromSeconds(10),
				"'Tools > Tool Window1' did not show the hidden tool window again (Issue #19).");

			using var dialogs = JsonDocument.Parse(await client.InvokeAsync("avd.dialogs.query"));
			Assert.Contains(
				dialogs.RootElement.GetProperty("questions").EnumerateArray().Select(q => q.GetString() ?? string.Empty),
				question => question.Contains("hide this tool", StringComparison.OrdinalIgnoreCase));

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task AutoHideWithTextBoxes_ContentAccessible_Issue62()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.AutoHideClick);
			using var client = await TryConnectAsync();

			var anchorable = FindContent(await QueryLayoutAsync(client), "anchorables", "AutoHide2 Content");
			Assert.NotNull(anchorable);
			var contentId = ReadString(anchorable.Value, "contentId");

			var tab = await client.QueryBoundsAsync("autohide-tab", contentId);
			await client.ClickAsync(tab.CenterX, tab.CenterY, TestContext.Current.CancellationToken);
			await WaitUntilAsync(
				async () =>
				{
					using var state = JsonDocument.Parse(await client.InvokeAsync("avd.query.autohide", "AutoHide2 Content"));
					return state.RootElement.GetProperty("requestedIsOpen").GetBoolean();
				},
				TimeSpan.FromSeconds(5),
				"'AutoHide2 Content' did not open its flyout.");

			// #62 is about the two text boxes inside a ScrollViewer: the count has to come from this
			// content, not from every edit control in the window, which is what the FlaUI case counted.
			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.content", "AutoHide2 Content"));
			var textBoxes = report.RootElement.GetProperty("textBoxes").GetArrayLength();
			Assert.True(textBoxes >= 2, $"'AutoHide2 Content' hosts {textBoxes} text box(es); the sample declares two.");

			await client.InvokeAsync("avd.document.activate", "document1");
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task AutoHideTabs_OnCorrectSide_Issue9()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			var main = await client.QueryBoundsAsync("main-window");

			foreach (var title in new[] { "AutoHide1 Content", "AutoHide2 Content" })
			{
				var anchorable = FindContent(layout, "anchorables", title);
				Assert.NotNull(anchorable);
				var tab = await client.QueryBoundsAsync("autohide-tab", ReadString(anchorable.Value, "contentId"));

				// #9 is about the side the strip attaches to, so the position is checked against the
				// window's left edge rather than against the window's midline, which almost any
				// position passes.
				Assert.True(
					tab.CenterX - main.X < 40,
					$"'{title}' should sit on the window's left edge (Issue #9). Tab={tab}; Window={main}");
				Assert.True(tab.X >= main.X - 8, $"'{title}' is outside the window. Tab={tab}; Window={main}");
			}
		}

		[Fact]
		public async Task AutoHideFlyout_IsResponsive_Issue10()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.AutoHideClick);
			using var client = await TryConnectAsync();

			var anchorable = FindContent(await QueryLayoutAsync(client), "anchorables", "AutoHide1 Content");
			Assert.NotNull(anchorable);
			var tab = await client.QueryBoundsAsync("autohide-tab", ReadString(anchorable.Value, "contentId"));

			// #10 was a hang, so the assertion is the round trip: each open and each close has to
			// complete quickly, three times in a row.
			for (var round = 1; round <= 3; round++)
			{
				var watch = Stopwatch.StartNew();
				await client.ClickAsync(tab.CenterX, tab.CenterY, TestContext.Current.CancellationToken);
				await WaitUntilAsync(
					async () =>
					{
						using var state = JsonDocument.Parse(await client.InvokeAsync("avd.query.autohide", "AutoHide1 Content"));
						return state.RootElement.GetProperty("requestedIsOpen").GetBoolean();
					},
					TimeSpan.FromSeconds(5),
					$"Auto-hide flyout did not open on round {round} (Issue #10).");
				var opened = watch.Elapsed;

				// The flyout stays while the pointer is over its sidebar button (AvalonDock's own rule, on every
				// backend), and the click left the pointer there, so move it into the document area first.
				var manager = await client.QueryBoundsAsync("manager");
				await NativeInputEnvironment.MovePointerAsync(client, manager.X + manager.Width - 40, manager.Y + manager.Height - 40, TestContext.Current.CancellationToken);

				await client.InvokeAsync("avd.document.activate", "document1");
				await WaitUntilAsync(
					async () =>
					{
						using var state = JsonDocument.Parse(await client.InvokeAsync("avd.query.autohide", "AutoHide1 Content"));
						return !state.RootElement.GetProperty("open").GetBoolean();
					},
					TimeSpan.FromSeconds(5),
					$"Auto-hide flyout did not close on round {round} (Issue #10).");
				var roundTrip = watch.Elapsed;

				Assert.True(
					opened < TimeSpan.FromSeconds(2) && roundTrip < TimeSpan.FromSeconds(4),
					$"Round {round} took {opened.TotalMilliseconds:F0} ms to open and {roundTrip.TotalMilliseconds:F0} ms round trip.");
			}

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task AddDynamicTools_DoesNotCrash_Issue382()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.document.activate", "document1");

			using var content = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
			Assert.Contains(
				"Click to add 2 documents",
				content.RootElement.GetProperty("buttons").EnumerateArray().Select(b => b.GetString() ?? string.Empty));

			// The button's handler also injects a "New Anchorable" into the left side; the FlaUI case
			// only looked for the documents, so the anchorable is asserted here as well.
			await client.InvokeAsync("avd.document.add-two");
			await WaitUntilAsync(
				async () =>
				{
					var layout = await QueryLayoutAsync(client);
					return DocumentTitles(layout).Contains("Test1")
						&& AnchorableTitles(layout).Contains("New Anchorable");
				},
				TimeSpan.FromSeconds(5),
				"Adding documents did not also add the 'New Anchorable' tool to the left side (Issue #382).");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task ShowWinFormsWindowViaMenu_Works()
		{
			using var client = await TryConnectAsync();

			// A hidden WinForms window is shown again by the same menu entry that activates a visible
			// one; both paths end with the tab present and the app alive. The hosted HWND is not
			// asserted on: it is a separate native control the docked layout does not own.
			await client.InvokeAsync("avd.menu.invoke", "Tools/WinForms Window");

			await WaitUntilAsync(
				async () => IsVisible(await QueryLayoutAsync(client), "anchorables", "WinForms Window"),
				TimeSpan.FromSeconds(5),
				"'Tools > WinForms Window' did not leave the WinForms tool window visible.");

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "anchorables", "WinForms Window"));
			await AssertAppAliveAsync(client);
		}

		private static bool IsVisible(JsonElement layout, string collection, string contentIdOrTitle)
		{
			var content = FindContent(layout, collection, contentIdOrTitle);
			return content.HasValue && ReadBool(content.Value, "isVisible");
		}
	}
}
