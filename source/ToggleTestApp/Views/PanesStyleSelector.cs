using System.Windows;
using System.Windows.Controls;
using AvalonDock.Core;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

/// <summary>
/// Selects the appropriate LayoutItem container style based on whether
/// the content is a toolbox (IToolboxViewModel) or a document (EditorTabViewModel).
/// </summary>
public class PanesStyleSelector : StyleSelector
{
    public Style? ToolboxStyle { get; set; }
    public Style? DocumentStyle { get; set; }

    public override Style? SelectStyle(object item, DependencyObject container)
    {
        if (item is IToolboxViewModel)
            return ToolboxStyle;

        if (item is EditorTabViewModel)
            return DocumentStyle;

        return base.SelectStyle(item, container);
    }
}
