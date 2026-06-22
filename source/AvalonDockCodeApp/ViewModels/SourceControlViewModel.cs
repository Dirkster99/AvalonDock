using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public enum FileChangeKind
{
	Modified,
	Added,
	Deleted,
	Untracked,
	Renamed
}

public partial class ChangeItem : ObservableObject
{
	[ObservableProperty] private string _filePath = string.Empty;
	[ObservableProperty] private string _fileName = string.Empty;
	[ObservableProperty] private FileChangeKind _kind;
	[ObservableProperty] private bool _isStaged;

	public string KindLabel => Kind switch
	{
		FileChangeKind.Modified => "M",
		FileChangeKind.Added => "A",
		FileChangeKind.Deleted => "D",
		FileChangeKind.Untracked => "U",
		FileChangeKind.Renamed => "R",
		_ => "?"
	};
}

public partial class SourceControlViewModel : ObservableToolboxBase
{
	private readonly Dispatcher _dispatcher;
	private Action<string>? _openFileCallback;
	private string _rootPath = string.Empty;

	public SourceControlViewModel()
	{
		Id = "Git";
		Title = "Source Control";
		ToolTipText = "Source Control (Ctrl+Shift+G)";
		Shortcut = "Ctrl+Shift+G";
		Zone = DockZone.LeftBottom;
		Icon = ToolboxIcons.Git;
		_dispatcher = Dispatcher.CurrentDispatcher;
	}

	[ObservableProperty] private string _status = "Not a git repository";
	[ObservableProperty] private string _branchName = string.Empty;
	[ObservableProperty] private string _commitMessage = string.Empty;
	[ObservableProperty] private ObservableCollection<ChangeItem> _stagedChanges = new();
	[ObservableProperty] private ObservableCollection<ChangeItem> _unstagedChanges = new();
	[ObservableProperty] private bool _isGitRepo;

	public void SetRootPath(string path)
	{
		_rootPath = path;
		RefreshCommand.Execute(null);
	}

	public void SetOpenFileCallback(Action<string> callback) => _openFileCallback = callback;

	[RelayCommand]
	private void Refresh()
	{
		StagedChanges.Clear();
		UnstagedChanges.Clear();

		if (string.IsNullOrEmpty(_rootPath) || !Directory.Exists(_rootPath))
		{
			IsGitRepo = false;
			Status = "No folder open";
			BranchName = string.Empty;
			return;
		}

		var gitDir = FindGitRoot(_rootPath);
		if (gitDir == null)
		{
			IsGitRepo = false;
			Status = "Not a git repository";
			BranchName = string.Empty;
			return;
		}

		IsGitRepo = true;
		BranchName = GetCurrentBranch(gitDir);

		var changes = GetGitStatus(gitDir);
		foreach (var change in changes)
		{
			if (change.IsStaged)
				StagedChanges.Add(change);
			else
				UnstagedChanges.Add(change);
		}

		if (StagedChanges.Count == 0 && UnstagedChanges.Count == 0)
			Status = "No changes";
		else
			Status = $"{StagedChanges.Count} staged, {UnstagedChanges.Count} changes";
	}

	[RelayCommand]
	private void StageFile(ChangeItem? item)
	{
		if (item == null || string.IsNullOrEmpty(_rootPath))
			return;

		var gitDir = FindGitRoot(_rootPath);
		if (gitDir == null)
			return;

		RunGit(gitDir, $"add \"{item.FilePath}\"");
		Refresh();
	}

	[RelayCommand]
	private void UnstageFile(ChangeItem? item)
	{
		if (item == null || string.IsNullOrEmpty(_rootPath))
			return;

		var gitDir = FindGitRoot(_rootPath);
		if (gitDir == null)
			return;

		RunGit(gitDir, $"reset HEAD \"{item.FilePath}\"");
		Refresh();
	}

	[RelayCommand]
	private void StageAll()
	{
		if (string.IsNullOrEmpty(_rootPath))
			return;

		var gitDir = FindGitRoot(_rootPath);
		if (gitDir == null)
			return;

		RunGit(gitDir, "add -A");
		Refresh();
	}

	[RelayCommand]
	private void UnstageAll()
	{
		if (string.IsNullOrEmpty(_rootPath))
			return;

		var gitDir = FindGitRoot(_rootPath);
		if (gitDir == null)
			return;

		RunGit(gitDir, "reset HEAD");
		Refresh();
	}

	[RelayCommand]
	private void Commit()
	{
		if (string.IsNullOrWhiteSpace(CommitMessage) || StagedChanges.Count == 0)
			return;

		var gitDir = FindGitRoot(_rootPath);
		if (gitDir == null)
			return;

		RunGit(gitDir, $"commit -m \"{CommitMessage.Replace("\"", "\\\"")}\"");
		CommitMessage = string.Empty;
		Refresh();
	}

	[RelayCommand]
	private void OpenFile(ChangeItem? item)
	{
		if (item != null)
			_openFileCallback?.Invoke(item.FilePath);
	}

	private static string? FindGitRoot(string path)
	{
		var dir = new DirectoryInfo(path);
		while (dir != null)
		{
			if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
				return dir.FullName;
			dir = dir.Parent;
		}

		return null;
	}

	private static string GetCurrentBranch(string gitRoot)
	{
		var result = RunGit(gitRoot, "rev-parse --abbrev-ref HEAD");
		return result.Trim();
	}

	private static List<ChangeItem> GetGitStatus(string gitRoot)
	{
		var output = RunGit(gitRoot, "status --porcelain=v1");
		var items = new List<ChangeItem>();

		foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
		{
			if (line.Length < 4)
				continue;

			var indexStatus = line[0];
			var workTreeStatus = line[1];
			var filePath = line[3..].Trim().Trim('"');

			var fullPath = Path.Combine(gitRoot, filePath.Replace('/', Path.DirectorySeparatorChar));
			var fileName = Path.GetFileName(filePath);

			if (indexStatus != ' ' && indexStatus != '?')
			{
				items.Add(new ChangeItem
				{
					FilePath = fullPath,
					FileName = fileName,
					Kind = ParseStatusChar(indexStatus),
					IsStaged = true
				});
			}

			if (workTreeStatus != ' ')
			{
				items.Add(new ChangeItem
				{
					FilePath = fullPath,
					FileName = fileName,
					Kind = workTreeStatus == '?' ? FileChangeKind.Untracked : ParseStatusChar(workTreeStatus),
					IsStaged = false
				});
			}
		}

		return items;
	}

	private static FileChangeKind ParseStatusChar(char c) => c switch
	{
		'M' => FileChangeKind.Modified,
		'A' => FileChangeKind.Added,
		'D' => FileChangeKind.Deleted,
		'R' => FileChangeKind.Renamed,
		_ => FileChangeKind.Modified
	};

	private static string RunGit(string workingDir, string args)
	{
		try
		{
			using var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "git",
					Arguments = args,
					WorkingDirectory = workingDir,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit(5000);
			return output;
		}
		catch
		{
			return string.Empty;
		}
	}
}