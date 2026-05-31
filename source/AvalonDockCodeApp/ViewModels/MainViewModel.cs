using System;
using System.IO;
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
	private readonly IDockLayoutService _dockService;

	/// <summary>The MVVM layout tree — bind to DockLayout on the DockingManager.</summary>
	public IRootDock DockLayout => _dockService.Layout;

	/// <summary>Provides typed access to the folder explorer VM via the layout service.</summary>
	public FolderExplorerViewModel? FolderExplorer => _dockService.GetAnchorable<FolderExplorerViewModel>();

	public MainViewModel(IDockLayoutService dockService)
	{
		_dockService = dockService;

		// Wire up the folder explorer's file-open callback
		var folderExplorer = FolderExplorer;
		if (folderExplorer != null)
		{
			folderExplorer.SetOpenFileCallback(OpenFile);

			// Default: open the AvalonDock source folder
			var defaultPath = Path.GetFullPath(
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
			if (Directory.Exists(defaultPath))
			{
				folderExplorer.LoadFolder(defaultPath);
			}
		}

		// Wire up search VM with root path and file-open callback
		var search = _dockService.GetAnchorable<SearchViewModel>();
		if (search != null)
		{
			search.SetOpenFileCallback(OpenFile);
			if (folderExplorer != null)
				search.SetRootPath(folderExplorer.RootPath);
		}

		// Wire up source control VM
		var sourceControl = _dockService.GetAnchorable<SourceControlViewModel>();
		if (sourceControl != null)
		{
			sourceControl.SetOpenFileCallback(OpenFile);
			if (folderExplorer != null)
				sourceControl.SetRootPath(folderExplorer.RootPath);
		}
	}

	public void OpenFile(string filePath)
	{
		_dockService.OpenOrActivateDocument(
			e => e.FilePath == filePath,
			() =>
			{
				var tab = new EditorTabViewModel();
				tab.LoadFile(filePath);
				return tab;
			});
	}

	[RelayCommand]
	private void CloseEditor(EditorTabViewModel? tab)
	{
		if (tab != null)
			_dockService.CloseDocument(tab);
	}

	[RelayCommand]
	private void OpenFolder()
	{
		var dialog = new Microsoft.Win32.OpenFolderDialog
		{
			Title = "Select a folder to open"
		};

		if (dialog.ShowDialog() == true)
		{
			FolderExplorer?.LoadFolder(dialog.FolderName);

			var search = _dockService.GetAnchorable<SearchViewModel>();
			search?.SetRootPath(dialog.FolderName);

			var sourceControl = _dockService.GetAnchorable<SourceControlViewModel>();
			sourceControl?.SetRootPath(dialog.FolderName);
		}
	}
}