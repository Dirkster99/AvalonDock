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
		public static bool SupportsNativeDrag =>
			OperatingSystem.IsMacOS() || OperatingSystem.IsWindows() || IsLinuxXInputAvailable;

		/// <summary>
		/// Linux injection goes through XTest, which needs a reachable X display. That covers X11 and
		/// XWayland; a pure Wayland session has no DISPLAY and is correctly reported as unsupported.
		/// </summary>
		public static bool IsLinuxXInputAvailable =>
			OperatingSystem.IsLinux() &&
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

		/// <summary>
		/// Linux does the whole decomposed press/drag-move/release gesture inside the agent (XTest), so
		/// no external tool is involved. macOS and Windows drive the bundled CliclickSharp helper.
		/// </summary>
		public static bool UsesAgentDecomposedInput => IsLinuxXInputAvailable;

		// Decomposed press/drag-move/release: the agent implements these via cliclick on macOS and
		// Windows (the bundled CliclickSharp) and XTest on Linux.
		public static bool RequiresCliclick => OperatingSystem.IsMacOS() || OperatingSystem.IsWindows();

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
			// Linux drives the decomposed endpoints through XTest, which holds the button state across
			// separate calls, so cliclick is not involved there.
			if (UsesAgentDecomposedInput)
				return;

			Assert.SkipUnless(RequiresCliclick,
				"Decomposed native input (press/drag-move/release) has no implementation on this " +
				"platform - WpfAgentService supports it via cliclick on macOS and Windows, or XTest on " +
				"Linux with a reachable X display.");

			if (!IsCliclickAvailable)
			{
				Assert.Fail(
					"cliclick is required for native press/drag-move/release input on macOS and Windows but " +
					"was not found (checked the bundled CliclickSharp next to the tests, then " +
					"/opt/homebrew/bin/cliclick, /usr/local/bin/cliclick and PATH on macOS).");
			}
		}

		/// <summary>Call at the top of any test that uses the monolithic avd.drag/avd.click endpoints.</summary>
		public static void EnsureNativeDragAvailable()
		{
			Assert.SkipUnless(SupportsNativeDrag,
				"Native OS-level drag/click injection has no implementation on this platform " +
				"(Linux needs a reachable X display for XTest).");
		}

		// Mirrors CliclickInput.ResolvePath (LeXtudio.DevFlow.Agent.Core) so this enforcement checks
		// exactly what the agent itself will try, rather than a looser or stricter approximation.
		private static bool ResolveCliclickAvailable() => ResolveCliclickPath() != null;

		private static string ResolveCliclickPath()
		{
			// The helper the DevFlow package ships (CliclickSharp, a C# port with cliclick's syntax)
			// is copied next to this assembly by the package's build targets.
			if (OperatingSystem.IsWindows())
			{
				// Windows on Arm: the native arm64 build when the package ships one, not the x64 one under emulation.
				if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
				{
					var bundledArm64 = Path.Combine(AppContext.BaseDirectory, "CliclickSharp.arm64.exe");
					if (File.Exists(bundledArm64))
						return bundledArm64;
				}

				var bundledWindows = Path.Combine(AppContext.BaseDirectory, "CliclickSharp.exe");
				return File.Exists(bundledWindows) ? bundledWindows : null;
			}

			if (!OperatingSystem.IsMacOS())
				return null;

			foreach (var candidate in new[] { "/opt/homebrew/bin/cliclick", "/usr/local/bin/cliclick" })
			{
				if (File.Exists(candidate))
					return candidate;
			}

			var pathVar = Environment.GetEnvironmentVariable("PATH");
			if (pathVar is null)
				return BundledMacCliclick();

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

			return BundledMacCliclick();
		}

		// The macOS legs install the original cliclick and the drag cases are tuned to it: with the C# port
		// first the same drags stopped reaching the drop targets, so the port is only the fallback there.
		private static string BundledMacCliclick()
		{
			var bundled = Path.Combine(AppContext.BaseDirectory, "CliclickSharp");
			return File.Exists(bundled) ? bundled : null;
		}

		/// <summary>
		/// Moves the OS pointer without pressing anything: through the agent where it keeps the button
		/// state itself (Linux/XTest), through cliclick's plain <c>m:</c> elsewhere (the agent's drag-move
		/// reports failure on macOS when no button is held).
		/// </summary>
		public static async System.Threading.Tasks.Task MovePointerAsync(DevFlowClient client, double x, double y, System.Threading.CancellationToken ct)
		{
			if (UsesAgentDecomposedInput)
			{
				await client.DragMoveAsync(x, y, ct).ConfigureAwait(false);
				return;
			}

			Assert.SkipUnless(CliclickPath != null, "No cliclick is available to move the pointer.");
			var startInfo = new System.Diagnostics.ProcessStartInfo { FileName = CliclickPath, UseShellExecute = false };
			startInfo.ArgumentList.Add($"m:{(int)Math.Round(x)},{(int)Math.Round(y)}");
			using var process = System.Diagnostics.Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start cliclick move.");
			await process.WaitForExitAsync(ct).ConfigureAwait(false);
			Assert.Equal(0, process.ExitCode);
		}
	}
}
