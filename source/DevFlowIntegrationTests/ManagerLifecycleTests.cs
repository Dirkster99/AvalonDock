using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Unloading and reloading the docking manager inside its host (#384, #437, #159) and the menu bar
	/// outside it. Ported from the FlaUI <c>ManagerLifecycleTests</c> fixture.
	/// </summary>
	/// <remarks>
	/// "Unload" removes the manager from the window's visual tree; the layout model survives, so the
	/// observable state is <c>managerHosted</c> rather than the document list. Where the original only
	/// asserted that the process was still alive, this port asserts that the manager actually left the
	/// tree and came back - in particular the #437 case, whose FlaUI version never unloaded anything.
	/// </remarks>
	[Collection("DevFlow")]
	public sealed class ManagerLifecycleTests : PortedTestAppTestBase
	{
		public ManagerLifecycleTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task UnloadAndReloadManager_Works_Issue384()
		{
			using var client = await TryConnectAsync();

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
			Assert.True(await IsManagerHostedAsync(client), "The manager should be hosted before the test starts.");

			await client.InvokeAsync("avd.menu.invoke", "Layout/Unload Manager");
			await WaitUntilAsync(
				async () => !await IsManagerHostedAsync(client),
				TimeSpan.FromSeconds(5),
				"'Layout > Unload Manager' did not remove the manager from the window.");

			// While unloaded the docking manager is not part of the visual tree at all, which is the
			// behaviour the FlaUI comment describes but never checked.
			// A manager that is out of the tree cannot be converted to screen coordinates at all, which
			// the bounds query reports as an error rather than as an empty rectangle.
			var inTree = true;
			try
			{
				var unloaded = await client.QueryBoundsAsync("manager");
				inTree = unloaded.Width > 0 || unloaded.Height > 0;
			}
			catch (HttpRequestException ex) when (ex.Message.Contains("Could not convert element point", StringComparison.Ordinal))
			{
				inTree = false;
			}
			catch (InvalidOperationException)
			{
				// Reported as empty (or not found) bounds, which the client rejects.
				inTree = false;
			}

			Assert.False(inTree, "The manager is still in the visual tree while unloaded.");
			await AssertAppAliveAsync(client);

			await client.InvokeAsync("avd.menu.invoke", "Layout/Load Manager");
			await WaitUntilAsync(
				() => IsManagerHostedAsync(client),
				TimeSpan.FromSeconds(5),
				"'Layout > Load Manager' did not put the manager back.");

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task UnloadManagerWithFloatingWindow_DoesNotCrash_Issue437()
		{
			using var client = await TryConnectAsync();

			// The FlaUI case created the floating window and then stopped without ever unloading, so
			// #437 was never actually exercised. Unloading with the window open is the regression.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", "Tools/New floating window")))
			{
			}

			await WaitUntilAsync(
				async () => ReadArray(await QueryLayoutAsync(client), "floatingWindows").Any(),
				TimeSpan.FromSeconds(10),
				"'Tools > New floating window' did not create a floating window.");

			await client.InvokeAsync("avd.menu.invoke", "Layout/Unload Manager");
			await WaitUntilAsync(
				async () => !await IsManagerHostedAsync(client),
				TimeSpan.FromSeconds(5),
				"Unloading the manager with a floating window open did not complete.");

			await AssertAppAliveAsync(client);

			await client.InvokeAsync("avd.menu.invoke", "Layout/Load Manager");
			await WaitUntilAsync(
				() => IsManagerHostedAsync(client),
				TimeSpan.FromSeconds(5),
				"Reloading the manager after unloading it with a floating window open did not complete.");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task RepeatedUnloadReload_DoesNotCrash_Issue159()
		{
			using var client = await TryConnectAsync();

			for (var cycle = 1; cycle <= 3; cycle++)
			{
				await client.InvokeAsync("avd.menu.invoke", "Layout/Unload Manager");
				await WaitUntilAsync(
					async () => !await IsManagerHostedAsync(client),
					TimeSpan.FromSeconds(5),
					$"Cycle {cycle}: the manager did not unload.");

				await client.InvokeAsync("avd.menu.invoke", "Layout/Load Manager");
				await WaitUntilAsync(
					() => IsManagerHostedAsync(client),
					TimeSpan.FromSeconds(5),
					$"Cycle {cycle}: the manager did not reload.");
			}

			Assert.NotNull(FindContent(await QueryLayoutAsync(client), "documents", "Document 1"));
			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task MenusWork_AfterManagerReload()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.menu.invoke", "Layout/Unload Manager");
			await WaitUntilAsync(
				async () => !await IsManagerHostedAsync(client),
				TimeSpan.FromSeconds(5),
				"The manager did not unload.");

			await client.InvokeAsync("avd.menu.invoke", "Layout/Load Manager");
			await WaitUntilAsync(() => IsManagerHostedAsync(client), TimeSpan.FromSeconds(5), "The manager did not reload.");

			using var menu = JsonDocument.Parse(await client.InvokeAsync("avd.menu.items"));
			var headers = ReadArray(menu.RootElement, "items").Select(i => ReadString(i, "header")).ToArray();
			Assert.Contains("Edit", headers);
			Assert.Contains("Tools", headers);
		}

		private static async Task<bool> IsManagerHostedAsync(DevFlowClient client)
		{
			using var window = JsonDocument.Parse(await client.InvokeAsync("avd.query.window"));
			return window.RootElement.GetProperty("managerHosted").GetBoolean();
		}
	}
}
