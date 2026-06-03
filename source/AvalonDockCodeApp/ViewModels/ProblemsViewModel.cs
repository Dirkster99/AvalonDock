using AvalonDock.Core;
using AvalonDock.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class ProblemsViewModel : ToolboxBase
{
	public ProblemsViewModel()
	{
		Id = "Problems";
		Title = "Problems";
		ToolTipText = "Problems (Ctrl+Shift+M)";
		Zone = DockZone.BottomLeft;
		Icon = ToolboxIcons.Problems;
	}

	[ObservableProperty] private string _status = "No problems detected";
}