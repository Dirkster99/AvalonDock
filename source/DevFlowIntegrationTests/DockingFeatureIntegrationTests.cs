using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	// Ported from the standalone tests/AvalonDock.IntegrationTests project (merged in):
	// exercises the DevFlow agent's UI-tree endpoint against the dragTest* layout,
	// complementing AvalonDockLayoutIntegrationTests which drives the same layout via
	// avd.query.layout instead of the raw visual tree.
	//
	// IMPORTANT: /api/v1/ui/tree (and /api/v1/ui/elements) resolve element bounds via
	// Visual.TransformToAncestor, which deadlocks the app's single UI thread when an
	// AvalonDock floating window is currently open (cross-window Visual bounds
	// resolution hangs acquiring the floating window's Dispatcher - a bug in the
	// LibreWpf/ProGPU multi-window hosting, not in these tests). Every test here calls
	// avd.test-layout.reset first, which explicitly closes any floating windows before
	// rebuilding the layout, to guarantee none are open when the tree is walked.
	[Collection("DevFlow")]
	public sealed class DockingFeatureIntegrationTests : IntegrationTestBase
	{
		[Fact]
		public async Task AgentStatus_ReturnsValidJson()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var status = await client.GetStatusAsync(TestContext.Current.CancellationToken);

			Assert.True(status.TryGetProperty("name", out _), "status missing 'name'");
			Assert.True(status.TryGetProperty("id", out _), "status missing 'id'");
			Assert.True(status.TryGetProperty("framework", out _), "status missing 'framework'");
		}

		[Fact]
		public async Task UITree_ReturnsNonEmpty()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var tree = await GetUITreeAsync(client);
			Assert.True(tree.GetArrayLength() > 0, "UI tree is empty");
		}

		[Fact]
		public async Task UITree_ContainsDockingManager()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var tree = await GetUITreeAsync(client);
			Assert.True(FindNodeByName(tree, "dockManager"), "UI tree does not contain DockingManager");
		}

		[Fact]
		public async Task FloatingWindows_CanBeDetected()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var tree = await GetUITreeAsync(client);
			var floatingWindows = FindNodesByType(tree, "LayoutFloatingWindowControl");
			Assert.NotNull(floatingWindows);
		}

		[Fact]
		public async Task AnchorablePanes_CanBeDetected()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var tree = await GetUITreeAsync(client);
			var anchorablePanes = FindNodesByType(tree, "LayoutAnchorablePaneControl");
			Assert.NotNull(anchorablePanes);
		}

		[Fact]
		public async Task DocumentPanes_CanBeDetected()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var tree = await GetUITreeAsync(client);
			var documentPanes = FindNodesByType(tree, "LayoutDocumentPaneControl");
			Assert.NotNull(documentPanes);
		}

		private static async Task<JsonElement> GetUITreeAsync(DevFlowClient client)
		{
			// See the class-level comment: closes any open floating window before the walk.
			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);

			var raw = await client.GetTreeAsync(TestContext.Current.CancellationToken);
			var root = JsonDocument.Parse(raw).RootElement.Clone();
			// The endpoint wraps the root array as {"elements": [...]}, not a bare array.
			return root.TryGetProperty("elements", out var elements) ? elements : root;
		}

		private static bool FindNodeByName(JsonElement element, string name)
		{
			if (element.ValueKind == JsonValueKind.Object)
			{
				// WpfVisualTreeWalker.GetStableId uses the FrameworkElement's x:Name as the
				// element's "id" when set (falling back to a generated id otherwise) - there is
				// no separate top-level "name" property on the serialized element.
				if (element.TryGetProperty("id", out var idProp) && idProp.GetString() == name)
					return true;

				if (element.TryGetProperty("children", out var children))
				{
					foreach (var child in children.EnumerateArray())
					{
						if (FindNodeByName(child, name))
							return true;
					}
				}
			}
			else if (element.ValueKind == JsonValueKind.Array)
			{
				foreach (var item in element.EnumerateArray())
				{
					if (FindNodeByName(item, name))
						return true;
				}
			}
			return false;
		}

		private static System.Collections.Generic.List<JsonElement> FindNodesByType(JsonElement element, string type)
		{
			var result = new System.Collections.Generic.List<JsonElement>();
			FindNodesByTypeRecursive(element, type, result);
			return result;
		}

		private static void FindNodesByTypeRecursive(JsonElement element, string type, System.Collections.Generic.List<JsonElement> result)
		{
			if (element.ValueKind == JsonValueKind.Object)
			{
				if (element.TryGetProperty("type", out var typeProp) && typeProp.GetString()?.Contains(type) == true)
					result.Add(element);

				if (element.TryGetProperty("children", out var children))
				{
					foreach (var child in children.EnumerateArray())
						FindNodesByTypeRecursive(child, type, result);
				}
			}
			else if (element.ValueKind == JsonValueKind.Array)
			{
				foreach (var item in element.EnumerateArray())
					FindNodesByTypeRecursive(item, type, result);
			}
		}
	}
}
