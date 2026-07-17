using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the anchor Side To Orientation Converter.
	/// </summary>
	[ValueConversion(typeof(AnchorSide), typeof(Orientation))]
	public class AnchorSideToOrientationConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			AnchorSide side = (AnchorSide)value;
			if (side == AnchorSide.Left || side == AnchorSide.Right)
				return Orientation.Vertical;

			return Orientation.Horizontal;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<AnchorSideToOrientationConverter>();
		}
	}
}