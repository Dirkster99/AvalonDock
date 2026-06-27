using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class ProblemsViewModel : ObservableToolboxBase
{
	public ProblemsViewModel()
	{
		Id = "Problems";
		Title = "Problems";
		ToolTipText = "Problems (Ctrl+Shift+M)";
		Shortcut = "Ctrl+Shift+M";
		Zone = DockZone.BottomLeft;
		Icon = ToolboxIcons.Problems;
	}

	[ObservableProperty] private string _status = "No problems detected";
}