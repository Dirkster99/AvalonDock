using System.Threading.Tasks;

namespace AvalonDock.DevFlowIntegrationTests
{
	public abstract class IntegrationTestBase
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
	}
}
