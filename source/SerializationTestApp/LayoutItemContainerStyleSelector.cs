using SerializationTestApp.Base;
using System.Windows;
using System.Windows.Controls;

namespace SerializationTestApp;

public class LayoutItemContainerStyleSelector : StyleSelector
{
	public Style? WindowStyle { get; set; }
	public Style? DocumentStyle { get; set; }

	public override Style? SelectStyle(object item, DependencyObject container)
	{
		if (item is DockableWindow) return WindowStyle;
		if (item is DockableDocument) return DocumentStyle;

		return base.SelectStyle(item, container);
	}
}
