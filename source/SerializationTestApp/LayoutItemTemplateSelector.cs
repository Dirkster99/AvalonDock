using System.Windows;
using System.Windows.Controls;

namespace SerializationTestApp;

public class LayoutItemTemplateSelector : DataTemplateSelector
{
	public DataTemplate? ExplorerTemplate { get; set; }
	public DataTemplate? PropertiesTemplate { get; set; }
	public DataTemplate? TextDocumentTemplate { get; set; }

	public override DataTemplate? SelectTemplate(object item, DependencyObject container)
	{
		if (item is ExplorerViewModel)
		{
			return ExplorerTemplate;
		}

		if (item is PropertiesViewModel)
		{
			return PropertiesTemplate;
		}

		if (item is TextDocumentViewModel)
		{
			return TextDocumentTemplate;
		}

		return base.SelectTemplate(item, container);
	}
}
