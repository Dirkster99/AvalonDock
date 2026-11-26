using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>Ports the remaining TestApp FlaUI stress regressions to cross-platform DevFlow.</summary>
	[Collection("DevFlow")]
	public sealed class StressTests : PortedTestAppTestBase
	{
		private static readonly string[] KeyboardBackendLimitations =
		{
			"EntryPointNotFoundException", "DllNotFoundException", "PlatformNotSupportedException",
		};

		public StressTests(TestAppFixture fixture) : base(fixture) { }

		[Fact]
		public async Task RapidDocumentSwitching_DoesNotCrash_Issue142()
		{
			using var client = await TryConnectAsync();
			for (var i = 0; i < 20; i++)
			{
				await ClickTabAsync(client, "document-tab", "document1");
				await ClickTabAsync(client, "document-tab", "document2");
			}
			var titles = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task TextPreserved_WhenSwitchingTabs_Issue139()
		{
			using var client = await TryConnectAsync();
			await client.InvokeAsync("avd.document.activate", "document1");
			await client.InvokeAsync("avd.focus.set", "document1");
			await client.InvokeAsync("avd.input.text", "TestData_Issue139");
			await client.InvokeAsync("avd.document.activate", "document2");
			await client.InvokeAsync("avd.document.activate", "document1");
			using var content = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
			Assert.Contains(content.RootElement.GetProperty("textBoxes").EnumerateArray(), text =>
				(text.GetString() ?? string.Empty).Contains("TestData_Issue139", StringComparison.Ordinal));
		}

		[Fact]
		public async Task RapidMouseOverToolTabs_DoesNotCrash_Issue90()
		{
			using var client = await TryConnectAsync();
			for (var i = 0; i < 10; i++)
			{
				var first = await client.QueryBoundsAsync("anchorable-tab", "toolWindow1");
				var second = await client.QueryBoundsAsync("anchorable-tab", "toolWindow2");
				await NativeInputEnvironment.MovePointerAsync(client, first.CenterX, first.CenterY, TestContext.Current.CancellationToken);
				await NativeInputEnvironment.MovePointerAsync(client, second.CenterX, second.CenterY, TestContext.Current.CancellationToken);
			}
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task RapidKeyboardInput_DoesNotLag_Issue162()
		{
			using var client = await TryConnectAsync();
			await client.InvokeAsync("avd.focus.set", "document1");
			await client.InvokeAsync("avd.input.text", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
			for (var i = 0; i < 10; i++)
			{
				using var left = JsonDocument.Parse(await client.InvokeAsync("avd.input.key", "Left"));
				AssertNoUnexpectedKeyError(left.RootElement);
				using var right = JsonDocument.Parse(await client.InvokeAsync("avd.input.key", "Right"));
				AssertNoUnexpectedKeyError(right.RootElement);
			}
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task RapidWindowResize_DoesNotCrash_Issue101()
		{
			using var client = await TryConnectAsync();
			using var original = JsonDocument.Parse(await client.InvokeAsync("avd.query.window"));
			var width = original.RootElement.GetProperty("width").GetDouble();
			var height = original.RootElement.GetProperty("height").GetDouble();
			for (var i = 0; i < 5; i++)
				await client.InvokeAsync("avd.window.resize", width - 100 + (i * 40), height - 50 + (i * 20));
			await client.InvokeAsync("avd.window.resize", width, height);
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task InteractionDuringLayout_DoesNotCrash_Issue42()
		{
			using var client = await TryConnectAsync();
			await client.InvokeAsync("avd.menu.invoke", "Layout/Save/Layout_4");
			await client.InvokeAsync("avd.document.activate", "document2");
			await client.InvokeAsync("avd.document.activate", "document1");
			await client.InvokeAsync("avd.menu.invoke", "Layout/Load/Layout_4");
			await client.InvokeAsync("avd.document.activate", "document1");
			Assert.Contains("Document 1", DocumentTitles(await QueryLayoutAsync(client)));
		}

		[Fact]
		public async Task RapidSaveLoadLayout_Stability_Issue356()
		{
			using var client = await TryConnectAsync();
			for (var i = 0; i < 10; i++)
			{
				await client.InvokeAsync("avd.menu.invoke", "Layout/Save/Layout_4");
				await client.InvokeAsync("avd.menu.invoke", "Layout/Load/Layout_4");
			}
			Assert.Contains("Document 1", DocumentTitles(await QueryLayoutAsync(client)));
			await AssertAppAliveAsync(client);
		}

		private static void AssertNoUnexpectedKeyError(JsonElement result)
		{
			var error = ReadString(result, "error");
			if (error != null)
				Assert.Contains(KeyboardBackendLimitations, limitation => error.StartsWith(limitation, StringComparison.Ordinal));
		}
	}

}
