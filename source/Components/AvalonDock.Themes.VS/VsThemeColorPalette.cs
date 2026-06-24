using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AvalonDock.Themes.VS
{
	/// <summary>
	/// Represents the color palette extracted from a .vstheme file's Environment category.
	/// Provides typed access to well-known VS color tokens used for docking UI.
	/// </summary>
	public class VsThemeColorPalette
	{
		private readonly Dictionary<string, VsThemeColorEntry> _colors;

		/// <summary>
		/// Initializes a new instance of the <see cref="VsThemeColorPalette"/> class.
		/// </summary>
		/// <param name="colors">The dictionary of parsed color entries.</param>
		public VsThemeColorPalette(Dictionary<string, VsThemeColorEntry> colors)
		{
			_colors = colors ?? throw new ArgumentNullException(nameof(colors));
		}

		/// <summary>
		/// Gets the background color for the specified token name.
		/// </summary>
		/// <param name="name">The VS Environment color token name.</param>
		/// <returns>The color value, or null if not found.</returns>
		public Color? GetBackground(string name)
		{
			return _colors.TryGetValue(name, out var entry) ? entry.Background : null;
		}

		/// <summary>
		/// Gets the foreground color for the specified token name.
		/// </summary>
		/// <param name="name">The VS Environment color token name.</param>
		/// <returns>The color value, or null if not found.</returns>
		public Color? GetForeground(string name)
		{
			return _colors.TryGetValue(name, out var entry) ? entry.Foreground : null;
		}

		/// <summary>
		/// Gets the background color for the specified token name, or a fallback if not found.
		/// </summary>
		/// <param name="name">The VS Environment color token name.</param>
		/// <param name="fallback">The fallback color to return if the token is not found.</param>
		/// <returns>The resolved color value.</returns>
		public Color GetBackgroundOrDefault(string name, Color fallback)
		{
			return GetBackground(name) ?? fallback;
		}

		/// <summary>
		/// Gets the foreground color for the specified token name, or a fallback if not found.
		/// </summary>
		/// <param name="name">The VS Environment color token name.</param>
		/// <param name="fallback">The fallback color to return if the token is not found.</param>
		/// <returns>The resolved color value.</returns>
		public Color GetForegroundOrDefault(string name, Color fallback)
		{
			return GetForeground(name) ?? fallback;
		}

		/// <summary>
		/// Gets the total number of color entries in the palette.
		/// </summary>
		public int Count => _colors.Count;

		/// <summary>
		/// Determines whether the palette contains a color entry with the specified name.
		/// </summary>
		/// <param name="name">The VS Environment color token name.</param>
		/// <returns>True if the token exists; otherwise, false.</returns>
		public bool Contains(string name)
		{
			return _colors.ContainsKey(name);
		}
	}
}