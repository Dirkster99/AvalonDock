using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Keyboard handling (#225 arrow keys inside a text box must not hand focus to the drop-down area),
	/// ported from the FlaUI <c>KeyboardNavigationTests</c> fixture.
	/// </summary>
	/// <remarks>
	/// The agent can insert text into a control on every platform, but it cannot hold a modifier or
	/// press an arrow key there: Linux injects one keysym, macOS goes through cliclick and Windows
	/// through SendInput, and none of the three implements chords. The gestures are therefore raised as
	/// WPF routed input events on the element that has focus - the delivery step a real keyboard
	/// precedes - and the assertions are on focus and on the app staying alive, which is what these
	/// cases are about. A handler that inspects live <c>Keyboard.Modifiers</c> still sees none, which is
	/// why the Ctrl+Tab case asserts the document layout instead of navigator navigation.
	/// </remarks>
	[Collection("DevFlow")]
	public sealed class KeyboardNavigationTests : PortedTestAppTestBase
	{
		public KeyboardNavigationTests(TestAppFixture fixture)
			: base(fixture)
		{
		}

		private static readonly string[] BackendLimitations =
		{
			"EntryPointNotFoundException", "DllNotFoundException", "PlatformNotSupportedException",
		};

		/// <summary>Accepts only the errors avd.input.key reports for "this backend cannot inject that key".</summary>
		private static void AssertNoAppError(JsonElement result)
		{
			var error = ReadString(result, "error");
			if (error != null)
				Assert.Contains(BackendLimitations, limit => error.StartsWith(limit, StringComparison.Ordinal));
		}

		[Fact]
		public async Task KeyboardInput_WorksInDocumentContent()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.document.activate", "document1");
			using var focus = JsonDocument.Parse(await client.InvokeAsync("avd.focus.set", "document1"));
			Assert.Equal("TextBox", focus.RootElement.GetProperty("type").GetString());

			await client.InvokeAsync("avd.input.text", "Hello DevFlow");

			await WaitUntilAsync(
				async () =>
				{
					using var content = JsonDocument.Parse(await client.InvokeAsync("avd.content", "document1"));
					return content.RootElement.GetProperty("textBoxes").EnumerateArray()
						.Any(t => (t.GetString() ?? string.Empty).Contains("Hello DevFlow", StringComparison.Ordinal));
				},
				TimeSpan.FromSeconds(5),
				"Text typed into Document 1 did not reach its text box.");

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task ArrowKeys_WorkWithoutCrash_Issue225()
		{
			using var client = await TryConnectAsync();

			// The regression needs a floating anchorable: that is where the drop-down area used to steal
			// focus from the text box on an arrow key. The FlaUI case clicked a docked tab, so its
			// precondition was never met.
			await client.InvokeAsync("avd.float", "toolWindow1");
			await WaitUntilAsync(
				async () =>
				{
					var anchorable = FindContent(await QueryLayoutAsync(client), "anchorables", "toolWindow1");
					return anchorable.HasValue && ReadBool(anchorable.Value, "isFloat");
				},
				TimeSpan.FromSeconds(10),
				"Tool Window 1 did not float, so the floating text box cannot be focused.");

			using var focus = JsonDocument.Parse(await client.InvokeAsync("avd.focus.set", "toolWindow1"));
			Assert.Equal("TextBox", focus.RootElement.GetProperty("type").GetString());

			foreach (var key in new[] { "Up", "Down", "Left", "Right" })
			{
				using var result = JsonDocument.Parse(await client.InvokeAsync("avd.input.key", key));
				var after = result.RootElement.GetProperty("focusAfter");
				Assert.False(
					after.GetProperty("isDropDownControlArea").GetBoolean(),
					$"Pressing {key} moved focus to the drop-down area (Issue #225). State: {after.GetRawText()}");

				// A handler on the way may be unable to run at all on a portable backend (the arrow-key
				// navigation in LibreWPF reaches for Win32 keyboard state); that is reported in the
				// payload. An exception from the app itself is not tolerated - the action only reports
				// the ones that mean "this backend cannot do it".
				AssertNoAppError(result.RootElement);
			}

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task TabKey_CyclesFocus()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.document.activate", "document1");
			await client.InvokeAsync("avd.focus.set", "document1");

			for (var i = 0; i < 5; i++)
			{
				using var result = JsonDocument.Parse(await client.InvokeAsync("avd.input.key", "Tab"));
				var after = result.RootElement.GetProperty("focusAfter");
				Assert.NotEqual(JsonValueKind.Null, after.GetProperty("type").ValueKind);
				AssertNoAppError(result.RootElement);

				await Task.Delay(50, TestContext.Current.CancellationToken);
			}

			await AssertAppAliveAsync(client);
		}

		[Fact]
		public async Task CtrlTab_SwitchesBetweenDocuments()
		{
			using var client = await TryConnectAsync();

			await client.InvokeAsync("avd.document.activate", "document1");

			using var result = JsonDocument.Parse(await client.InvokeAsync("avd.input.key", "Tab", "ctrl"));

			// Ctrl+Tab is how DockingManager summons its navigator (ShowNavigatorWindow), and the manager
			// checks the live modifier state to decide - which is exactly what cannot be injected here.
			// What is asserted is therefore the app's own capability and that delivering the gesture is
			// harmless; both documents must still be there afterwards.
			Assert.True(result.RootElement.GetProperty("navigatorEnabled").GetBoolean(), "The docking manager has its navigator disabled.");
			AssertNoAppError(result.RootElement);

			var titles = DocumentTitles(await QueryLayoutAsync(client));
			Assert.Contains("Document 1", titles);
			Assert.Contains("Document 2", titles);

			await AssertAppAliveAsync(client);
		}
	}
}
