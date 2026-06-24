using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public partial class FileTreeItem : ObservableObject
{
	[ObservableProperty] private string _name = string.Empty;

	[ObservableProperty] private string _fullPath = string.Empty;

	[ObservableProperty] private bool _isDirectory;

	[ObservableProperty] private bool _isExpanded;

	[ObservableProperty] private ObservableCollection<FileTreeItem> _children = new();

	public void LoadChildren()
	{
		if (!IsDirectory || Children.Count > 0)
			return;

		try
		{
			foreach (var dir in Directory.GetDirectories(FullPath).OrderBy(d => Path.GetFileName(d)))
			{
				Children.Add(
					new FileTreeItem
					{
						Name = Path.GetFileName(dir),
						FullPath = dir,
						IsDirectory = true
					});
			}

			foreach (var file in Directory.GetFiles(FullPath).OrderBy(f => Path.GetFileName(f)))
			{
				Children.Add(
					new FileTreeItem
					{
						Name = Path.GetFileName(file),
						FullPath = file,
						IsDirectory = false
					});
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (IOException)
		{
		}
	}
}

public partial class FolderExplorerViewModel : ObservableToolboxBase
{
	private Action<string> _openFileCallback;

	[ObservableProperty] private string _rootPath = string.Empty;

	[ObservableProperty] private ObservableCollection<FileTreeItem> _rootItems = new();

	[ObservableProperty] private FileTreeItem? _selectedItem;

	public FolderExplorerViewModel(Action<string> openFileCallback)
	{
		_openFileCallback = openFileCallback;
		Id = "Explorer";
		Title = "Explorer";
		ToolTipText = "Explorer (Ctrl+Shift+E)";
		Shortcut = "Ctrl+Shift+E";
		Zone = DockZone.LeftTop;
		Icon = ToolboxIcons.Explorer;
	}

	public void SetOpenFileCallback(Action<string> callback)
	{
		_openFileCallback = callback;
	}

	public void LoadFolder(string path)
	{
		RootPath = path;
		RootItems.Clear();

		if (!Directory.Exists(path))
			return;

		var root = new FileTreeItem
		{
			Name = Path.GetFileName(path),
			FullPath = path,
			IsDirectory = true,
			IsExpanded = true
		};
		root.LoadChildren();
		RootItems.Add(root);
	}

	[RelayCommand]
	private void OpenFile(FileTreeItem? item)
	{
		if (item is { IsDirectory: false })
		{
			_openFileCallback(item.FullPath);
		}
	}

	[RelayCommand]
	private void ExpandItem(FileTreeItem? item)
	{
		item?.LoadChildren();
	}
}