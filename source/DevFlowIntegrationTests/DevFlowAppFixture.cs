using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AvalonDock.DevFlowIntegrationTests;

/// <summary>Drives TestApp, the classic sample whose layout the docking tests exercise.</summary>
[CollectionDefinition("DevFlow")]
public sealed class DevFlowCollection : ICollectionFixture<TestAppFixture>
{
}

/// <summary>
/// Drives AvalonDockCodeApp (ToggleDockingManager). A separate application, a separate agent and a
/// separate port: the two samples carry different controls, so they cannot share one process.
/// </summary>
[CollectionDefinition("DevFlowCodeApp")]
public sealed class DevFlowCodeAppCollection : ICollectionFixture<CodeAppFixture>
{
}

public sealed class TestAppFixture : DevFlowAppFixture
{
    public TestAppFixture() : base("TestApp", 9223)
    {
    }
}

public sealed class CodeAppFixture : DevFlowAppFixture
{
    public CodeAppFixture() : base("AvalonDockCodeApp", 9224)
    {
    }
}

/// <summary>
/// Starts one sample application (Debug build, DevFlow agent on <see cref="AgentPort"/>) and keeps
/// it alive for the duration of a test collection, restarting it on demand.
/// </summary>
public class DevFlowAppFixture : IAsyncLifetime
{
    private Process _process;
    // The CI job uploads ${{ runner.temp }}/avalondock-devflow-*.log, so the log has to be written
    // there: on Windows Path.GetTempPath() is a per-user directory (%LOCALAPPDATA%\Temp) while the
    // runner's temp is elsewhere (D:\a\_temp), and on the Unix runners it is /private/var/... rather
    // than /tmp - the job uploads /tmp as well, but only the runner's own directory is guaranteed to be
    // collected on every leg. Without RUNNER_TEMP (a developer machine) the old paths are kept.
    private static readonly string TestAppLogPathFormat = Path.Combine(
        Environment.GetEnvironmentVariable("RUNNER_TEMP") is { Length: > 0 } runnerTemp
            ? runnerTemp
            : OperatingSystem.IsWindows() ? Path.GetTempPath() : "/tmp",
        "avalondock-devflow-{0}-{1}.log");
    private readonly object _logSync = new();
    private List<string> _hiddenAppNames = new();
    private readonly string _projectName;
    private bool _logStarted;
    private readonly int _port;

    protected DevFlowAppFixture(string projectName, int port)
    {
        _projectName = projectName;
        _port = port;
        LogPath = string.Format(TestAppLogPathFormat, projectName.ToLowerInvariant(), port);
    }

    public int AgentPort => _port;
    public string LogPath { get; }

    /// <summary>
    /// Locates the .NET host the sample applications are started with. The file name matters: on
    /// Windows the host is <c>dotnet.exe</c>, so a PATH scan for <c>dotnet</c> finds nothing at all
    /// even on a machine where the SDK is installed properly.
    /// </summary>
    private static string ResolveDotnetPath()
    {
        var fileName = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";

        var candidates = new List<string>();
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(dotnetRoot))
            candidates.Add(Path.Combine(dotnetRoot, fileName));

