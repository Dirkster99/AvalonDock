using System.Windows;
using System.Windows.Controls;
using AvalonDock.Core;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp.Views;

/// <summary>
/// Selects the appropriate LayoutItem container style based on whether
/// the content is a toolbox (IToolbox) or a document (EditorTabViewModel).
/// </summary>
public class PanesStyleSelector : StyleSelector
{
	public Style? ToolboxStyle { get; set; }
	public Style? DocumentStyle { get; set; }

	public override Style? SelectStyle(object item, DependencyObject container)
	{
		if (item is IToolbox)
			return ToolboxStyle;

		if (item is EditorTabViewModel)
			return DocumentStyle;

		return base.SelectStyle(item, container);
	}
}