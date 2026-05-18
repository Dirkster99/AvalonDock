using AvalonDock.Core;
using AvalonDock.Mvvm;

namespace ToggleTestApp.ViewModels;

public partial class TerminalViewModel : ToolboxBase
{
    public TerminalViewModel()
    {
        Id = "Terminal";
        Title = "Terminal";
        ToolTipText = "Terminal (Ctrl+`)";
        Side = ToolboxSide.Bottom;
        IsOpenByDefault = true;
        Icon = ToolboxIcons.Terminal;
    }
}
