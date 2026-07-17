using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public partial class SearchMatch : ObservableObject
{
	[ObservableProperty] private string _filePath = string.Empty;
	[ObservableProperty] private string _fileName = string.Empty;
	[ObservableProperty] private int _lineNumber;
	[ObservableProperty] private string _lineText = string.Empty;
	[ObservableProperty] private string _prefix = string.Empty;
	[ObservableProperty] private string _matchText = string.Empty;
	[ObservableProperty] private string _suffix = string.Empty;
}

public partial class SearchFileGroup : ObservableObject
{
	[ObservableProperty] private string _filePath = string.Empty;
	[ObservableProperty] private string _fileName = string.Empty;
	[ObservableProperty] private int _matchCount;
	[ObservableProperty] private bool _isExpanded = true;
	[ObservableProperty] private ObservableCollection<SearchMatch> _matches = new();
}

public partial class SearchViewModel : ObservableToolboxBase
{
	private readonly Dispatcher _dispatcher;
	private CancellationTokenSource _searchCts = new();
	private Action<string>? _openFileCallback;
	private string _rootPath = string.Empty;

	public SearchViewModel()
	{
		Id = "Search";
		Title = "Search";
		ToolTipText = "Search (Ctrl+Shift+F)";
		Shortcut = "Ctrl+Shift+F";
		Zone = DockZone.LeftTop;
		Icon = ToolboxIcons.Search;
		_dispatcher = Dispatcher.CurrentDispatcher;
	}

	[ObservableProperty] private string _searchText = string.Empty;
	[ObservableProperty] private bool _matchCase;
	[ObservableProperty] private bool _wholeWord;
	[ObservableProperty] private bool _useRegex;
	[ObservableProperty] private string _includeFilter = string.Empty;
	[ObservableProperty] private string _excludeFilter = string.Empty;
	[ObservableProperty] private string _statusText = "Ready";
	[ObservableProperty] private int _totalMatches;
	[ObservableProperty] private int _totalFiles;
	[ObservableProperty] private ObservableCollection<SearchFileGroup> _results = new();

	public void SetRootPath(string path) => _rootPath = path;

	public void SetOpenFileCallback(Action<string> callback) => _openFileCallback = callback;

	partial void OnSearchTextChanged(string value)
	{
		if (!string.IsNullOrEmpty(value) && value.Length >= 2)
		{
			SearchCommand.Execute(null);
		}
		else if (string.IsNullOrEmpty(value))
		{
			Results.Clear();
			TotalMatches = 0;
			TotalFiles = 0;
			StatusText = "Ready";
		}
	}

	[RelayCommand]
	private async Task SearchAsync()
	{
		_searchCts.Cancel();
		_searchCts.Dispose();
		_searchCts = new CancellationTokenSource();
		var token = _searchCts.Token;

		if (string.IsNullOrWhiteSpace(SearchText) || string.IsNullOrEmpty(_rootPath))
		{
			Results.Clear();
			TotalMatches = 0;
			TotalFiles = 0;
			StatusText = string.IsNullOrEmpty(_rootPath)
				? "No folder open"
				: "Ready";
			return;
		}

		StatusText = "Searching...";
		Results.Clear();
		TotalMatches = 0;
		TotalFiles = 0;

		var searchText = SearchText;
		var matchCase = MatchCase;
		var wholeWord = WholeWord;
		var useRegex = UseRegex;
		var includeFilter = IncludeFilter;
		var excludeFilter = ExcludeFilter;
		var rootPath = _rootPath;

		try
		{
			await Task.Run(() =>
			{
				var files = GetSearchableFiles(rootPath, includeFilter, excludeFilter);
				var fileCount = 0;
				var matchCount = 0;

				foreach (var filePath in files)
				{
					if (token.IsCancellationRequested)
						return;

					var matches = SearchFile(filePath, searchText, matchCase, wholeWord, useRegex);
					if (matches.Count > 0)
					{
						fileCount++;
						matchCount += matches.Count;

						var group = new SearchFileGroup
						{
							FilePath = filePath,
							FileName = Path.GetFileName(filePath),
							MatchCount = matches.Count,
							Matches = new ObservableCollection<SearchMatch>(matches)
						};

						_dispatcher.Invoke(() =>
						{
							Results.Add(group);
							TotalFiles = fileCount;
							TotalMatches = matchCount;
						});
					}

					if (matchCount > 5000)
						break;
				}

				_dispatcher.Invoke(() =>
				{
					StatusText = matchCount == 0
						? "No results found"
						: $"{matchCount} results in {fileCount} files";
				});
			}, token);
		}
		catch (OperationCanceledException)
		{
		}
	}

	[RelayCommand]
	private void OpenMatch(SearchMatch? match)
	{
		if (match != null)
			_openFileCallback?.Invoke(match.FilePath);
	}

