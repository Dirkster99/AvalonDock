using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the overlay Window To Visibility Converter.
	/// </summary>
	public class OverlayWindowToVisibilityConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isHostedInFloatingWindow = value is OverlayWindow overlayWindow && overlayWindow.IsHostedInFloatingWindow;
			if (parameter == null || !bool.TryParse(parameter.ToString(), out bool isLarge))
			{
				isLarge = false;
			}

			return isHostedInFloatingWindow && isLarge ? Visibility.Hidden : Visibility.Visible;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<OverlayWindowToVisibilityConverter>();
		}
	}
}