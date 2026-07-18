using System;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	// Ported from the standalone tests/AvalonDock.IntegrationTests project (merged in):
	// float/dock/hide/show error paths, theme switching, new-floating-window, and a
	// global-coordinate drag smoke test, none of which AvalonDockLayoutIntegrationTests covers.
	//
	// avd.test-layout.reset has no counterpart to restore the *default* (toolWindow1/
	// toolWindow2/document1) layout, and every test in this collection shares one live
	// TestApp instance — so every test here resets to the dragTest* layout itself and
	// restores the pre-test layout in a finally block, instead of assuming the default
	// layout's content IDs are still present.
	[Collection("DevFlow")]
	public sealed class DragDropFeatureIntegrationTests : IntegrationTestBase
	{
		public DragDropFeatureIntegrationTests(DevFlowAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task Float_UnknownAnchorable_ReturnsError()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var result = await client.InvokeAsync("avd.float", "nonexistent-id");
			Assert.Contains("not found", result);
		}

		[Fact]
		public async Task Dock_NonFloatingAnchorable_ReturnsError()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);

			// avd.dock (MainWindow.xaml.cs) only searches dockManager.Layout.FloatingWindows -
			// there is no distinct "not floating" message; a docked anchorable and an unknown
			// ContentId both return "not found".
			var result = await client.InvokeAsync("avd.dock", "dragTestTool");
			Assert.Contains("not found", result);
		}

		[Fact]
		public async Task Show_NonHiddenAnchorable_ReturnsError()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);

			var result = await client.InvokeAsync("avd.show", "dragTestTool");
			Assert.Contains("not hidden", result);
		}

		[Fact]
		public async Task AddAnchorable_ThenQuery()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			try
			{
				var before = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));

				var result = await client.InvokeAsync("avd.add-anchorable", "Test Anchorable");
				Assert.Contains("Added anchorable", result);

				var after = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.Equal(before.Anchorables.Count + 1, after.Anchorables.Count);
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		[Theory]
		[InlineData("ArcDark")]
		[InlineData("ArcLight")]
		[InlineData("VS2013Blue")]
		[InlineData("VS2013Dark")]
		[InlineData("VS2013Light")]
		[InlineData("Metro")]
		[InlineData("Aero")]
		public async Task SwitchTheme_Verifies(string themeTag)
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			try
			{
				var result = await client.InvokeAsync("avd.switch-theme", themeTag);
				Assert.Equal($"Switched to '{themeTag}'", result);
			}
			finally
			{
				// Restore MainWindow.xaml's actual startup theme (ArcDarkTheme) so later tests in this
				// run don't inherit whichever theme this Theory iteration left applied. Using "Generic"
				// here previously left the app in its bland, un-styled fallback theme for the rest of
				// the process's lifetime (there's no per-test app restart) - which silently changes the
				// docking chrome's visual tree enough to break drop-target compass detection in
				// DropTargetZoneIntegrationTests for every test that happened to run afterward.
				await client.InvokeAsync("avd.switch-theme", "ArcDark");
			}
		}

		[Fact]
		public async Task SwitchTheme_Unknown_ReturnsError()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			var result = await client.InvokeAsync("avd.switch-theme", "NonexistentTheme");
			Assert.Contains("Unknown theme", result);
		}

		[Fact]
		public async Task NewFloatingWindow_CreatesFloatingAnchorable()
		{
			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			try
			{
				var before = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));

				var result = await client.InvokeAsync("avd.new-floating", "My Floating Window");
				Assert.Contains("Created floating", result);

				var after = DockLayoutSnapshot.Parse(await client.InvokeAsync("avd.query.layout"));
				Assert.True(after.FloatingWindows.Count > before.FloatingWindows.Count,
					$"Expected more floating windows ({after.FloatingWindows.Count} > {before.FloatingWindows.Count})");
			}
			finally
			{
				// The new floating window has no ContentId to dock back individually, so
				// restore the whole pre-test layout to close it and avoid leaking a real
				// native window into later tests.
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}

		[Fact]
		public async Task Drag_GlobalCoordinates_Succeeds()
		{
			NativeInputEnvironment.EnsureNativeDragAvailable();
			await IsolateDesktopForNativeInputAsync();

			using var client = await TryConnectAsync();
			if (client == null)
				return;

			await client.InvokeAsync("avd.test-layout.reset");
			await client.WaitForAvalonDockTestLayoutReadyAsync(TestContext.Current.CancellationToken);
			var originalXml = await client.InvokeAsync("avd.layout.serialize");
			try
			{
				var floatResult = await client.InvokeAsync("avd.new-floating", "Drag Test Window");
				Assert.Contains("Created floating", floatResult);

					var manager = await client.QueryBoundsAsync("manager");
					await NativeInputIntegrationTests.AssertSafeDragStartAsync(
						client,
						manager.CenterX,
						manager.CenterY,
						"DockingManager",
						TestContext.Current.CancellationToken);

					var dragResult = await client.DragAsync(new DragRequest
				{
					FromX = manager.CenterX,
					FromY = manager.CenterY,
					ToX = manager.CenterX + 8,
					ToY = manager.CenterY + 8,
					Steps = 12,
					Global = true,
				}, TestContext.Current.CancellationToken);

				Assert.True(dragResult.TryGetProperty("ok", out var ok), "drag response missing 'ok'");
				Assert.True(ok.GetBoolean(), "drag returned ok=false");
			}
			finally
			{
				await client.InvokeAsync("avd.layout.restore", originalXml);
			}
		}
	}
}
