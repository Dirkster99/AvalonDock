using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	public abstract class IntegrationTestBase : IAsyncLifetime
	{
		private static int? _fixturePort;
		private readonly DevFlowAppFixture _fixture;

		protected IntegrationTestBase()
		{
		}

		protected IntegrationTestBase(DevFlowAppFixture fixture)
		{
			_fixture = fixture;
		}

		internal static void SetFixturePort(int port) => _fixturePort = port;

		protected static async Task<DevFlowClient> TryConnectAsync()
		{
			var port = _fixturePort ?? DevFlowClient.ResolvePortOrNull() ?? 9223;
			var client = new DevFlowClient(port);
			if (await client.IsReachableAsync())
				return client;

			client.Dispose();
			if (_fixturePort.HasValue || DevFlowClient.ResolvePortOrNull().HasValue)
				throw new System.InvalidOperationException($"DevFlow agent not reachable on port {port}.");

			return null;
		}

		protected Task IsolateDesktopForNativeInputAsync()
		{
			if (_fixture == null)
				return Task.CompletedTask;

			return _fixture.IsolateDesktopForNativeInputAsync();
		}

		protected Task RestartTestAppAsync()
		{
			return RestartTestAppAndPositionGuardAsync();
		}

		public async ValueTask InitializeAsync()
		{
			await PositionAndArmMainWindowGuardAsync();
		}

		public async ValueTask DisposeAsync()
		{
			await AssertMainWindowDidNotMoveAsync();
		}

		private async Task RestartTestAppAndPositionGuardAsync()
		{
			await AssertMainWindowDidNotMoveAsync();
			if (_fixture != null)
				await _fixture.RestartAsync();
			await PositionAndArmMainWindowGuardAsync();
		}

		private static async Task PositionAndArmMainWindowGuardAsync()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.position-main-window", 50, 40);
			await client.InvokeAsync("avd.main-window-position-guard.start");
		}

		private static async Task AssertMainWindowDidNotMoveAsync()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			using var result = JsonDocument.Parse(
				await client.InvokeAsync("avd.main-window-position-guard.query"));
			var root = result.RootElement;
			Assert.True(root.GetProperty("armed").GetBoolean(), "Main-window position guard was not armed.");
			Assert.False(
				root.GetProperty("moved").GetBoolean(),
				$"Main window moved during the test. Guard state: {root.GetRawText()}");
		}
	}
}
