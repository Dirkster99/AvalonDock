using System;
using System.Windows.Data;
using System.Windows.Markup;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the layout Item From Layout Model Converter.
	/// </summary>
	public class LayoutItemFromLayoutModelConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is LayoutContent layoutModel))
				return null;

			if (layoutModel.Root == null)
				return null;

			if (layoutModel.Root.Manager == null)
				return null;

			if (layoutModel.Root.Manager.GetLayoutItemFromModel(layoutModel) == null)
				return Binding.DoNothing;

			return layoutModel.Root.Manager.GetLayoutItemFromModel(layoutModel);
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<LayoutItemFromLayoutModelConverter>();
		}
	}
}