	[RelayCommand]
	private void ClearSearch()
	{
		SearchText = string.Empty;
		Results.Clear();
		TotalMatches = 0;
		TotalFiles = 0;
		StatusText = "Ready";
	}

	private static string[] GetSearchableFiles(
		string rootPath, string includeFilter, string excludeFilter)
	{
		var extensions = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".cs", ".xaml", ".xml", ".json", ".csproj", ".sln",
			".props", ".targets", ".md", ".txt", ".js", ".ts",
			".css", ".html", ".htm", ".py", ".config", ".yml", ".yaml"
		};

		var includePatterns = string.IsNullOrWhiteSpace(includeFilter)
			? Array.Empty<string>()
			: includeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		var excludeDirs = string.IsNullOrWhiteSpace(excludeFilter)
			? new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{ "bin", "obj", ".git", ".vs", "node_modules", "packages" }
			: new System.Collections.Generic.HashSet<string>(
				excludeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
				StringComparer.OrdinalIgnoreCase);

		var files = new System.Collections.Generic.List<string>();
		CollectFiles(rootPath, extensions, excludeDirs, includePatterns, files);
		return files.ToArray();
	}

	private static void CollectFiles(
		string directory,
		System.Collections.Generic.HashSet<string> extensions,
		System.Collections.Generic.HashSet<string> excludeDirs,
		string[] includePatterns,
		System.Collections.Generic.List<string> result)
	{
		try
		{
			foreach (var file in Directory.EnumerateFiles(directory))
			{
				var ext = Path.GetExtension(file);
				if (!extensions.Contains(ext))
					continue;

				if (includePatterns.Length > 0)
				{
					var matched = false;
					foreach (var incl in includePatterns)
					{
						if (file.EndsWith(incl.TrimStart('*'), StringComparison.OrdinalIgnoreCase))
						{
							matched = true;
							break;
						}
					}

					if (!matched)
						continue;
				}

				result.Add(file);
			}

			foreach (var subDir in Directory.EnumerateDirectories(directory))
			{
				var dirName = Path.GetFileName(subDir);
				if (excludeDirs.Contains(dirName))
					continue;

				CollectFiles(subDir, extensions, excludeDirs, includePatterns, result);
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (IOException)
		{
		}
	}

	private static System.Collections.Generic.List<SearchMatch> SearchFile(
		string filePath, string searchText, bool matchCase, bool wholeWord, bool useRegex)
	{
		var matches = new System.Collections.Generic.List<SearchMatch>();
		try
		{
			var info = new FileInfo(filePath);
			if (info.Length > 2 * 1024 * 1024)
				return matches;

			var lines = File.ReadAllLines(filePath);
			Regex? regex = null;
			if (useRegex)
			{
				var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
				try
				{
					regex = new Regex(searchText, options);
				}
				catch (ArgumentException)
				{
					return matches;
				}
			}

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var comparison = matchCase
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase;

				if (useRegex && regex != null)
				{
					var m = regex.Match(line);
					if (m.Success)
					{
						if (wholeWord && !IsWholeWord(line, m.Index, m.Length))
							continue;

						matches.Add(CreateMatch(filePath, i + 1, line, m.Index, m.Length));
					}
				}
				else
				{
					int idx = line.IndexOf(searchText, comparison);
					if (idx >= 0)
					{
						if (wholeWord && !IsWholeWord(line, idx, searchText.Length))
							continue;

						matches.Add(CreateMatch(filePath, i + 1, line, idx, searchText.Length));
					}
				}

				if (matches.Count >= 100)
					break;
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}

		return matches;
	}

	private static SearchMatch CreateMatch(string filePath, int lineNumber, string line, int matchStart, int matchLength)
	{
		var trimmed = line.TrimStart();
		var offset = line.Length - trimmed.Length;
		var adjStart = Math.Max(0, matchStart - offset);
		var adjLength = Math.Min(matchLength, trimmed.Length - adjStart);

		return new SearchMatch
		{
			FilePath = filePath,
			FileName = Path.GetFileName(filePath),
			LineNumber = lineNumber,
			LineText = trimmed,
			Prefix = adjStart > 0 ? trimmed[..adjStart] : string.Empty,
			MatchText = adjLength > 0 ? trimmed.Substring(adjStart, adjLength) : string.Empty,
			Suffix = adjStart + adjLength < trimmed.Length ? trimmed[(adjStart + adjLength)..] : string.Empty
		};
	}

	private static bool IsWholeWord(string text, int start, int length)
	{
		if (start > 0 && char.IsLetterOrDigit(text[start - 1]))
			return false;
		var end = start + length;
		if (end < text.Length && char.IsLetterOrDigit(text[end]))
			return false;
		return true;
	}
}