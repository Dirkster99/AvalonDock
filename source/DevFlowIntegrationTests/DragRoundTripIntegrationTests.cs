using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	public sealed class DragRoundTripIntegrationTests
	{
		[Fact]
		public async Task AgentStatus_ReportsDragCapability()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var status = await client.GetStatusAsync(TestContext.Current.CancellationToken);
			Assert.Equal("wpf", status.GetProperty("framework").GetString());
			Assert.True(status.GetProperty("capabilities").GetProperty("drag").GetBoolean());
		}

		[Fact]
		public async Task QueryLayout_ReturnsAvalonDockContent()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.WaitForAvalonDockReadyAsync(TestContext.Current.CancellationToken);
			var json = await client.InvokeAsync("avd.query.layout");
			var snap = DockLayoutSnapshot.Parse(json);

			Assert.Contains(snap.Documents, d => d.ContentId == "document1");
			Assert.Contains(snap.Anchorables, a => a.ContentId == "toolWindow1");
		}

		[Fact]
		public async Task DragEndpoint_UsesNativeMouseInjectionOnMacOS()
		{
			if (!string.Equals(Environment.GetEnvironmentVariable("DEVFLOW_TEST_NATIVE_INPUT"), "1", StringComparison.Ordinal))
				return;

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.WaitForAvalonDockReadyAsync(TestContext.Current.CancellationToken);
			var result = await client.DragAsync(new DragRequest
			{
				Global = true,
				FromX = 20,
				FromY = 20,
				ToX = 21,
				ToY = 21,
				Steps = 1
			}, TestContext.Current.CancellationToken);

			var raw = result.GetRawText();
			Assert.DoesNotContain("only supported on Windows", raw);

			if (OperatingSystem.IsMacOS())
			{
				Assert.Contains("native-global", raw);
			}
		}

		private static async Task<DevFlowClient> TryConnectAsync()
		{
			var port = DevFlowClient.ResolvePortOrNull() ?? 9223;
			var client = new DevFlowClient(port);
			if (await client.IsReachableAsync())
				return client;

			client.Dispose();
			if (DevFlowClient.ResolvePortOrNull().HasValue)
				throw new System.InvalidOperationException($"No DevFlow agent reachable on port {port}.");

			return null;
		}
	}
}
