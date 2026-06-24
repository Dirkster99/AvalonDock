using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ToggleTestApp.Views;

/// <summary>
/// Converts IsDirectory bool to a Segoe MDL2 Assets icon glyph.
/// </summary>
public class FileIconConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is true ? "\uE8B7" : "\uE8A5"; // Folder : Page
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

/// <summary>
/// Converts IsDirectory to icon color (gold for folders, gray for files).
/// </summary>
public class FileColorConverter : IValueConverter
{
	private static readonly SolidColorBrush FolderBrush = new(Color.FromRgb(0xDC, 0xB6, 0x7A));
	private static readonly SolidColorBrush FileBrush = new(Color.FromRgb(0xCC, 0xCC, 0xCC));

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is true ? FolderBrush : FileBrush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

/// <summary>
/// Returns Collapsed when value is null, Visible otherwise.
/// </summary>
public class NullToCollapsedConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value == null ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}