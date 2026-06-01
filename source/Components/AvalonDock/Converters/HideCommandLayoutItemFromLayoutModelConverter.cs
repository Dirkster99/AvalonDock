using System;
using System.Windows.Data;
using System.Windows.Markup;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the hide Command Layout Item From Layout Model Converter.
	/// </summary>
	public class HideCommandLayoutItemFromLayoutModelConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// when this converter is called layout could be constructing so many properties here are potentially not valid
			if (!(value is LayoutContent layoutModel))
				return null;

			if (layoutModel.Root == null)
				return null;

			if (layoutModel.Root.Manager == null)
				return null;

			if (!(layoutModel.Root.Manager.GetLayoutItemFromModel(layoutModel) is LayoutAnchorableItem layoutItemModel))
				return Binding.DoNothing;

			return layoutItemModel.HideCommand;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<HideCommandLayoutItemFromLayoutModelConverter>();
		}
	}
}