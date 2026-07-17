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
		/// Creates a new palette by layering the given override entries on top of this one.
		/// </summary>
		/// <remarks>
		/// This is used to apply a VS2026 JSON override file (which only contains the
		/// tokens that were changed) on top of a full base palette. The merge is performed
		/// per field: an override that only specifies a background keeps the base
		/// foreground for that token, and vice versa. Tokens present only in
		/// <paramref name="overrides"/> are added.
		/// </remarks>
		/// <param name="overrides">The palette whose entries take precedence.</param>
		/// <returns>A new merged palette. Neither input palette is modified.</returns>
		public VsThemeColorPalette Merge(VsThemeColorPalette overrides)
		{
			if (overrides == null)
			{
				throw new ArgumentNullException(nameof(overrides));
			}

			var merged = new Dictionary<string, VsThemeColorEntry>(_colors);
			foreach (var kvp in overrides._colors)
			{
				if (merged.TryGetValue(kvp.Key, out var baseEntry))
				{
					merged[kvp.Key] = new VsThemeColorEntry(
						kvp.Value.Background ?? baseEntry.Background,
						kvp.Value.Foreground ?? baseEntry.Foreground);
				}
				else
				{
					merged[kvp.Key] = kvp.Value;
				}
			}

			return new VsThemeColorPalette(merged);
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