using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DocumentDock _documentDock;

    /// <summary>The MVVM layout tree — bound to DockLayout DP on the DockingManager.</summary>
    [ObservableProperty]
    private IRootDock _dockLayout;

    /// <summary>Provides typed access to the folder explorer VM.</summary>
    public FolderExplorerViewModel FolderExplorer { get; }

    public MainViewModel(IEnumerable<IToolbox> toolboxes, FolderExplorerViewModel folderExplorer)
    {
        FolderExplorer = folderExplorer;

        _documentDock = new DocumentDock
        {
            Id = "Documents",
            Title = "Documents",
            VisibleDockables = new ObservableCollection<IDockable>()
        };

        var toolDock = new ToolDock
        {
            Id = "Tools",
            Title = "Tools",
            VisibleDockables = new ObservableCollection<IDockable>(toolboxes.Cast<IDockable>())
        };

        _dockLayout = new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = new ObservableCollection<IDockable> { _documentDock, toolDock }
        };

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
        var existing = _documentDock.VisibleDockables?
            .OfType<EditorTabViewModel>()
            .FirstOrDefault(e => e.FilePath == filePath);

        if (existing != null)
        {
            DockLayout.ActiveDockable = existing;
            return;
        }

        var tab = new EditorTabViewModel();
        tab.LoadFile(filePath);
        _documentDock.VisibleDockables?.Add(tab);
        DockLayout.ActiveDockable = tab;
    }

    [RelayCommand]
    private void CloseEditor(EditorTabViewModel? tab)
    {
        if (tab == null) return;
        _documentDock.VisibleDockables?.Remove(tab);

        if (DockLayout.ActiveDockable == tab)
        {
            DockLayout.ActiveDockable = _documentDock.VisibleDockables?
                .OfType<EditorTabViewModel>().LastOrDefault();
        }
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
