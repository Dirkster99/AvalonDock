using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Baseline triage for TestApp: if any of these fail, every other TestApp suite is meaningless.
	/// Ported from the FlaUI <c>SmokeTests</c> fixture; read-only, so it does not restart the app.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class SmokeTests : PortedTestAppTestBase
	{
		public SmokeTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task Application_Starts_MainWindowAppears()
		{
			using var client = await TryConnectAsync();
			await AssertAppAliveAsync(client);

			using var window = JsonDocument.Parse(await client.InvokeAsync("avd.query.window"));
			var root = window.RootElement;
			Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()), "The main window should have a title.");
			Assert.True(root.GetProperty("width").GetDouble() > 0, "The main window should have a width.");
			Assert.True(root.GetProperty("height").GetDouble() > 0, "The main window should have a height.");
			Assert.NotEqual("Minimized", root.GetProperty("state").GetString());
		}

		[Fact]
		public async Task MainWindow_HasCorrectTitle()
		{
			using var client = await TryConnectAsync();

			using var window = JsonDocument.Parse(await client.InvokeAsync("avd.query.window"));
			var title = window.RootElement.GetProperty("title").GetString();
			Assert.Contains("MainWindow", title, StringComparison.Ordinal);
		}

		[Fact]
		public async Task DockingManager_IsPresent()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);

			// The FlaUI case accepted either element; both are asserted here, because a manager that has
			// one but not the other is exactly the partial-load state worth catching.
			Assert.NotNull(FindContent(layout, "documents", "document1"));
			Assert.NotNull(FindContent(layout, "anchorables", "toolWindow1"));
		}

		[Fact]
		public async Task MainMenu_IsAccessible()
		{
			using var client = await TryConnectAsync();

			using var document = JsonDocument.Parse(await client.InvokeAsync("avd.menu.items"));
			var headers = ReadArray(document.RootElement, "items")
				.Select(i => ReadString(i, "header"))
				.ToArray();

			Assert.Contains("Edit", headers);
			Assert.Contains("Layout", headers);
			Assert.Contains("Tools", headers);
		}

		[Fact]
		public async Task InitialDocuments_AllPresent()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			var titles = DocumentTitles(layout);
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);
		}

		[Fact]
		public async Task InitialToolWindows_AllPresent()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			var titles = AnchorableTitles(layout);
			Assert.Contains("WinForms Window", titles);
			Assert.Contains("Tool Window 1", titles);
			Assert.Contains("Tool Window 2", titles);

			// Docked, not floating: the manager has not created any floating window at startup.
			var floating = ReadArray(layout, "floatingWindows").ToArray();
			Assert.Empty(floating);
		}

		[Fact]
		public async Task AutoHidePanels_ArePresent()
		{
			using var client = await TryConnectAsync();

			var layout = await QueryLayoutAsync(client);
			foreach (var title in new[] { "AutoHide1 Content", "AutoHide2 Content" })
			{
				var anchorable = FindContent(layout, "anchorables", title);
				Assert.NotNull(anchorable);
				Assert.True(
					ReadBool(anchorable.Value, "isAutoHidden"),
					$"'{title}' should be an auto-hidden sidebar panel, not a docked one. State: {anchorable.Value.GetRawText()}");
			}
		}

		[Fact]
		public async Task MainWindow_HasReasonableSize()
		{
			using var client = await TryConnectAsync();

			using var window = JsonDocument.Parse(await client.InvokeAsync("avd.query.window"));
			var width = window.RootElement.GetProperty("width").GetDouble();
			var height = window.RootElement.GetProperty("height").GetDouble();

			Assert.True(width > 400, $"The main window is {width}px wide; a window that small cannot host a docking layout.");
			Assert.True(height > 300, $"The main window is {height}px high; a window that small cannot host a docking layout.");
		}
	}
}
