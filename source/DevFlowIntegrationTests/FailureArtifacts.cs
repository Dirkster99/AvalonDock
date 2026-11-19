using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// What a failing case leaves behind so it can be understood without the machine it ran on: the screen,
	/// and the application's own reports of its layout, focus and input state.
	/// </summary>
	/// <remarks>
	/// <para>
	/// xUnit v3 has no failure callback, so this is driven from the places a failure surfaces: the client
	/// captures when a call to the agent fails, and <see cref="PortedTestAppTestBase.WaitUntilAsync"/>
	/// captures when a state never arrived. A case that wants more (a suite-specific probe, or an
	/// assertion on a returned payload) wraps its body in <see cref="RunAsync"/>.
	/// </para>
	/// <para>
	/// Everything here is best-effort. A capture that throws would replace the failure it was meant to
	/// explain, so every step is guarded and failures are reported on stderr only.
	/// </para>
	/// </remarks>
	internal static class FailureArtifacts
	{
		// A capture issues several agent calls of its own; the flag keeps a failing call from capturing
		// through the very path that is capturing it.
		[ThreadStatic]
		private static bool s_capturing;

		private static DateTime s_lastCaptureUtc = DateTime.MinValue;

		/// <summary>The agent the current case talks to, set when a case connects.</summary>
		internal static DevFlowClient ActiveClient { get; set; }

		internal static string Directory
		{
			get
			{
				// The runner uploads <workspace>/TestResults; a local run keeps the files next to the tests.
				string root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
				if (string.IsNullOrEmpty(root))
				{
					root = AppContext.BaseDirectory;
				}

				return Path.Combine(root, "TestResults", "failures");
			}
		}

		/// <summary>Runs one case and captures the desktop and the app state if it fails.</summary>
		internal static async Task RunAsync(DevFlowClient client, string caseName, Func<Task> body, params string[] extraActions)
		{
			try
			{
				await body();
			}
			catch
			{
				await CaptureAsync(client, caseName, extraActions);
				throw;
			}
		}

		/// <summary>
		/// Captures once per case: a case can fail through several waits, and one screenshot per failure
		/// is what explains it. <paramref name="caseName"/> names the files; pass the test name when known.
		/// </summary>
		internal static async Task CaptureAsync(DevFlowClient client, string caseName, params string[] extraActions)
		{
			if (client == null || s_capturing)
			{
				return;
			}

			// Two captures closer than this are the same failure being reported twice.
			DateTime now = DateTime.UtcNow;
			if (now - s_lastCaptureUtc < TimeSpan.FromSeconds(5))
			{
				return;
			}

			s_lastCaptureUtc = now;
			s_capturing = true;
			try
			{
				string prefix = Path.Combine(Directory, $"{Sanitize(caseName)}-{now:HHmmss}");
				System.IO.Directory.CreateDirectory(Directory);

				await WriteStateAsync(client, prefix + "-layout.json", "avd.query.layout");
				await WriteStateAsync(client, prefix + "-input.json", "avd.input.query");
				await WriteStateAsync(client, prefix + "-focus.json", "avd.focus.query");
				await WriteStateAsync(client, prefix + "-winforms-tree.json", "avd.winforms.tree");
				foreach (string action in extraActions)
				{
					await WriteStateAsync(client, $"{prefix}-{action.Replace('.', '_')}.json", action);
				}

				CaptureScreen(prefix + ".png");
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[failures] capture for '{caseName}': {ex.Message}");
			}
			finally
			{
				s_capturing = false;
			}
		}

		/// <summary>The name xUnit is running now, for a failure captured outside a case's own wrapper.</summary>
		internal static string CurrentCaseName
		{
			get
			{
				try
				{
					string name = TestContext.Current.Test?.TestDisplayName;
					return string.IsNullOrEmpty(name) ? "unknown-case" : name;
				}
				catch
				{
					return "unknown-case";
				}
			}
		}

		private static async Task WriteStateAsync(DevFlowClient client, string path, string action)
		{
			try
			{
				string result = await client.InvokeAsync(action);
				await File.WriteAllTextAsync(path, result);
			}
			catch (Exception ex)
			{
				// An app that does not implement the action (another sample) is not a capture failure.
				Console.Error.WriteLine($"[failures] {Path.GetFileName(path)}: {ex.Message}");
			}
		}

		/// <summary>
		/// Whether this host can be asked for a screenshot without a permission prompt: a macOS CI runner
		/// cannot, and asking blocks there.
		/// </summary>
		private static bool CanCaptureScreen =>
			!(OperatingSystem.IsMacOS() &&
				string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.Ordinal));

		/// <summary>
		/// Saves the whole screen with the platform's own tool. A tool that is missing, or a capture that
		/// fails, is reported on stderr; the state files are still there.
		/// </summary>
		private static void CaptureScreen(string path)
		{
			try
			{
				if (!CanCaptureScreen)
				{
					// macOS asks for Screen Recording permission on first use and the capture then waits for
					// an answer nobody can give on a runner, which is how a failing case turns into a hung
					// job (the state files above are still written).
					Console.Error.WriteLine("[failures] screen capture skipped on this host");
					return;
				}

				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
				if (OperatingSystem.IsMacOS())
				{
					Run("screencapture", new[] { "-x", path });
				}
				else if (OperatingSystem.IsWindows())
				{
					const string script = """
						Add-Type -AssemblyName System.Windows.Forms, System.Drawing
						$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
						$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
						[System.Drawing.Graphics]::FromImage($bitmap).CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
						$bitmap.Save($env:DEVFLOW_FAILURE_CAPTURE, [System.Drawing.Imaging.ImageFormat]::Png)
						""";
					Run(
						"pwsh",
						new[] { "-NoProfile", "-NonInteractive", "-Command", script },
						("DEVFLOW_FAILURE_CAPTURE", path));
				}
				else
				{
					// ImageMagick's import is what the Linux legs have; a bare X server has no capture tool.
					Run("import", new[] { "-window", "root", path });
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[failures] no screenshot: {ex.Message}");
			}
		}

		private static void Run(string fileName, string[] arguments, params (string Name, string Value)[] environment)
		{
			var startInfo = new ProcessStartInfo(fileName)
			{
				UseShellExecute = false,
				RedirectStandardError = true,
			};
			foreach (string argument in arguments)
			{
				startInfo.ArgumentList.Add(argument);
			}

			foreach ((string name, string value) in environment)
			{
				startInfo.Environment[name] = value;
			}

			using Process process = Process.Start(startInfo)
				?? throw new InvalidOperationException($"'{fileName}' could not be started.");
			if (!process.WaitForExit(30_000))
			{
				process.Kill();
				process.WaitForExit(5_000);
				throw new TimeoutException($"'{fileName}' did not finish in 30 seconds.");
			}

			if (process.ExitCode != 0)
			{
				throw new InvalidOperationException($"'{fileName}' exited with {process.ExitCode}: {process.StandardError.ReadToEnd()}");
			}
		}

		private static string Sanitize(string name)
		{
			var builder = new System.Text.StringBuilder(name.Length);
			foreach (char c in name)
			{
				builder.Append(char.IsLetterOrDigit(c) || c is '.' or '_' or '-' ? c : '_');
			}

			return builder.ToString();
		}
	}
}
