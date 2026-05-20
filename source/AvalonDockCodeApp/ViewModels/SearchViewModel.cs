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
		Zone = DockZone.LeftTop;
		Icon = ToolboxIcons.Search;
	}

	[ObservableProperty] private string _searchText = string.Empty;
}