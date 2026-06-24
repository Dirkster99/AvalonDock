using System;
using System.Windows.Data;
using System.Windows.Markup;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the anchor Side To Angle Converter.
	/// </summary>
	[ValueConversion(typeof(AnchorSide), typeof(double))]
	public class AnchorSideToAngleConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			AnchorSide side = (AnchorSide)value;
			if (side == AnchorSide.Left || side == AnchorSide.Right)
				return 90.0;

			return Binding.DoNothing;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<AnchorSideToAngleConverter>();
		}
	}
}