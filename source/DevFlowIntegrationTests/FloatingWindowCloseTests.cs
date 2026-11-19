using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// #368: closing a floating tool window through the window manager (Alt+F4, taskbar, system menu)
	/// must run the anchorables' hiding lifecycle - leaving them re-showable - instead of destroying
	/// them. Ported from the FlaUI <c>FloatingWindowCloseTests</c> fixture.
	/// </summary>
	/// <remarks>
	/// The FlaUI case posted <c>WM_SYSCOMMAND/SC_CLOSE</c> with <c>PostMessage</c> and then looked for a
	/// <c>#32770</c> dialog to press. Neither is cross-platform, and the modal confirmation cannot be
	/// clicked by a headless run in any case. A window manager's close ends in the window's own
	/// Closing/Closed lifecycle, which is what <c>avd.close-floating</c> invokes, and the sample records
	/// every confirmation it asks for, which is what replaces the dialog lookup.
	/// </remarks>
	[Collection("DevFlow")]
	public sealed class FloatingWindowCloseTests : PortedTestAppTestBase
	{
		public FloatingWindowCloseTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		[Fact]
		public async Task FloatingToolWindow_SystemClose_CanBeReshown_Issue368()
		{
			using var client = await TryConnectAsync();

			using (JsonDocument.Parse(await client.InvokeAsync("avd.dialogs.answer", "yes")))
			{
			}

			// The menu path, as a user would: it floats Tool Window 1.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", "Tools/Float Tool Window1")))
			{
			}

			await WaitUntilAsync(
				async () => IsFloating(await QueryLayoutAsync(client), "Tool Window 1"),
				TimeSpan.FromSeconds(10),
				"'Tools > Float Tool Window1' did not float the tool window.");

			var floating = await client.QueryBoundsAsync("floating-window", "toolWindow1");
			Assert.True(floating.Width > 0 && floating.Height > 0, $"The floating tool window has no size. Bounds={floating}");

			// Close it the way the window manager does.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.close-floating", "toolWindow1")))
			{
			}

			await WaitUntilAsync(
				// The model keeps the emptied floating window around (it is what the tool window is
				// re-shown into), so "survived" means the window still holds the tool window.
				async () => !IsFloating(await QueryLayoutAsync(client), "Tool Window 1"),
				TimeSpan.FromSeconds(5),
				"The floating window survived its own close.",
				async () => $"dialogs={await client.InvokeAsync("avd.dialogs.query")} layout={await client.InvokeAsync("avd.query.layout")}");

			var layout = await QueryLayoutAsync(client);
			var anchorable = FindContent(layout, "anchorables", "Tool Window 1");
			Assert.NotNull(anchorable);
			Assert.False(
				ReadBool(anchorable.Value, "isVisible"),
				$"Closing the floating window should have hidden the tool window, not left it visible. State: {anchorable.Value.GetRawText()}");

			// The Hiding event is what makes the tool window re-showable, so the confirmation it raises
			// is the observable proof that the lifecycle ran.
			using var dialogs = JsonDocument.Parse(await client.InvokeAsync("avd.dialogs.query"));
			Assert.Contains(
				dialogs.RootElement.GetProperty("questions").EnumerateArray().Select(q => q.GetString() ?? string.Empty),
				question => question.Contains("hide this tool", StringComparison.OrdinalIgnoreCase));

			await AssertAppAliveAsync(client);

			// And it comes back through the menu.
			using (JsonDocument.Parse(await client.InvokeAsync("avd.menu.invoke", "Tools/Tool Window1")))
			{
			}

			await WaitUntilAsync(
				async () =>
				{
					var content = FindContent(await QueryLayoutAsync(client), "anchorables", "Tool Window 1");
					return content.HasValue && ReadBool(content.Value, "isVisible");
				},
				TimeSpan.FromSeconds(10),
				"The tool window could not be shown again after its floating window was closed (Issue #368).");
		}

		private static bool IsFloating(JsonElement layout, string title)
		{
			var anchorable = FindContent(layout, "anchorables", title);
			return anchorable.HasValue && ReadBool(anchorable.Value, "isFloat");
		}
	}
}
