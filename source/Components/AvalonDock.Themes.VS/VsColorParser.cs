using System.Globalization;
using System.Windows.Media;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Helper for converting Visual Studio theme color strings into WPF colors.
	/// Visual Studio stores colors as 8-digit AARRGGBB hexadecimal values in both
	/// the XML <c>.vstheme</c> format (<c>Source</c> attribute) and the VS2026
	/// JSON override format (<c>Background</c>/<c>Foreground</c> values).
	/// </summary>
	internal static class VsColorParser
	{
		/// <summary>
		/// Parses an 8-digit AARRGGBB hexadecimal color string.
		/// </summary>
		/// <param name="argb">The color string (for example, <c>FF1E1E1E</c>).</param>
		/// <returns>The parsed color, or <c>null</c> if the value is not a valid 8-digit AARRGGBB string.</returns>
		public static Color? FromArgbHex(string argb)
		{
			if (string.IsNullOrEmpty(argb) || argb.Length != 8)
			{
				return null;
			}

			if (!byte.TryParse(argb.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a) ||
				!byte.TryParse(argb.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
				!byte.TryParse(argb.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
				!byte.TryParse(argb.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
			{
				return null;
			}

			return Color.FromArgb(a, r, g, b);
		}
	}
}