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

[CollectionDefinition("DevFlow")]
public sealed class DevFlowCollection : ICollectionFixture<DevFlowAppFixture>
{
}

public class DevFlowAppFixture : IAsyncLifetime
{
    private Process _process;
    private const int Port = 9223;
    private static readonly string TestAppLogPath = Path.Combine(
        OperatingSystem.IsWindows() ? Path.GetTempPath() : "/tmp",
        $"avalondock-devflow-testapp-{Port}.log");
    private readonly object _logSync = new();
    private List<string> _hiddenAppNames = new();

    public int AgentPort => Port;
    public string LogPath => TestAppLogPath;

    private static string ResolveDotnetPath()
    {
        foreach (var candidate in new[]
        {
            "/usr/local/share/dotnet/dotnet",
            "/opt/homebrew/bin/dotnet",
            "/usr/local/share/dotnet/x64/dotnet",
        })
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
                    var candidate = Path.Combine(dir, "dotnet");
                    if (File.Exists(candidate))
                        return candidate;
                }
                catch { }
            }
        }

        throw new FileNotFoundException("dotnet not found on system PATH or at known locations.");
    }

    public async ValueTask InitializeAsync()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(DevFlowAppFixture).Assembly.Location);
        var testAppDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "TestApp"));

        if (!Directory.Exists(testAppDir))
            throw new DirectoryNotFoundException(
                $"TestApp project not found at {testAppDir} (resolved from {assemblyDir})");

        var dotnetPath = ResolveDotnetPath();
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        var noBuild = string.Equals(
            Environment.GetEnvironmentVariable("DEVFLOW_TESTAPP_NO_BUILD"),
            "1",
            StringComparison.Ordinal);
        File.WriteAllText(TestAppLogPath, $"TestApp log started {DateTimeOffset.Now:O}{Environment.NewLine}");

        void AppendLog(string stream, string line)
        {
            lock (_logSync)
            {
                File.AppendAllText(TestAppLogPath, $"[{stream}] {line}{Environment.NewLine}");
            }
        }

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetPath,
                Arguments = $"run {(noBuild ? "--no-build " : string.Empty)}--project \"{testAppDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    ["DEVFLOW_AGENT_PORT"] = Port.ToString(),
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
                $"Failed to start TestApp. Ensure .NET SDK is installed. Error: {ex.Message}", ex);
        }

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

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
                    $"TestApp exited prematurely (exit code: {_process.ExitCode}). Log: {TestAppLogPath}.{error}");
            }

            try
            {
                var response = await httpClient.GetAsync($"http://localhost:{Port}/api/v1/agent/status");
                if (response.IsSuccessStatusCode)
                {
                    IntegrationTestBase.SetFixturePort(Port);
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
            $"TestApp did not start within 120 seconds on port {Port}. Log: {TestAppLogPath}.{stderr}");
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

    // Native-input tests (see NativeInputEnvironment) drive REAL OS-level mouse events via cliclick,
    // at absolute screen coordinates. Bring TestApp foreground immediately before those gestures, but
    // do not hide the user's desktop by default: that makes local debugging painful and looks like the
    // app is stealing focus. Set DEVFLOW_TEST_ISOLATE_DESKTOP=1 for the old full-desktop isolation.
    internal async Task IsolateDesktopForNativeInputAsync()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            if (!string.Equals(
                    Environment.GetEnvironmentVariable("DEVFLOW_TEST_ISOLATE_DESKTOP"),
                    "1",
                    StringComparison.Ordinal))
            {
                using var foregroundResp = await httpClient.PostAsync(
                    $"http://localhost:{Port}/api/v1/invoke/actions/avd.activate",
                    new StringContent("{\"args\":[]}", Encoding.UTF8, "application/json"));
                return;
            }

            var namesRaw = await RunOsaScriptAsync(
                "tell application \"System Events\"\n" +
                "    set procList to {}\n" +
                "    repeat with p in (every application process whose visible is true)\n" +
                "        set n to name of p\n" +
                "        if n is not \"Finder\" then\n" +
                "            set procList to procList & n\n" +
                "        end if\n" +
                "    end repeat\n" +
                "end tell\n" +
                "set AppleScript's text item delimiters to linefeed\n" +
                "return procList as text");

            _hiddenAppNames = namesRaw
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            foreach (var name in _hiddenAppNames)
            {
                await RunOsaScriptAsync(
                    $"tell application \"System Events\" to set visible of process \"{EscapeForAppleScript(name)}\" to false");
            }

            // Bring TestApp itself to the foreground now that nothing else is competing for it.
            using var resp = await httpClient.PostAsync(
                $"http://localhost:{Port}/api/v1/invoke/actions/avd.activate",
                new StringContent("{\"args\":[]}", Encoding.UTF8, "application/json"));
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
