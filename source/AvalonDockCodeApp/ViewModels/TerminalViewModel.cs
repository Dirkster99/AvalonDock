using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public partial class TerminalViewModel : ObservableToolboxBase, IDisposable
{
	private Process? _shellProcess;
	private readonly Dispatcher _dispatcher;

	[ObservableProperty] private string _output = string.Empty;

	[ObservableProperty] private string _inputCommand = string.Empty;

	public TerminalViewModel()
	{
		Id = "Terminal";
		Title = "Terminal";
		ToolTipText = "Terminal (Ctrl+`)";
		Shortcut = "Ctrl+OemTilde";
		Zone = DockZone.BottomRight;
		IsOpenByDefault = true;
		Icon = ToolboxIcons.Terminal;

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
					FileName = "powershell",
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					StandardOutputEncoding = Encoding.UTF8,
					StandardErrorEncoding = Encoding.UTF8,
					WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
				},
				EnableRaisingEvents = true
			};

			_shellProcess.OutputDataReceived += (_, e) =>
			{
				if (e.Data != null)
					AppendOutput(e.Data);
			};

			_shellProcess.ErrorDataReceived += (_, e) =>
			{
				if (e.Data != null)
					AppendOutput(e.Data);
			};

			_shellProcess.Start();
			_shellProcess.BeginOutputReadLine();
			_shellProcess.BeginErrorReadLine();
		}
		catch (Exception ex)
		{
			Output = $"Failed to start shell: {ex.Message}\n";
		}
	}

	private void AppendOutput(string text)
	{
		_dispatcher.BeginInvoke(() => { Output += text + "\n"; });
	}

	[RelayCommand]
	private void SendCommand()
	{
		if (string.IsNullOrEmpty(InputCommand) || _shellProcess?.HasExited != false)
			return;

		AppendOutput($"> {InputCommand}");
		_shellProcess.StandardInput.WriteLine(InputCommand);
		InputCommand = string.Empty;
	}

	[RelayCommand]
	private void Clear()
	{
		Output = string.Empty;
	}

	public void Dispose()
	{
		try
		{
			if (_shellProcess is { HasExited: false })
			{
				_shellProcess.Kill();
			}
		}
		catch
		{
		}

		_shellProcess?.Dispose();
	}
}