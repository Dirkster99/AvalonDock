using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ToggleTestApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<EditorTabViewModel> _openEditors = new();

    [ObservableProperty]
    private EditorTabViewModel? _activeEditor;

    /// <summary>All registered toolbox ViewModels — bound to AnchorablesSource.</summary>
    public ObservableCollection<IToolboxViewModel> Toolboxes { get; }

    /// <summary>Provides typed access to the folder explorer VM.</summary>
    public FolderExplorerViewModel FolderExplorer { get; }

    public MainViewModel(IEnumerable<IToolboxViewModel> toolboxes, FolderExplorerViewModel folderExplorer)
    {
        FolderExplorer = folderExplorer;
        Toolboxes = new ObservableCollection<IToolboxViewModel>(toolboxes);

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
        var existing = OpenEditors.FirstOrDefault(e => e.FilePath == filePath);
        if (existing != null)
        {
            ActiveEditor = existing;
            return;
        }

        var tab = new EditorTabViewModel();
        tab.LoadFile(filePath);
        OpenEditors.Add(tab);
        ActiveEditor = tab;
    }

    [RelayCommand]
    private void CloseEditor(EditorTabViewModel? tab)
    {
        if (tab == null) return;
        OpenEditors.Remove(tab);
        if (ActiveEditor == tab)
        {
            ActiveEditor = OpenEditors.LastOrDefault();
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
