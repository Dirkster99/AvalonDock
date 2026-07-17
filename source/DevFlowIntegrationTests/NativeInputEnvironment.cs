using System;
using System.IO;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	// Centralizes native-input platform/tooling checks so native-drag tests genuinely pass, fail, or
	// report a real xUnit "skipped" status - instead of silently `return`ing at the top of the test
	// body, which is what every native-input test in this project did previously (gated behind an
	// opt-in DEVFLOW_TEST_NATIVE_INPUT/DEVFLOW_TEST_NATIVE_SMOKE env var that was never set by
	// default in any local run or CI). A silent early return is indistinguishable from a real pass in
	// xUnit's summary, so none of those tests were ever actually exercising native input.
	internal static class NativeInputEnvironment
	{
		// Full avd.drag/avd.click: WpfAgentService (LeXtudio.DevFlow.Agent.Wpf) falls back to real
		// OS-level injection - CGEventPost on macOS, SendInput on Windows - via
		// TryNativeMouseDrag/TryNativeMouseClick when no portable-WPF-dispatcher path is available.
		public static bool SupportsNativeDrag => OperatingSystem.IsMacOS() || OperatingSystem.IsWindows();

		// Decomposed press/drag-move/release: WpfAgentService only implements these via cliclick's
		// CGEventPost wrapper on macOS (TryPressResponseAsync/TryDragMoveResponseAsync/
		// TryReleaseResponseAsync) - there is no Windows fallback for them.
		public static bool RequiresCliclick => OperatingSystem.IsMacOS();

		public static bool IsCliclickAvailable { get; } = ResolveCliclickAvailable();
		public static string CliclickPath { get; } = ResolveCliclickPath();

		/// <summary>
		/// Call at the top of any test that uses the decomposed press/drag-move/release endpoints.
		/// Skips (a real xUnit skip, not a silent pass) on a platform with no implementation at all;
		/// FAILS - deliberately, not skips - on macOS if cliclick isn't actually installed, since that
		/// is a fixable environment gap rather than a platform limitation.
		/// </summary>
		public static void EnsureDecomposedInputAvailable()
		{
			Assert.SkipUnless(RequiresCliclick,
				"Decomposed native input (press/drag-move/release) has no implementation on this " +
				"platform - WpfAgentService only supports it via cliclick on macOS.");

			if (!IsCliclickAvailable)
			{
				Assert.Fail(
					"cliclick is required for native press/drag-move/release input on macOS but was " +
					"not found (checked /opt/homebrew/bin/cliclick, /usr/local/bin/cliclick, and PATH). " +
					"Install it with 'brew install cliclick'.");
			}
		}

		/// <summary>Call at the top of any test that uses the monolithic avd.drag/avd.click endpoints.</summary>
		public static void EnsureNativeDragAvailable()
		{
			Assert.SkipUnless(SupportsNativeDrag,
				"Native OS-level drag/click injection has no implementation on this platform.");
		}

		// Mirrors CliclickInput.ResolvePath (LeXtudio.DevFlow.Agent.Core) so this enforcement checks
		// exactly what the agent itself will try, rather than a looser or stricter approximation.
		private static bool ResolveCliclickAvailable() => ResolveCliclickPath() != null;

		private static string ResolveCliclickPath()
		{
			if (!OperatingSystem.IsMacOS())
				return null;

			foreach (var candidate in new[] { "/opt/homebrew/bin/cliclick", "/usr/local/bin/cliclick" })
			{
				if (File.Exists(candidate))
					return candidate;
			}

			var pathVar = Environment.GetEnvironmentVariable("PATH");
			if (pathVar is null)
				return null;

			foreach (var dir in pathVar.Split(Path.PathSeparator))
			{
				try
				{
					var candidate = Path.Combine(dir, "cliclick");
					if (File.Exists(candidate))
						return candidate;
				}
				catch
				{
				}
			}

			return null;
		}
	}
}
