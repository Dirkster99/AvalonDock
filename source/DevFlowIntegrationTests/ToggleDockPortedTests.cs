using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Cross-platform ports of the ToggleDock FlaUI suites. The CodeApp exposes its actual
	/// ToggleDockButton tree and registered key bindings through DevFlow, so these tests verify
	/// the same state transitions without treating a platform-specific UIA tree as the contract.
	/// </summary>
	[Collection("DevFlowCodeApp")]
	public sealed class ToggleDockPortedTests : IntegrationTestBase
	{
		private const int DefaultPort = 9224;
		private readonly CodeAppFixture fixture;

		public ToggleDockPortedTests(CodeAppFixture fixture)
			: base(fixture)
		{
			this.fixture = fixture;
		}

		protected override int AgentPort => DefaultPort;

		public override async ValueTask InitializeAsync()
		{
			await fixture.RestartAsync();
		}

		[Fact]
		public async Task Application_Starts_MainWindowAppears()
		{
			using var client = await TryConnectAsync();
			using var report = await QueryToggleBarsAsync(client);
			Assert.Contains("AvalonDock Code", report.RootElement.GetProperty("windowTitle").GetString());
			Assert.True(report.RootElement.GetProperty("count").GetInt32() > 0);
		}

		[Fact]
		public async Task MainWindow_HasCorrectTitle()
		{
			using var client = await TryConnectAsync();
			using var report = await QueryToggleBarsAsync(client);
			Assert.Contains("AvalonDock Code", report.RootElement.GetProperty("windowTitle").GetString());
		}

		[Fact]
		public async Task Documents_ArePresent()
		{
			using var client = await TryConnectAsync();
			using var report = await QueryAnchorablesAsync(client);
			Assert.Contains(ReadArray(report.RootElement, "documents"), d => ReadString(d, "title") == "Welcome");
		}

		[Fact]
		public async Task ToggleButtons_ArePresent_ForLeftSide()
		{
			using var client = await TryConnectAsync();
			using var report = await QueryToggleBarsAsync(client);
			AssertButton(report.RootElement, "Explorer");
			AssertButton(report.RootElement, "Search");
		}

		[Fact]
		public async Task ToggleButtons_ArePresent_ForBottomSide()
		{
			using var client = await TryConnectAsync();
			using var report = await QueryToggleBarsAsync(client);
			AssertButton(report.RootElement, "Problems");
			AssertButton(report.RootElement, "Terminal");
		}

		[Fact]
		public async Task ToggleButtons_InitialState_AllUnchecked()
		{
			using var client = await TryConnectAsync();
			using var report = await QueryToggleBarsAsync(client);
			foreach (var button in ReadArray(report.RootElement, "buttons"))
			{
				var expected = ReadString(button, "title") == "Terminal";
				Assert.Equal(expected, ReadBool(button, "isChecked"));
			}
		}

		[Fact]
		public async Task ClickToggleButton_DocksAnchorable()
		{
			using var client = await TryConnectAsync();
			await ClickAsync(client, "Explorer");
			Assert.True(await IsCheckedAsync(client, "Explorer"));
		}

		[Fact]
		public async Task ClickToggleButton_Again_HidesAnchorable()
		{
			using var client = await TryConnectAsync();
			await ClickAsync(client, "Explorer");
			await ClickAsync(client, "Explorer");
			Assert.False(await IsCheckedAsync(client, "Explorer"));
		}

		[Fact]
		public async Task ExclusiveToggle_SameSection_OnlyOneActive()
		{
			using var client = await TryConnectAsync();
			await ClickAsync(client, "Explorer");
			await ClickAsync(client, "Search");
			Assert.False(await IsCheckedAsync(client, "Explorer"));
			Assert.True(await IsCheckedAsync(client, "Search"));
		}

		[Fact]
		public async Task DifferentSections_Toggle_WorkIndependently()
		{
			using var client = await TryConnectAsync();
			var explorerBefore = await IsCheckedAsync(client, "Explorer");
			var terminalBefore = await IsCheckedAsync(client, "Terminal");
			await ClickAsync(client, "Explorer");
			await ClickAsync(client, "Terminal");
			Assert.NotEqual(explorerBefore, await IsCheckedAsync(client, "Explorer"));
			Assert.NotEqual(terminalBefore, await IsCheckedAsync(client, "Terminal"));
		}

		[Fact]
		public async Task PinButton_ActsAsMinimize_SendsBackToSidebar()
		{
			using var client = await TryConnectAsync();
			await ClickAsync(client, "Explorer");
			Assert.True(await IsCheckedAsync(client, "Explorer"));
			await ClickAsync(client, "Explorer");
			Assert.False(await IsCheckedAsync(client, "Explorer"));
		}

		[Fact]
		public async Task SaveAndLoadLayout_KeepsExactlyTheSameSidebarButtons()
		{
			using var client = await TryConnectAsync();
			var before = await ButtonTitlesAsync(client);
			await client.InvokeAsync("avd.layout.save");
			await client.InvokeAsync("avd.layout.load");
			Assert.Equal(before, await ButtonTitlesAsync(client));
		}

		[Fact]
		public async Task AfterLoadingLayout_TheSidebarButtonsStillToggleTheirToolWindow()
		{
			using var client = await TryConnectAsync();
			await client.InvokeAsync("avd.layout.save");
			await client.InvokeAsync("avd.layout.load");
			var before = await IsCheckedAsync(client, "Explorer");
			await ClickAsync(client, "Explorer");
			Assert.NotEqual(before, await IsCheckedAsync(client, "Explorer"));
		}

		[Fact]
		public async Task LoadingLayoutWithUnknownToolbox_AddsNoGhostButton()
		{
			using var client = await TryConnectAsync();
			var expected = await ButtonTitlesAsync(client);
			await client.InvokeAsync("avd.layout.load-unknown");
			var after = await ButtonTitlesAsync(client);
			Assert.DoesNotContain("Ghost Toolbox", after);
			Assert.Equal(expected, after);
		}

		[Fact]
		public async Task LoadingLayoutRepeatedly_DoesNotAccumulateSidebarButtons()
		{
			using var client = await TryConnectAsync();
			await client.InvokeAsync("avd.layout.save");
			await client.InvokeAsync("avd.layout.load");
			var once = await ButtonTitlesAsync(client);
			await client.InvokeAsync("avd.layout.load");
			await client.InvokeAsync("avd.layout.load");
			Assert.Equal(once, await ButtonTitlesAsync(client));
		}

		[Fact]
		public async Task Shortcut_TogglesExplorer_OnAndOff()
		{
			using var client = await TryConnectAsync();
			await AssertShortcutRegisteredAsync(client, "Ctrl+Shift+E");
			await client.InvokeAsync("avd.shortcut.invoke", "Ctrl+Shift+E");
			Assert.True(await IsCheckedAsync(client, "Explorer"));
			await client.InvokeAsync("avd.shortcut.invoke", "Ctrl+Shift+E");
			Assert.False(await IsCheckedAsync(client, "Explorer"));
		}

		[Fact]
		public async Task Shortcut_TogglesOnlyTargetToolbox()
		{
			using var client = await TryConnectAsync();
			await AssertShortcutRegisteredAsync(client, "Ctrl+Shift+F");
			await client.InvokeAsync("avd.shortcut.invoke", "Ctrl+Shift+F");
			Assert.True(await IsCheckedAsync(client, "Search"));
			Assert.False(await IsCheckedAsync(client, "Explorer"));
		}

		private static async Task ClickAsync(DevFlowClient client, string title)
		{
			using var ignored = JsonDocument.Parse(await client.InvokeAsync("avd.toggle.click", title));
		}

		private static async Task<JsonDocument> QueryToggleBarsAsync(DevFlowClient client) =>
			JsonDocument.Parse(await client.InvokeAsync("avd.query.toggle-bars"));

		private static async Task<JsonDocument> QueryAnchorablesAsync(DevFlowClient client) =>
			JsonDocument.Parse(await client.InvokeAsync("avd.query.anchorables"));

		private static async Task<bool> IsCheckedAsync(DevFlowClient client, string title)
		{
			using var report = await QueryToggleBarsAsync(client);
			return ReadBool(GetButton(report.RootElement, title), "isChecked");
		}

		private static async Task<string[]> ButtonTitlesAsync(DevFlowClient client)
		{
			using var report = await QueryToggleBarsAsync(client);
			return ReadArray(report.RootElement, "buttons")
				.Select(b => ReadString(b, "title"))
				.OrderBy(title => title, StringComparer.Ordinal)
				.ToArray();
		}

		private static async Task AssertShortcutRegisteredAsync(DevFlowClient client, string gesture)
		{
			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.query.shortcuts"));
			Assert.Contains(ReadArray(report.RootElement, "bindings"), b => ReadString(b, "gesture") == gesture);
		}

		private static void AssertButton(JsonElement root, string title) => _ = GetButton(root, title);

		private static JsonElement GetButton(JsonElement root, string title)
		{
			foreach (var button in ReadArray(root, "buttons"))
			{
				if (ReadString(button, "title") == title)
					return button;
			}

			throw new Xunit.Sdk.XunitException($"No sidebar toggle button titled '{title}' was reported.");
		}

		private static string ReadString(JsonElement element, string property) =>
			element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
				? value.GetString()
				: null;

		private static bool ReadBool(JsonElement element, string property) =>
			element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.True;

		private static IReadOnlyList<JsonElement> ReadArray(JsonElement element, string property) =>
			element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
				? value.EnumerateArray().ToArray()
				: Array.Empty<JsonElement>();
	}
}
