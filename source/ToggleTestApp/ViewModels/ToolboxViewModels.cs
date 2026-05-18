using AvalonDock.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class SearchViewModel : ObservableObject, IToolboxViewModel
{
    public string Title => "Search";
    public string ContentId => "Search";
    public string? ToolTipText => "Search (Ctrl+Shift+F)";
    public ToolboxSide Side => ToolboxSide.Left;
    public bool IsOpenByDefault => false;
    public object? Icon => ToolboxIcons.Search;

    [ObservableProperty]
    private string _searchText = string.Empty;
}

public partial class SourceControlViewModel : ObservableObject, IToolboxViewModel
{
    public string Title => "Source Control";
    public string ContentId => "Git";
    public string? ToolTipText => "Source Control (Ctrl+Shift+G)";
    public ToolboxSide Side => ToolboxSide.Left;
    public bool IsOpenByDefault => false;
    public object? Icon => ToolboxIcons.Git;

    [ObservableProperty]
    private string _status = "No changes detected";
}

public partial class ProblemsViewModel : ObservableObject, IToolboxViewModel
{
    public string Title => "Problems";
    public string ContentId => "Problems";
    public string? ToolTipText => "Problems (Ctrl+Shift+M)";
    public ToolboxSide Side => ToolboxSide.Bottom;
    public bool IsOpenByDefault => false;
    public object? Icon => ToolboxIcons.Problems;

    [ObservableProperty]
    private string _status = "No problems detected";
}
