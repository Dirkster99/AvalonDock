using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>Cross-platform ports of the ToggleDock Window-mode FlaUI regressions.</summary>
	[Collection("DevFlowCodeApp")]
	public sealed class DetachedWindowPortedTests : IntegrationTestBase
	{
		private const string Explorer = "Explorer";
		private readonly CodeAppFixture fixture;

		public DetachedWindowPortedTests(CodeAppFixture fixture)
			: base(fixture)
		{
			this.fixture = fixture;
		}

		protected override int AgentPort => 9224;

		public override async ValueTask InitializeAsync()
		{
			await fixture.RestartAsync();
		}

		[Fact]
		public async Task ViewModeMenu_OffersWindowItem()
		{
			using var client = await TryConnectAsync();
			using var mode = JsonDocument.Parse(await client.InvokeAsync("avd.window-mode.query", Explorer));
			var window = FindMenuItem(mode.RootElement, "Window");
			Assert.True(window.GetProperty("isEnabled").GetBoolean());
		}

		[Fact]
		public async Task SelectWindowMode_DetachesIntoTopLevelWindow()
		{
			using var client = await TryConnectAsync();
			await DetachAsync(client);
			using var detached = await QueryDetachedAsync(client);
			Assert.Equal(1, detached.RootElement.GetProperty("count").GetInt32());
			Assert.Equal(Explorer, FindDetachedWindow(detached.RootElement).GetProperty("title").GetString());
		}

		[Fact]
		public async Task DetachedWindow_IsIndependentTopLevelWindow()
		{
			using var client = await TryConnectAsync();
			await DetachAsync(client);
			using var detached = await QueryDetachedAsync(client);
			var window = FindDetachedWindow(detached.RootElement);
			Assert.True(window.GetProperty("ownerIsNull").GetBoolean());
			Assert.True(window.GetProperty("showInTaskbar").GetBoolean());
			Assert.Equal("SingleBorderWindow", window.GetProperty("windowStyle").GetString());
			Assert.Equal("CanResize", window.GetProperty("resizeMode").GetString());
		}

		[Fact]
		public async Task ClosingDetachedWindow_DocksContentBack()
		{
			using var client = await TryConnectAsync();
			await DetachAsync(client);
			await client.InvokeAsync("avd.detached.close", Explorer);
			using var detached = await QueryDetachedAsync(client);
			Assert.Equal(0, detached.RootElement.GetProperty("count").GetInt32());
			using var anchorables = JsonDocument.Parse(await client.InvokeAsync("avd.query.anchorables"));
			Assert.Contains(ReadArray(anchorables.RootElement, "anchorables"), a => ReadString(a, "title") == Explorer);
		}

		[Fact]
		public async Task SelectWindowMode_Twice_DocksContentBack()
		{
			using var client = await TryConnectAsync();
			await DetachAsync(client);
			await client.InvokeAsync("avd.window-mode.toggle", Explorer);
			using var detached = await QueryDetachedAsync(client);
			Assert.Equal(0, detached.RootElement.GetProperty("count").GetInt32());
		}

		[Fact]
		public async Task StripeButton_WhileDetached_DoesNotDockAndKeepsWindowAlive()
		{
			using var client = await TryConnectAsync();
			await DetachAsync(client);
			await client.InvokeAsync("avd.toggle.click", Explorer);
			using var detached = await QueryDetachedAsync(client);
			Assert.Equal(1, detached.RootElement.GetProperty("count").GetInt32());
			Assert.True(FindDetachedWindow(detached.RootElement).GetProperty("isVisible").GetBoolean());
		}

		[Fact]
		public async Task DetachedWindow_CarriesOptionsMenu()
		{
			using var client = await TryConnectAsync();
			await DetachAsync(client);
			using var detached = await QueryDetachedAsync(client);
			Assert.True(FindDetachedWindow(detached.RootElement).GetProperty("hasOptionsHeader").GetBoolean());
		}

		private static async Task DetachAsync(DevFlowClient client)
		{
			await client.InvokeAsync("avd.window-mode.toggle", Explorer);
			using var detached = await QueryDetachedAsync(client);
			Assert.Equal(1, detached.RootElement.GetProperty("count").GetInt32());
		}

		private static async Task<JsonDocument> QueryDetachedAsync(DevFlowClient client) =>
			JsonDocument.Parse(await client.InvokeAsync("avd.detached.query"));

		private static JsonElement FindMenuItem(JsonElement root, string header) =>
			ReadArray(root, "items").Single(item => ReadString(item, "header") == header);

		private static JsonElement FindDetachedWindow(JsonElement root) =>
			ReadArray(root, "windows").Single(window => ReadString(window, "title") == Explorer);

		private static string ReadString(JsonElement element, string property) =>
			element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
				? value.GetString()
				: null;

		private static JsonElement[] ReadArray(JsonElement element, string property) =>
			element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
				? value.EnumerateArray().ToArray()
				: Array.Empty<JsonElement>();
	}
}
