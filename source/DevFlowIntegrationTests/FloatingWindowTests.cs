using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// The floating window <c>Tools ▸ New floating window</c> creates: exactly one (#254), activatable
	/// (#408), sized to its content (#174), and harmless to the main window (#349). Ported from the
	/// FlaUI <c>FloatingWindowTests</c> fixture.
	/// </summary>
	[Collection("DevFlow")]
	public sealed class FloatingWindowTests : PortedTestAppTestBase
	{
		public FloatingWindowTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task NewFloatingWindow_CreatesFloatingWindow_Issue254()
		{
			using var client = await TryConnectAsync();

			var before = ReadArray(await QueryLayoutAsync(client), "floatingWindows").Count();

			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", "Tools/New floating window")))
			{
			}

			await WaitUntilAsync(
				async () => ReadArray(await QueryLayoutAsync(client), "floatingWindows").Count() > before,
				TimeSpan.FromSeconds(10),
				"'Tools > New floating window' did not create a floating window.");

			// #254 was the entry creating two windows, which "at least one" would never have caught.
			var after = ReadArray(await QueryLayoutAsync(client), "floatingWindows").Count();
			Assert.Equal(before + 1, after);
		}

		[Fact]
		public async Task FloatingWindow_CanBeActivated_Issue408()
		{
			using var client = await TryConnectAsync();

			await CreateFloatingWindowAsync(client);
			var bounds = await client.QueryBoundsAsync("floating-window");

			// #408: a floating window can be activated and stays on screen.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.activate-floating")))
			{
			}

			await WaitUntilAsync(
				async () =>
				{
					var current = await QueryLayoutAsync(client);
					return ReadArray(current, "floatingWindows").Any(f =>
						ReadBool(f, "isVisible") || ReadArray(f, "contentIds").Any());
				},
				TimeSpan.FromSeconds(5),
				"The floating window disappeared when it was activated.");

			var after = await client.QueryBoundsAsync("floating-window");
			Assert.True(after.Width > 0 && after.Height > 0, $"The floating window has no size after activation. Bounds={after}");
			Assert.True(after.X + after.Width > 0 && after.Y + after.Height > 0, $"The floating window is off screen. Bounds={after}");
			Assert.True(bounds.Width > 0, $"The floating window had no size before activation. Bounds={bounds}");
		}

		[Fact]
		public async Task MainWindow_AccessibleWithFloatingWindow_Issue349()
		{
			using var client = await TryConnectAsync();

			await CreateFloatingWindowAsync(client);

			// #349: the docking window must stay usable while a floating window is open.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.activate")))
			{
			}

			var layout = await QueryLayoutAsync(client);
			Assert.NotNull(FindContent(layout, "documents", "Document 1"));

			using var report = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
			Assert.True(report.RootElement.GetProperty("viewLoaded").GetBoolean());
			Assert.Contains(
				"Document 1 Content",
				report.RootElement.GetProperty("textBoxes").EnumerateArray().Select(t => t.GetString() ?? string.Empty));
		}

		[Fact]
		public async Task FloatingWindow_HasReasonableSize_Issue174()
		{
			using var client = await TryConnectAsync();

			await CreateFloatingWindowAsync(client);

			// The window is reported as soon as it is in the layout, which is before its first
			// measure pass has sized it, so give the layout a moment before judging the size.
			ElementBounds bounds = default;
			string lastError = null;
			var settle = DateTime.UtcNow + TimeSpan.FromSeconds(5);
			do
			{
				try
				{
					bounds = await client.QueryBoundsAsync("floating-window");
					lastError = null;
					if (bounds.Width >= 300 && bounds.Height >= 300)
						break;
				}
				catch (InvalidOperationException ex)
				{
					// An unsized window reports empty bounds, which the client rejects.
					lastError = ex.Message;
				}

				await Task.Delay(250, TestContext.Current.CancellationToken);
			}
			while (DateTime.UtcNow < settle);

			Assert.True(lastError == null, $"The floating window never reported usable bounds: {lastError}");

			// The FlaUI case accepted anything larger than 50x50. The content declares MinWidth and
			// MinHeight of 300 and AutoWindowSizeWhenOpened is on, so a smaller window means #174 is
			// back and the content is clipped.
			Assert.True(
				bounds.Width >= 300 && bounds.Height >= 300,
				$"The floating window should be sized to its content (at least 300x300); it is {bounds.Width}x{bounds.Height}. Payload: {await client.InvokeAsync("avd.query.bounds", "floating-window")}");
		}

		private async Task CreateFloatingWindowAsync(DevFlowClient client)
		{
			var initial = await QueryLayoutAsync(client);
			if (ReadArray(initial, "floatingWindows").Any())
				return;

			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", "Tools/New floating window")))
			{
			}

			var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
			while (DateTime.UtcNow < deadline)
			{
				using var document = JsonDocument.Parse(await client.InvokeAsync("avd.query.layout"));
				if (ReadArray(document.RootElement, "floatingWindows").Any())
					return;

				await Task.Delay(200, TestContext.Current.CancellationToken);
			}

			throw new Xunit.Sdk.XunitException("'Tools > New floating window' did not create a floating window.");
		}
	}
}
