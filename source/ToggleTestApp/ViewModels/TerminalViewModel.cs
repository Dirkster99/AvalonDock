using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using AvalonDock.Core;
using AvalonDock.Mvvm;

namespace ToggleTestApp.ViewModels;

public partial class TerminalViewModel : ToolboxBase
{
    private Process? _process;
    private bool _running;

    public ObservableCollection<TerminalLine> OutputLines { get; } = new();

    public TerminalViewModel()
    {
        Id = "Terminal";
        Title = "Terminal";
        ToolTipText = "Terminal (Ctrl+`)";
        Side = ToolboxSide.Bottom;
        IsOpenByDefault = true;
        Icon = ToolboxIcons.Terminal;
    }

    public void StartProcess()
    {
        if (_running) return;

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoLogo -NoProfile",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        _process = new Process { StartInfo = psi };
        _process.OutputDataReceived += OnOutputReceived;
        _process.ErrorDataReceived += OnOutputReceived;
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        _running = true;
    }

    public void StopProcess()
    {
        _running = false;
        if (_process != null && !_process.HasExited)
        {
            try { _process.Kill(); } catch { }
        }
        _process?.Dispose();
        _process = null;
    }

    public void ExecuteLine(string line)
    {
        if (_process == null || _process.HasExited) return;
        _process.StandardInput.WriteLine(line);
    }

    private void OnOutputReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) return;
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            OutputLines.Add(new TerminalLine(e.Data));
        });
    }
}

public class TerminalLine
{
    public string Value { get; }
    public TerminalLine(string value) => Value = value;
}
