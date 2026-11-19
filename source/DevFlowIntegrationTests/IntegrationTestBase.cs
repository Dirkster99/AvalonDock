using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Shared plumbing for suites that talk to a DevFlow agent: which port to connect to, and the
	/// per-test window-position guard.
	/// </summary>
	/// <remarks>
	/// The port is a property of the subclass, not a static: two suites run against two different
	/// sample applications (TestApp on 9223, AvalonDockCodeApp on 9224), and a process-wide "the port"
	/// would make whichever suite initialised first decide where every later suite connects.
	/// </remarks>
	public abstract class IntegrationTestBase : IAsyncLifetime
	{
		private readonly DevFlowAppFixture _fixture;

		protected DevFlowAppFixture Fixture => _fixture;

		protected IntegrationTestBase()
		{
		}

		protected IntegrationTestBase(DevFlowAppFixture fixture)
		{
			_fixture = fixture;
		}

		/// <summary>The DevFlow port of the application this suite drives.</summary>
		protected abstract int AgentPort { get; }

		protected DevFlowClient CreateClient() => new(AgentPort);

		protected async Task<DevFlowClient> TryConnectAsync()
		{
			var client = CreateClient();
			if (await client.IsReachableAsync())
			{
				// Failures captured from the shared plumbing (a failed agent call, a state that never
				// arrived) name their files after the case that is running and talk to this agent.
				FailureArtifacts.ActiveClient = client;
				return client;
			}

			client.Dispose();
			throw new InvalidOperationException($"DevFlow agent not reachable on port {AgentPort}.");
		}

		/// <summary>Runs one case and captures the screen and the app state if it fails.</summary>
		protected static Task RunWithFailureArtifactsAsync(DevFlowClient client, string caseName, Func<Task> body)
			=> FailureArtifacts.RunAsync(client, caseName, body);

		protected Task IsolateDesktopForNativeInputAsync()
		{
			if (_fixture == null)
				return Task.CompletedTask;

			return _fixture.IsolateDesktopForNativeInputAsync();
		}

		public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

		public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}

	/// <summary>
	/// Base for suites that drive TestApp, which can be repositioned and restarted (the native-input
	/// tests need both). AvalonDockCodeApp has no such actions, so that plumbing stays here.
	/// </summary>
	public abstract class TestAppIntegrationTestBase : IntegrationTestBase
	{
		private const int DefaultPort = 9223;

		protected TestAppIntegrationTestBase()
		{
		}

		protected TestAppIntegrationTestBase(DevFlowAppFixture fixture)
			: base(fixture)
		{
		}

		protected override int AgentPort =>
			int.TryParse(Environment.GetEnvironmentVariable("DEVFLOW_TEST_PORT"), out var port) && port > 0
				? port
				: DefaultPort;

		protected Task RestartTestAppAsync()
		{
			return RestartTestAppAndPositionGuardAsync();
		}

		public override async ValueTask InitializeAsync()
		{
			await PositionAndArmMainWindowGuardAsync();
		}

		public override async ValueTask DisposeAsync()
		{
			await AssertMainWindowDidNotMoveAsync();
		}

		private async Task RestartTestAppAndPositionGuardAsync()
		{
			await AssertMainWindowDidNotMoveAsync();
			if (Fixture != null)
				await Fixture.RestartAsync();
			await PositionAndArmMainWindowGuardAsync();
		}

		private async Task PositionAndArmMainWindowGuardAsync()
		{
			using var client = await TryConnectAsync();
			await client.InvokeAsync("avd.position-main-window", 50, 40);
			await client.InvokeAsync("avd.main-window-position-guard.start");
		}

		private async Task AssertMainWindowDidNotMoveAsync()
		{
			using var client = await TryConnectAsync();

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
