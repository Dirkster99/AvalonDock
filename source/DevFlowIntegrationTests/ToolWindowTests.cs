using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Tool windows: the docked panes and their tabs (#375), the content they host, the auto-hide
	/// side buttons (#362, #169) and the floating window the Tools menu creates. Ported from the FlaUI
	/// <c>ToolWindowTests</c> fixture.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class ToolWindowTests : PortedTestAppTestBase
	{
		public ToolWindowTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task InitialToolWindows_AreVisible()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			foreach (var title in new[] { "WinForms Window", "Tool Window 1", "Tool Window 2" })
			{
				var anchorable = FindContent(layout, "anchorables", title);
				Assert.NotNull(anchorable);
				Assert.True(
					ReadBool(anchorable.Value, "isVisible"),
					$"'{title}' should be docked and visible at startup. State: {anchorable.Value.GetRawText()}");
			}
		}

		[Fact]
		public async Task ClickToolWindowTab_ActivatesIt_Issue375()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.ToolTabClick);
			using var client = await TryConnectAsync();

			// A real click on the tab's header, not a model change: #375 is about the tab's own
			// input handling.
			await ClickTabAsync(client, "anchorable-tab", "toolWindow1");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "toolWindow1",
				TimeSpan.FromSeconds(5),
				"Clicking the 'Tool Window 1' tab did not make it the active content.");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task ToolWindowContent_IsAccessible()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.ToolTabClick);
			using var client = await TryConnectAsync();

			await ClickTabAsync(client, "anchorable-tab", "toolWindow1");
			await WaitUntilAsync(
				async () => ActiveContentId(await QueryLayoutAsync(client)) == "toolWindow1",
				TimeSpan.FromSeconds(5),
				"The tool window did not activate.");

			// The FlaUI case asserted only that the app had not exited; the point of activating a tool
			// window is that its content arrives, so both halves are checked here: the user control it
			// hosts and the timer text box beside it.
			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.content", "toolWindow1"));
			var root = report.RootElement;
			Assert.True(root.GetProperty("viewLoaded").GetBoolean());
			Assert.Contains(
				"This Comtrol demos an Auto-Size FloatingWindow",
				root.GetProperty("textBlocks").EnumerateArray().Select(t => t.GetString() ?? string.Empty));
			Assert.Contains(
				root.GetProperty("textBoxes").EnumerateArray().Select(t => t.GetString() ?? string.Empty),
				text => text.Contains("Tool Window 1 Attached to Timer", StringComparison.Ordinal));
		}

		[Fact]
		public async Task ToolWindows_OnDifferentSides_AreAccessible()
		{
			using var client = await TryConnectAsync();

			// The FlaUI case only counted three tabs and checked no side at all. The two panes the
			// sample declares are on opposite sides of the document area, which is what "on different
			// sides" has to mean, so that is what is asserted.
			var manager = await client.QueryBoundsAsync("manager");
			var left = await client.QueryBoundsAsync("anchorable-pane", "WinFormsWindow");
			var right = await client.QueryBoundsAsync("anchorable-pane", "toolWindow1");

			Assert.True(
				left.CenterX < manager.CenterX,
				$"'WinForms Window' should be docked left of the documents. Pane={left}; Manager={manager}");
			Assert.True(
				right.CenterX > manager.CenterX,
				$"'Tool Window 1' should be docked right of the documents. Pane={right}; Manager={manager}");
		}

		[Fact]
		public async Task AutoHideTabs_ArePresent_Issue362()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			foreach (var title in new[] { "AutoHide1 Content", "AutoHide2 Content" })
			{
				var anchorable = FindContent(layout, "anchorables", title);
				Assert.NotNull(anchorable);
				Assert.True(
					ReadBool(anchorable.Value, "isAutoHidden"),
					$"'{title}' should be an auto-hidden sidebar panel (Issue #362). State: {anchorable.Value.GetRawText()}");

				var tab = await client.QueryBoundsAsync("autohide-tab", ReadString(anchorable.Value, "contentId"));
				Assert.True(tab.Width > 0 && tab.Height > 0, $"'{title}' has no sidebar button to click. Bounds={tab}");
			}
		}

		[Fact]
		public async Task ClickAutoHideTab_OpensFlyout_Issue169()
		{
			KnownLibreWpfGaps.SkipOnLibreWpf(KnownLibreWpfGaps.AutoHideClick);
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			var anchorable = FindContent(layout, "anchorables", "AutoHide1 Content");
			Assert.NotNull(anchorable);
			var contentId = ReadString(anchorable.Value, "contentId");

			var tab = await client.QueryBoundsAsync("autohide-tab", contentId);
			var started = DateTimeOffset.UtcNow;
			await client.ClickAsync(tab.CenterX, tab.CenterY, TestContext.Current.CancellationToken);

			// #169 was a click on this sidebar button spinning the CPU (or crashing) before the flyout
			// appeared; the original asserted only that the app survived. Opening is the contract, so
			// it is asserted, and the latency is bounded because an unresponsive flyout is the bug.
			await WaitUntilAsync(
				async () =>
				{
					using var state = JsonDocument.Parse(await client.InvokeAsync("avd.query.autohide", "AutoHide1 Content"));
					return state.RootElement.GetProperty("requestedIsOpen").GetBoolean();
				},
				TimeSpan.FromSeconds(5),
				"Clicking 'AutoHide1 Content' did not open its flyout (Issue #169).");

			var elapsed = DateTimeOffset.UtcNow - started;
			Assert.True(elapsed < TimeSpan.FromSeconds(3), $"The flyout took {elapsed.TotalMilliseconds:F0} ms to open.");

			// The flyout hosts the anchorable's content, so its text box is reachable while it is open.
			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.content", "AutoHide1 Content"));
			Assert.Contains(
				report.RootElement.GetProperty("textBoxes").EnumerateArray().Select(t => t.GetString() ?? string.Empty),
				text => text.Contains("AutoHide Attached to Timer", StringComparison.Ordinal));

			// The flyout stays while the pointer is over its sidebar button (AvalonDock's own rule, on every
			// backend), and the click left the pointer there, so move it into the document area first.
			var manager = await client.QueryBoundsAsync("manager");
			await NativeInputEnvironment.MovePointerAsync(client, manager.X + manager.Width - 40, manager.Y + manager.Height - 40, TestContext.Current.CancellationToken);

			// Activating a document is how the sample dismisses the flyout.
			await client.InvokeAsync("avd.document.activate", "document1");
			await WaitUntilAsync(
				async () =>
				{
					using var state = JsonDocument.Parse(await client.InvokeAsync("avd.query.autohide", "AutoHide1 Content"));
					return !state.RootElement.GetProperty("open").GetBoolean();
				},
				TimeSpan.FromSeconds(5),
				"Activating a document did not dismiss the auto-hide flyout.");
		}

		[Fact]
		public async Task NewFloatingWindow_CreatesFloatingToolWindow()
		{
			using var client = await TryConnectAsync();

			var before = ReadArray(await QueryLayoutAsync(client), "floatingWindows").Count();

			// The menu path, because that is what a user does and what the case exercises.
			var result = await client.InvokeAsync("avd.menu.invoke", "Tools/New floating window");
			using (JsonDocument.Parse(result))
			{
			}

			await WaitUntilAsync(
				async () => ReadArray(await QueryLayoutAsync(client), "floatingWindows").Count() > before,
				TimeSpan.FromSeconds(10),
				$"'Tools > New floating window' did not add a floating window (was {before}).");
		}
	}
}