        if (OperatingSystem.IsWindows())
        {
            // Where the official installer and actions/setup-dotnet put it.
            candidates.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet",
                fileName));
            candidates.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "dotnet",
                fileName));
        }
        else
        {
            candidates.Add("/usr/local/share/dotnet/" + fileName);
            candidates.Add("/opt/homebrew/bin/" + fileName);
            candidates.Add("/usr/local/share/dotnet/x64/" + fileName);
        }

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        var envPath = Environment.GetEnvironmentVariable("PATH");
        if (envPath != null)
        {
            foreach (var dir in envPath.Split(Path.PathSeparator))
            {
                try
                {
                    var candidate = Path.Combine(dir, fileName);
                    if (File.Exists(candidate))
                        return candidate;
                }
                catch { }
            }
        }

        throw new FileNotFoundException(
            $"The .NET host ({fileName}) was not found on PATH or at a known location. " +
            $"PATH: {envPath}");
    }

    public async ValueTask InitializeAsync()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(DevFlowAppFixture).Assembly.Location);
        var testAppDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", _projectName));

        if (!Directory.Exists(testAppDir))
            throw new DirectoryNotFoundException(
                $"{_projectName} project not found at {testAppDir} (resolved from {assemblyDir})");

        var dotnetPath = ResolveDotnetPath();
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        var noBuild = string.Equals(
            Environment.GetEnvironmentVariable("DEVFLOW_TESTAPP_NO_BUILD"),
            "1",
            StringComparison.Ordinal);
        // Appended, not replaced: the suite restarts the app per test, and the interesting log is
        // often the one of an instance that has already been replaced.
        var header = $"{_projectName} log started {DateTimeOffset.Now:O}{Environment.NewLine}";
        lock (_logSync)
        {
            if (_logStarted)
                File.AppendAllText(LogPath, header);
            else
                File.WriteAllText(LogPath, header);
            _logStarted = true;
        }

        void AppendLog(string stream, string line)
        {
            lock (_logSync)
            {
                File.AppendAllText(LogPath, $"[{stream}] {line}{Environment.NewLine}");
            }
        }

        // `dotnet run --no-build` still evaluates the whole project (SDK resolution, LibreWPF.Sdk
        // targets, restore assets) before it starts anything. With one restart per test that is ~10 s
        // each on a windows-11-arm runner - 27 minutes for 80 tests - so a prebuilt app is started
        // directly from its output folder.
        var appDll = noBuild
            ? Path.Combine(testAppDir, "bin", "librewpf", "Debug", "net10.0-windows", _projectName + ".dll")
            : null;
        var startDirect = appDll != null && File.Exists(appDll);

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetPath,
                Arguments = startDirect
                    ? $"\"{appDll}\""
                    : $"run {(noBuild ? "--no-build " : string.Empty)}--project \"{Path.Combine(testAppDir, _projectName + ".LibreWPF.csproj")}\"",
                WorkingDirectory = startDirect ? Path.GetDirectoryName(appDll)! : testAppDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    ["DEVFLOW_AGENT_PORT"] = _port.ToString(),
                },
            },
        };

        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
                AppendLog("stdout", e.Data);
            }
        };
        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
                AppendLog("stderr", e.Data);
            }
        };

        try
        {
            _process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to start {_projectName}. Ensure .NET SDK is installed. Error: {ex.Message}", ex);
        }

        var started = _process;
        started.EnableRaisingEvents = true;
        started.Exited += (sender, _) =>
        {
            // A crash is otherwise only visible as "agent not reachable" in the next test. The
            // process is the one the handler was attached to, not whatever _process is by now.
            try { AppendLog("fixture", $"{_projectName} exited with code {((Process)sender).ExitCode}"); }
            catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException) { }
        };
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        var startWatch = Stopwatch.StartNew();

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(120);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (_process.HasExited)
            {
                var error = errorBuilder.Length > 0
                    ? $"\nStderr:\n{errorBuilder}"
                    : "";
                throw new InvalidOperationException(
                    $"{_projectName} exited prematurely (exit code: {_process.ExitCode}). Log: {LogPath}.{error}");
            }

            try
            {
                var response = await httpClient.GetAsync($"http://127.0.0.1:{_port}/api/v1/agent/status");
                if (response.IsSuccessStatusCode)
                {
                    // Per-test restarts make this the dominant cost on slow runners, so it is recorded.
                    AppendLog("fixture", $"{_projectName} ready on {_port} after {startWatch.ElapsedMilliseconds} ms");
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        var stderr = errorBuilder.Length > 0
            ? $"\nStderr output:\n{errorBuilder}"
            : "";

        throw new TimeoutException(
            $"{_projectName} did not start within 120 seconds on port {_port}. Log: {LogPath}.{stderr}");
    }

    public async ValueTask DisposeAsync()
    {
        await RestoreDesktopAsync();

        if (_process != null && !_process.HasExited)
        {
            try { _process.Kill(entireProcessTree: true); }
            catch { }
            await _process.WaitForExitAsync();
            _process.Dispose();
        }
    }

	internal async Task RestartAsync()
	{
		if (_process != null && !_process.HasExited)
		{
			try { _process.Kill(entireProcessTree: true); }
			catch { }
			await _process.WaitForExitAsync();
			_process.Dispose();
			_process = null;
		}

		// A new process has no mutation lease, and the id held for the old one would be rejected.
		DevFlowClient.ForgetLease(_port);

		await InitializeAsync();
	}

    // Native-input tests (see NativeInputEnvironment) drive REAL OS-level mouse events via cliclick,
    // at absolute screen coordinates. The per-test base has already positioned and guarded the main
    // window, so this must only activate it. Repositioning after the guard is armed can transiently
    // send the LibreWPF NSWindow off-screen. Hiding
    // other apps can mask bad drag-start coordinates by making an invalid screen point appear safe.
    internal async Task IsolateDesktopForNativeInputAsync()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        try
        {
            // Through DevFlowClient so the call carries the mutation lease (avd.activate moves the
            // app window, and the agent rejects any mutation that arrives without one).
            using var client = new DevFlowClient(_port);
            await client.InvokeAsync("avd.activate");
        }
        catch
        {
            // Best-effort: if System Events isn't scriptable here (no Accessibility permission, no
            // GUI session, etc.), fall through and let native-input tests fail/skip on their own
            // merits rather than blocking the whole fixture on desktop isolation.
        }
    }

    private async Task RestoreDesktopAsync()
    {
        if (_hiddenAppNames.Count == 0)
            return;

        foreach (var name in _hiddenAppNames)
        {
            try
            {
                await RunOsaScriptAsync(
                    $"tell application \"System Events\" to set visible of process \"{EscapeForAppleScript(name)}\" to true");
            }
            catch
            {
            }
        }

        _hiddenAppNames.Clear();
    }

    private static async Task<string> RunOsaScriptAsync(string script)
    {
        var psi = new ProcessStartInfo("osascript")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        psi.ArgumentList.Add("-e");
        psi.ArgumentList.Add(script);

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start osascript.");
        var stdout = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();
        return stdout.Trim();
    }

    private static string EscapeForAppleScript(string value) => value.Replace("\"", "\\\"");
}
