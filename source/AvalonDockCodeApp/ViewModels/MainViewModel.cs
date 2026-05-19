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

    /// <summary>Provides typed access to the folder explorer VM.</summary>
    public FolderExplorerViewModel FolderExplorer { get; }

    public MainViewModel(IDockLayoutService dockService, FolderExplorerViewModel folderExplorer)
    {
        _dockService = dockService;
        FolderExplorer = folderExplorer;

        // Default: open the AvalonDock source folder
        var defaultPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
        if (Directory.Exists(defaultPath))
        {
            folderExplorer.LoadFolder(defaultPath);
        }
    }

    public void OpenFile(string filePath)
    {
        _dockService.OpenOrActivateDocument<EditorTabViewModel>(
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
            FolderExplorer.LoadFolder(dialog.FolderName);
        }
    }
}
