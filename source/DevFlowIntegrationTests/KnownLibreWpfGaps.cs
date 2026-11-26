using System;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Cases that pass on real WPF (the Windows legs) and fail on LibreWPF for a reason the run has
	/// already narrowed down to the backend rather than to AvalonDock or to the test. They are skipped
	/// on LibreWPF with that reason, so the gap stays visible in every run instead of turning a leg red,
	/// and docs/librewpf.md (section 9) records the evidence for each. Set
	/// <c>DEVFLOW_RUN_KNOWN_GAPS=1</c> to run them anyway, e.g. when checking a LibreWPF fix.
	/// </summary>
	internal static class KnownLibreWpfGaps
	{
		/// <summary>The backend is LibreWPF: everywhere but Windows, where the samples use real WPF.</summary>
		public static bool IsLibreWpf => !OperatingSystem.IsWindows();

		private static bool RunAnyway =>
			string.Equals(Environment.GetEnvironmentVariable("DEVFLOW_RUN_KNOWN_GAPS"), "1", StringComparison.Ordinal);

		/// <summary>Skips on LibreWPF unless known gaps were asked for.</summary>
		public static void SkipOnLibreWpf(string gap)
		{
			if (IsLibreWpf && !RunAnyway)
				Assert.Skip($"Known LibreWPF gap (passes on real WPF): {gap}");
		}

		/// <summary>
		/// Skips on a macOS CI runner (no real desktop session) unless known gaps were asked for. On a
		/// Mac with a desktop session these cases run and pass.
		/// </summary>
		public static void SkipOnMacOS(string gap)
		{
			var onRunner = string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.Ordinal);
			if (OperatingSystem.IsMacOS() && onRunner && !RunAnyway)
				Assert.Skip($"Known macOS CI-runner gap: {gap}");
		}

		/// <summary>
		/// ProGPU's hit test finds no region for the auto-hide anchors (they sit under a rotating
		/// LayoutTransform, and the layout clip collapses to zero area). Fixed by ProGPU 02119efdf, which is
		/// not in a ProGPU package on nuget.org yet (0.1.0-preview.70 does not contain it).
		/// </summary>
		public const string AutoHideClick =
			"a click on an auto-hide anchor does not reach it: ProGPU's hit test collapses the rotated anchor's clip to zero area (fixed by ProGPU 02119efdf, not in a nuget.org ProGPU package yet)";

		/// <summary>
		/// Passed on the LibreWPF builds that carried ProGPU 02119efdf; with the nuget.org packages the click
		/// reaches the tab but does not always activate it (the tab activates on mouse up).
		/// </summary>
		public const string ToolTabClick =
			"a click on a tool-window tab does not reliably activate it with the nuget.org ProGPU packages (it passed on the builds carrying ProGPU 02119efdf)";

		public const string MacNativeDrag =
			"the cliclick-driven drag/resizer gesture does not reach AvalonDock on the macOS CI runner (no real desktop session); both cases pass on a Mac desktop";
	}
}
