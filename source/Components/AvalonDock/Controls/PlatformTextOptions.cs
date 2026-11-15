using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Text options the themes use where the value depends on the WPF implementation.
	/// </summary>
	public static class PlatformTextOptions
	{
		/// <summary>
		/// Gets the <see cref="System.Windows.Media.TextFormattingMode"/> for small UI text such as context menus and auto-hide tabs:
		/// <see cref="TextFormattingMode.Display"/> on WPF, and <see cref="TextFormattingMode.Ideal"/> on LibreWPF,
		/// whose published text stack throws <see cref="System.PlatformNotSupportedException"/> for Display.
		/// </summary>
		public static TextFormattingMode TextFormattingMode =>
#if LIBREWPF
			TextFormattingMode.Ideal;
#else
			TextFormattingMode.Display;
#endif
	}
}