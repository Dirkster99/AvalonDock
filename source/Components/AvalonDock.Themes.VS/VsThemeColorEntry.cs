using System.Windows.Media;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Holds the background and foreground colors for a single .vstheme color entry.
	/// </summary>
	public class VsThemeColorEntry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VsThemeColorEntry"/> class.
		/// </summary>
		/// <param name="background">The background color, or null if not defined.</param>
		/// <param name="foreground">The foreground color, or null if not defined.</param>
		public VsThemeColorEntry(Color? background, Color? foreground)
		{
			Background = background;
			Foreground = foreground;
		}

		/// <summary>
		/// Gets the background color, or null if not defined or not CT_RAW.
		/// </summary>
		public Color? Background { get; }

		/// <summary>
		/// Gets the foreground color, or null if not defined or not CT_RAW.
		/// </summary>
		public Color? Foreground { get; }
	}
}