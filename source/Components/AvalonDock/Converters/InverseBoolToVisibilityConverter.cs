using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the inverse Bool To Visibility Converter.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class InverseBoolToVisibilityConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is bool)) throw new ArgumentException("Invalid argument type. Expected argument: bool.", nameof(value));
			if (targetType != typeof(Visibility)) throw new ArgumentException("Invalid return type. Expected type: Visibility", nameof(targetType));
			var val = !(bool)value;
			if (val) return Visibility.Visible;
			return parameter is Visibility ? parameter : Visibility.Collapsed;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is Visibility)) throw new ArgumentException("Invalid argument type. Expected argument: Visibility.", nameof(value));
			if (targetType != typeof(bool)) throw new ArgumentException("Invalid return type. Expected type: bool", nameof(targetType));
			Visibility val = (Visibility)value;
			return val != Visibility.Visible;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<InverseBoolToVisibilityConverter>();
		}
	}
}