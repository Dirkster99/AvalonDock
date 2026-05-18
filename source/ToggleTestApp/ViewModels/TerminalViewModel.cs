using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;
using AvalonDock.Core;
using AvalonDock.Mvvm;

namespace ToggleTestApp.ViewModels;

public partial class TerminalViewModel : ToolboxBase, IDisposable
{
    private Process? _shellProcess;
    private readonly Dispatcher _dispatcher;
    private readonly StringBuilder _pendingOutput = new();
    private bool _outputPending;

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<string>? PromptReady;

    public string CurrentDirectory { get; private set; }

    public TerminalViewModel()
    {
        Id = "Terminal";
        Title = "Terminal";
        ToolTipText = "Terminal (Ctrl+`)";
        Side = ToolboxSide.Bottom;
        IsOpenByDefault = true;
        Icon = ToolboxIcons.Terminal;

        CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _dispatcher = Dispatcher.CurrentDispatcher;
        StartShell();
    }

    private void StartShell()
    {
        try
        {
            _shellProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoLogo -NoProfile -NoExit -Command \"function prompt { }\"",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = CurrentDirectory
                },
                EnableRaisingEvents = true
            };

            _shellProcess.OutputDataReceived += OnDataReceived;
            _shellProcess.ErrorDataReceived += OnDataReceived;
            _shellProcess.Exited += OnProcessExited;
            _shellProcess.Start();
            _shellProcess.BeginOutputReadLine();
            _shellProcess.BeginErrorReadLine();

            // Request initial working directory
            RequestPromptUpdate();
        }
        catch (Exception ex)
        {
            OutputReceived?.Invoke(this, $"Failed to start shell: {ex.Message}\n");
        }
    }

    public void ExecuteCommand(string command)
    {
        if (string.IsNullOrEmpty(command) || _shellProcess?.HasExited != false)
            return;

        _shellProcess.StandardInput.WriteLine(command);
        RequestPromptUpdate();
    }

    private void RequestPromptUpdate()
    {
        if (_shellProcess?.HasExited != false) return;
        _shellProcess.StandardInput.WriteLine("Write-Host \"::PWD::$((Get-Location).Path)\"");
    }

    private void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) return;

        // Intercept pwd marker
        if (e.Data.StartsWith("::PWD::"))
        {
            var dir = e.Data.Substring(7);
            CurrentDirectory = dir;
            _dispatcher.BeginInvoke(() => PromptReady?.Invoke(this, dir));
            return;
        }

        lock (_pendingOutput)
        {
            _pendingOutput.AppendLine(e.Data);
            if (!_outputPending)
            {
                _outputPending = true;
                _dispatcher.BeginInvoke(FlushOutput, DispatcherPriority.Background);
            }
        }
    }

    private void FlushOutput()
    {
        string text;
        lock (_pendingOutput)
        {
            text = _pendingOutput.ToString();
            _pendingOutput.Clear();
            _outputPending = false;
        }

        if (!string.IsNullOrEmpty(text))
        {
            OutputReceived?.Invoke(this, text);
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        _dispatcher.BeginInvoke(() =>
        {
            OutputReceived?.Invoke(this, "\n[Process exited]\n");
        });
    }

    public void Dispose()
    {
        if (_shellProcess is { HasExited: false })
        {
            try { _shellProcess.Kill(); } catch { }
        }
        _shellProcess?.Dispose();
    }
}
