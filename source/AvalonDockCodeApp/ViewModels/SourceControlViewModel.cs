using AvalonDock.Core;
using AvalonDock.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ToggleTestApp.ViewModels;

public partial class SourceControlViewModel : ToolboxBase
{
	public SourceControlViewModel()
	{
		Id = "Git";
		Title = "Source Control";
		ToolTipText = "Source Control (Ctrl+Shift+G)";
		Zone = DockZone.LeftTop;
		Icon = ToolboxIcons.Git;
	}

	[ObservableProperty] private string _status = "No changes detected";
}