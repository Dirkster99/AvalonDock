using System;
using System.Windows.Data;
using System.Windows.Markup;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the auto Hide Command Layout Item From Layout Model Converter.
	/// </summary>
	public class AutoHideCommandLayoutItemFromLayoutModelConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// when this converter is called layout could be constructing so many properties here are potentially not valid
			if (value as LayoutContent == null)
				return null;

			if ((value as LayoutContent).Root == null)
				return null;

			if ((value as LayoutContent).Root.Manager == null)
				return null;

			if (!((value as LayoutContent).Root.Manager.GetLayoutItemFromModel(value as LayoutContent) is LayoutAnchorableItem layoutItemModel))
				return Binding.DoNothing;

			return layoutItemModel.AutoHideCommand;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<AutoHideCommandLayoutItemFromLayoutModelConverter>();
		}
	}
}