using AvalonDock.Core;
using AvalonDock.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class SearchViewModel : ToolboxBase
{
    public SearchViewModel()
    {
        Id = "Search";
        Title = "Search";
        ToolTipText = "Search (Ctrl+Shift+F)";
        Side = ToolboxSide.Left;
        Icon = ToolboxIcons.Search;
    }

    [ObservableProperty]
    private string _searchText = string.Empty;
}

public partial class SourceControlViewModel : ToolboxBase
{
    public SourceControlViewModel()
    {
        Id = "Git";
        Title = "Source Control";
        ToolTipText = "Source Control (Ctrl+Shift+G)";
        Side = ToolboxSide.Left;
        Icon = ToolboxIcons.Git;
    }

    [ObservableProperty]
    private string _status = "No changes detected";
}

public partial class ProblemsViewModel : ToolboxBase
{
    public ProblemsViewModel()
    {
        Id = "Problems";
        Title = "Problems";
        ToolTipText = "Problems (Ctrl+Shift+M)";
        Side = ToolboxSide.Bottom;
        Icon = ToolboxIcons.Problems;
    }

    [ObservableProperty]
    private string _status = "No problems detected";
}
